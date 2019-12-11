using UnityEngine;
using System.Linq;
using CustomUI.Utilities;
using Harmony;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace CustomFloorPlugin {
    public static class Extentions {
        public static void InvokePrivateMethod<T>(this object obj, string methodName, object[] methodParams) {
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





        public static Action<GameObject> loadAfterDelayActionDelegate;

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

        void TransitionPrep() {
            Debug.Log("Destroying:");
            int i = 0;
            while(TubeLight.SpawnedObjects.Count != 0) {
                GameObject gameObject = TubeLight.SpawnedObjects[0];
                TubeLight.SpawnedObjects.Remove(gameObject);
                GameObject.DestroyImmediate(gameObject);
                Debug.Log("..." + ++i + "...");
            }
            Debug.Log("GameObjects");
            foreach(BloomPrePassLightTypeSO type in Traverse.Create(typeof(BloomPrePassLight)).Field<Dictionary<BloomPrePassLightTypeSO, HashSet<BloomPrePassLight>>>("_bloomLightsDict").Value.Keys) {
                Debug.Log("Name: " + type.name);
            }
            foreach(BloomPrePassLight.LightsDataItem type in Traverse.Create(typeof(BloomPrePassLight)).Field<List<BloomPrePassLight.LightsDataItem>>("_lightsDataItems").Value) {
                Debug.Log("Type: " + type.lightType.name + ", Count:" + type.lights.Count);
            }

            Debug.Log("Clearing Lists");
            Debug.Log("Length of List: " + Traverse.Create(typeof(BloomPrePassLight)).Field<List<BloomPrePassLight.LightsDataItem>>("_lightsDataItems").Value.Count);
            Traverse.Create(typeof(BloomPrePassLight)).Field<List<BloomPrePassLight.LightsDataItem>>("_lightsDataItems").Value = new List<BloomPrePassLight.LightsDataItem>();
            Traverse.Create(typeof(BloomPrePassLight)).Field<Dictionary<BloomPrePassLightTypeSO, HashSet<BloomPrePassLight>>>("_bloomLightsDict").Value = new Dictionary<BloomPrePassLightTypeSO, HashSet<BloomPrePassLight>>();

            Debug.Log("Length of List: " + Traverse.Create(typeof(BloomPrePassLight)).Field<List<BloomPrePassLight.LightsDataItem>>("_lightsDataItems").Value.Count);
            Debug.Log("Attempting to restore List");
            i = 0;
            for(int j = 0; j < SceneManager.sceneCount; j++) {
                Scene scene = SceneManager.GetSceneAt(i);
                if(!scene.name.StartsWith("Menu") && scene.name.EndsWith("Environment")) {
                    foreach(BloomPrePassLight bloomLight in GetAllBlooms()) {
                        Type type = bloomLight.GetType();
                        Debug.Log("Type: " + type.FullName);
                        if(bloomLight is MeshBloomPrePassLight meshLight) {
                            Debug.Log("_isRegistered: " + Traverse.Create(meshLight).Field<bool>("_isRegistered").Value);
                            Traverse.Create(meshLight).Field<bool>("_isRegistered").Value = false;
                            Debug.Log("_isRegistered: " + Traverse.Create(meshLight).Field<bool>("_isRegistered").Value);
                            meshLight.InvokePrivateMethod<BloomPrePassLight>("RegisterLight", new object[0]);
                            Debug.Log("_isRegistered: " + Traverse.Create(meshLight).Field<bool>("_isRegistered").Value);
                        } else if(bloomLight is TubeBloomPrePassLight tubeLight) {
                            Debug.Log("_isRegistered: " + Traverse.Create(tubeLight).Field<bool>("_isRegistered").Value);
                            Traverse.Create(tubeLight).Field<bool>("_isRegistered").Value = false;
                            Debug.Log("_isRegistered: " + Traverse.Create(tubeLight).Field<bool>("_isRegistered").Value);
                            tubeLight.InvokePrivateMethod<BloomPrePassLight>("RegisterLight", new object[0]);
                            Debug.Log("_isRegistered: " + Traverse.Create(tubeLight).Field<bool>("_isRegistered").Value);
                        }

                        Debug.Log("Blooms \"Re-Registered\": " + ++i);
                        Debug.Log("Length of List: " + Traverse.Create(typeof(BloomPrePassLight)).Field<List<BloomPrePassLight.LightsDataItem>>("_lightsDataItems").Value.Count);
                    }
                }
            }
            Plugin.ToggleBlooms();
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
        //Traverse.Create(instanceObject).Method("PrivateInstanceMethod");
        static bool IsCoActive = false;
        void TransitionFinalize() {
            for(int i = 0; i < SceneManager.sceneCount; i++) {
                Scene scene = SceneManager.GetSceneAt(i);
                if(!scene.name.StartsWith("Menu") && scene.name.EndsWith("Environment")) {
                    GameObject customLightsHolder = new GameObject("CustomLights");
                    SceneManager.MoveGameObjectToScene(customLightsHolder, scene);
                    Debug.Log("Finding Manager");
                    Plugin.FindManager(scene);
                    Debug.Log("Trying to launch Awakes");
                    PlatformManager.loadAfterDelayActionDelegate(customLightsHolder);
                    //try {
                    //    Debug.Log("Trying to reregister all lights");
                    //    Plugin.Instance.ReregisterLights();
                    //} catch {
                    //    Debug.Log("Failed miserably you dumbass!");
                    //}
                }
            }
        }
        private void Start() {
            EnvironmentArranger.arrangement = (EnvironmentArranger.Arrangement)Plugin.config.GetInt("Settings", "EnvironmentArrangement", 0, true);

            EnvironmentSceneOverrider.overrideMode = (EnvironmentSceneOverrider.EnvOverrideMode)Plugin.config.GetInt("Settings", "EnvironmentOverrideMode", 0, true);

            EnvironmentSceneOverrider.GetSceneInfos();

            EnvironmentSceneOverrider.OverrideEnvironmentScene();


            menuEnvHider = new EnvironmentHider();
            gameEnvHider = new EnvironmentHider();
            platformLoader = new PlatformLoader();

            BSEvents.gameSceneLoaded += HandleGameSceneLoaded;
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

        //public IEnumerator<WaitForSeconds> loadAfterDelay() {
        //    if(IsCoActive) {
        //        Debug.Log("Co is active, exiting!");
        //        yield break;
        //    }
        //    IsCoActive = true;
        //    for(int i = 10; i > 0; i--) {
        //        Debug.Log("Waiting: " + i + "...");
        //        yield return new WaitForSeconds(1);
        //    }
        //    Debug.Log("Trying to launch Awakes");
        //    PlatformManager.loadAfterDelayActionDelegate();
        //    try{
        //        Debug.Log("Trying to reregister all lights");
        //        Plugin.Instance.ReregisterLights();
        //    } catch {
        //        Debug.Log("Failed miserably you dumbass!");
        //    }
        //    Debug.Log("Setting Co inactive");
        //    IsCoActive = false;
        //}
        private void HandleGameSceneLoaded() {
            gameEnvHider.FindEnvironment();
            gameEnvHider.HideObjectsForPlatform(currentPlatform);

            EnvironmentArranger.RearrangeEnvironment();
            TubeLightManager.CreateAdditionalLightSwitchControllers();
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
