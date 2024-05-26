using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CustomFloorPlugin.Helpers;

using IPA.Loader;
using IPA.Utilities;

using JetBrains.Annotations;

using Newtonsoft.Json;

using SiraUtil.Web;

using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// A class that handles all interaction with outside plugins, at the moment just SongCore and Cinema<br/>
    /// Also detects changes in the directory and reflects them in-game, e.g. loading, updating or removing platforms
    /// </summary>
    [UsedImplicitly]
    internal class ConnectionManager : IInitializable, IDisposable
    {
        private readonly IHttpService _httpService;
        private readonly PlatformManager _platformManager;
        private readonly PlatformSpawner _platformSpawner;

        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly bool _isSongCoreInstalled;
        private readonly bool _isCinemaInstalled;

        public ConnectionManager(IHttpService httpService, PlatformManager platformManager, PlatformSpawner platformSpawner)
        {
            _httpService = httpService;
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
            if (_isCinemaInstalled)
                InitializeCinemaConnection();
            if (_isSongCoreInstalled)
                InitializeSongCoreConnection();
        }

        public void Dispose()
        {
            _fileSystemWatcher.Changed -= OnFileChanged;
            _fileSystemWatcher.Created -= OnFileCreated;
            _fileSystemWatcher.Deleted -= OnFileDeleted;
            _fileSystemWatcher.Dispose();
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            if (_isCinemaInstalled)
                DisposeCinemaConnection();
            if (_isSongCoreInstalled)
                DisposeSongCoreConnection();
        }

        /// <summary>
        /// Replaces the old platform with the updated version
        /// </summary>
        // ReSharper disable once AsyncVoidMethod
        private async void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (!UnityGame.OnMainThread)
                await UnityGame.SwitchToMainThreadAsync();
            if (!_platformManager.AllPlatforms.TryGetFirst(x => x.fullPath == e.FullPath, out CustomPlatform platform)) return;
            bool wasActivePlatform = platform == _platformManager.ActivePlatform;
            CustomPlatform? newPlatform = await _platformManager.CreatePlatformAsync(e.FullPath);
            if (!wasActivePlatform || newPlatform is null) return;
            await _platformSpawner.ChangeToPlatformAsync(newPlatform);
        }

        /// <summary>
        /// Create the new platform and add it to the UI
        /// </summary>
        // ReSharper disable once AsyncVoidMethod
        private async void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            if (!UnityGame.OnMainThread)
                await UnityGame.SwitchToMainThreadAsync();
            CustomPlatform? newPlatform = await _platformManager.CreatePlatformAsync(e.FullPath);
            if (newPlatform is null) return;
            _platformManager.AllPlatforms.AddSorted(PlatformManager.BuildInPlatformsCount, _platformManager.AllPlatforms.Count - PlatformManager.BuildInPlatformsCount, newPlatform);
        }

        /// <summary>
        /// Destroy the platform and remove all references
        /// </summary>
        // ReSharper disable once AsyncVoidMethod
        private async void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            if (!UnityGame.OnMainThread)
                await UnityGame.SwitchToMainThreadAsync();
            if (!_platformManager.AllPlatforms.TryGetFirst(x => x.fullPath == e.FullPath, out CustomPlatform platform)) return;
            if (platform == _platformManager.ActivePlatform) await _platformSpawner.ChangeToPlatformAsync(_platformManager.DefaultPlatform);
            _platformManager.AllPlatforms.Remove(platform);
            UnityEngine.Object.Destroy(platform.gameObject);
        }

        private void InitializeSongCoreConnection()
        {
            SongCore.Plugin.CustomSongPlatformSelectionDidChange += OnSongCoreEvent;
            SongCore.Collections.RegisterCapability("Custom Platforms");
        }

        private void DisposeSongCoreConnection()
        {
            SongCore.Plugin.CustomSongPlatformSelectionDidChange -= OnSongCoreEvent;
            SongCore.Collections.DeregisterCapability("Custom Platforms");
        }

        private void InitializeCinemaConnection() => BeatSaberCinema.Events.AllowCustomPlatform += OnCinemaEvent;

        private void DisposeCinemaConnection() => BeatSaberCinema.Events.AllowCustomPlatform -= OnCinemaEvent;

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
        /// <param name="beatmapLevel">The <see cref="BeatmapLevel"/> the platform was requested for</param>
        // ReSharper disable once AsyncVoidMethod
        private async void OnSongCoreEvent(bool usePlatform, string? name, string? hash, BeatmapLevel beatmapLevel)
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

            await DownloadPlatform(url, _cancellationTokenSource.Token);
        }

        /// <summary>
        /// Asynchronously downloads a <see cref="CustomPlatform"/> from modelsaber if the selected level requires it
        /// </summary>
        private async Task DownloadPlatform(string url, CancellationToken cancellationToken)
        {
            IHttpResponse downloadDataWebResponse = await _httpService.GetAsync(url, null, cancellationToken);
            if (!downloadDataWebResponse.Successful) return;
            string json = await downloadDataWebResponse.ReadAsStringAsync();
            PlatformDownloadData? platformDownloadData = JsonConvert.DeserializeObject<Dictionary<string, PlatformDownloadData>>(json)?.FirstOrDefault().Value;
            if (platformDownloadData?.download is null) return;
            IHttpResponse platDownloadWebResponse = await _httpService.GetAsync(platformDownloadData.download, null, cancellationToken);
            if (!platDownloadWebResponse.Successful) return;
            byte[] platData = await platDownloadWebResponse.ReadAsByteArrayAsync();
            string path = Path.Combine(_platformManager.DirectoryPath, $"{platformDownloadData.name}.plat");
            _fileSystemWatcher.EnableRaisingEvents = false;
            await File.WriteAllBytesAsync(path, platData, cancellationToken);
            _fileSystemWatcher.EnableRaisingEvents = true;
            CustomPlatform? requestedPlatform = await _platformManager.CreatePlatformAsync(path);
            if (cancellationToken.IsCancellationRequested || requestedPlatform is null) return;
            _platformManager.AllPlatforms.AddSorted(PlatformManager.BuildInPlatformsCount, _platformManager.AllPlatforms.Count - PlatformManager.BuildInPlatformsCount, requestedPlatform);
            _platformManager.APIRequestedPlatform = requestedPlatform;
        }
    }
}
