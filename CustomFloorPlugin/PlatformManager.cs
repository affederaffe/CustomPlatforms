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

namespace CustomFloorPlugin {
    internal static class Extentions {
        internal static void InvokePrivateMethod<T>(this object obj, string methodName, params object[] methodParams) {
            var method = typeof(T).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(obj, methodParams);
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

            Plugin.gsm.MarkSceneAsPersistent("PlatformManagerDump");
            Plugin.gsm.transitionDidStartEvent += TransitionPrep;
            Plugin.gsm.transitionDidFinishEvent += TransitionFinalize;
        }

        [HarmonyPatch(typeof(EnvironmentOverrideSettingsPanelController))]
        [HarmonyPatch("HandleOverrideEnvironmentsToggleValueChanged")]
        public class EnviromentOverideSettings_Patch {
            static public void Postfix(OverrideEnvironmentSettings ____overrideEnvironmentSettings) {
                if(____overrideEnvironmentSettings.overrideEnvironments == true) {
                    Plugin.Log("Enviroment Override On");
                }

                if(____overrideEnvironmentSettings.overrideEnvironments == false) {
                    Plugin.Log("Enviroment Override Off");
                }
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
                }
            } catch(EnvironmentSceneNotFoundException) {

            }
            //UnregisterLights();
            EmptyLightRegisters();
        }
        void TransitionFinalize(ScenesTransitionSetupDataSO ignored1, DiContainer ignored2) {
            try {
                if(!GetCurrentEnvironment().name.StartsWith("Menu")) {
                FindManager();
                    Plugin.Log("Game load detected");
                    TempChangeToPlatform(currentPlatformIndex);
                    PlatformLoader.AddManagers(currentPlatform.gameObject);
                    SpawnCustomLights();
                    EnvironmentArranger.RearrangeEnvironment();
                    TubeLightManager.CreateAdditionalLightSwitchControllers();
                } else {
                    Plugin.Log("Menu load detected");
                    //RegisterLights();
                }
            } catch(EnvironmentSceneNotFoundException) {

            } catch(NullReferenceException) {

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

        internal static BeatmapObjectCallbackController GetBeatmapObjectCallbackController() {
            try {
                return Plugin.FindFirst<BeatmapObjectCallbackController>();
            } catch(Plugin.ComponentNotFoundException e) {
                throw new BeatmapObjectCallbackControllerNotFoundException(e.TypeInfo);
            }
            
        }
        public sealed class BeatmapObjectCallbackControllerNotFoundException:Plugin.ComponentNotFoundException {
            internal BeatmapObjectCallbackControllerNotFoundException(TypeInfo T) :
                base(T) {

            }
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
                try {
                    FindManager();
                } catch(EnvironmentSceneNotFoundException) {

                }
            }
            if(Input.GetKeyDown(KeyCode.Keypad7)) {
                EnvHider.HideObjectsForPlatform(currentPlatform);
            }
        }
        internal static IEnumerator<WaitForEndOfFrame> HideForPlatformAfterOneFrame(CustomPlatform customPlatform) {
            yield return new WaitForEndOfFrame();
            Instance.EnvHider.HideObjectsForPlatform(Instance.currentPlatform);
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
        internal static void FindManager() {
            Scene scene = GetCurrentEnvironment();
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
            GameObject[] roots = scene.GetRootGameObjects();
            foreach(GameObject root in roots) {
                RecursiveFindManager(root);
            }
            if(!(manager == null)) {
                LightManager = manager;
                Debug.Log("Manager found at:" + GetFullPath(manager.gameObject));
            } else {
                throw new ManagerNotFoundException();
            }
        }
        internal class ManagerNotFoundException:Exception {
            internal ManagerNotFoundException():
                base("No Manager could be found!") {

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
        internal static string GetFullPath(GameObject gameObject) {
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
        internal class EnvironmentSceneNotFoundException:Exception {
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
            StartCoroutine(HideForPlatformAfterOneFrame(GetPlatform(index)));

            // Update lightSwitchEvent TubeLight references
            TubeLightManager.UpdateEventTubeLightList();
            platformIndex = oldIndex;




            ///////////////////////////////////
            foreach(TubeLight tubeLight in GameObject.FindObjectsOfType<TubeLight>()) {
                tubeLight.LogSomething();
            }
        }
    }
}
