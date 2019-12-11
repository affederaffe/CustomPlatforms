using UnityEngine;
using IPA;
using UnityEngine.SceneManagement;
using CustomFloorPlugin.Util;
using CustomUI.Utilities;
using Harmony;
using System.Collections.Generic;
using System.Reflection.Emit;
using System;
using System.Linq;
using System.Reflection;

namespace CustomFloorPlugin {
    public class Plugin:IBeatSaberPlugin {
        public static BS_Utils.Utilities.Config config;
        public static IPA.Logging.Logger logger;
        private bool init = false;

        public static Plugin Instance = null;
        


        public void Init(object thisWillBeNull, IPA.Logging.Logger logger) {
            Plugin.logger = logger;
        }

        public void OnApplicationStart() {
            //Instance = this;
            BSEvents.OnLoad();
            BSEvents.menuSceneLoadedFresh += OnMenuSceneLoadedFresh;
            BSEvents.gameSceneLoaded += OnGameSceneLoaded;
            //BSEvents.menuSceneActive += MenuSceneActive;
            
            HarmonyInstance hi = HarmonyInstance.Create("com.rolopogo.customplatforms");
            hi.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

        }
        //private void MenuSceneActive() {
        //    Debug.Log("Destroying:");
        //    int i = 0;
        //    while(TubeLight.SpawnedObjects.Count != 0) {
        //        GameObject gameObject = TubeLight.SpawnedObjects[0];
        //        TubeLight.SpawnedObjects.Remove(gameObject);
        //        GameObject.Destroy(gameObject);
        //        Debug.Log("..." + (i + 1) + "...");
        //        i++;
        //    }
        //    Debug.Log("GameObjects");
        //    Debug.Log("Clearing List");
        //    //Traverse.Create(typeof(BloomPrePassLight)).Field<List<BloomPrePassLight.LightsDataItem>>("_lightsDataItems").Value = new List<BloomPrePassLight.LightsDataItem>();
        //    Debug.Log("ReStartingCoroutine");
        //    PlatformManager.Instance.ReStartCoroutine();
        //    //Traverse.Create(tubeBloomLight).Method("UnregisterLight");
        //    //tubeBloomLight.gameObject.SetActive(false);

        //    //tubeBloomLight.Refresh();

        //}

        //[HarmonyPatch(typeof(LightWithId))]
        //[HarmonyPatch("Start")]
        public class LightWithId_Start_Patch {
            public static LightWithIdManager GameLightManager = null;
            //    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
            //        List<CodeInstruction> newCodes = new List<CodeInstruction>();
            //        newCodes.Add(new CodeInstruction(opcode: OpCodes.Ret));
            //        Debug.Log("Patching: LightWithID_Start()");
            //        return newCodes.AsEnumerable();
            //    }
        }
        //[HarmonyPatch(typeof(LightWithId))]
        //[HarmonyPatch("OnDestroy")]
        //public class LightWithIdManager_RegisterLight_Patch {
        //    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> oldOpCodes) {
        //        List<CodeInstruction> newOpCodes = new List<CodeInstruction>();
        //        newOpCodes.Add(new CodeInstruction(opcode: OpCodes.Ret));
        //        Debug.Log("Patching: LightWithID_OnDestroy()");
        //        return newOpCodes.AsEnumerable();
        //    }
        //}
        private void OnMenuSceneLoadedFresh() {
            if(!init) {
                init = true;
                Instance = this;
                config = new BS_Utils.Utilities.Config("Custom Platforms");
                PlatformManager.OnLoad();
            }
        }

        public void OnApplicationQuit() {
            BSEvents.menuSceneLoadedFresh -= OnMenuSceneLoadedFresh;

        }

        // Neuer Code
        public void OnGameSceneLoaded() {
            ToggleBlooms();
            Scene gameScene = SceneManager.GetActiveScene();
            for(int i = 0; i < SceneManager.sceneCount; i++) {
                Scene scene = SceneManager.GetSceneAt(i);
                if(!scene.name.StartsWith("Menu") && scene.name.EndsWith("Environment")) {
                    Debug.Log("Found Game Scene at: " + scene.name);
                    gameScene = scene;
                }
            }
            Debug.Log("The following game scene has been loaded:" + gameScene.name);
            FindManager(gameScene);
            ReregisterLights();
            TubeLightManager.UpdateEventTubeLightList();
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode) {

        }
        ////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Deprecated???
        /// </summary>
        /// <param name="sceneName"></param>
        public static void ToggleBlooms(string sceneName = "MenuEnvironment") {

            GameObject[] roots = SceneManager.GetSceneByName(sceneName).GetRootGameObjects();

            void RecursiveToggleBloomPrePass(GameObject directParent) {
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
            foreach(GameObject root in roots) {
                RecursiveToggleBloomPrePass(root);
            }
        }
        public static void FindManager(Scene scene) {

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
            LightWithId_Start_Patch.GameLightManager = manager;
        }
        /// <summary>
        /// Baustelle 1
        /// </summary>
        public void ReregisterLights() {
            Debug.Log("Reregister at:" + LightWithId_Start_Patch.GameLightManager.name);
            Traverse.Create(LightWithId_Start_Patch.GameLightManager).Field<List<LightWithId>[]>("_lights").Value = new List<LightWithId>[21];
            foreach(LightWithId light in GameObject.FindObjectsOfType<LightWithId>()) {
                Traverse.Create(light).Field<LightWithIdManager>("_lighManager").Value = LightWithId_Start_Patch.GameLightManager;
                LightWithId_Start_Patch.GameLightManager.RegisterLight(light);
            }
            TubeLightManager.UpdateEventTubeLightList();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////

        public void OnSceneUnloaded(Scene scene) {

        }



        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene) {
        }

        public void OnUpdate() {

        }

        public void OnFixedUpdate() {

        }
    }

}