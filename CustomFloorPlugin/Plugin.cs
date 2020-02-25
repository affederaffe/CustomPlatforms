using BS_Utils.Utilities;
using CustomFloorPlugin.Exceptions;
using CustomFloorPlugin.UI;
using CustomFloorPlugin.Utilities;
using CustomFloorPlugin.HarmonyPatches;
using IPA;
using System.Linq;
using UnityEngine.SceneManagement;
using static CustomFloorPlugin.Utilities.UnityObjectSearching;
using static CustomFloorPlugin.Utilities.Logging;

namespace CustomFloorPlugin {

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1812:Avoid unistantiated internal classes", Justification = "Instantiated by BSIPA")]
    internal class Plugin:IBeatSaberPlugin {
        internal static Config config = new Config("Custom Platforms");
        internal static GameScenesManager Gsm {
            get {
                if(_Gsm == null) {
                    _Gsm = SceneManager.GetSceneByName("PCInit").GetRootGameObjects().First(x => x.name == "AppCoreSceneContext")?.GetComponent<MarkSceneAsPersistent>().GetPrivateField<GameScenesManager>("_gameScenesManager");
                }
                return _Gsm;
            }
        }
        private static GameScenesManager _Gsm;

        /// <summary>
        /// Do not call this out of gamescene, or else.
        /// </summary>
        internal static BeatmapObjectCallbackController Bocc {
            get {
                if(_Bocc == null) {
                    try {
                        _Bocc = FindFirst<BeatmapObjectCallbackController>();
                    } catch(ComponentNotFoundException e) {
                        Log("Tried Referencing BOCC out of context, returning null!");
                        Log(e);
                    }

                }
                return _Bocc;
            }
        }
        private static BeatmapObjectCallbackController _Bocc;

        public void Init(IPA.Logging.Logger logger) {
            Logging.logger = logger;
        }

        public void OnApplicationStart() {
            BSEvents.OnLoad();
            BSEvents.menuSceneLoadedFresh += InitAfterLoad;
            Patcher.Patch();


            Stuff_That_Doesnt_Belong_Here_But_Has_To_Be_Here_Because_Bsml_Isnt_Half_As_Stable_Yet_As_CustomUI_Was_But_CustomUI_Has_Been_Killed_Already();
        }

        /// <summary>
        /// Holds any clutter that's not supposed to be here, but has to.<br/>
        /// WARNING:<br/>
        /// NOTHING IS READY TO USE AT THIS POINT IN TIME!<br/>
        /// ABSOLUTELY NOTHING!<br/>
        /// BECAUSE NOTHING HAS LOADED IN YET!<br/>
        /// THAT'S WHY NOTHING SHOULD BE HERE!<br/>
        /// EXSPECIALLY NOT THIS!<br/>
        /// DON'T PUT ANY LOADING-SENSITIVE LOGIC HERE!<br/>
        /// <br/>
        /// YES, I AM SERIOUS!
        /// </summary>
        private void Stuff_That_Doesnt_Belong_Here_But_Has_To_Be_Here_Because_Bsml_Isnt_Half_As_Stable_Yet_As_CustomUI_Was_But_CustomUI_Has_Been_Killed_Already() {
            PlatformUI.SetupMenuButtons();
        }

        private void InitAfterLoad() {
            BSEvents.menuSceneLoadedFresh -= InitAfterLoad;
            PlatformManager.Instance.Load();
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
