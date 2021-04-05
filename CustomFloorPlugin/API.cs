using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using CustomFloorPlugin.Configuration;
using CustomFloorPlugin.UI;

using IPA.Loader;

using Newtonsoft.Json;

using SiraUtil.Tools;

using UnityEngine;
using UnityEngine.Networking;

using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// A class that handles all interaction with outside plugins, atm just SongCore and Cinema
    /// Also detects changes in the platforms directory and loads added platforms automaticly / deletes removed ones
    /// </summary>
    internal class API : IInitializable, IDisposable
    {
        private readonly SiraLog _siraLog;
        private readonly PluginConfig _config;
        private readonly PlatformManager _platformManager;
        private readonly PlatformSpawner _platformSpawner;
        private readonly PlatformListsView _platformListsView;
        private readonly FileSystemWatcher _fileSystemWatcher;

        private bool apiRequest;

        public API(SiraLog siraLog,
                   PluginConfig config,
                   PlatformManager platformManager,
                   PlatformSpawner platformSpawner,
                   PlatformListsView platformListsView)
        {
            _siraLog = siraLog;
            _config = config;
            _platformManager = platformManager;
            _platformSpawner = platformSpawner;
            _platformListsView = platformListsView;
            _fileSystemWatcher = new(_config.CustomPlatformsDirectory, "*.plat");
        }

        private enum SubscriptionType
        {
            Subscribe,
            Unsubscribe
        }

        public void Initialize()
        {
            _fileSystemWatcher.Changed += OnFileChanged;
            _fileSystemWatcher.Created += OnFileCreated;
            _fileSystemWatcher.Deleted += OnFileDeleted;
            _fileSystemWatcher.EnableRaisingEvents = true;
            if (PluginManager.GetPlugin("SongCore") != null)
                SubscribeToSongCoreEvent(SubscriptionType.Subscribe);
            if (PluginManager.GetPlugin("Cinema") != null)
                SubscribeToCinemaEvent(SubscriptionType.Subscribe);
        }

        public void Dispose()
        {
            _fileSystemWatcher.Changed -= OnFileChanged;
            _fileSystemWatcher.Created -= OnFileCreated;
            _fileSystemWatcher.Deleted -= OnFileDeleted;
            _fileSystemWatcher.Dispose();
            if (PluginManager.GetPlugin("SongCore") != null)
                SubscribeToCinemaEvent(SubscriptionType.Unsubscribe);
            if (PluginManager.GetPlugin("Cinema") != null)
                SubscribeToCinemaEvent(SubscriptionType.Unsubscribe);
        }

        /// <summary>
        /// Replaces the old platform with the updated version
        /// </summary>
        private async void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (_platformManager.platformFilePaths.TryGetValue(e.FullPath, out CustomPlatform platform))
            {
                await _platformManager.allPlatformsTask;
                bool isActivePlatform = _platformManager.activePlatform == platform;
                await _platformManager.CreatePlatformAsync(e.FullPath);
                if (isActivePlatform)
                {
                    int index = _platformManager.GetIndexForType(PlatformType.Active);
                    await _platformSpawner.ChangeToPlatformAsync(index);
                }
            }
        }

        /// <summary>
        /// Create the new platform and add it to the UI
        /// </summary>
        private async void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            await _platformManager.allPlatformsTask;
            CustomPlatform newPlatform = await _platformManager.CreatePlatformAsync(e.FullPath);
            _platformManager.allPlatformsTask.Result.Add(newPlatform);
            _platformListsView.AddCellForPlatform(newPlatform, true);
            if (_platformManager.activePlatform == newPlatform)
            {
                int index = _platformManager.allPlatformsTask.Result.IndexOf(newPlatform);
                await _platformSpawner.ChangeToPlatformAsync(index);
            }
            if (apiRequest)
            {
                _platformManager.apiRequestedPlatform = newPlatform;
                apiRequest = false;
            }
        }

        /// <summary>
        /// Destroy the platform and remove all references
        /// </summary>
        private async void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            if (_platformManager.platformFilePaths.TryGetValue(e.FullPath, out CustomPlatform platform))
            {
                await _platformManager.allPlatformsTask;
                _platformListsView.RemoveCellForPlatformAsync(platform);
                if (_platformManager.activePlatform == platform)
                    await _platformSpawner.ChangeToPlatformAsync(0);
                _platformManager.platformFilePaths.Remove(platform.fullPath);
                _platformManager.allPlatformsTask.Result.Remove(platform);
                GameObject.Destroy(platform.gameObject);
            }
        }

        /// <summary>
        /// (Un-)Subscribes to SongCore's event, call this after checking if the plugin exists (optional dependency)
        /// </summary>
        private void SubscribeToSongCoreEvent(SubscriptionType subscriptionType)
        {
            if (subscriptionType == SubscriptionType.Subscribe)
                SongCore.Plugin.CustomSongPlatformSelectionDidChange += HandleSongCoreEvent;
            else
                SongCore.Plugin.CustomSongPlatformSelectionDidChange -= HandleSongCoreEvent;
        }

        /// <summary>
        /// (Un-)Subscribes to Cinema's event, call this after checking if the plugin exists (optional dependency)
        /// </summary>
        private void SubscribeToCinemaEvent(SubscriptionType subscriptionType)
        {
            if (subscriptionType == SubscriptionType.Subscribe)
                BeatSaberCinema.Events.AllowCustomPlatform += HandleCinemaEvent;
            else
                BeatSaberCinema.Events.AllowCustomPlatform -= HandleCinemaEvent;
        }

        /// <summary>
        /// Disable platform spawning as required by Cinema
        /// </summary>
        private async void HandleCinemaEvent(bool allowPlatform)
        {
            if (!allowPlatform)
            {
                await _platformManager.allPlatformsTask;
                _platformManager.apiRequestedPlatform = _platformManager.allPlatformsTask.Result[0];
            }
        }

        /// <summary>
        /// The class the API response of modelsaber is deserialized on
        /// </summary>
        [Serializable]
        public class PlatformDownloadData
        {
            public string name;
            public string download;
        }

        /// <summary>
        /// Asynchronously downloads a <see cref="CustomPlatform"/> from modelsaber if the selected level requires it
        /// </summary>
        /// <param name="usePlatform">Wether the selected song requests a platform or not</param>
        /// <param name="name">The name of the requested platform</param>
        /// <param name="hash">The hash of the requested platform</param>
        /// <param name="level">The song the platform was requested for</param>
        private async void HandleSongCoreEvent(bool usePlatform, string name, string hash, IPreviewBeatmapLevel level)
        {
            // No platform is requested, abort
            if (!usePlatform)
            {
                _platformManager.apiRequestedPlatform = null;
                _platformManager.apiRequestedLevelId = null;
                return;
            }

            _platformManager.apiRequestedLevelId = level.levelID;

            // Test if the requested platform is already downloaded
            foreach (CustomPlatform platform in await _platformManager.allPlatformsTask)
            {
                if (platform.platHash == hash || platform.platName.StartsWith(name))
                {
                    _platformManager.apiRequestedPlatform = platform;
                    return;
                }
            }

            if (hash != null)
            {
                PlatformDownloadData downloadData = await GetPlatformDownloadDataAsync("https://modelsaber.com/api/v2/get.php?type=platform&filter=hash:" + hash);
                if (downloadData != null)
                {
                    byte[] platData = await DownloadPlatformAsync(downloadData);
                    SavePlatformToFile(platData, downloadData.name);
                }
            }

            else if (name != null)
            {
                PlatformDownloadData downloadData = await GetPlatformDownloadDataAsync("https://modelsaber.com/api/v2/get.php?type=platform&filter=name:" + name);
                if (downloadData != null)
                {
                    byte[] platData = await DownloadPlatformAsync(downloadData);
                    SavePlatformToFile(platData, downloadData.name);
                }
            }
        }

        /// <summary>
        /// Asynchronously downloads the <see cref="PlatformDownloadData"/> from modelsaber
        /// </summary>
        /// <param name="uri">The platforms download link</param>
        private async Task<PlatformDownloadData> GetPlatformDownloadDataAsync(string uri)
        {
            TaskCompletionSource<PlatformDownloadData> taskSource = new();
            using UnityWebRequest www = UnityWebRequest.Get(uri);
            UnityWebRequestAsyncOperation webRequest = www.SendWebRequest();
            webRequest.completed += delegate
            {
                if (www.isNetworkError || www.isHttpError)
                {
                    _siraLog.Error("Error downloading a platform: \n" + www.error);
                    return;
                }
                Dictionary<string, PlatformDownloadData> downloadData = JsonConvert.DeserializeObject<Dictionary<string, PlatformDownloadData>>(www.downloadHandler.text);
                taskSource.TrySetResult(downloadData.FirstOrDefault().Value);
            };
            return await taskSource.Task;
        }

        /// <summary>
        /// Asynchronously downloads the .plat file from modelsaber and saves it in the CustomPlatforms directory
        /// </summary>
        /// <param name="data">The deserialized API response containing the download link to the .plat file</param>
        private async Task<byte[]> DownloadPlatformAsync(PlatformDownloadData data)
        {
            TaskCompletionSource<byte[]> taskSource = new();
            using UnityWebRequest www = UnityWebRequest.Get(data.download);
            UnityWebRequestAsyncOperation webRequest = www.SendWebRequest();
            webRequest.completed += delegate
            {
                if (www.isNetworkError || www.isHttpError)
                {
                    _siraLog.Error("Error downloading a platform: \n" + www.error);
                    return;
                }
                taskSource.TrySetResult(www.downloadHandler.data);
            };
            return await taskSource.Task;
        }

        /// <summary>
        /// Saves the downloaded <see cref="CustomPlatform"/>
        /// </summary>
        private void SavePlatformToFile(byte[] platformData, string name)
        {
            if (platformData != null)
            {
                apiRequest = true;
                string destination = Path.Combine(_config.CustomPlatformsDirectory, name + ".plat");
                File.WriteAllBytes(destination, platformData);
            }
        }
    }
}