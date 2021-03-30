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
        private readonly PlatformLoader _platformLoader;
        private readonly PlatformManager _platformManager;
        private readonly PlatformSpawner _platformSpawner;
        private readonly PlatformListsView _platformListsView;

        private FileSystemWatcher _fileSystemWatcher;
        private bool apiRequest;

        public API(SiraLog siraLog,
                   PluginConfig config,
                   PlatformLoader platformLoader,
                   PlatformManager platformManager,
                   PlatformSpawner platformSpawner,
                   PlatformListsView platformListsView)
        {
            _siraLog = siraLog;
            _config = config;
            _platformLoader = platformLoader;
            _platformManager = platformManager;
            _platformSpawner = platformSpawner;
            _platformListsView = platformListsView;
        }

        private enum SubscriptionType
        {
            Subscribe,
            Unsubscribe
        }

        public void Initialize()
        {
            _fileSystemWatcher = new FileSystemWatcher(_config.CustomPlatformsDirectory, "*.plat");
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
        /// Destroy the old platform and spawn the new one.
        /// </summary>
        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            SharedCoroutineStarter.instance.StartCoroutine(WaitAndChange());
            IEnumerator<WaitForEndOfFrame> WaitAndChange()
            {
                yield return new WaitForEndOfFrame();
                if (_platformLoader.platformFilePaths.TryGetValue(e.FullPath, out CustomPlatform platform))
                {
                    bool wasActivePlatform = _platformManager.activePlatform == platform;
                    _ = _platformLoader.LoadFromFileAsync(e.FullPath, (CustomPlatform platform) =>
                    {
                        _platformManager.HandlePlatformLoaded(platform);
                        if (wasActivePlatform)
                        {
                            int index =_platformManager.allPlatforms.IndexOf(_platformManager.activePlatform);
                            _platformSpawner.ChangeToPlatform(index);
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Create the new platform and add it to the UI
        /// </summary>
        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            SharedCoroutineStarter.instance.StartCoroutine(WaitAndCreate());
            IEnumerator<WaitForEndOfFrame> WaitAndCreate()
            {
                yield return new WaitForEndOfFrame();
                _ = _platformLoader.LoadFromFileAsync(e.FullPath, (CustomPlatform platform) =>
                {
                    _platformManager.HandlePlatformLoaded(platform);
                    CustomPlatform newPlatform = _platformLoader.platformFilePaths[platform.fullPath];
                    _platformListsView.AddCellForPlatform(newPlatform, true);
                    if (apiRequest)
                    {
                        _platformManager.apiRequestedPlatform = newPlatform;
                        apiRequest = false;
                    }
                });
            }
        }

        /// <summary>
        /// Destroy the platform and remove all references
        /// </summary>
        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            SharedCoroutineStarter.instance.StartCoroutine(WaitAndDelete());
            IEnumerator<WaitForEndOfFrame> WaitAndDelete()
            {
                yield return new WaitForEndOfFrame();
                if (_platformLoader.platformFilePaths.TryGetValue(e.FullPath, out CustomPlatform platform))
                {
                    _platformListsView.RemoveCellForPlatform(platform);
                    if (_platformManager.activePlatform == platform)
                        _platformSpawner.ChangeToPlatform(0);

                    _platformLoader.platformFilePaths.Remove(platform.fullPath);
                    _platformManager.allPlatforms.Remove(platform);
                    GameObject.Destroy(platform.gameObject);
                }
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
        private void HandleCinemaEvent(bool allowPlatform)
        {
            if (!allowPlatform)
                _platformManager.apiRequestedPlatform = _platformManager.allPlatforms[0];
        }

        /// <summary>
        /// The class the API response of modelsaber is deserialized on
        /// </summary>
        [Serializable]
        public class PlatformDownloadData
        {
            public string[] tags;
            public string type;
            public string name;
            public string author;
            public string image;
            public string hash;
            public string bsaber;
            public string download;
            public string install_link;
            public string date;
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
            foreach (CustomPlatform platform in _platformManager.allPlatforms)
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
        /// <param name="uri">The URI to the platform</param>
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