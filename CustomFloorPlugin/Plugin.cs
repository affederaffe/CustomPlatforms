using BS_Utils.Utilities;

using CustomFloorPlugin.HarmonyPatches;
using CustomFloorPlugin.UI;

using IPA;


namespace CustomFloorPlugin {


    /// <summary>
    /// Main Plugin executable, loaded and instantiated by BSIPA before the game starts<br/>
    /// Different callbacks will be notified throughout the games lifespan, and can be used as hooks.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1812:Avoid unistantiated internal classes", Justification = "Instantiated by BSIPA")]
    [Plugin(RuntimeOptions.SingleStartInit)]
    internal class Plugin {


        /// <summary>
        /// Initializes the Plugin, or in this case: Only the logger.
        /// </summary>
        /// <param name="logger">The instance of the IPA logger that BSIPA hands to plugins on initialization</param>
        [Init]
        public void Init(IPA.Logging.Logger logger) {
            Utilities.Logger.logger = logger;
        }


        /// <summary>
        /// Performs initialization<br/>
        /// [Called by BSIPA]
        /// </summary>
        [OnStart]
        public void OnApplicationStart() {
            BSEvents.OnLoad();
            BSEvents.menuSceneLoadedFresh += InitAfterLoad;
            Patcher.Patch();
            PlatformUI.SetupMenuButtons();
        }


        /// <summary>
        /// Performs initialization steps after the game has loaded into the main menu for the first time
        /// </summary>
        private void InitAfterLoad() {
            BSEvents.menuSceneLoadedFresh -= InitAfterLoad;
            PlatformManager.Init();
        }
    }
}