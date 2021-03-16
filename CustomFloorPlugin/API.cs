using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using BeatSaberMarkupLanguage.Components;

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
        private readonly AssetLoader _assetLoader;
        private readonly PlatformLoader _platformLoader;
        private readonly PlatformManager _platformManager;
        private readonly PlatformSpawner _platformSpawner;
        private readonly PlatformListsView _platformListsView;

        public API(SiraLog siraLog,
                   AssetLoader assetLoader,
                   PlatformLoader platformLoader,
                   PlatformManager platformManager,
                   PlatformSpawner platformSpawner,
                   PlatformListsView platformListsView)
        {
            _siraLog = siraLog;
            _assetLoader = assetLoader;
            _platformLoader = platformLoader;
            _platformManager = platformManager;
            _platformSpawner = platformSpawner;
            _platformListsView = platformListsView;
        }

        private FileSystemWatcher _fileSystemWatcher;

        private bool apiRequest;

        public void Initialize()
        {
            _fileSystemWatcher = new FileSystemWatcher(_platformManager.customPlatformsFolderPath, "*.plat");
            _fileSystemWatcher.Created += (object sender, FileSystemEventArgs e) => SharedCoroutineStarter.instance.StartCoroutine(OnFileCreated(sender, e));
            _fileSystemWatcher.Deleted += (object sender, FileSystemEventArgs e) => SharedCoroutineStarter.instance.StartCoroutine(OnFileDeleted(sender, e));
            _fileSystemWatcher.EnableRaisingEvents = true;
            if (PluginManager.GetPlugin("SongCore") != null)
                SubscribeToSongCoreEvent();
            if (PluginManager.GetPlugin("Cinema") != null)
                SubscribeToCinemaEvent();
        }

        public void Dispose()
        {
            _fileSystemWatcher.Created -= (object sender, FileSystemEventArgs e) => SharedCoroutineStarter.instance.StartCoroutine(OnFileCreated(sender, e));
            _fileSystemWatcher.Deleted -= (object sender, FileSystemEventArgs e) => SharedCoroutineStarter.instance.StartCoroutine(OnFileDeleted(sender, e));
            _fileSystemWatcher.Dispose();
        }

        private IEnumerator<WaitForEndOfFrame> OnFileCreated(object sender, FileSystemEventArgs e)
        {
            yield return new WaitForEndOfFrame();
            SharedCoroutineStarter.instance.StartCoroutine(_platformLoader.LoadFromFileAsync(e.FullPath, (CustomPlatform platform, string path) =>
            {
                _platformManager.HandlePlatformLoaded(platform, path);

                if (_platformListsView.allListTables != null)
                {
                    if (platform.icon == null)
                        platform.icon = _assetLoader.fallbackCover;
                    CustomListTableData.CustomCellInfo cell = new(platform.platName, platform.platAuthor, platform.icon);
                    foreach (CustomListTableData listTable in _platformListsView.allListTables)
                    {
                        listTable.data.Add(cell);
                        listTable.tableView.ReloadData();
                    }
                }

                if (apiRequest)
                {
                    _platformManager.apiRequestIndex = _platformManager.allPlatforms.IndexOf(platform);
                    apiRequest = false;
                }
            }));
        }

        /// <summary>
        /// Destroy the platform and remove all references
        /// </summary>
        private IEnumerator<WaitForEndOfFrame> OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            yield return new WaitForEndOfFrame();
            if (_platformLoader.platformFilePaths.TryGetValue(e.FullPath, out CustomPlatform platform))
            {
                if (_platformListsView.allListTables != null)
                {
                    foreach (CustomListTableData listTable in _platformListsView.allListTables)
                    {
                        CustomListTableData.CustomCellInfo deletedPlatformCell = listTable.data.Find(x => x.text == platform.platName && x.subtext == platform.platAuthor);
                        listTable.data.Remove(deletedPlatformCell);
                        listTable.tableView.ReloadData();
                    }
                    bool platformDidChange = false;
                    if (_platformManager.currentSingleplayerPlatform == platform)
                    {
                        _platformListsView.singleplayerPlatformListTable.tableView.SelectCellWithIdx(0);
                        platformDidChange = true;
                    }

                    if (_platformManager.currentMultiplayerPlatform == platform)
                    {
                        _platformListsView.multiplayerPlatformListTable.tableView.SelectCellWithIdx(0);
                        platformDidChange = true;
                    }

                    if (_platformManager.currentA360Platform == platform)
                    {
                        _platformListsView.a360PlatformListTable.tableView.SelectCellWithIdx(0);
                        platformDidChange = true;
                    }
                    if (platformDidChange)
                        _platformSpawner.ChangeToPlatform(0);

                }
                else
                {
                    bool platformDidChange = false;
                    if (_platformManager.currentSingleplayerPlatform == platform)
                    {
                        _platformManager.currentSingleplayerPlatform = _platformManager.allPlatforms[0];
                        platformDidChange = true;
                    }
                    if (_platformManager.currentMultiplayerPlatform == platform)
                    {
                        _platformManager.currentMultiplayerPlatform = _platformManager.allPlatforms[0];
                        platformDidChange = true;
                    }
                    if (_platformManager.currentA360Platform == platform)
                    {
                        _platformManager.currentA360Platform = _platformManager.allPlatforms[0];
                    }
                    if (platformDidChange)
                        _platformSpawner.ChangeToPlatform(0);
                }

                _platformLoader.platformFilePaths.Remove(platform.fullPath);
                _platformManager.allPlatforms.Remove(platform);
                GameObject.Destroy(platform.gameObject);
            }
        }

        /// <summary>
        /// Subscribes to SongCore's event, calling it after checking if the plugin exists (optional dependency)
        /// </summary>
        private void SubscribeToSongCoreEvent()
        {
            SongCore.Plugin.CustomSongPlatformSelectionDidChange += (bool usePlatform, string name, string hash, IPreviewBeatmapLevel level) => SharedCoroutineStarter.instance.StartCoroutine(HandleSongSelected(usePlatform, name, hash, level));
        }

        /// <summary>
        /// Subscribes to Cinema's event, calling it after checking if the plugin exists (optional dependency)
        /// </summary>
        private void SubscribeToCinemaEvent()
        {
            BeatSaberCinema.Events.AllowCustomPlatform += (bool allowPlatform) =>
            {
                if (!allowPlatform) _platformManager.apiRequestIndex = 0;
            };
        }

        /// <summary>
        /// The class the API response of modelsaber is deserialized on
        /// </summary>
        [Serializable]
        private class PlatformDownloadData
        {
            public string[] tags = Array.Empty<string>();
            public string type = string.Empty;
            public string name = string.Empty;
            public string author = string.Empty;
            public string image = string.Empty;
            public string hash = string.Empty;
            public string bsaber = string.Empty;
            public string download = string.Empty;
            public string install_link = string.Empty;
            public string date = string.Empty;
        }

        /// <summary>
        /// Handles when a song is selected, downloading a <see cref="CustomPlatform"/> from modelsaber if needed
        /// </summary>
        /// <param name="usePlatform">Wether the selected song requests a platform or not</param>
        /// <param name="name">The name of the requested platform</param>
        /// <param name="hash">The hash of the requested platform</param>
        /// <param name="level">The song the platform was requested for</param>
        /// <returns></returns>
        private IEnumerator<UnityWebRequestAsyncOperation> HandleSongSelected(bool usePlatform, string name, string hash, IPreviewBeatmapLevel level)
        {
            // No platform is requested, abort
            if (!usePlatform)
            {
                _platformManager.apiRequestIndex = -1;
                _platformManager.apiRequestedLevelId = null;
                yield break;
            }

            _platformManager.apiRequestedLevelId = level.levelID;

            // Test if the requested platform is already downloaded
            for (int i = 0; i < _platformManager.allPlatforms.Count; i++)
            {
                if (_platformManager.allPlatforms[i].platHash == hash || _platformManager.allPlatforms[i].platName.StartsWith(name))
                {
                    _platformManager.apiRequestIndex = i;
                    yield break;
                }
            }

            if (hash != null)
            {
                using UnityWebRequest www = UnityWebRequest.Get("https://modelsaber.com/api/v2/get.php?type=platform&filter=hash:" + hash);
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                    _siraLog.Error("Error downloading a platform: \n" + www.error);
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
                    _siraLog.Info("Error downloading a platform: \n" + www.error);
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

        /// <summary>
        /// Downloads the .plat file from modelsaber, then saves it in the CustomPlatforms directory and loads it
        /// </summary>
        /// <param name="data">The API deserialized API response containing the download link to the .plat file</param>
        /// <returns></returns>
        private IEnumerator<UnityWebRequestAsyncOperation> DownloadSavePlatform(PlatformDownloadData data)
        {
            using UnityWebRequest www = UnityWebRequest.Get(data.download);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
                _siraLog.Info("Error downloading a platform: \n" + www.error);
            else
            {
                string destination = Path.Combine(_platformManager.customPlatformsFolderPath, data.name + ".plat");
                File.WriteAllBytes(destination, www.downloadHandler.data);
                apiRequest = true;
            }
        }
    }
}
