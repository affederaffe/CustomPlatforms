using CustomFloorPlugin.Exceptions;
using CustomFloorPlugin.Extensions;
using CustomFloorPlugin.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using static CustomFloorPlugin.Utilities.Logging;
using static CustomFloorPlugin.Utilities.BeatSaberSearching;

namespace CustomFloorPlugin {

    public sealed partial class PlatformManager:MonoBehaviour {
        public static List<CustomPlatform> AllPlatforms {
            get;
            private set;
        }
        public static int CurrentPlatformIndex {
            get {
                return AllPlatforms.IndexOf(CurrentPlatform);
            }
        }
        public static CustomPlatform CurrentPlatform {
            get;
            private set;
        }
        internal static PlatformManager Instance {
            get {
                if(_Instance == null) {
                    _Instance = new GameObject("Platform Manager").AddComponent<PlatformManager>();
                    SceneManager.MoveGameObjectToScene(_Instance.gameObject, PlatformManagerScene);
                }
                return _Instance;
            }
        }
        private static PlatformManager _Instance;
        internal static Scene PlatformManagerScene {
            get {
                if(_PlatformManagerScene == null) {
                    _PlatformManagerScene = SceneManager.CreateScene("PlatformManagerDump", new CreateSceneParameters(LocalPhysicsMode.None));
                }
                return _PlatformManagerScene.Value;
            }
        }
        private static Scene? _PlatformManagerScene;

        internal static GameObject Heart;
        internal static List<GameObject> SpawnedObjects = new List<GameObject>();
        internal static List<Component> SpawnedComponents = new List<Component>();
        internal static Action<LightWithIdManager> SpawnQueue = delegate {
            Log("Spawning Lights");
        };
        internal static CustomPlatform activePlatform;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void Update() {
            if(Input.GetKeyDown(KeyCode.Keypad0)) {
                Log();
                Heart.SetActive(false);
                Heart.GetComponent<LightWithId>().SetPrivateField("_lightManager", FindLightWithIdManager());
                Heart.SetActive(true);
            }
        }
        /// <summary>
        /// Executing this line causes the Platform Manager Instance to be loaded. No really. It does.
        /// </summary>
        internal void Load() {

            EnvironmentSceneOverrider.Load();

            Plugin.Gsm.transitionDidStartEvent += TransitionPrep;
            Plugin.Gsm.transitionDidFinishEvent += TransitionFinalize;

            //Create platforms list
            AllPlatforms = PlatformLoader.CreateAllPlatforms(Instance.transform);
            CurrentPlatform = AllPlatforms[0];
            // Retrieve saved path from player prefs if it exists
            if(Plugin.config.HasKey("Data", "CustomPlatformPath")) {
                string savedPath = Plugin.config.GetString("Data", "CustomPlatformPath");
                // Check if this path was loaded and update our platform index
                for(int i = 0; i < AllPlatforms.Count; i++) {
                    if(savedPath == AllPlatforms[i].platName + AllPlatforms[i].platAuthor) {
                        CurrentPlatform = AllPlatforms[i];
                        break;
                    }
                }
            }

            LoadHeart();
            
        }
        private void TransitionPrep(float ignored) {
            Log("Transition Prep");
            try {
                if(!GetCurrentEnvironment().name.StartsWith("Menu", Constants.StrInv)) {
                    PlatformLifeCycleManagement.InternalChangeToPlatform(0);
                } else {
                    Heart.SetActive(false);
                }
            } catch(EnvironmentSceneNotFoundException e) {
                Log(e);
            }
            Log("Transition Prep finished");
        }
        private void TransitionFinalize(ScenesTransitionSetupDataSO ignored1, DiContainer ignored2) {
            Log("Transition Finalize");
            try {
                if(!GetCurrentEnvironment().name.StartsWith("Menu", Constants.StrInv)) {
                    try {
                        if(!Settings.PlayerData.overrideEnvironmentSettings.overrideEnvironments) {
                            TubeLightUtilities.CreateAdditionalLightSwitchControllers(FindLightWithIdManager());
                            if(!platformSpawned) {
                                PlatformLifeCycleManagement.InternalChangeToPlatform();
                            }
                        }
                    } catch(ManagerNotFoundException e) {
                        Log(e);
                    }
                } else {
                    platformSpawned = false;
                    Heart.SetActive(Settings.ShowHeart);
                    Heart.GetComponent<LightWithId>().ColorWasSet(Color.magenta);
                }
            } catch(EnvironmentSceneNotFoundException e) {
                Log(e);
            }
            Log("Transition Finalize finished");
        }
        internal static IEnumerator<WaitForEndOfFrame> HideForPlatformAfterOneFrame(CustomPlatform customPlatform) {
            yield return new WaitForEndOfFrame();
            EnvironmentHider.HideObjectsForPlatform(customPlatform);
        }

        internal static void SetPlatformAndShow(int index) {

            // Increment index
            // To the uninitiated: The modulo operator makes sure that the values wraps around instead of throwing an exception... i need to change that later :>
            CurrentPlatform = AllPlatforms[index % AllPlatforms.Count];

            // Save path into Modprefs
            // Edit: Save path into YOUR OWN GOD DAMN SETTINGS FILE!!!
            Plugin.config.SetString("Data", "CustomPlatformPath", CurrentPlatform.platName + CurrentPlatform.platAuthor);

            //Show the platform
            PlatformLifeCycleManagement.InternalChangeToPlatform(index);

        }
        /// <summary>
        /// Stores a requested platform ID
        /// </summary>
        private static int? kyleBuffer = null;
        /// <summary>
        /// Stores an overflown platform ID if <see cref="kyleBuffer"/> already stores an ID.
        /// </summary>
        private static int? errBuffer = null;
        /// <summary>
        /// This variable indicates whether or not a platform has been spawned since leaving the menu.<br/>
        /// It exists because I have no control over the order in which callbacks are executed
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
            Log();

            if(kyleBuffer != null) {
                errBuffer = index;
                throw new StackedRequestsException();
            } else {
                kyleBuffer = index;
            }
            try {
                if(!GetCurrentEnvironment().name.StartsWith("Menu", Constants.StrInv) && platformSpawned) {

                    PlatformLifeCycleManagement.InternalChangeToPlatform();

                }
            } catch(EnvironmentSceneNotFoundException e) {
                IPA.Logging.Logger.Level L = IPA.Logging.Logger.Level.Warning;
                Log("TempChangeToPlatform was called out of place. Please send me a bug report.", L);
                Log(e, L);
            }
        }
        public CustomPlatform AddPlatform(string path) {
            CustomPlatform newPlatform = PlatformLoader.LoadPlatformBundle(path, transform);
            if(newPlatform != null) {
                AllPlatforms.Add(newPlatform);
            }
            return newPlatform;
        }
        internal static void ChangeToPlatform(int? index = null) {
            if(index == null) {
                PlatformLifeCycleManagement.InternalChangeToPlatform();
            } else {
                PlatformLifeCycleManagement.InternalChangeToPlatform(index.Value);
            }
        }
        /// <summary>
        /// Overrides the previous request, if there was one<br/>
        /// May trigger a platform change if called in-song (Primarily to catch late scene change callbacks)
        /// </summary>
        internal static void OverridePreviousRequest() {
            if(errBuffer != null) {
                kyleBuffer = errBuffer;
                errBuffer = null;
                try {
                    if(!GetCurrentEnvironment().name.StartsWith("Menu", Constants.StrInv)) {
                        PlatformLifeCycleManagement.InternalChangeToPlatform();
                    }
                } catch(EnvironmentSceneNotFoundException e) {
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
        /// </summary>
        private void LoadHeart() {
            Scene greenDay = SceneManager.LoadScene("GreenDayGrenadeEnvironment", new LoadSceneParameters(LoadSceneMode.Additive));
            StartCoroutine(fuckUnity());
            IEnumerator<WaitUntil> fuckUnity() {//did you know loaded scenes are loaded asynchronously, regarless if you use async or not?
                yield return new WaitUntil(() => { return greenDay.isLoaded; });
                GameObject root = greenDay.GetRootGameObjects()[0];
                Heart = root.transform.Find("GreenDayCity/armHeartLighting").gameObject;
                Heart.transform.parent = null;
                Heart.name = "<3";
                SceneManager.MoveGameObjectToScene(Heart, PlatformManagerScene);
                SceneManager.UnloadSceneAsync("GreenDayGrenadeEnvironment");

                Settings.ShowHeartChanged += Heart.SetActive;

                using Stream manifestResourceStream = Assembly.GetAssembly(GetType()).GetManifestResourceStream("CustomFloorPlugin.heart.mesh");
                using StreamReader streamReader = new StreamReader(manifestResourceStream);

                string meshfile = streamReader.ReadToEnd();
                string[] dimension1 = meshfile.Split('|');
                string[][] dimension2 = new string[][] { dimension1[0].Split('/'), dimension1[1].Split('/') };
                string[][] string_vector3s = new string[dimension2[0].Length][];

                int i = 0;
                foreach(string string_vector3 in dimension2[0]) {
                    string_vector3s[i++] = string_vector3.Split(',');
                }

                List<Vector3> vertices = new List<Vector3>();
                List<int> triangles = new List<int>();
                foreach(string[] string_vector3 in string_vector3s) {
                    vertices.Add(new Vector3(float.Parse(string_vector3[0], Constants.NumInv), float.Parse(string_vector3[1], Constants.NumInv), float.Parse(string_vector3[2], Constants.NumInv)));
                }
                foreach(string s_int in dimension2[1]) {
                    triangles.Add(int.Parse(s_int, Constants.NumInv));
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

                Heart.GetComponent<LightWithId>().ColorWasSet(Color.magenta);
                Heart.SetActive(Settings.ShowHeart);
            }
            
        }
    }
}
