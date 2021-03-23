using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        private FileSystemWatcher _fileSystemWatcher;
        private bool apiRequest;

        private enum SubscriptionType
        {
            Subscribe,
            Unsubscribe
        }

        public void Initialize()
        {
            _fileSystemWatcher = new FileSystemWatcher(_config.CustomPlatformsDirectory, "*.plat");
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
            _fileSystemWatcher.Created -= OnFileCreated;
            _fileSystemWatcher.Deleted -= OnFileDeleted;
            _fileSystemWatcher.Dispose();
            if (PluginManager.GetPlugin("SongCore") != null)
                SubscribeToCinemaEvent(SubscriptionType.Unsubscribe);
            if (PluginManager.GetPlugin("Cinema") != null)
                SubscribeToCinemaEvent(SubscriptionType.Unsubscribe);
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            SharedCoroutineStarter.instance.StartCoroutine(_platformLoader.LoadFromFileAsync(e.FullPath, (CustomPlatform platform) =>
            {
                _platformManager.HandlePlatformLoaded(platform);
                CustomPlatform newPlatform = _platformLoader.platformFilePaths[platform.fullPath];
                _platformListsView.AddCellForPlatform(newPlatform, true);
                if (apiRequest)
                {
                    _platformManager.apiRequestedPlatform = newPlatform;
                    apiRequest = false;
                }
            }));
        }

        /// <summary>
        /// Destroy the platform and remove all references
        /// </summary>
        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
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
        /// Downloads a <see cref="CustomPlatform"/> from modelsaber if the selected level requires it
        /// </summary>
        /// <param name="usePlatform">Wether the selected song requests a platform or not</param>
        /// <param name="name">The name of the requested platform</param>
        /// <param name="hash">The hash of the requested platform</param>
        /// <param name="level">The song the platform was requested for</param>
        private void HandleSongCoreEvent(bool usePlatform, string name, string hash, IPreviewBeatmapLevel level)
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

            SharedCoroutineStarter.instance.StartCoroutine(SendWebRequest());
            IEnumerator<UnityWebRequestAsyncOperation> SendWebRequest()
            {
                if (hash != null)
                {
                    using UnityWebRequest www = UnityWebRequest.Get("https://modelsaber.com/api/v2/get.php?type=platform&filter=hash:" + hash);
                    yield return www.SendWebRequest();

                    if (www.isNetworkError || www.isHttpError)
                    {
                        _siraLog.Error("Error downloading a platform: \n" + www.error);
                    }
                    else
                    {
                        Dictionary<string, PlatformDownloadData> downloadData = JsonConvert.DeserializeObject<Dictionary<string, PlatformDownloadData>>(www.downloadHandler.text);
                        PlatformDownloadData data = downloadData.FirstOrDefault().Value;
                        if (data != null)
                        {
                            SharedCoroutineStarter.instance.StartCoroutine(DownloadSavePlatform(data));
                        }
                    }
                }

                else if (name != null)
                {
                    using UnityWebRequest www = UnityWebRequest.Get("https://modelsaber.com/api/v2/get.php?type=platform&filter=name:" + name);
                    yield return www.SendWebRequest();

                    if (www.isNetworkError || www.isHttpError)
                    {
                        _siraLog.Error("Error downloading a platform: \n" + www.error);
                    }
                    else
                    {
                        Dictionary<string, PlatformDownloadData> downloadData = JsonConvert.DeserializeObject<Dictionary<string, PlatformDownloadData>>(www.downloadHandler.text);
                        PlatformDownloadData data = downloadData.FirstOrDefault().Value;
                        if (data != null)
                        {
                            SharedCoroutineStarter.instance.StartCoroutine(DownloadSavePlatform(data));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Downloads the .plat file from modelsaber and saves it in the CustomPlatforms directory
        /// </summary>
        /// <param name="data">The API deserialized API response containing the download link to the .plat file</param>
        private IEnumerator<UnityWebRequestAsyncOperation> DownloadSavePlatform(PlatformDownloadData data)
        {
            using UnityWebRequest www = UnityWebRequest.Get(data.download);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
                _siraLog.Error("Error downloading a platform: \n" + www.error);
            else
            {
                apiRequest = true;
                string destination = Path.Combine(_config.CustomPlatformsDirectory, data.name + ".plat");
                File.WriteAllBytes(destination, www.downloadHandler.data);
            }
        }
    }
}
