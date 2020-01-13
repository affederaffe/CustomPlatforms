using CustomFloorPlugin.Exceptions;
using Harmony;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace CustomFloorPlugin {
    public static class Extentions {
        internal static void InvokePrivateMethod<T>(this object obj, string methodName, params object[] methodParams) {
            var method = typeof(T).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(obj, methodParams);
        }
        internal static void SetPrivateField<T>(this T obj, string fieldName, object value) {
            try {
                typeof(T).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(obj, value);
            } catch {
                obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(obj, value);
            }
        }
        /// <summary>
        /// Returns the full path of a GameObject in the scene hierarchy.
        /// </summary>
        /// <param name="gameObject">The instance of a GameObject to generate a path for.</param>
        /// <returns></returns>
        public static string GetFullPath(this GameObject gameObject) {
            StringBuilder path = new StringBuilder();
            while(true) {
                path.Insert(0, "/" + gameObject.name);
                if(gameObject.transform.parent == null) {
                    path.Insert(0, gameObject.scene.name);
                    break;
                }
                gameObject = gameObject.transform.parent.gameObject;
            }
            return path.ToString();
        }
        /// <summary>
        /// Returns the full path of a Component in the scene hierarchy.
        /// </summary>
        /// <param name="component">The instance of a Component to generate a path for.</param>
        /// <returns></returns>
        public static string GetFullPath(this Component component) {
            StringBuilder path = new StringBuilder(component.gameObject.GetFullPath().ToString());
            path.Append("/" + component.GetType().Name);
            return path.ToString();
        }
    }

    public class PlatformManager:MonoBehaviour {
        public static PlatformManager Instance;
        internal static List<GameObject> SpawnedObjects = new List<GameObject>();
        internal static List<Component> SpawnedComponents = new List<Component>();
        EnvironmentHider EnvHider;

        PlatformLoader platformLoader;

        static CustomPlatform[] platforms;
        static private int platformIndex = 0;

        static void OnSpawnStart() { Plugin.Log("Spawning Lights"); }
        internal delegate void SpawnQueueType();
        internal static SpawnQueueType SpawnQueue = new SpawnQueueType(OnSpawnStart);
        internal static Scene scene;
        internal static LightWithIdManager LightManager = null;
        internal static GameObject Heart;
        internal static bool showHeart;

        internal static void OnLoad() {
            if(Instance != null) return;
            GameObject go = new GameObject("Platform Manager");
            go.AddComponent<PlatformManager>();
        }

        private void Awake() {
            if(Instance != null) return;
            Instance = this;
            scene = SceneManager.CreateScene("PlatformManagerDump", new CreateSceneParameters(LocalPhysicsMode.None));
            SceneManager.MoveGameObjectToScene(gameObject, scene);
            Plugin.gsm.MarkSceneAsPersistent("PlatformManagerDump");
            Plugin.gsm.transitionDidStartEvent += TransitionPrep;
            Plugin.gsm.transitionDidFinishEvent += TransitionFinalize;
            Scene greenDay = SceneManager.LoadScene("GreenDayGrenadeEnvironment", new LoadSceneParameters(LoadSceneMode.Additive));
            StartCoroutine(fuckUnity());
            IEnumerator<WaitUntil> fuckUnity() {//did you know loaded scenes are loaded asynchronously, regarless if you use async or not?
                yield return new WaitUntil(() => { return greenDay.isLoaded; });
                GameObject gameObject = greenDay.GetRootGameObjects()[0];
                Heart = gameObject.transform.Find("GreenDayCity/armHeartLighting").gameObject;
                Heart.transform.parent = null;
                Heart.name = "<3";
                SceneManager.MoveGameObjectToScene(Heart, scene);
                SceneManager.UnloadSceneAsync("GreenDayGrenadeEnvironment");

                SwapHeartMesh();
            }
        }
        void TransitionPrep(float ignored) {
            //DestroyCustomLights();
            try {
                if(!GetCurrentEnvironment().name.StartsWith("Menu")) {
                    DestroyCustomLights();
                    InternalTempChangeToPlatform(0);

                } else {
                    Heart.SetActive(false);
                }
            } catch(EnvironmentSceneNotFoundException e) {
                Plugin.Log(e);
            }
            EmptyLightRegisters();
        }
        void TransitionFinalize(ScenesTransitionSetupDataSO ignored1, DiContainer ignored2) {
            try {
                if(!GetCurrentEnvironment().name.StartsWith("Menu")) {
                    try {
                        FindManager();
                        if(!Resources.FindObjectsOfTypeAll<PlayerDataModelSO>()[0].playerData.overrideEnvironmentSettings.overrideEnvironments) {
                            Plugin.Log("Blorp");
                            InternalTempChangeToPlatform();
                            PlatformLoader.AddManagers(currentPlatform.gameObject);
                            SpawnCustomLights();
                            StartCoroutine(ReplaceAllMaterialsAfterOneFrame());
                            EnvironmentArranger.RearrangeEnvironment();
                            TubeLightManager.CreateAdditionalLightSwitchControllers();
                        }
                    } catch(ManagerNotFoundException e) {
                        Plugin.Log(e);
                    }
                } else {
                    Heart.SetActive(showHeart);
                    Heart.GetComponent<LightWithId>().ColorWasSet(Color.magenta);
                }
            } catch(EnvironmentSceneNotFoundException e) {
                Plugin.Log(e);
            }
        }
        private void Start() {
            EnvironmentSceneOverrider.GetSceneInfos();
            EnvironmentSceneOverrider.OverrideEnvironmentScene();

            EnvHider = new EnvironmentHider();
            platformLoader = new PlatformLoader();


            RefreshPlatforms();
            PlatformUI.OnLoad();
        }

        public CustomPlatform AddPlatform(string path) {
            CustomPlatform newPlatform = platformLoader.LoadPlatformBundle(path, transform);
            if(newPlatform != null) {
                var platList = platforms.ToList();
                platList.Add(newPlatform);
                platforms = platList.ToArray();
            }
            return newPlatform;
        }

        public void RefreshPlatforms() {

            if(platforms != null) {
                foreach(CustomPlatform platform in platforms) {
                    Destroy(platform.gameObject);
                }
            }
            platforms = platformLoader.CreateAllPlatforms(transform);
            // Retrieve saved path from player prefs if it exists
            if(Plugin.config.HasKey("Data", "CustomPlatformPath")) {
                string savedPath = Plugin.config.GetString("Data", "CustomPlatformPath");
                // Check if this path was loaded and update our platform index
                for(int i = 0; i < platforms.Length; i++) {
                    if(savedPath == platforms[i].platName + platforms[i].platAuthor) {
                        platformIndex = i;
                        break;
                    }
                }
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private void Update() {
            if(Input.GetKeyDown(KeyCode.Keypad0)) {
                Heart.SetActive(false);
                Heart.GetComponent<LightWithId>().SetPrivateField("_lightManager", LightManager);
                Heart.SetActive(true);
            }
        }
        internal static IEnumerator<WaitForEndOfFrame> HideForPlatformAfterOneFrame(CustomPlatform customPlatform) {
            yield return new WaitForEndOfFrame();
            Instance.EnvHider.HideObjectsForPlatform(customPlatform);
        }
        internal static IEnumerator<WaitForEndOfFrame> ReplaceAllMaterialsAfterOneFrame() {
            yield return new WaitForEndOfFrame();
            MaterialSwapper.ReplaceAllMaterials();

        }
        /// <exception cref="ManagerNotFoundException"></exception>
        internal static void FindManager() {
            Scene? scene;
            try {
                scene = GetCurrentEnvironment();
            } catch(EnvironmentSceneNotFoundException e) {
                throw new ManagerNotFoundException(e);
            }

            LightWithIdManager manager = null;
            void RecursiveFindManager(GameObject directParent) {
                for(int i = 0; i < directParent.transform.childCount; i++) {
                    GameObject child = directParent.transform.GetChild(i).gameObject;
                    if(child.GetComponent<LightWithIdManager>() != null) {
                        manager = child.GetComponent<LightWithIdManager>();
                    }
                    if(child.transform.childCount != 0) {
                        RecursiveFindManager(child);
                    }
                }
            }
            GameObject[] roots = scene?.GetRootGameObjects();
            foreach(GameObject root in roots) {
                RecursiveFindManager(root);
            }
            if(manager != null) {
                LightManager = manager;
            } else {
                throw new ManagerNotFoundException();
            }
        }
        void DestroyCustomLights() {
            while(SpawnedObjects.Count != 0) {
                GameObject gameObject = SpawnedObjects[0];
                SpawnedObjects.Remove(gameObject);
                Destroy(gameObject);
            }
            while(SpawnedComponents.Count != 0) {
                Component component = SpawnedComponents[0];
                SpawnedComponents.Remove(component);
                Destroy(component);
            }
        }
        void SpawnCustomLights() {
            SpawnQueue();
        }
        void EmptyLightRegisters() {
            Traverse.Create(typeof(BloomPrePassLight)).Field<List<BloomPrePassLight.LightsDataItem>>("_lightsDataItems").Value = new List<BloomPrePassLight.LightsDataItem>();
            Traverse.Create(typeof(BloomPrePassLight)).Field<Dictionary<BloomPrePassLightTypeSO, HashSet<BloomPrePassLight>>>("_bloomLightsDict").Value = new Dictionary<BloomPrePassLightTypeSO, HashSet<BloomPrePassLight>>();
        }
        /// <exception cref="EnvironmentSceneNotFoundException"></exception>
        internal static Scene GetCurrentEnvironment() {
            Scene scene = new Scene();
            Scene environmentScene = scene;
            for(int i = 0; i < SceneManager.sceneCount; i++) {
                scene = SceneManager.GetSceneAt(i);
                if(scene.name.EndsWith("Environment")) {
                    if(!environmentScene.IsValid() || environmentScene.name.StartsWith("Menu"))
                        environmentScene = scene;
                }
            }
            if(environmentScene.IsValid()) {
                return environmentScene;
            }
            throw new EnvironmentSceneNotFoundException();
        }
        public int currentPlatformIndex { get { return platformIndex; } }

        public static CustomPlatform currentPlatform { get { return platforms[platformIndex]; } }

        public CustomPlatform[] GetPlatforms() {
            return platforms;
        }

        public CustomPlatform GetPlatform(int i) {
            return platforms.ElementAt(i);
        }

        internal void SetPlatform(int index) {
            // Hide current Platform
            currentPlatform.gameObject.SetActive(false);

            // Increment index
            platformIndex = index % platforms.Length;

            // Save path into ModPrefs
            Plugin.config.SetString("Data", "CustomPlatformPath", currentPlatform.platName + currentPlatform.platAuthor);

            // Show new platform
            currentPlatform.gameObject.SetActive(true);

            // Hide environment for new platform
            EnvHider.HideObjectsForPlatform(currentPlatform);

            // Update lightSwitchEvent TubeLight references
            TubeLightManager.UpdateEventTubeLightList();
        }
        /// <summary>
        /// Stores a requested platform ID
        /// </summary>
        internal static int? kyleBuffer = null;
        /// <summary>
        /// Stores an overflown platform ID if <see cref="kyleBuffer"/> already stores an ID.
        /// </summary>
        internal static int? errBuffer = null;
        /// <summary>
        /// Please use <see cref="TempChangeToPlatform(int)"/> instead.
        /// </summary>
        //[Obsolete("Please use TempChangeToPlatform instead", false)]
        //public void ChangeToPlatform(int index, bool ignored = true) {
        //    try {
        //        TempChangeToPlatform(index);
        //    } catch(StackedRequestsException e) {
        //        e.OverridePreviousRequest();
        //    } finally {
        //        InternalTempChangeToPlatform();
        //    }
        //}
        /// <summary>
        /// This function handles outside requests to temporarily change to a specific platform.<br/>
        /// It caches the request and will consume it when a level is played.<br/>
        /// This can be called both before, and after loading.<br/>
        /// It does not require you to reset the platform back to the last known state after the level.
        /// </summary>
        /// <param name="index">Index of the desired platform</param>
        /// <exception cref="StackedRequestsException"></exception>
        public static void TempChangeToPlatform(int index) {
            if(kyleBuffer.HasValue) {
                errBuffer = index;
                throw new StackedRequestsException();
            } else {
                kyleBuffer = index;
            }
            try {
                if(!GetCurrentEnvironment().name.StartsWith("Menu")) {
                    InternalTempChangeToPlatform();
                }
            } catch(EnvironmentSceneNotFoundException e) {
                IPA.Logging.Logger.Level L = IPA.Logging.Logger.Level.Warning;
                Plugin.Log("TempChangeToPlatform was called out of place. Please send me a bug report.", L);
                Plugin.Log(e, L);
            }
        }
        internal static void InternalTempChangeToPlatform() {
            if(kyleBuffer.HasValue) {
                InternalTempChangeToPlatform(kyleBuffer.Value);
                kyleBuffer = null;
            } else {
                InternalTempChangeToPlatform(platformIndex);
            }
        }
        internal static void InternalTempChangeToPlatform(int index) {
            // Hide current Platform
            currentPlatform.gameObject.SetActive(false);
            int oldIndex = platformIndex;
            // Increment index
            platformIndex = index % platforms.Length;

            // Show new platform
            currentPlatform.gameObject.SetActive(true);

            // Hide environment for new platform
            Instance.StartCoroutine(HideForPlatformAfterOneFrame(currentPlatform));

            // Update lightSwitchEvent TubeLight references
            TubeLightManager.UpdateEventTubeLightList();
            platformIndex = oldIndex;
        }
        internal static void InternalOverridePreviousRequest() {
            kyleBuffer = errBuffer;
            errBuffer = null;
            try {
                if(!GetCurrentEnvironment().name.StartsWith("Menu")) {
                    InternalTempChangeToPlatform();
                }
            } catch(EnvironmentSceneNotFoundException e) {
                IPA.Logging.Logger.Level L = IPA.Logging.Logger.Level.Warning;
                Plugin.Log("OverridePreviousRequest was called out of place. Please send me a bug report.", L);
                Plugin.Log(e, L);
            }
        }
        void SwapHeartMesh() {
            System.Globalization.NumberFormatInfo numberFormat = System.Globalization.NumberFormatInfo.InvariantInfo;
            using Stream manifestResourceStream = Assembly.GetAssembly(GetType()).GetManifestResourceStream("CustomFloorPlugin.heart.mesh");
            using StreamReader streamReader = new StreamReader(manifestResourceStream);

            string meshfile = streamReader.ReadToEnd();
            string[] dimension1 = meshfile.Split('|');

            string[][] s_vector3s = new string[dimension1[0].Split('/').Length][];

            int i = 0;
            foreach(string s_vector3 in dimension1[0].Split('/')) {
                s_vector3s[i++] = s_vector3.Split(',');
            }

            List<Vector3> vertices = new List<Vector3>();
            foreach(string[] s_vector3 in s_vector3s) {
                vertices.Add(new Vector3(float.Parse(s_vector3[0], numberFormat), float.Parse(s_vector3[1], numberFormat), float.Parse(s_vector3[2], numberFormat)));
            }

            List<int> triangles = new List<int>();
            foreach(string s_int in dimension1[1].Split('/')) {
                triangles.Add(int.Parse(s_int));
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();

            Vector3 position = new Vector3(-8f, 25f, 26f);
            Quaternion rotation = Quaternion.Euler(-100f, 90f, 90f);
            Vector3 scale = new Vector3(25f, 25f, 25f);

            Heart.GetComponent<MeshFilter>().mesh = mesh;
            Heart.transform.position = position;
            Heart.transform.rotation = rotation;
            Heart.transform.localScale = scale;

            Heart.GetComponent<LightWithId>().ColorWasSet(Color.magenta);

            Heart.SetActive(showHeart);
        }
    }
}
