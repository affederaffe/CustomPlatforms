using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

using CustomFloorPlugin.Configuration;

using Newtonsoft.Json;

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

using Zenject;


namespace CustomFloorPlugin {


    /// <summary>
    /// Handles Platforms, and hearts, everything about them
    /// </summary>
    public class PlatformManager : MonoBehaviour {

        [Inject]
        private readonly PluginConfig _config;

        [Inject]
        private readonly PlatformLoader _loader;

        /// <summary>
        /// List of all loaded Platforms, publicly editable. For no good reason.
        /// </summary>
        public List<CustomPlatform> AllPlatforms {
            get;
            private set;
        }

        /// <summary>
        /// The index of the currently selected <see cref="CustomPlatform"/> for singleplayer<br/>
        /// Returns 0 if the selection goes AWOL...
        /// </summary>
        public int CurrentSingleplayerPlatformIndex {
            get {
                int idx = -1;
                if (currentSingleplayerPlatform != null) {
                    idx = AllPlatforms.IndexOf(currentSingleplayerPlatform);
                }
                if (idx != -1) {
                    return idx;
                }
                else {
                    return 0;
                }
            }
        }

        /// <summary>
        /// The index of the currently selected <see cref="CustomPlatform"/> for multiplayer<br/>
        /// Returns 0 if the selection goes AWOL...
        /// </summary>
        public int CurrentMultiplayerPlatformIndex {
            get {
                int idx = -1;
                if (currentMultiplayerPlatform != null) {
                    idx = AllPlatforms.IndexOf(currentMultiplayerPlatform);
                }
                if (idx != -1) {
                    return idx;
                }
                else {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Stores the index of an API requested <see cref="CustomPlatform"/>
        /// </summary>
        public int apiRequestIndex = -1;

        /// <summary>
        /// Stores the BeatmapLevel the Platform was requested for
        /// </summary>
        public string apiRequestedLevelId;

        /// <summary>
        /// Keeps track of the currently selected <see cref="PlatformType"/>
        /// </summary>
        internal PlatformType currentPlatformType = PlatformType.Singleplayer;

        /// <summary>
        /// Keeps track of the currently selected <see cref="CustomPlatform"/>
        /// </summary>
        internal CustomPlatform currentSingleplayerPlatform;

        /// <summary>
        /// Keeps track of the currently selected <see cref="CustomPlatform"/>
        /// </summary>
        internal CustomPlatform currentMultiplayerPlatform;

        /// <summary>
        /// Keeps track of the currently active <see cref="CustomPlatform"/>
        /// </summary>
        internal CustomPlatform activePlatform;

        /// <summary>
        /// Acts as a prefab for custom light sources that require meshes...<br/>
        /// Not 100% bug free tbh<br/>
        /// <br/>
        /// Also:<br/>
        /// We love Beat Saber
        /// </summary>
        internal static GameObject Heart;

        /// <summary>
        /// Used as a platform in Platform Preview if <see cref="CustomPlatform.hideDefaultPlatform"/> is false
        /// </summary>
        internal static GameObject PlayersPlace;

        /// <summary>
        /// GameObject used when AlwaysShowFeet is true
        /// Reference set in the <see cref="EnvironmentHider"/>
        /// </summary>
        internal static GameObject Feet;

        /// <summary>
        /// The Light Source used for Non-Mesh Lights
        /// </summary>
        internal static GameObject LightSource;

        /// <summary>
        /// Used as a prefab for light effects in multiplayer
        /// </summary>
        internal static GameObject LightEffects;

        /// <summary>
        /// Keeps track of all spawned custom <see cref="GameObject"/>s, whichs lifetime ends on any scene transition
        /// </summary>
        internal static List<GameObject> SpawnedObjects = new List<GameObject>();

        /// <summary>
        /// Keeps track of all spawned custom <see cref="Component"/>s, whichs lifetime ends on any scene transition
        /// </summary>
        internal static List<Component> SpawnedComponents = new List<Component>();

        /// <summary>
        /// This <see cref="Action"/> <see langword="delegate"/> is called whenever a platform is activated,<br/>
        /// after all instantiated objects implementing <see cref="INotifyOnEnableOrDisable"/> have been notified.
        /// </summary>
        internal static Action<LightWithIdManager> SpawnQueue = delegate { };

        /// <summary>
        /// Initializes the <see cref="PlatformManager"/>
        /// </summary>
        public void Start() {
            StartCoroutine(IHateUnity());
            IEnumerator<WaitForEndOfFrame> IHateUnity() {
                yield return new WaitForEndOfFrame();
                LoadAssets();
                Reload();
                SongCore.Plugin.CustomSongPlatformSelectionDidChange += (bool usePlatform, string name, string hash, IPreviewBeatmapLevel level) => StartCoroutine(HandleSongSelected(usePlatform, name, hash, level));
            }
        }

        internal void Reload() {
            AllPlatforms = _loader.CreateAllPlatforms(transform);
            currentSingleplayerPlatform = AllPlatforms[0];
            currentMultiplayerPlatform = AllPlatforms[0];
            if (_config.SingleplayerPlatformPath != null) {
                for (int i = 0; i < AllPlatforms.Count; i++) {
                    if (_config.SingleplayerPlatformPath == AllPlatforms[i].platName + AllPlatforms[i].platAuthor) {
                        currentSingleplayerPlatform = AllPlatforms[i];
                        break;
                    }
                }
            }
            if (_config.MultiplayerPlatformPath != null) {
                for (int i = 0; i < AllPlatforms.Count; i++) {
                    if (_config.MultiplayerPlatformPath == AllPlatforms[i].platName + AllPlatforms[i].platAuthor) {
                        currentMultiplayerPlatform = AllPlatforms[i];
                        break;
                    }
                }
            }
        }

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

        private IEnumerator<UnityWebRequestAsyncOperation> HandleSongSelected(bool usePlatform, string name, string hash, IPreviewBeatmapLevel level) {

            // No platform is requested, abort
            if (!usePlatform) {
                apiRequestIndex = -1;
                apiRequestedLevelId = null;
                yield break;
            }

            apiRequestedLevelId = level.levelID;

            // Test if the requested platform is already downloaded
            for (int i = 0; i < AllPlatforms.Count; i++) {
                if (AllPlatforms[i].platHash == hash || AllPlatforms[i].platName.StartsWith(name)) {
                    apiRequestIndex = i;
                    yield break;
                }
            }

            if (hash != null) {
                using UnityWebRequest www = UnityWebRequest.Get("https://modelsaber.com/api/v1/platform/get.php?filter=hash:" + hash);
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError) {
                    Utilities.Logging.Log("Error downloading a platform: \n" + www.error, IPA.Logging.Logger.Level.Error);
                }
                else {
                    Dictionary<string, PlatformDownloadData> downloadData = JsonConvert.DeserializeObject<Dictionary<string, PlatformDownloadData>>(www.downloadHandler.text);
                    PlatformDownloadData data = downloadData.FirstOrDefault().Value;
                    if (data != null) {
                        StartCoroutine(DownloadSaveAndAddPlatform(data));
                    }
                }
            }

            else if (name != null) {
                using UnityWebRequest www = UnityWebRequest.Get("https://modelsaber.com/api/v1/platform/get.php?filter=name:" + name);
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError) {
                    Utilities.Logging.Log("Error downloading a platform: \n" + www.error, IPA.Logging.Logger.Level.Error);
                }
                else {
                    Dictionary<string, PlatformDownloadData> downloadData = JsonConvert.DeserializeObject<Dictionary<string, PlatformDownloadData>>(www.downloadHandler.text);
                    PlatformDownloadData data = downloadData.FirstOrDefault().Value;
                    if (data != null) {
                        StartCoroutine(DownloadSaveAndAddPlatform(data));
                    }
                }
            }
        }

        private IEnumerator<UnityWebRequestAsyncOperation> DownloadSaveAndAddPlatform(PlatformDownloadData data) {
            using UnityWebRequest www = UnityWebRequest.Get(data.download);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError) {
                Utilities.Logging.Log("Error downloading a platform: \n" + www.error, IPA.Logging.Logger.Level.Error);
            }
            else {
                string destination = Path.Combine(_loader.customPlatformsFolderPath, data.name + ".plat");
                File.WriteAllBytes(destination, www.downloadHandler.data);
                CustomPlatform newPlatform = _loader.LoadPlatformBundle(destination, transform);
                AllPlatforms.Add(newPlatform);
                apiRequestIndex = AllPlatforms.IndexOf(newPlatform);
            }
        }

        /// <summary>
        /// Steals the heart from the GreenDayScene<br/>
        /// Then De-Serializes the data from the embedded resource heart.mesh onto the GreenDayHeart to make it more visually pleasing<br></br>
        /// Also adjusts it position and color.<br></br>
        /// Gets the Non-Mesh LightSource and the PlayersPlace used in the Platform Preview too.<br></br>
        /// Now also steals the LightEffects for multiplayer, this scene is really useful
        /// </summary>
        private void LoadAssets() {
            Scene greenDay = SceneManager.LoadScene("GreenDayGrenadeEnvironment", new LoadSceneParameters(LoadSceneMode.Additive));
            StartCoroutine(fuckUnity());
            IEnumerator<WaitUntil> fuckUnity() {//did you know loaded scenes are loaded asynchronously, regarless if you use async or not?
                yield return new WaitUntil(() => { return greenDay.isLoaded; });
                GameObject root = greenDay.GetRootGameObjects()[0];

                Heart = root.transform.Find("GreenDayCity/armHeartLighting").gameObject;
                Heart.SetActive(false);
                Heart.transform.SetParent(transform);
                Heart.name = "<3";

                PlayersPlace = root.transform.Find("PlayersPlace").gameObject;
                PlayersPlace.SetActive(false);
                PlayersPlace.transform.SetParent(transform);

                LightSource = root.transform.Find("GlowLineL (2)").gameObject;
                LightSource.SetActive(false);
                LightSource.transform.SetParent(transform);
                LightSource.name = "LightSource";

                LightEffects = root.transform.Find("LightEffects").gameObject;
                LightEffects.SetActive(false);
                LightEffects.transform.SetParent(transform);

                SceneManager.UnloadSceneAsync("GreenDayGrenadeEnvironment");

                using Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CustomFloorPlugin.heart.mesh");
                using StreamReader streamReader = new StreamReader(manifestResourceStream);

                string meshfile = streamReader.ReadToEnd();
                string[] dimension1 = meshfile.Split('|');
                string[][] dimension2 = new string[][] { dimension1[0].Split('/'), dimension1[1].Split('/') };
                string[][] string_vector3s = new string[dimension2[0].Length][];

                int i = 0;
                foreach (string string_vector3 in dimension2[0]) {
                    string_vector3s[i++] = string_vector3.Split(',');
                }

                List<Vector3> vertices = new List<Vector3>();
                List<int> triangles = new List<int>();
                foreach (string[] string_vector3 in string_vector3s) {
                    vertices.Add(new Vector3(float.Parse(string_vector3[0], NumberFormatInfo.InvariantInfo), float.Parse(string_vector3[1], NumberFormatInfo.InvariantInfo), float.Parse(string_vector3[2], NumberFormatInfo.InvariantInfo)));
                }
                foreach (string s_int in dimension2[1]) {
                    triangles.Add(int.Parse(s_int, NumberFormatInfo.InvariantInfo));
                }

                Mesh mesh = new Mesh {
                    vertices = vertices.ToArray(),
                    triangles = triangles.ToArray()
                };
                Vector3 position = new Vector3(-8f, 25f, 26f);
                Quaternion rotation = Quaternion.Euler(-100f, 90f, 90f);
                Vector3 scale = new Vector3(25f, 25f, 25f);

                Heart.GetComponent<MeshFilter>().mesh = mesh;
                Heart.transform.position = position;
                Heart.transform.rotation = rotation;
                Heart.transform.localScale = scale;

                Heart.SetActive(_config.ShowHeart);
                Heart.GetComponent<InstancedMaterialLightWithId>().ColorWasSet(Color.magenta);
            }
        }
    }
}
