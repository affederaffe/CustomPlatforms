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
            BSEvents.OnLoad();
            BSEvents.menuSceneLoadedFresh += OnMenuSceneLoadedFresh;
            
            HarmonyInstance hi = HarmonyInstance.Create("com.rolopogo.customplatforms");
            hi.PatchAll(Assembly.GetExecutingAssembly());

        }
        private void OnMenuSceneLoadedFresh() {
            if(!init) {
                init = true;
                Instance = this;
                config = new BS_Utils.Utilities.Config("Custom Platforms");
                PlatformManager.OnLoad();
            }
        }

        //public void OnGameSceneLoaded() {
        //    Scene gameScene = PlatformManager.GetCurrentEnvironment();
        //    bool DidMenuload = gameScene.name.StartsWith("Menu") ? true : false;
        //    if(!DidMenuload) {
        //        ToggleBlooms();
        //        Debug.Log("The following game scene has been loaded:" + gameScene.name);
        //        PlatformManager.FindManager();
        //        ReregisterLights();
        //        TubeLightManager.UpdateEventTubeLightList();
        //    }
        //}

        ////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// Baustelle 1
        /// </summary>
        public void ReregisterLights() {
            Debug.Log("Reregister at:" + PlatformManager.LightManager.name);
            Traverse.Create(PlatformManager.LightManager).Field<List<LightWithId>[]>("_lights").Value = new List<LightWithId>[21];
            foreach(LightWithId light in GameObject.FindObjectsOfType<LightWithId>()) {
                Traverse.Create(light).Field<LightWithIdManager>("_lighManager").Value = PlatformManager.LightManager;
                PlatformManager.LightManager.RegisterLight(light);
            }
            TubeLightManager.UpdateEventTubeLightList();
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////
        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode) { }
        public void OnSceneUnloaded(Scene scene) { }
        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene) { }
        public void OnUpdate() { }
        public void OnFixedUpdate() { }
        public void OnApplicationQuit() { }
    }
}