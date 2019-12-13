using UnityEngine;
using System.Linq;
using CustomUI.Utilities;
using Harmony;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Text;

namespace CustomFloorPlugin {
    public static class Extentions {
        public static void InvokePrivateMethod<T>(this object obj, string methodName, params object[] methodParams) {
            var method = typeof(T).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(obj, methodParams);
        }
    }

    public class PlatformManager:MonoBehaviour {
        public static PlatformManager Instance;

        private EnvironmentHider menuEnvHider;
        private EnvironmentHider gameEnvHider;

        private PlatformLoader platformLoader;

        private CustomPlatform[] platforms;
        private int platformIndex = 0;
        
        static void OnSpawnStart(){Debug.Log("Spawning Lights");}
        internal delegate void SpawnQueueType();
        internal static SpawnQueueType SpawnQueue = new SpawnQueueType(OnSpawnStart);
        
        internal static LightWithIdManager LightManager = null;

        public static void OnLoad() {
            if(Instance != null) return;
            GameObject go = new GameObject("Platform Manager");
            go.AddComponent<PlatformManager>();
        }

        private void Awake() {
            if(Instance != null) return;
            Instance = this;
            SceneManager.MoveGameObjectToScene(gameObject, SceneManager.CreateScene("PlatformManagerDump"));
            GameScenesManagerSO.MarkSceneAsPersistent("PlatformManagerDump");
            GameScenesManagerSO.beforeDismissingScenesSignal.Subscribe(TransitionPrep);
            GameScenesManagerSO.transitionDidFinishSignal.Subscribe(TransitionFinalize);
        }

        [HarmonyPatch(typeof(EnvironmentOverrideSettingsPanelController))]
        [HarmonyPatch("HandleOverrideEnvironmentsToggleValueChanged")]
        public class EnviromentOverideSettings_Patch
        {
            static public void Postfix(OverrideEnvironmentSettings ____overrideEnvironmentSettings)
            {
                if (____overrideEnvironmentSettings.overrideEnvironments == true) {
                    Debug.Log("Enviroment Override On");
                }

                if (____overrideEnvironmentSettings.overrideEnvironments == false)
                {
                    Debug.Log("Enviroment Override Off");
                }
            }
        }

        void TransitionPrep() {
            DestroyCustomLights();
            if(GetCurrentEnvironment().name.StartsWith("Menu")) {
                ChangeToPlatform(currentPlatformIndex);
            }
            UnregisterLights();
            EmptyLightRegisters();
        }
        void TransitionFinalize() {
            FindManager();
            if(!GetCurrentEnvironment().name.StartsWith("Menu")) {
                Debug.Log("Game load detected");
                gameEnvHider.FindEnvironment();
                gameEnvHider.HideObjectsForPlatform(currentPlatform);
                EnvironmentArranger.RearrangeEnvironment();
                TubeLightManager.CreateAdditionalLightSwitchControllers();
                SpawnCustomLights();
            } else {
                Debug.Log("Menu load detected");
                menuEnvHider.HideObjectsForPlatform(GetPlatform(0));
                TempChangeToPlatform(0);
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
            Debug.Log("returned an Array of " + lights.Count + " BloomLights");
            return lights;
        }
        
        private void Start() {
            EnvironmentArranger.arrangement = (EnvironmentArranger.Arrangement)Plugin.config.GetInt("Settings", "EnvironmentArrangement", 0, true);
            EnvironmentSceneOverrider.overrideMode = (EnvironmentSceneOverrider.EnvOverrideMode)Plugin.config.GetInt("Settings", "EnvironmentOverrideMode", 0, true);
            EnvironmentSceneOverrider.GetSceneInfos();
            EnvironmentSceneOverrider.OverrideEnvironmentScene();

            menuEnvHider = new EnvironmentHider();
            gameEnvHider = new EnvironmentHider();
            platformLoader = new PlatformLoader();


            RefreshPlatforms();
            menuEnvHider.FindEnvironment();
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
            if(Input.GetKeyDown(KeyCode.P)) {
                NextPlatform();
            }
            if(Input.GetKeyDown(KeyCode.Keypad0)) {
                EmptyLightRegisters();
            }
            if(Input.GetKeyDown(KeyCode.Keypad1)) {
                ToggleBlooms();
            }
            if(Input.GetKeyDown(KeyCode.Keypad2)) {
                RegisterLights();
            }
            if(Input.GetKeyDown(KeyCode.Keypad3)) {
                UnregisterLights();
            }
            if(Input.GetKeyDown(KeyCode.Keypad4)) {
                SpawnCustomLights();
            }
            if(Input.GetKeyDown(KeyCode.Keypad5)) {
                DestroyCustomLights();
            }
            if(Input.GetKeyDown(KeyCode.Keypad6)) {
                FindManager();
            }
            if(Input.GetKeyDown(KeyCode.Keypad7)) {
                gameEnvHider.FindEnvironment();
                gameEnvHider.HideObjectsForPlatform(currentPlatform);
            }

        }
        internal static IEnumerator<WaitForEndOfFrame> ToggleBlooms(string sceneName = "MenuEnvironment") {
            yield return new WaitForEndOfFrame();
            Debug.Log("Toggling Blooms");
            Debug.Log("Getting Root Objects");
            GameObject[] roots = SceneManager.GetSceneByName(sceneName).GetRootGameObjects();
            foreach(GameObject root in roots) {
                Debug.Log("Starting Recursive Toggling");
                RecursiveToggleBloomPrePass(root);
            }
            Debug.Log("Finished Toggling");
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
        internal static void FindManager() {
            Scene scene = GetCurrentEnvironment();
            Debug.Log("Finding Manager");
            LightWithIdManager manager = null;
            void FindManager(GameObject directParent) {
                for(int i = 0; i < directParent.transform.childCount; i++) {
                    GameObject child = directParent.transform.GetChild(i).gameObject;
                    if(child.GetComponent<LightWithIdManager>() != null) {
                        manager = child.GetComponent<LightWithIdManager>();
                    }
                    if(child.transform.childCount != 0) {
                        FindManager(child);
                    }
                }
            }
            GameObject[] roots = scene.GetRootGameObjects();
            foreach(GameObject root in roots) {
                FindManager(root);
            }
            LightManager = manager;
        }
        void DestroyCustomLights() {
            Debug.Log("Destroying:");
            int i = 0;
            while(TubeLight.SpawnedObjects.Count != 0) {
                GameObject gameObject = TubeLight.SpawnedObjects[0];
                TubeLight.SpawnedObjects.Remove(gameObject);
                GameObject.DestroyImmediate(gameObject);
                Debug.Log("..." + ++i + "...");
            }
            Debug.Log("GameObjects");
        }
        void SpawnCustomLights(){
            Debug.Log("Trying to launch Awakes");
            Debug.Log("Spawnqueue has: " + SpawnQueue.GetInvocationList().Length + " entries.");
            PlatformManager.SpawnQueue();
        }
        void EmptyLightRegisters() {
            Debug.Log("Clearing Lists");
            Debug.Log("Length of List: " + Traverse.Create(typeof(BloomPrePassLight)).Field<List<BloomPrePassLight.LightsDataItem>>("_lightsDataItems").Value.Count);
            Traverse.Create(typeof(BloomPrePassLight)).Field<List<BloomPrePassLight.LightsDataItem>>("_lightsDataItems").Value = new List<BloomPrePassLight.LightsDataItem>();
            Traverse.Create(typeof(BloomPrePassLight)).Field<Dictionary<BloomPrePassLightTypeSO, HashSet<BloomPrePassLight>>>("_bloomLightsDict").Value = new Dictionary<BloomPrePassLightTypeSO, HashSet<BloomPrePassLight>>();
            Debug.Log("Length of List: " + Traverse.Create(typeof(BloomPrePassLight)).Field<List<BloomPrePassLight.LightsDataItem>>("_lightsDataItems").Value.Count);
        }
        void UnregisterLights() {
            int i = 0;
            Debug.Log("Trying to unregister all lights:");
            foreach(BloomPrePassLight bloomLight in GetAllBlooms(true)) {
                Debug.Log("Before Loop");
                Debug.Log("Type: " + bloomLight.GetType().FullName);
                Debug.Log("Path: " + GetFullPath(bloomLight.gameObject));
                Debug.Log("LightType: " + Traverse.Create(bloomLight).Field<BloomPrePassLightTypeSO>("_lightType").Value.name);
                Traverse.Create(bloomLight).Field<bool>("visible").Value = false;
                bloomLight.InvokePrivateMethod<BloomPrePassLight>("UnregisterLight");
                Debug.Log("Registered: " + Traverse.Create(bloomLight).Field<bool>("_isRegistered").Value);

                Debug.Log("Loops finished: " + ++i);
            }
        }
        void RegisterLights() {
            int i = 0;
            Debug.Log("Trying to register all lights:");
            foreach(BloomPrePassLight bloomLight in GetAllBlooms()) {
                Debug.Log("Type: " + bloomLight.GetType().FullName + ", Path: " + GetFullPath(bloomLight.gameObject) + ", LightType: " + Traverse.Create(bloomLight).Field<BloomPrePassLightTypeSO>("_registeredWithLightType").Value.name);
                Traverse.Create(bloomLight).Field<bool>("visible").Value = true;
                bloomLight.InvokePrivateMethod<BloomPrePassLight>("RegisterLight");
                Debug.Log("Registered: " + Traverse.Create(bloomLight).Field<bool>("_isRegistered").Value);

                Debug.Log("Loop: " + ++i);
            }
        }
        string GetFullPath(GameObject gameObject) {
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
        class EnvironmentSceneNotFoundException:Exception {
            internal EnvironmentSceneNotFoundException() :
                base("No Environment Scene could be found!") {

            }
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
            ChangeToPlatform(platformIndex + 1);
        }

        public void PrevPlatform() {
            ChangeToPlatform(platformIndex - 1);
        }

        public void ChangeToPlatform(int index, bool save = true) {
            Debug.Log("This is a permanent change");
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
            menuEnvHider.HideObjectsForPlatform(currentPlatform);
            gameEnvHider.HideObjectsForPlatform(currentPlatform);

            // Update lightSwitchEvent TubeLight references
            TubeLightManager.UpdateEventTubeLightList();
        }

        internal void TempChangeToPlatform(int index) {
            Debug.Log("This is a temporary change");
            
            // Hide current Platform
            currentPlatform.gameObject.SetActive(false);
            int oldIndex = platformIndex;
            // Increment index
            platformIndex = index % platforms.Length;

            // Show new platform
            currentPlatform.gameObject.SetActive(true);

            // Hide environment for new platform
            menuEnvHider.HideObjectsForPlatform(currentPlatform);
            gameEnvHider.HideObjectsForPlatform(currentPlatform);

            // Update lightSwitchEvent TubeLight references
            TubeLightManager.UpdateEventTubeLightList();
            platformIndex = oldIndex;
        }
    }
}
