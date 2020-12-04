using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using CustomFloorPlugin.Exceptions;
using CustomFloorPlugin.UI;

using UnityEngine;
using UnityEngine.SceneManagement;

using Zenject;

using static CustomFloorPlugin.GlobalCollection;
using static CustomFloorPlugin.Utilities.BeatSaberSearching;
using static CustomFloorPlugin.Utilities.Logging;


namespace CustomFloorPlugin {


    /// <summary>
    /// Handles Platforms, and hearts, everything about them
    /// </summary>
    public static partial class PlatformManager {


        /// <summary>
        /// List of all loaded Platforms, publicly editable. For no good reason.
        /// </summary>
        public static List<CustomPlatform> AllPlatforms {
            get;
            private set;
        }


        /// <summary>
        /// The index of the currently selected <see cref="CustomPlatform"/><br/>
        /// Returns 0 if the selection goes AWOL...
        /// </summary>
        public static int CurrentPlatformIndex {
            get {
                int idx = -1;
                if (CurrentPlatform != null) {
                    idx = AllPlatforms.IndexOf(CurrentPlatform);
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
        /// Keeps track of the currently selected <see cref="CustomPlatform"/>
        /// </summary>
        public static CustomPlatform CurrentPlatform {
            get;
            private set;
        }


        /// <summary>
        /// The <see cref="GameObject"/> which the <see cref="PlatformManager"/> and it's submodules use as an in-world anchor
        /// </summary>
        private static GameObject Anchor {
            get {
                if (_Anchor == null) {
                    _Anchor = new GameObject("Platform Manager");
                    SceneManager.MoveGameObjectToScene(_Anchor, SCENE);
                }
                return _Anchor;
            }
        }
        private static GameObject _Anchor;


        /// <summary>
        /// Acts as a prefab for custom light sources that require meshes...<br/>
        /// Not 100% bug free tbh<br/>
        /// <br/>
        /// Also:<br/>
        /// We love Beat Saber
        /// </summary>
        internal static GameObject Heart;


        /// <summary>
        /// Real Prefab for lights, has to be Inactive to prevent NullReference spam
        /// </summary>
        internal static GameObject InactiveHeart {
            get {
                if (_InactiveHeart == null) {
                    bool active = Heart.activeSelf;
                    Heart.SetActive(false);
                    _InactiveHeart = GameObject.Instantiate(Heart);
                    Heart.SetActive(active);
                    Heart.GetComponent<InstancedMaterialLightWithId>().ColorWasSet(Color.magenta);
                }
                return _InactiveHeart;
            }
        }
        private static GameObject _InactiveHeart;


        /// <summary>
        /// Used as a platform in Platform Preview if <see cref="CustomPlatform.hideDefaultPlatform"/> is false.
        /// </summary>
        internal static GameObject PlayersPlace;


        /// <summary>
        /// The Light Source used since 1.13.0 broke the lasers in Menu.
        /// </summary>
        internal static GameObject LightSource;


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
        internal static Action<LightWithIdManager> SpawnQueue = delegate {
            Log("Spawning Lights");
        };


        /// <summary>
        /// Keeps track of the currently active <see cref="CustomPlatform"/>
        /// </summary>
        internal static CustomPlatform activePlatform;


        /// <summary>
        /// Initializes the <see cref="PlatformManager"/>
        /// </summary>
        internal static void Init() {
            Anchor.AddComponent<EasterEggs>();
            GSM.transitionDidStartEvent += (float ignored) => { TransitionPrep(); };
            GSM.transitionDidFinishEvent += (ScenesTransitionSetupDataSO ignored1, DiContainer ignored2) => { TransitionFinalize(); };
            Reload();
        }

        internal static void Reload() {
            PlatformLoader.LoadScripts();
            AllPlatforms = PlatformLoader.CreateAllPlatforms(Anchor.transform);
            CurrentPlatform = AllPlatforms[0];
            if (CONFIG.HasKey("Data", "CustomPlatformPath")) {
                string savedPath = CONFIG.GetString("Data", "CustomPlatformPath");
                for (int i = 0; i < AllPlatforms.Count; i++) {
                    if (savedPath == AllPlatforms[i].platName + AllPlatforms[i].platAuthor) {
                        CurrentPlatform = AllPlatforms[i];
                        break;
                    }
                }
            }

            LoadHeartAndLightSource();
            LoadDefaultPlatform();
        }

        /// <summary>
        /// Prepares for a scene transition, removes all custom elements
        /// </summary>
        private static void TransitionPrep() {
            try {
                Scene currentEvironment = GetCurrentEnvironment();
                if (!currentEvironment.name.StartsWith("Menu", STR_INV)) {
                    PlatformLifeCycleManagement.InternalChangeToPlatform(0);
                }
                else {
                    Heart.SetActive(false);
                }
            }
            catch (EnvironmentSceneNotFoundException) { }
        }


        /// <summary>
        /// Finishes up scene transitions, spawning the selected <see cref="CustomPlatform"/> if needed
        /// </summary>
        private static void TransitionFinalize() {
            try {
                Scene currentEvironment = GetCurrentEnvironment();
                if (!currentEvironment.name.StartsWith("Menu", STR_INV) && MultiplayerCheck() && D360Check()  && currentEvironment.name != "TutorialEnvironment") { //Excluding TutorialEnvironment for Counters+ to work properly
                    try {
                        Settings.UpdatePlayerData();
                        if (EnvironmentSceneOverrider.didOverrideEnvironment || (PlatformsListView.EnvOr == EnvOverrideMode.Song && !Settings.PlayerData.overrideEnvironmentSettings.overrideEnvironments) || currentEvironment.name.StartsWith("Multiplayer", STR_INV)) {
                            if (!platformSpawned) {
                                PlatformLifeCycleManagement.InternalChangeToPlatform();
                                EnvironmentSceneOverrider.Revert();
                            }
                        }
                    }
                    catch (ManagerNotFoundException e) {
                        Log(e);
                    }
                }
                else if (currentEvironment.name == "TutorialEnvironment") {
                    Heart.SetActive(false);
                }
                else {
                    platformSpawned = false;
                    Heart.SetActive(Settings.ShowHeart);
                    Heart.GetComponent<LightWithIdMonoBehaviour>().ColorWasSet(Color.magenta);
                }
            }
            catch (EnvironmentSceneNotFoundException) { }
        }


        /// <summary>
        /// Changes to a specific <see cref="CustomPlatform"/> and saves the choice
        /// </summary>
        /// <param name="index">
        /// ">The index of the new platform in <see cref="AllPlatforms"/></param>
        internal static void SetPlatformAndShow(int index) {
            CurrentPlatform = AllPlatforms[index % AllPlatforms.Count];
            CONFIG.SetString("Data", "CustomPlatformPath", CurrentPlatform.platName + CurrentPlatform.platAuthor);
            PlatformLifeCycleManagement.InternalChangeToPlatform(index);
        }


        /// <summary>
        /// Stores a platform ID that was requested through the public API
        /// </summary>
        private static int? kyleBuffer = null;


        /// <summary>
        /// Stores an overflown platform ID if <see cref="kyleBuffer"/> already stores an ID.
        /// </summary>
        private static int? errBuffer = null;


        /// <summary>
        /// This flag indicates whether or not a platform has been spawned already.<br/>
        /// It exists because I have no control over the order in which event callbacks are executed
        /// </summary>
        private static bool platformSpawned;


        /// <summary>
        /// This function handles outside requests to temporarily change to a specific platform.<br/>
        /// It caches the request and will consume it when a level is played.<br/>
        /// This can be called both before, and after loading.<br/>
        /// Has to be called BEFORE the level is loaded in order for the right vanilla environment to load.<br/>
        /// It does not require you to reset the platform back to the last known state after the level.
        /// </summary>
        /// <param name="index">Index of the desired platform</param>
        /// <exception cref="StackedRequestsException"></exception>
        public static void TempChangeToPlatform(int index) {
            if (kyleBuffer != null) {
                errBuffer = index;
                throw new StackedRequestsException();
            }
            else {
                kyleBuffer = index;
            }
            try {
                if (!GetCurrentEnvironment().name.StartsWith("Menu", STR_INV) && platformSpawned) {
                    PlatformLifeCycleManagement.InternalChangeToPlatform();
                }
            }
            catch (EnvironmentSceneNotFoundException e) {
                IPA.Logging.Logger.Level L = IPA.Logging.Logger.Level.Warning;
                Log("TempChangeToPlatform was called out of place. Please send me a bug report.", L);
                Log(e, L);
            }
        }


        /// <summary>
        /// Allows dynamic loading of <see cref="CustomPlatform"/>s from files.
        /// </summary>
        /// <param name="path">The path of file containing the <see cref="CustomPlatform"/></param>
        /// <returns>The reference to the loaded <see cref="CustomPlatform"/></returns>
        public static CustomPlatform AddPlatform(string path) {
            CustomPlatform newPlatform = PlatformLoader.LoadPlatformBundle(path, Anchor.transform);
            if (newPlatform != null) {
                AllPlatforms.Add(newPlatform);
            }
            return newPlatform;
        }


        /// <summary>
        /// Changes to a platform in <see cref="AllPlatforms"/>, based on <paramref name="index"/><br/>
        /// Leave <paramref name="index"/> at <see langword="null"/> to change to the currently selected platform instead<br/>
        /// Acts as an interface between <see cref="PlatformLifeCycleManagement"/> and the rest of the plugin
        /// </summary>
        /// <param name="index">The index of the <see cref="CustomPlatform"/> in the list <see cref="AllPlatforms"/></param>
        internal static void ChangeToPlatform(int? index = null) {
            if (index == null) {
                PlatformLifeCycleManagement.InternalChangeToPlatform();
            }
            else {
                PlatformLifeCycleManagement.InternalChangeToPlatform(index.Value);
            }
        }


        /// <summary>
        /// Overrides the previous request, if there was one<br/>
        /// May trigger a platform change if called in-song (Primarily to catch late scene change callbacks)
        /// </summary>
        internal static void OverridePreviousRequest() {
            if (errBuffer != null) {
                kyleBuffer = errBuffer;
                errBuffer = null;
                try {
                    if (!GetCurrentEnvironment().name.StartsWith("Menu", STR_INV)) {
                        PlatformLifeCycleManagement.InternalChangeToPlatform();
                    }
                }
                catch (EnvironmentSceneNotFoundException e) {
                    IPA.Logging.Logger.Level L = IPA.Logging.Logger.Level.Warning;
                    Log("OverridePreviousRequest was called out of place. Please send me a bug report.", L);
                    Log(e, L);
                }
            }
        }


        /// <summary>
        /// Steals the heart from the GreenDayScene<br/>
        /// Then De-Serializes the data from the embedded resource heart.mesh onto the GreenDayHeart to make it more visually pleasing<br/>
        /// Also adjusts it position and color.
        /// Now Loads a Light Prefab since the one in the Menu is fucked.
        /// </summary>
        private static void LoadHeartAndLightSource() {
            Scene greenDay = SceneManager.LoadScene("GreenDayGrenadeEnvironment", new LoadSceneParameters(LoadSceneMode.Additive));
            SharedCoroutineStarter.instance.StartCoroutine(fuckUnity());
            IEnumerator<WaitUntil> fuckUnity() {//did you know loaded scenes are loaded asynchronously, regarless if you use async or not?
                yield return new WaitUntil(() => { return greenDay.isLoaded; });
                GameObject root = greenDay.GetRootGameObjects()[0];
                Heart = root.transform.Find("GreenDayCity/armHeartLighting").gameObject;
                Heart.SetActive(false);
                Heart.transform.SetParent(null);
                Heart.name = "<3";
                SceneManager.MoveGameObjectToScene(Heart, SCENE);

                LightSource = root.transform.Find("GlowLineL (2)").gameObject;
                LightSource.SetActive(false);
                LightSource.transform.SetParent(null);
                LightSource.name = "LightSource";
                SceneManager.MoveGameObjectToScene(LightSource, SCENE);
                SceneManager.UnloadSceneAsync("GreenDayGrenadeEnvironment");

                Settings.ShowHeartChanged += Heart.SetActive;

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
                    vertices.Add(new Vector3(float.Parse(string_vector3[0], NUM_INV), float.Parse(string_vector3[1], NUM_INV), float.Parse(string_vector3[2], NUM_INV)));
                }
                foreach (string s_int in dimension2[1]) {
                    triangles.Add(int.Parse(s_int, NUM_INV));
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

                LightWithIdManager manager = FindLightWithIdManager(GetCurrentEnvironment());
                InstancedMaterialLightWithId lightWithId = Heart.GetComponent<InstancedMaterialLightWithId>();
                typeof(LightWithIdMonoBehaviour).GetField("_lightManager", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(lightWithId, manager);

                Heart.SetActive(Settings.ShowHeart);
                Heart.GetComponent<InstancedMaterialLightWithId>().ColorWasSet(Color.magenta);
            }
        }


        /// <summary>
        /// Steals the default Platform from the Default Environment to display it in Platform Preview.
        /// </summary>
        private static void LoadDefaultPlatform() {
            Scene env = SceneManager.LoadScene("DefaultEnvironment", new LoadSceneParameters(LoadSceneMode.Additive));
            SharedCoroutineStarter.instance.StartCoroutine(fuckUnity());
            IEnumerator<WaitUntil> fuckUnity() {//did you know loaded scenes are loaded asynchronously, regarless if you use async or not?
                yield return new WaitUntil(() => { return env.isLoaded; });
                GameObject root = env.GetRootGameObjects()[0];
                PlayersPlace = root.transform.Find("PlayersPlace").gameObject;
                PlayersPlace.SetActive(false);
                PlayersPlace.transform.SetParent(null);
                SceneManager.MoveGameObjectToScene(PlayersPlace, SCENE);
                SceneManager.UnloadSceneAsync("DefaultEnvironment");
            }
        }

        private static bool MultiplayerCheck() {
            Scene currentEvironment = GetCurrentEnvironment();
            if (currentEvironment.name.StartsWith("Multiplayer", STR_INV)) {
                return Settings.UseInMultiplayer;
            }
            else {
                return true;
            }
        }

        private static bool D360Check() {
            string[] d360Environments = {
                "GlassDesertEnvironment"
            };
            Scene currentEnvironment = GetCurrentEnvironment();
            foreach (string environmentName in d360Environments) {
                if (currentEnvironment.name == environmentName) {
                    return Settings.UseIn360;
                }
            }
            return true;
        }
    }
}