using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CustomFloorPlugin.Helpers;

using IPA.Loader;
using IPA.Utilities;
using IPA.Utilities.Async;

using SiraUtil;

using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// A class that handles all interaction with outside plugins, at the moment just SongCore and Cinema<br/>
    /// Also detects changes in the directory and reflects them in-game, e.g. loading, updating or removing platforms
    /// </summary>
    internal class API : IInitializable, IDisposable
    {
        private readonly WebClient _webClient;
        private readonly PlatformManager _platformManager;
        private readonly PlatformSpawner _platformSpawner;

        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly bool _isSongCoreInstalled;
        private readonly bool _isCinemaInstalled;

        public API(WebClient webClient,
                   PlatformManager platformManager,
                   PlatformSpawner platformSpawner)
        {
            _webClient = webClient;
            _platformManager = platformManager;
            _platformSpawner = platformSpawner;
            _fileSystemWatcher = new FileSystemWatcher(_platformManager.DirectoryPath, "*.plat");
            _cancellationTokenSource = new CancellationTokenSource();
            _isSongCoreInstalled = PluginManager.GetPlugin("SongCore") is not null;
            _isCinemaInstalled = PluginManager.GetPlugin("Cinema") is not null;
        }

        public void Initialize()
        {
            _fileSystemWatcher.Changed += OnFileChanged;
            _fileSystemWatcher.Created += OnFileCreated;
            _fileSystemWatcher.Deleted += OnFileDeleted;
            _fileSystemWatcher.EnableRaisingEvents = true;
            if (_isSongCoreInstalled) SubscribeToSongCoreEvent();
            if (_isCinemaInstalled) SubscribeToCinemaEvent();
        }

        public void Dispose()
        {
            _fileSystemWatcher.Changed -= OnFileChanged;
            _fileSystemWatcher.Created -= OnFileCreated;
            _fileSystemWatcher.Deleted -= OnFileDeleted;
            _fileSystemWatcher.Dispose();
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            if (_isSongCoreInstalled) UnsubscribeFromSongCoreEvent();
            if (_isCinemaInstalled) UnsubscribeFromCinemaEvent();
        }

        /// <summary>
        /// Replaces the old platform with the updated version
        /// </summary>
        private async void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (!UnityGame.OnMainThread)
            {
                _ = UnityMainThreadTaskScheduler.Factory.StartNew(() => OnFileChanged(sender, e));
                return;
            }

            if (_platformManager.AllPlatforms.TryGetFirst(x => x.fullPath == e.FullPath, out CustomPlatform platform))
            {
                bool wasActivePlatform = _platformManager.ActivePlatform == platform;
                CustomPlatform? newPlatform = await _platformManager.CreatePlatformAsync(e.FullPath);
                if (!wasActivePlatform || newPlatform is null) return;
                _ = _platformSpawner.ChangeToPlatformAsync(newPlatform);
            }
        }

        /// <summary>
        /// Create the new platform and add it to the UI
        /// </summary>
        private async void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            if (!UnityGame.OnMainThread)
            {
                _ = UnityMainThreadTaskScheduler.Factory.StartNew(() => OnFileCreated(sender, e));
                return;
            }

            CustomPlatform? newPlatform = await _platformManager.CreatePlatformAsync(e.FullPath);
            if (newPlatform is null) return;
            _platformManager.AllPlatforms.AddSorted(1, _platformManager.AllPlatforms.Count - 1, newPlatform, null);
        }

        /// <summary>
        /// Destroy the platform and remove all references
        /// </summary>
        private async void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            if (!UnityGame.OnMainThread)
            {
                _ = UnityMainThreadTaskScheduler.Factory.StartNew(() => OnFileDeleted(sender, e));
                return;
            }

            if (_platformManager.AllPlatforms.TryGetFirst(x => x.fullPath == e.FullPath, out CustomPlatform platform))
            {
                if (_platformManager.ActivePlatform == platform) await _platformSpawner.ChangeToPlatformAsync(_platformManager.DefaultPlatform);
                _platformManager.AllPlatforms.Remove(platform);
                UnityEngine.Object.Destroy(platform.gameObject);
            }
        }

        private void SubscribeToSongCoreEvent() => SongCore.Plugin.CustomSongPlatformSelectionDidChange += OnSongCoreEvent;

        private void UnsubscribeFromSongCoreEvent() => SongCore.Plugin.CustomSongPlatformSelectionDidChange -= OnSongCoreEvent;

        private void SubscribeToCinemaEvent() => BeatSaberCinema.Events.AllowCustomPlatform += OnCinemaEvent;

        private void UnsubscribeFromCinemaEvent() => BeatSaberCinema.Events.AllowCustomPlatform -= OnCinemaEvent;

        /// <summary>
        /// Disable platform spawning as required by Cinema
        /// </summary>
        private void OnCinemaEvent(bool allowPlatform)
        {
            if (!allowPlatform)
                _platformManager.APIRequestedPlatform = _platformManager.DefaultPlatform;
        }

        /// <summary>
        /// The class the API response of modelsaber is deserialized on
        /// </summary>
        [Serializable]
        public class PlatformDownloadData
        {
            public string? name;
            public string? download;
        }

        /// <summary>
        /// Sets up the requested platform and downloads it if needed
        /// </summary>
        /// <param name="usePlatform">Whether the selected song requests a platform or not</param>
        /// <param name="name">The name of the requested platform</param>
        /// <param name="hash">The hash of the requested platform</param>
        /// /// <param name="_1">The <see cref="IPreviewBeatmapLevel"/> the platform was requested for</param>
        private void OnSongCoreEvent(bool usePlatform, string? name, string? hash, IPreviewBeatmapLevel _1)
        {
            // No platform is requested, abort
            if (!usePlatform)
            {
                _platformManager.APIRequestedPlatform = null;
                return;
            }

            // Check if the requested platform is already downloaded
            if (_platformManager.AllPlatforms.TryGetFirst(x => x.platHash == hash || x.platName == name, out CustomPlatform platform))
            {
                _platformManager.APIRequestedPlatform = platform;
                return;
            }

            string url = hash is not null ? $"https://modelsaber.com/api/v2/get.php?type=platform&filter=hash:{hash}"
                : name is not null ? $"https://modelsaber.com/api/v2/get.php?type=platform&filter=name:{name}"
                : throw new ArgumentNullException($"{nameof(hash)}, {nameof(name)}", "Invalid platform request");

            Task.Run(() => DownloadPlatform(url));
        }

        /// <summary>
        /// Asynchronously downloads a <see cref="CustomPlatform"/> from modelsaber if the selected level requires it
        /// </summary>
        private async void DownloadPlatform(string url)
        {
            try
            {
                CancellationToken cancellationToken = _cancellationTokenSource.Token;
                WebResponse downloadDataWebResponse = await _webClient.GetAsync(url, cancellationToken);
                if (!downloadDataWebResponse.IsSuccessStatusCode) return;
                PlatformDownloadData platformDownloadData = downloadDataWebResponse.ContentToJson<Dictionary<string, PlatformDownloadData>>().First().Value;
                WebResponse platDownloadWebResponse = await _webClient.GetAsync(platformDownloadData.download, cancellationToken);
                if (!platDownloadWebResponse.IsSuccessStatusCode) return;
                byte[] platData = platDownloadWebResponse.ContentToBytes();
                string path = Path.Combine(_platformManager.DirectoryPath, $"{platformDownloadData.name}.plat");
                _fileSystemWatcher.EnableRaisingEvents = false;
                File.WriteAllBytes(path, platData);
                _fileSystemWatcher.EnableRaisingEvents = true;
                CustomPlatform? requestedPlatform = await _platformManager.CreatePlatformAsync(path);
                cancellationToken.ThrowIfCancellationRequested();
                if (requestedPlatform is null) return;
                _platformManager.AllPlatforms.AddSorted(1, _platformManager.AllPlatforms.Count - 1, requestedPlatform, null);
                _platformManager.APIRequestedPlatform = requestedPlatform;
            }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
        }
    }
}