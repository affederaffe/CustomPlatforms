using UnityEngine;
using IPA;
using UnityEngine.SceneManagement;
using CustomFloorPlugin.Util;
using Harmony;
using System.Collections.Generic;
using System.Reflection.Emit;
using System;
using System.Linq;
using System.Reflection;
using BS_Utils.Utilities;


namespace CustomFloorPlugin {
    public class Plugin:IBeatSaberPlugin {
        internal static Config config;
        internal static IPA.Logging.Logger logger;
        internal static GameScenesManager gsm = null;
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
            StuffThatDoesntBelongHereButHasToBeHereBecauseBsmlIsntHalfAsStableYetAsCustomUIWasButCustomUiHasBeenKilledAlready();
        }
        /// <summary>
        /// Holds any clutter that's not supposed to be here, but has to.
        /// </summary>
        private void StuffThatDoesntBelongHereButHasToBeHereBecauseBsmlIsntHalfAsStableYetAsCustomUIWasButCustomUiHasBeenKilledAlready() {
            PlatformUI.SetupMenuButtons();
        }

        private void OnMenuSceneLoadedFresh() {
            if(!init){
                gsm = SceneManager.GetSceneByName("PCInit").GetRootGameObjects().First<GameObject>(x => x.name == "AppCoreSceneContext")?.GetComponent<MarkSceneAsPersistent>().GetPrivateField<GameScenesManager>("_gameScenesManager");
                init = true;
                Instance = this;
                config = new Config("Custom Platforms");
                PlatformManager.OnLoad();
            }
        }

        internal static void Log(string message) {
            BS_Utils.Utilities.Logger.Log("CustomFloorPlugin", message);
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