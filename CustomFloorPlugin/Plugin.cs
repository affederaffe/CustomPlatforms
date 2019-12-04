using UnityEngine;
using IPA;
using UnityEngine.SceneManagement;
using CustomFloorPlugin.Util;
using CustomUI.Utilities;
using Harmony;


namespace CustomFloorPlugin {
    public class Plugin:IBeatSaberPlugin {
        public static BS_Utils.Utilities.Config config;
        public static IPA.Logging.Logger logger;
        private bool init = false;


        public void Init(object thisWillBeNull, IPA.Logging.Logger logger) {
            Plugin.logger = logger;
        }

        public void OnApplicationStart() {
            
            //Instance = this;
            BSEvents.OnLoad();
            BSEvents.menuSceneLoadedFresh += OnMenuSceneLoadedFresh;
            
            HarmonyInstance hi = HarmonyInstance.Create("com.rolopogo.customplatforms");
            hi.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
        }
        [HarmonyPatch(typeof(LightWithId))]
        [HarmonyPatch("Start")]
        public class LightsWithId_Patch {
            static bool HasBeenRun = false;
            public static LightWithIdManager CorrectlySpelledManagerName = null;
            static public void Postfix(LightWithIdManager ____lighManager, LightWithId __instance) {
                if(HasBeenRun) return;
                //HasBeenRun = true;
                CorrectlySpelledManagerName = ____lighManager;
                if(CorrectlySpelledManagerName != null) {
                    System.Console.WriteLine("LightmanagerRefSet");
                }
            }
        }
        private void OnMenuSceneLoadedFresh() {
            if(!init) {
                init = true;
                config = new BS_Utils.Utilities.Config("Custom Platforms");
                PlatformManager.OnLoad();
            }

            //TubeLightManager.UpdateEventTubeLightList();

            LightWithId[] lights = Object.FindObjectsOfType<LightWithId>();
            LightWithId[] lights2 = Resources.FindObjectsOfTypeAll<LightWithId>();


            if(LightsWithId_Patch.CorrectlySpelledManagerName != null) {
                Traverse.Create(LightsWithId_Patch.CorrectlySpelledManagerName).SetPrivateField("_lights", new System.Collections.Generic.List<LightWithId>[21]);
            }
            foreach(LightWithId light in lights) {
                LightsWithId_Patch.CorrectlySpelledManagerName.RegisterLight(light);

            }
            foreach(LightWithId light in lights2) {
                LightsWithId_Patch.CorrectlySpelledManagerName.RegisterLight(light);
            }
        }

        public void OnApplicationQuit() {
            BSEvents.menuSceneLoadedFresh -= OnMenuSceneLoadedFresh;

        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode) {
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////
        void ToggleBlooms(Scene scene) {
            GameObject[] roots = scene.GetRootGameObjects();
            
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

            foreach(GameObject gameObject in roots) {
                RecursiveToggleBloomPrePass(gameObject);
            }
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