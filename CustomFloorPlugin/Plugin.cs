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
            HarmonyPatches.Patcher.Patch();
            StuffThatDoesntBelongHereButHasToBeHereBecauseBsmlIsntHalfAsStableYetAsCustomUIWasButCustomUiHasBeenKilledAlready();
        }

        /// <summary>
        /// Holds any clutter that's not supposed to be here, but has to.
        /// </summary>
        private void StuffThatDoesntBelongHereButHasToBeHereBecauseBsmlIsntHalfAsStableYetAsCustomUIWasButCustomUiHasBeenKilledAlready() {
            PlatformUI.SetupMenuButtons();
        }

        private void OnMenuSceneLoadedFresh() {
            if(!init) {
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
        internal static void Log(Exception e) {
            Log("An error has been caught:\n" + e.GetType().Name + "\n At:\n" + e.StackTrace + "\nWith message:\n" + e.Message);
        }
        /// <summary>
        /// Searches all currently loaded scenes to find the first Component of type T in the game, regardless if it's active and enabled or not.
        /// </summary>
        /// <typeparam name="T">What Type to look for</typeparam>
        /// <returns></returns>
        public static T FindFirst<T>() {
            object component;
            bool FindFirst(GameObject gameObject) {
                component = gameObject.GetComponent<T>();
                if(component != null) {
                    return true;
                } else if(gameObject.transform.childCount != 0) {
                    for(int i = 0; i < gameObject.transform.childCount; i++) {
                        if(FindFirst(gameObject.transform.GetChild(i).gameObject)) {
                            return true;
                        }
                    }
                }
                return false;
            }
            for(int i = 0; i < SceneManager.sceneCount; i++) {
                Scene scene = SceneManager.GetSceneAt(i);
                foreach(GameObject root in scene.GetRootGameObjects()) {
                    if(FindFirst(root)) {
                        return (T)component;
                    }
                }
            }
            throw new ComponentNotFoundException(typeof(T).GetTypeInfo());
        }
        /// <summary>
        /// Provides the TypeInfo of the Component that wasn't found under ComponentNotFoundException.TypeInfo
        /// </summary>
        public class ComponentNotFoundException:Exception {
            public TypeInfo TypeInfo;
            internal ComponentNotFoundException(TypeInfo T):
                base("No such Component currently present on any GameObject in any scene: " + T.AssemblyQualifiedName) {
                TypeInfo = T;
            }
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