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
            GameScenesManagerSO.transitionDidFinishSignal.Subscribe(HandleGameSceneLoaded);
            GameScenesManagerSO.transitionDidFinishSignal.Subscribe(TransitionFinalize);
        }

        void TransitionPrep() {
            DestroyCustomLights();
            foreach(BloomPrePassLightTypeSO type in Traverse.Create(typeof(BloomPrePassLight)).Field<Dictionary<BloomPrePassLightTypeSO, HashSet<BloomPrePassLight>>>("_bloomLightsDict").Value.Keys) {
                Debug.Log("Name: " + type.name);
            }
            foreach(BloomPrePassLight.LightsDataItem type in Traverse.Create(typeof(BloomPrePassLight)).Field<List<BloomPrePassLight.LightsDataItem>>("_lightsDataItems").Value) {
                Debug.Log("Type: " + type.lightType.name + ", Count:" + type.lights.Count);
            }
        }
        List<BloomPrePassLight> GetAllBlooms() {
            List<BloomPrePassLight> lights = new List<BloomPrePassLight>();
            for(int i = 0; i < SceneManager.sceneCount; i++) {
                Scene scene = SceneManager.GetSceneAt(i);
                GameObject[] roots = scene.GetRootGameObjects();
                foreach(GameObject root in roots) {
                    lights.AddRange(root.GetComponentsInChildren<BloomPrePassLight>(false));
                }
            }
            Debug.Log("returned an Array of " + lights.Count + " BloomLights");
            return lights;
        }
        void TransitionFinalize() {
            Debug.Log("This is called in BeatSabersHandle");
            Scene currentScene = GetCurrentEnvironment();
            bool DidMenuload = currentScene.name.StartsWith("Menu") ? true : false;
            Debug.Log("The loaded scene type has been determined");
            EmptyLightRegisters();
            if(!DidMenuload) {
                FindManager();
                SpawnCustomLights();
                Debug.Log("Attempting to restore List");
                RegisterLights();
            }
            Debug.Log("Toggling Blooms");
            Plugin.ToggleBlooms();
        }
        private void Start() {
            EnvironmentArranger.arrangement = (EnvironmentArranger.Arrangement)Plugin.config.GetInt("Settings", "EnvironmentArrangement", 0, true);
            EnvironmentSceneOverrider.overrideMode = (EnvironmentSceneOverrider.EnvOverrideMode)Plugin.config.GetInt("Settings", "EnvironmentOverrideMode", 0, true);
            EnvironmentSceneOverrider.GetSceneInfos();
            EnvironmentSceneOverrider.OverrideEnvironmentScene();

            menuEnvHider = new EnvironmentHider();
            gameEnvHider = new EnvironmentHider();
            platformLoader = new PlatformLoader();

            BSEvents.menuSceneLoadedFresh += HandleMenuSceneLoadedFresh;
            BSEvents.menuSceneLoaded += HandleMenuSceneLoaded;

            RefreshPlatforms();
            HandleMenuSceneLoadedFresh();
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

            ChangeToPlatform(platformIndex);

        }


        private void HandleGameSceneLoaded() {
            if(!GetCurrentEnvironment().name.StartsWith("Menu")) {
                Debug.Log("This is called in HandleGameSceneLoaded");
                gameEnvHider.FindEnvironment();
                gameEnvHider.HideObjectsForPlatform(currentPlatform);
                EnvironmentArranger.RearrangeEnvironment();
                TubeLightManager.CreateAdditionalLightSwitchControllers();
            }
        }

        private void HandleMenuSceneLoadedFresh() {
            ChangeToPlatform(platformIndex);
            menuEnvHider.FindEnvironment();
            HandleMenuSceneLoaded();
            //StartCoroutine(loadAfterDelay());
        }
        public void ReStartCoroutine() {

            //StartCoroutine(loadAfterDelay());
        }
        private void HandleMenuSceneLoaded() {
            menuEnvHider.HideObjectsForPlatform(currentPlatform);
        }

        private void Update() {
            if(Input.GetKeyDown(KeyCode.P)) {
                NextPlatform();
            }
            if(Input.GetKeyDown(KeyCode.Keypad0)) {
                EmptyLightRegisters();
            }
            if(Input.GetKeyDown(KeyCode.Keypad1)) {
                Plugin.ToggleBlooms();
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
            foreach(BloomPrePassLight bloomLight in GetAllBlooms()) {
                Debug.Log("Type: " + bloomLight.GetType().FullName + ", Path: " + GetFullPath(bloomLight.gameObject) + ", LightType: " + Traverse.Create(bloomLight).Field<BloomPrePassLightTypeSO>("_registeredWithLightType").Value.name);

                bloomLight.InvokePrivateMethod<BloomPrePassLight>("UnregisterLight");
                Debug.Log("Registered: " + Traverse.Create(bloomLight).Field<bool>("_isRegistered").Value);

                Debug.Log("Loop: " + ++i);
            }
        }
        void RegisterLights() {
            int i = 0;
            Debug.Log("Trying to register all lights:");
            foreach(BloomPrePassLight bloomLight in GetAllBlooms()) {
                Debug.Log("Type: " + bloomLight.GetType().FullName + ", Path: " + GetFullPath(bloomLight.gameObject) + ", LightType: " + Traverse.Create(bloomLight).Field<BloomPrePassLightTypeSO>("_registeredWithLightType").Value.name);

                bloomLight.InvokePrivateMethod<BloomPrePassLight>("RegisterLight");
                Debug.Log("Registered: " + Traverse.Create(bloomLight).Field<bool>("_isRegistered").Value);

                Debug.Log("Loop: " + ++i);
            }
        }
        string GetFullPath(GameObject gameObject) {
            StringBuilder path = new StringBuilder();
            while(true) {
                path.Append("/" + gameObject.name, 0, gameObject.name.Length + 1);
                if(gameObject.transform.parent == null) {
                    path.Append(gameObject.scene.name, 0, gameObject.scene.name.Length);
                    break;
                }
                gameObject = gameObject.transform.parent.gameObject;
            }
            return path.ToString();
        }
        internal static Scene GetCurrentEnvironment() {
            Scene scene = new Scene();
            for(int i = 0; i < SceneManager.sceneCount; i++) {
                scene = SceneManager.GetSceneAt(i);
                if(scene.name.EndsWith("Environment")) {
                    return scene;
                }
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
