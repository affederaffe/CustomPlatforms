using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using CustomFloorPlugin.Configuration;
using CustomFloorPlugin.Helpers;
using IPA.Loader;
using IPA.Utilities;
using IPA.Utilities.Async;

using SiraUtil;

using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// A class that handles all interaction with outside plugins, atm just SongCore and Cinema
    /// Also detects changes in the platforms directory and loads added platforms automatically / deletes removed ones
    /// </summary>
    internal class API : IInitializable, IDisposable
    {
        private readonly WebClient _webClient;
        private readonly PluginConfig _config;
        private readonly PlatformManager _platformManager;
        private readonly PlatformSpawner _platformSpawner;
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly bool _isSongCoreInstalled;
        private readonly bool _isCinemaInstalled;

        public API(WebClient webClient,
                   PluginConfig config,
                   PlatformManager platformManager,
                   PlatformSpawner platformSpawner)
        {
            _webClient = webClient;
            _config = config;
            _platformManager = platformManager;
            _platformSpawner = platformSpawner;
            _fileSystemWatcher = new FileSystemWatcher(_config.CustomPlatformsDirectory, "*.plat");
            _isSongCoreInstalled = PluginManager.GetPlugin("SongCore") != null;
            _isCinemaInstalled = PluginManager.GetPlugin("BeatSaberCinema") != null;
        }

        public void Initialize()
        {
            _fileSystemWatcher.Changed += OnFileChanged;
            _fileSystemWatcher.Created += OnFileCreated;
            _fileSystemWatcher.Deleted += OnFileDeleted;
            _fileSystemWatcher.EnableRaisingEvents = true;
            if (_isSongCoreInstalled)
                SubscribeToSongCoreEvent();
            if (_isCinemaInstalled)
                SubscribeToCinemaEvent();
        }

        public void Dispose()
        {
            _fileSystemWatcher.Changed -= OnFileChanged;
            _fileSystemWatcher.Created -= OnFileCreated;
            _fileSystemWatcher.Deleted -= OnFileDeleted;
            _fileSystemWatcher.Dispose();
            if (_isSongCoreInstalled)
                UnsubscribeFromSongCoreEvent();
            if (_isCinemaInstalled)
                UnsubscribeFromCinemaEvent();
        }

        /// <summary>
        /// Replaces the old platform with the updated version
        /// </summary>
        private async void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (!UnityGame.OnMainThread)
            {
                await UnityMainThreadTaskScheduler.Factory.StartNew(() => OnFileChanged(sender, e));
                return;
            }

            if (_platformManager.PlatformFilePaths.TryGetValue(e.FullPath, out CustomPlatform platform))
            {
                bool wasActivePlatform = _platformManager.ActivePlatform == platform;
                CustomPlatform? newPlatform = await _platformManager.CreatePlatformAsync(e.FullPath);
                if (newPlatform == null) return;
                if (wasActivePlatform)
                {
                    int index = _platformManager.AllPlatforms.IndexOf(newPlatform);
                    _ = _platformSpawner.ChangeToPlatformAsync(index);
                }
            }
        }

        /// <summary>
        /// Create the new platform and add it to the UI
        /// </summary>
        private async void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            if (!UnityGame.OnMainThread)
            {
                await UnityMainThreadTaskScheduler.Factory.StartNew(() => OnFileCreated(sender, e));
                return;
            }

            CustomPlatform? newPlatform = await _platformManager.CreatePlatformAsync(e.FullPath);
            if (newPlatform == null) return;
            _platformManager.AllPlatforms.AddSorted(1, _platformManager.AllPlatforms.Count - 1, newPlatform);
        }

        /// <summary>
        /// Destroy the platform and remove all references
        /// </summary>
        private async void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            if (!UnityGame.OnMainThread)
            {
                await UnityMainThreadTaskScheduler.Factory.StartNew(() => OnFileDeleted(sender, e));
                return;
            }

            if (_platformManager.PlatformFilePaths.TryGetValue(e.FullPath, out CustomPlatform platform))
            {
                if (_platformManager.ActivePlatform == platform)
                    _ = _platformSpawner.ChangeToPlatformAsync(0);
                _platformManager.PlatformFilePaths.Remove(platform.fullPath);
                _platformManager.AllPlatforms.Remove(platform);
                UnityEngine.Object.Destroy(platform.gameObject);
            }
        }

        private void SubscribeToSongCoreEvent() => SongCore.Plugin.CustomSongPlatformSelectionDidChange += OnSongCoreEvent;
        private void SubscribeToCinemaEvent() => BeatSaberCinema.Events.AllowCustomPlatform += OnCinemaEvent;
        private void UnsubscribeFromSongCoreEvent() => SongCore.Plugin.CustomSongPlatformSelectionDidChange -= OnSongCoreEvent;
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
        /// Asynchronously downloads a <see cref="CustomPlatform"/> from modelsaber if the selected level requires it
        /// </summary>
        /// <param name="usePlatform">Whether the selected song requests a platform or not</param>
        /// <param name="name">The name of the requested platform</param>
        /// <param name="hash">The hash of the requested platform</param>
        /// <param name="level">The song the platform was requested for</param>
        private async void OnSongCoreEvent(bool usePlatform, string? name, string? hash, IPreviewBeatmapLevel level)
        {
            // No platform is requested, abort
            if (!usePlatform)
            {
                _platformManager.APIRequestedPlatform = null;
                _platformManager.APIRequestedLevelId = null;
                return;
            }

            _platformManager.APIRequestedLevelId = level.levelID;

            // Check if the requested platform is already downloaded
            foreach (CustomPlatform platform in _platformManager.AllPlatforms)
            {
                if (platform.platHash == hash || platform.platName == name)
                {
                    _platformManager.APIRequestedPlatform = platform;
                    return;
                }
            }

            string url = hash != null ? $"https://modelsaber.com/api/v2/get.php?type=platform&filter=hash:{hash}"
                       : name != null ? $"https://modelsaber.com/api/v2/get.php?type=platform&filter=name:{name}"
                       : throw new ArgumentNullException($"{nameof(hash)}, {nameof(name)}", "Invalid platform request");

            WebResponse downloadDataWebResponse = await _webClient.GetAsync(url, CancellationToken.None);
            if (!downloadDataWebResponse.IsSuccessStatusCode) return;
            PlatformDownloadData platformDownloadData = downloadDataWebResponse.ContentToJson<Dictionary<string, PlatformDownloadData>>().First().Value;
            WebResponse platDownloadWebResponse = await _webClient.GetAsync(platformDownloadData.download, CancellationToken.None);
            if (!platDownloadWebResponse.IsSuccessStatusCode) return;
            byte[] platData = platDownloadWebResponse.ContentToBytes();
            string path = Path.Combine(_config.CustomPlatformsDirectory, $"{platformDownloadData.name}.plat");
            _fileSystemWatcher.EnableRaisingEvents = false;
            File.WriteAllBytes(path, platData);
            _fileSystemWatcher.EnableRaisingEvents = true;
            _platformManager.APIRequestedPlatform = await _platformManager.CreatePlatformAsync(path);
        }
    }
}