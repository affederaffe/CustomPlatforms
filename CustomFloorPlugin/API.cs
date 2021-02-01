using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using IPA.Loader;

using Newtonsoft.Json;

using UnityEngine.Networking;

using Zenject;


namespace CustomFloorPlugin {


    /// <summary>
    /// A class that handles all interaction with outside plugins, atm just SongCore and Cinema
    /// </summary>
    internal class API : IInitializable {

        [Inject]
        private readonly PlatformLoader _platformLoader;

        [Inject]
        private readonly PlatformManager _platformManager;

        public void Initialize() {
            if (PluginManager.GetPlugin("SongCore") != null) {
                SubscribeToSongCoreEvent();
            }
            if (PluginManager.GetPlugin("Cinema") != null) {
                SubscribeToCinemaEvent();
            }
        }

        /// <summary>
        /// Subscribes to SongCore's event, calling it after checking if the plugin exists (optional dependency)
        /// </summary>
        private void SubscribeToSongCoreEvent() {
            SongCore.Plugin.CustomSongPlatformSelectionDidChange += (bool usePlatform, string name, string hash, IPreviewBeatmapLevel level) => SharedCoroutineStarter.instance.StartCoroutine(HandleSongSelected(usePlatform, name, hash, level));
        }

        /// <summary>
        /// Subscribes to Cinema's event, calling it after checking if the plugin exists (optional dependency)
        /// </summary>
        private void SubscribeToCinemaEvent() {
            BeatSaberCinema.Events.AllowCustomPlatform += (bool allowPlatform) => {
                if (!allowPlatform) _platformManager.apiRequestIndex = 0;
            };
        }

        /// <summary>
        /// The class the API response of modelsaber is deserialized on
        /// </summary>
        [Serializable]
        private class PlatformDownloadData {
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
        private IEnumerator<UnityWebRequestAsyncOperation> HandleSongSelected(bool usePlatform, string name, string hash, IPreviewBeatmapLevel level) {

            // No platform is requested, abort
            if (!usePlatform) {
                _platformManager.apiRequestIndex = -1;
                _platformManager.apiRequestedLevelId = null;
                yield break;
            }

            _platformManager.apiRequestedLevelId = level.levelID;

            // Test if the requested platform is already downloaded
            for (int i = 0; i < _platformManager.AllPlatforms.Count; i++) {
                if (_platformManager.AllPlatforms[i].platHash == hash || _platformManager.AllPlatforms[i].platName.StartsWith(name)) {
                    _platformManager.apiRequestIndex = i;
                    yield break;
                }
            }

            if (hash != null) {
                using UnityWebRequest www = UnityWebRequest.Get("https://modelsaber.com/api/v2/get.php?type=platform&filter=hash:" + hash);
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError) {
                    Utilities.Logging.Log("Error downloading a platform: \n" + www.error, IPA.Logging.Logger.Level.Error);
                }
                else {
                    Dictionary<string, PlatformDownloadData> downloadData = JsonConvert.DeserializeObject<Dictionary<string, PlatformDownloadData>>(www.downloadHandler.text);
                    PlatformDownloadData data = downloadData.FirstOrDefault().Value;
                    if (data != null) {
                        SharedCoroutineStarter.instance.StartCoroutine(DownloadSaveAndAddPlatform(data));
                    }
                }
            }

            else if (name != null) {
                using UnityWebRequest www = UnityWebRequest.Get("https://modelsaber.com/api/v2/get.php?type=platform&filter=name:" + name);
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError) {
                    Utilities.Logging.Log("Error downloading a platform: \n" + www.error, IPA.Logging.Logger.Level.Error);
                }
                else {
                    Dictionary<string, PlatformDownloadData> downloadData = JsonConvert.DeserializeObject<Dictionary<string, PlatformDownloadData>>(www.downloadHandler.text);
                    PlatformDownloadData data = downloadData.FirstOrDefault().Value;
                    if (data != null) {
                        SharedCoroutineStarter.instance.StartCoroutine(DownloadSaveAndAddPlatform(data));
                    }
                }
            }
        }

        /// <summary>
        /// Downloads the .plat file from modelsaber, then saves it in the CustomPlatforms directory and loads it
        /// </summary>
        /// <param name="data">The API deserialized API response containing the download link to the .plat file</param>
        /// <returns></returns>
        private IEnumerator<UnityWebRequestAsyncOperation> DownloadSaveAndAddPlatform(PlatformDownloadData data) {
            using UnityWebRequest www = UnityWebRequest.Get(data.download);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError) {
                Utilities.Logging.Log("Error downloading a platform: \n" + www.error, IPA.Logging.Logger.Level.Error);
            }
            else {
                string destination = Path.Combine(_platformLoader.customPlatformsFolderPath, data.name + ".plat");
                File.WriteAllBytes(destination, www.downloadHandler.data);
                CustomPlatform newPlatform = _platformLoader.LoadPlatformBundle(destination, _platformManager.transform);
                _platformManager.AllPlatforms.Add(newPlatform);
                _platformManager.apiRequestIndex = _platformManager.AllPlatforms.IndexOf(newPlatform);
            }
        }
    }
}
