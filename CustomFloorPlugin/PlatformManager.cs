using UnityEngine;
using System.Linq;
using Harmony;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Text;
using BS_Utils.Utilities;
using Zenject;
using CustomFloorPlugin.Exceptions;
using System.IO;

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

        CustomPlatform[] platforms;
        private int platformIndex = 0;

        static void OnSpawnStart() { Plugin.Log("Spawning Lights"); }
        internal delegate void SpawnQueueType();
        internal static SpawnQueueType SpawnQueue = new SpawnQueueType(OnSpawnStart);
        internal static Scene scene;
        internal static LightWithIdManager LightManager = null;
        internal static GameObject Heart;

        public static void OnLoad() {
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
            IEnumerator<WaitUntil> fuckUnity() {
                yield return new WaitUntil(() => { return greenDay.isLoaded; });
                GameObject gameObject = greenDay.GetRootGameObjects()[0];
                Heart = gameObject.transform.Find("GreenDayCity/armHeartLighting").gameObject;
                Heart.transform.parent = null;
                Heart.name = "<3";
                SceneManager.MoveGameObjectToScene(Heart, scene);
                SceneManager.UnloadSceneAsync("GreenDayGrenadeEnvironment");

                //<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//
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
                //<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//<3//
            }
        }
        void TransitionPrep(float ignored) {
            Plugin.Log("TransitionPrep has been triggered, here is a handy list of all curently loaded scenes :)");
            for(int i = 0; i < SceneManager.sceneCount; i++) {
                Scene scene = SceneManager.GetSceneAt(i);
                Plugin.Log(scene.name);
            }
            //DestroyCustomLights();
            try {
                if(!GetCurrentEnvironment().name.StartsWith("Menu")) {
                    DestroyCustomLights();
                    TempChangeToPlatform(0);

                } else {
                    Heart.SetActive(false);
                }
            } catch(EnvironmentSceneNotFoundException e) {
                Plugin.Log(e);
            }
            //UnregisterLights();
            EmptyLightRegisters();
        }
        void TransitionFinalize(ScenesTransitionSetupDataSO ignored1, DiContainer ignored2) {
            if(!GetCurrentEnvironment().name.StartsWith("Menu")) {
                Plugin.Log("Game load detected");
                try {
                    FindManager();
                    TempChangeToPlatform(currentPlatformIndex);
                    PlatformLoader.AddManagers(currentPlatform.gameObject);
                    SpawnCustomLights();
                    StartCoroutine(ReplaceAllMaterialsAfterOneFrame());
                    EnvironmentArranger.RearrangeEnvironment();
                    TubeLightManager.CreateAdditionalLightSwitchControllers();
                    //RegisterLights();
                } catch(ManagerNotFoundException e) {
                    Plugin.Log(e);
                }
            } else {
                Plugin.Log("Menu load detected");
                Heart.SetActive(true);
                Heart.GetComponent<LightWithId>().ColorWasSet(Color.magenta);
            }
        }
        /// <summary>
        /// Searches for all BloomPrePassLights in all loaded scenes.
        /// </summary>
        /// <param name="inactives">
        /// Whether or not inactive Blooms should be included in the returned List.
        /// </param>
        /// <returns></returns>
        List<BloomPrePassLight> GetAllBlooms(bool inactives = false) {
            List<BloomPrePassLight> lights = new List<BloomPrePassLight>();
            for(int i = 0; i < SceneManager.sceneCount; i++) {
                Scene scene = SceneManager.GetSceneAt(i);
                GameObject[] roots = scene.GetRootGameObjects();
                foreach(GameObject root in roots) {
                    lights.AddRange(root.GetComponentsInChildren<BloomPrePassLight>(inactives));
                }
            }
            Plugin.Log("returned an Array of " + lights.Count + " BloomLights");
            return lights;
        }

        private void Start() {
            EnvironmentArranger.arrangement = (EnvironmentArranger.Arrangement)Plugin.config.GetInt("Settings", "EnvironmentArrangement", 0, true);
            EnvironmentSceneOverrider.overrideMode = (EnvironmentSceneOverrider.EnvOverrideMode)Plugin.config.GetInt("Settings", "EnvironmentOverrideMode", 0, true);
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
                //In case you ever need a panic button
                for(int i = 0; i < SceneManager.sceneCount; i++) {
                    Scene scene = SceneManager.GetSceneAt(i);
                    foreach(GameObject root in scene.GetRootGameObjects()) {
                        root.SetActive(false);
                    }
                }
            }
            if(Input.GetKeyDown(KeyCode.Keypad1)) {
                //This Debug-Key allows you to print an exhaustive list of all registered LightWithId's in the environment.
                LightWithIdManager lightWithIdManager = Plugin.FindFirst<LightWithIdManager>(GetCurrentEnvironment());
                Plugin.Log("------------");
                Plugin.Log(lightWithIdManager.GetFullPath());
                Plugin.Log("Registered lights: \n");
                Traverse<List<LightWithId>[]> _lights = Traverse.Create(lightWithIdManager).Field<List<LightWithId>[]>("_lights");
                for(int i = 0; i < _lights.Value.Length; i++) {
                    List<LightWithId> list = _lights.Value[i];
                    if(list == null) {
                        Plugin.Log("--No List at ID " + i);
                    } else if(list.Count == 0) {
                        Plugin.Log("--No Lights at ID " + i);
                    } else {
                        Plugin.Log("--" + list.Count + " " + (list.Count == 1 ? "Light" : "Lights") + " at ID " + i + ":");
                        foreach(LightWithId light in list) {
                            Plugin.Log(light.GetFullPath());
                        }
                    }
                }
                Plugin.Log("Done logging lights");
                Plugin.Log("------------");
                List<LightWithId> lightWithIds = new List<LightWithId>(), uLightWithIds = new List<LightWithId>(), temp;
                try {
                    temp = new List<LightWithId>();
                    temp.AddRange(Plugin.FindAll<LightWithId>(GetCurrentEnvironment()));
                    lightWithIds.AddRange(temp);
                    uLightWithIds.AddRange(temp);
                } catch(ComponentNotFoundException e) {
                    Plugin.Log(e);
                    Plugin.Log("The standard environment had no lights?", IPA.Logging.Logger.Level.Notice);
                }
                try {
                    temp = new List<LightWithId>();
                    temp.AddRange(Plugin.FindAll<LightWithId>(currentPlatform.gameObject));
                    lightWithIds.AddRange(temp);
                    uLightWithIds.AddRange(temp);
                } catch(ComponentNotFoundException) {
                    Plugin.Log("The current platform had no lights");
                }
                try {
                    temp = new List<LightWithId>();
                    temp.AddRange(Plugin.FindAll<LightWithId>(SceneManager.GetSceneByName("GameCore")));
                    lightWithIds.AddRange(temp);
                    uLightWithIds.AddRange(temp);
                } catch(ComponentNotFoundException) {
                    Plugin.Log("There are no relevant objects in GameCore");
                }
                foreach(LightWithId lightWithId in lightWithIds) {
                    if(_lights.Value[lightWithId.id]?.Contains(lightWithId) == true) {
                        uLightWithIds.Remove(lightWithId);
                    }
                }
                Plugin.Log("There " + ((uLightWithIds.Count == 1) ? "is" : "are") + " " + ((uLightWithIds.Count != 0) ? uLightWithIds.Count.ToString() : "no") + " unregistered " + ((uLightWithIds.Count == 1) ? "light" : "lights") + ((uLightWithIds.Count != 0) ? ":" : "!"));
                HashSet<LightWithIdManager> managers = new HashSet<LightWithIdManager>();
                foreach(LightWithId lightWithId in uLightWithIds) {
                    managers.Add(lightWithId.GetPrivateField<LightWithIdManager>("_lightManager"));
                }
                Plugin.Log(managers);
                Plugin.Log("------------");
            }
            if(Input.GetKeyDown(KeyCode.Keypad2)) {
                Heart.SetActive(false);
                Heart.GetComponent<LightWithId>().SetPrivateField("_lightManager", LightManager);
                Heart.SetActive(true);
            }
            if(Input.GetKeyDown(KeyCode.Keypad3)) {

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
        internal static IEnumerator<WaitForEndOfFrame> ToggleBlooms(string sceneName = "MenuEnvironment") {
            yield return new WaitForEndOfFrame();
            Plugin.Log("Toggling Blooms");
            Plugin.Log("Getting Root Objects");
            GameObject[] roots = SceneManager.GetSceneByName(sceneName).GetRootGameObjects();
            foreach(GameObject root in roots) {
                Plugin.Log("Starting Recursive Toggling");
                RecursiveToggleBloomPrePass(root);
            }
            Plugin.Log("Finished Toggling");
        }
        internal static IEnumerator<WaitForEndOfFrame> ToggleBlooms(GameObject gameObject) {
            yield return new WaitForEndOfFrame();
            RecursiveToggleBloomPrePass(gameObject);
        }
        private static void RecursiveToggleBloomPrePass(GameObject directParent) {
            for(int i = 0; i < directParent.transform.childCount; i++) {
                GameObject child = directParent.transform.GetChild(i).gameObject;
                if(child.GetComponent<BloomPrePassLight>() != null) {
                    child.transform.parent = null;
                    child.SetActive(!child.activeSelf);
                    child.SetActive(!child.activeSelf);
                    child.transform.parent = directParent.transform;
                }
                if(child.transform.childCount != 0) {
                    RecursiveToggleBloomPrePass(child);
                }
            }
        }
        /// <exception cref="ManagerNotFoundException"></exception>
        internal static void FindManager() {
            Scene? scene;
            try {
                scene = GetCurrentEnvironment();
            } catch(EnvironmentSceneNotFoundException e) {
                throw new ManagerNotFoundException(e);
            }

            Plugin.Log("Finding Manager");
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
                Plugin.Log("Manager found at:" + manager.GetFullPath());
            } else {
                throw new ManagerNotFoundException();
            }
        }
        void DestroyCustomLights() {
            Plugin.Log("Destroying:");
            int i = 0;
            while(SpawnedObjects.Count != 0) {
                GameObject gameObject = SpawnedObjects[0];
                SpawnedObjects.Remove(gameObject);
                Destroy(gameObject);
                i++;
            }
            Plugin.Log(i.ToString() + " GameObjects");
            Plugin.Log("And");
            i = 0;
            while(SpawnedComponents.Count != 0) {
                Component component = SpawnedComponents[0];
                SpawnedComponents.Remove(component);
                Destroy(component);
                i++;
            }
            Plugin.Log(i.ToString() + " Components");
        }
        void SpawnCustomLights() {
            Plugin.Log("Trying to launch Awakes");
            Plugin.Log("Spawnqueue has: " + SpawnQueue.GetInvocationList().Length + " entries.");
            SpawnQueue();
        }
        void EmptyLightRegisters() {
            Plugin.Log("Clearing Lists");
            Plugin.Log("Length of List: " + Traverse.Create(typeof(BloomPrePassLight)).Field<List<BloomPrePassLight.LightsDataItem>>("_lightsDataItems").Value.Count);
            Traverse.Create(typeof(BloomPrePassLight)).Field<List<BloomPrePassLight.LightsDataItem>>("_lightsDataItems").Value = new List<BloomPrePassLight.LightsDataItem>();
            Traverse.Create(typeof(BloomPrePassLight)).Field<Dictionary<BloomPrePassLightTypeSO, HashSet<BloomPrePassLight>>>("_bloomLightsDict").Value = new Dictionary<BloomPrePassLightTypeSO, HashSet<BloomPrePassLight>>();
            Plugin.Log("Length of List: " + Traverse.Create(typeof(BloomPrePassLight)).Field<List<BloomPrePassLight.LightsDataItem>>("_lightsDataItems").Value.Count);
        }
        void UnregisterLights() {
            int i = 0;
            Plugin.Log("Trying to unregister all lights:");
            foreach(BloomPrePassLight bloomLight in GetAllBlooms(true)) {
                Traverse.Create(bloomLight).Field<bool>("visible").Value = false;
                bloomLight.InvokePrivateMethod<BloomPrePassLight>("UnregisterLight");
                i++;
            }
            Plugin.Log("Loops finished, tally total: " + ++i);
        }
        void RegisterLights() {
            int i = 0;
            Plugin.Log("Trying to register all lights:");
            foreach(BloomPrePassLight bloomLight in GetAllBlooms()) {
                Traverse.Create(bloomLight).Field<bool>("visible").Value = true;
                bloomLight.InvokePrivateMethod<BloomPrePassLight>("RegisterLight");
                i++;
            }
            Plugin.Log("Loops finished, tally total: " + ++i);
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

        public CustomPlatform currentPlatform { get { return platforms[platformIndex]; } }

        public CustomPlatform[] GetPlatforms() {
            return platforms;
        }

        public CustomPlatform GetPlatform(int i) {
            return platforms.ElementAt(i);
        }
        public void NextPlatform() {
            throw new NotImplementedException();
        }

        public void PrevPlatform() {
            throw new NotImplementedException();
        }

        public void ChangeToPlatform(int index, bool save = true) {
            Plugin.Log("This is a permanent change");
            // Hide current Platform
            currentPlatform.gameObject.SetActive(false);

            // Increment index
            platformIndex = index % platforms.Length;

            // Save path into ModPrefs
            if(save)
                Plugin.config.SetString("Data", "CustomPlatformPath", currentPlatform.platName + currentPlatform.platAuthor);

            // Show new platform
            currentPlatform.gameObject.SetActive(true);

            // Hide environment for new platform
            EnvHider.HideObjectsForPlatform(currentPlatform);

            // Update lightSwitchEvent TubeLight references
            TubeLightManager.UpdateEventTubeLightList();
        }

        internal void TempChangeToPlatform(int index) {
            Plugin.Log("This is a temporary change");

            // Hide current Platform
            currentPlatform.gameObject.SetActive(false);
            int oldIndex = platformIndex;
            // Increment index
            platformIndex = index % platforms.Length;

            // Show new platform
            currentPlatform.gameObject.SetActive(true);

            // Hide environment for new platform
            StartCoroutine(HideForPlatformAfterOneFrame(currentPlatform));

            // Update lightSwitchEvent TubeLight references
            TubeLightManager.UpdateEventTubeLightList();
            platformIndex = oldIndex;
        }
    }
}
