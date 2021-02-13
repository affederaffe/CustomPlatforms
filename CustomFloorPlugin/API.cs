using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using BeatSaberMarkupLanguage.Components;

using CustomFloorPlugin.UI;

using IPA.Loader;

using Newtonsoft.Json;

using UnityEngine;
using UnityEngine.Networking;

using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// A class that handles all interaction with outside plugins, atm just SongCore and Cinema
    /// Also detects changes in the platforms directory and loads added platforms automaticly
    /// </summary>
    internal class API : IInitializable, IDisposable
    {
        [Inject]
        private readonly PlatformLoader _platformLoader;

        [Inject]
        private readonly PlatformManager _platformManager;

        [Inject]
        private readonly PlatformSpawnerMenu _platformSpawner;

        [Inject]
        private readonly PlatformListsView _platformListsView;

        private FileSystemWatcher _fileSystemWatcher;

        private bool apiRequest;

        public void Initialize()
        {
            _fileSystemWatcher = new FileSystemWatcher(_platformLoader.customPlatformsFolderPath, "*.plat");
            _fileSystemWatcher.Created += (object sender, FileSystemEventArgs e) => SharedCoroutineStarter.instance.StartCoroutine(OnFileCreated(sender, e));
            _fileSystemWatcher.Deleted += (object sender, FileSystemEventArgs e) => SharedCoroutineStarter.instance.StartCoroutine(OnFileDeleted(sender, e));
            _fileSystemWatcher.EnableRaisingEvents = true;
            if (PluginManager.GetPlugin("SongCore") != null)
            {
                SubscribeToSongCoreEvent();
            }
            if (PluginManager.GetPlugin("Cinema") != null)
            {
                SubscribeToCinemaEvent();
            }
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
            CustomPlatform newPlatform = _platformLoader.GetPlatformInfo(e.FullPath);
            _platformLoader.customPlatformPaths.Add(newPlatform, e.FullPath);
            _platformManager.allPlatforms.Add(newPlatform);

            if (_platformListsView.allListTables != null)
            {
                CustomListTableData.CustomCellInfo cell = new CustomListTableData.CustomCellInfo(newPlatform.platName, newPlatform.platAuthor, newPlatform.icon);
                foreach (CustomListTableData listTable in _platformListsView.allListTables)
                {
                    listTable.data.Add(cell);
                    listTable.tableView.ReloadData();
                }
            }

            if (apiRequest)
            {
                _platformManager.apiRequestIndex = _platformManager.allPlatforms.IndexOf(newPlatform);
                apiRequest = false;
            }
        }

        /// <summary>
        /// Destroy the platform and remove all references
        /// </summary>
        private IEnumerator<WaitForEndOfFrame> OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            yield return new WaitForEndOfFrame();
            if (_platformLoader.customPlatformPaths.ContainsValue(e.FullPath))
            {
                KeyValuePair<CustomPlatform, string> deletedPlatformPair = _platformLoader.customPlatformPaths.First(x => x.Value == e.FullPath);
                _platformLoader.customPlatformPaths.Remove(deletedPlatformPair.Key);
                _platformManager.allPlatforms.Remove(deletedPlatformPair.Key);
                if (_platformListsView.allListTables != null)
                {
                    foreach (CustomListTableData listTable in _platformListsView.allListTables)
                    {
                        CustomListTableData.CustomCellInfo deletedPlatformCell = listTable.data.Find(x => x.text == deletedPlatformPair.Key.platName && x.subtext == deletedPlatformPair.Key.platAuthor);
                        listTable.data.Remove(deletedPlatformCell);
                        listTable.tableView.ReloadData();
                    }
                }
                if (_platformManager.currentSingleplayerPlatform == deletedPlatformPair.Key)
                {
                    _platformManager.currentSingleplayerPlatform = _platformManager.allPlatforms[0];
                    if (_platformListsView.singleplayerPlatformListTable != null)
                    {
                        _platformListsView.singleplayerPlatformListTable.tableView.SelectCellWithIdx(0);
                        _platformListsView.singleplayerPlatformListTable.tableView.ScrollToCellWithIdx(0, HMUI.TableViewScroller.ScrollPositionType.End, true);
                        if (_platformManager.currentPlatformType == PlatformType.Singleplayer) _platformSpawner.ChangeToPlatform(0);
                    }
                }
                if (_platformManager.currentMultiplayerPlatform == deletedPlatformPair.Key)
                {
                    _platformManager.currentMultiplayerPlatform = _platformManager.allPlatforms[0];
                    if (_platformListsView.multiplayerPlatformListTable != null)
                    {
                        _platformListsView.multiplayerPlatformListTable.tableView.SelectCellWithIdx(0);
                        _platformListsView.multiplayerPlatformListTable.tableView.ScrollToCellWithIdx(0, HMUI.TableViewScroller.ScrollPositionType.End, true);
                        if (_platformManager.currentPlatformType == PlatformType.Multiplayer) _platformSpawner.ChangeToPlatform(0);
                    }
                }
                if (_platformManager.currentA360Platform == deletedPlatformPair.Key)
                {
                    _platformManager.currentA360Platform = _platformManager.allPlatforms[0];
                    if (_platformListsView.a360PlatformListTable != null)
                    {
                        _platformListsView.a360PlatformListTable.tableView.SelectCellWithIdx(0);
                        _platformListsView.a360PlatformListTable.tableView.ScrollToCellWithIdx(0, HMUI.TableViewScroller.ScrollPositionType.End, true);
                        if (_platformManager.currentPlatformType == PlatformType.A360) _platformSpawner.ChangeToPlatform(0);
                    }
                }
                GameObject.Destroy(deletedPlatformPair.Key.gameObject);
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
                {
                    Utilities.Logging.Log("Error downloading a platform: \n" + www.error, IPA.Logging.Logger.Level.Error);
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
                    Utilities.Logging.Log("Error downloading a platform: \n" + www.error, IPA.Logging.Logger.Level.Error);
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
            {
                Utilities.Logging.Log("Error downloading a platform: \n" + www.error, IPA.Logging.Logger.Level.Error);
            }
            else
            {
                string destination = Path.Combine(_platformLoader.customPlatformsFolderPath, data.name + ".plat");
                File.WriteAllBytes(destination, www.downloadHandler.data);
                apiRequest = true;
            }
        }
    }
}
