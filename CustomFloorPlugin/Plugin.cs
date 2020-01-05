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
using IPA.Logging;
using BS_Utils.Utilities;
using System.Text;
using Level = IPA.Logging.Logger.Level;
using CustomFloorPlugin.Exceptions;

namespace CustomFloorPlugin {
    internal class Plugin:IBeatSaberPlugin {
        internal static Config config;
        internal static GameScenesManager gsm = null;
        private static IPA.Logging.Logger logger;
        /// <summary>
        /// Do not call this out of gamescene, or else.
        /// </summary>
        private static BeatmapObjectCallbackController _bocc;
        internal static BeatmapObjectCallbackController bocc {
            get {
                if(_bocc == null) {
                    _bocc = FindFirst<BeatmapObjectCallbackController>();
                }
                return _bocc;
            }
            private set {
                _bocc = value;
            }
        }

        private bool init = false;

        public static Plugin Instance = null;

        public void Init(IPA.Logging.Logger logger) {
            Plugin.logger = logger;
        }

        public void OnApplicationStart() {
            BSEvents.OnLoad();
            BSEvents.menuSceneLoadedFresh += OnMenuSceneLoadedFresh;
            HarmonyPatches.Patcher.Patch();
            Stuff_That_Doesnt_Belong_Here_But_Has_To_Be_Here_Because_Bsml_Isnt_Half_As_Stable_Yet_As_CustomUI_Was_But_CustomUI_Has_Been_Killed_Already();
        }

        /// <summary>
        /// Holds any clutter that's not supposed to be here, but has to.
        /// </summary>
        private void Stuff_That_Doesnt_Belong_Here_But_Has_To_Be_Here_Because_Bsml_Isnt_Half_As_Stable_Yet_As_CustomUI_Was_But_CustomUI_Has_Been_Killed_Already() {
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
        internal static void Log(string message, Level level = Level.Info) {
            logger.Log(level, message);
        }
        internal static void Log(Exception e, Level level = Level.Notice) {
            Log("An error has been caught:\n" + e.GetType().Name + "\n At:\n" + e.StackTrace + "\nWith message:\n" + e.Message, level);
            if(e.InnerException != null) {
                Log("---Inner Exception:---", level);
                Log(e, level);
            }
        }
        internal static void Log<T>(List<T> messages, Level level = Level.Info) {
            foreach(T message in messages) {
                try {
                    if(message is GameObject) {
                        Log(message as GameObject, level);
                    } else if(message is Component) {
                        Log(message as Component, level);
                    } else {
                        Log(message, level);
                    }
                } catch(Exception e) {
                    Log(e, Level.Error);
                }
            }
        }
        internal static void Log<T>(T[] messages, Level level = Level.Info) {
            foreach(T message in messages) {
                try {
                    if(message is GameObject) {
                        Log(message as GameObject, level);
                    } else if(message is Component) {
                        Log(message as Component, level);
                    } else {
                        Log(message, level);
                    }
                } catch(Exception e) {
                    Log(e, Level.Error);
                }
            }
        }
        internal static void Log<T>(HashSet<T> messages, Level level = Level.Info) {
            foreach(T message in messages) {
                try {
                    if(message is GameObject) {
                        Log(message as GameObject, level);
                    } else if(message is Component) {
                        Log(message as Component, level);
                    } else {
                        Log(message, level);
                    }
                } catch(Exception e) {
                    Log(e, Level.Error);
                }
            }
        }
        internal static void Log(StringBuilder message, Level level = Level.Info) {
            try {
                Log(message.ToString(), level);

            } catch(Exception e) {
                Log(e, Level.Error);
            }
        }
        internal static void Log(Component message, Level level = Level.Info) {
            try {
                Log(message.GetFullPath(), level);
            } catch(Exception e) {
                Log(e, Level.Error);
            }
        }
        internal static void Log(GameObject message, Level level = Level.Info) {
            try {
                Log(message.GetFullPath(), level);
            } catch(Exception e) {
                Log(e, Level.Error);
            }
        }
        internal static void Log(object message, Level level = Level.Info) {
            try {
                Log(message.ToString(), level);
            } catch(Exception e) {
                Log(e, Level.Error);
            }
        }
        /// <summary>
        /// Searches all currently loaded <see cref="Scene"/>s to find the first <see cref="Component"/> of type <typeparamref name="T"/> in the game, regardless if it's active or not.
        /// </summary>
        /// <typeparam name="T">What to look for</typeparam>
        /// <exception cref="ComponentNotFoundException"></exception>
        /// <returns></returns>
        public static T FindFirst<T>() {
            object component;
            for(int i = 0; i < SceneManager.sceneCount; i++) {
                Scene scene = SceneManager.GetSceneAt(i);
                foreach(GameObject root in scene.GetRootGameObjects()) {
                    if(RecursiveFindFirst<T>(root.transform, out component)) {
                        return (T)component;
                    }
                }
            }
            throw new ComponentNotFoundException(typeof(T).GetTypeInfo());
        }
        /// <summary>
        /// Searches the given <see cref="Scene"/> for the first <see cref="Component"/> of type <typeparamref name="T"/>, regardless if it's active or not.
        /// </summary>
        /// <typeparam name="T">What to look for</typeparam>
        /// <param name="scene"> Where to look</param>
        /// <exception cref="ComponentNotFoundException"></exception>
        /// <returns></returns>
        public static T FindFirst<T>(Scene scene) {
            object component;
            foreach(GameObject root in scene.GetRootGameObjects()) {
                if(RecursiveFindFirst<T>(root.transform, out component)) {
                    return (T)component;
                }

            }
            throw new ComponentNotFoundException(typeof(T).GetTypeInfo());
        }
        /// <summary>
        /// Searches under the given <see cref="GameObject"/> for the first <see cref="Component"/> of type <typeparamref name="T"/>, regardless if it's active or not.
        /// </summary>
        /// <typeparam name="T">What to look for</typeparam>
        /// <param name="gameObject"> Where to look</param>
        /// <exception cref="ComponentNotFoundException"></exception>
        /// <returns></returns>
        public static T FindFirst<T>(GameObject gameObject) {
            object component;
            if(RecursiveFindFirst<T>(gameObject.transform, out component)) {
                return (T)component;
            }
            throw new ComponentNotFoundException(typeof(T).GetTypeInfo());
        }
        private static bool RecursiveFindFirst<T>(Transform transform, out object component) {
            component = transform.GetComponent<T>();
            if(component != null) {
                return true;
            } else if(transform.childCount != 0) {
                for(int i = 0; i < transform.childCount; i++) {
                    if(RecursiveFindFirst<T>(transform.GetChild(i), out component)) {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Finds all <see cref="Component"/>s of Type <typeparamref name="T"/>, regardless if active or not.
        /// </summary>
        /// <typeparam name="T">What to look for</typeparam>
        /// <exception cref="ComponentNotFoundException"></exception>
        /// <returns></returns>
        public static List<T> FindAll<T>() {
            List<T> components = new List<T>();
            for(int i = 0; i < SceneManager.sceneCount; i++) {
                Scene scene = SceneManager.GetSceneAt(i);
                foreach(GameObject root in scene.GetRootGameObjects()) {
                    RecursiveFindAll<T>(root.transform, ref components);
                }
            }
            if(components.Count == 0) {
                throw new ComponentNotFoundException(typeof(T).GetTypeInfo());
            }
            return components;
        }
        /// <summary>
        /// Finds all <see cref="Component"/>s of Type <typeparamref name="T"/> inside the specified <see cref="Scene"/>, regardless if active or not.
        /// </summary>
        /// <typeparam name="T">What to look for</typeparam>
        /// <param name="scene">Where to look</param>
        /// <exception cref="ComponentNotFoundException"></exception>
        /// <returns></returns>
        public static List<T> FindAll<T>(Scene scene) {
            List<T> components = new List<T>();
            foreach(GameObject root in scene.GetRootGameObjects()) {
                RecursiveFindAll<T>(root.transform, ref components);
            }
            if(components.Count == 0) {
                throw new ComponentNotFoundException(typeof(T).GetTypeInfo());
            }
            return components;
        }
        /// <summary>
        /// Finds all <see cref="Component"/>s of Type <typeparamref name="T"/> under the specified <see cref="GameObject"/>, regardless if active or not.
        /// </summary>
        /// <typeparam name="T">What Type to look for</typeparam>
        /// <param name="gameObject">Where to look</param>
        /// <exception cref="ComponentNotFoundException"></exception>
        /// <returns></returns>
        public static List<T> FindAll<T>(GameObject gameObject) {
            List<T> components = new List<T>();
            RecursiveFindAll<T>(gameObject.transform, ref components);
            if(components.Count == 0) {
                throw new ComponentNotFoundException(typeof(T).GetTypeInfo());
            }
            return components;
        }
        private static void RecursiveFindAll<T>(Transform transform, ref List<T> components) {
            components.AddRange(transform.GetComponents<T>());
            if(transform.childCount != 0) {
                for(int i = 0; i < transform.childCount; i++) {
                    RecursiveFindAll<T>(transform.GetChild(i), ref components);
                }
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
