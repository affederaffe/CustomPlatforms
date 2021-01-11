using BS_Utils.Utilities;

using IPA;
using IPA.Config.Stores;

using SiraUtil.Zenject;

using CustomFloorPlugin.HarmonyPatches;
using CustomFloorPlugin.Installers;


namespace CustomFloorPlugin {


    /// <summary>
    /// Main Plugin executable, loaded and instantiated by BSIPA before the game starts<br/>
    /// Different callbacks will be notified throughout the games lifespan, and can be used as hooks.
    /// </summary>
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

        [Init]
        public void InitWithConfig(IPA.Config.Config conf) {
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
        }

        [Init]
        public void InitWithZenjector(Zenjector zenjector) {
            zenjector.OnMenu<OnMenuInstaller>();
            zenjector.OnGame<OnGameInstaller>();
        }


        /// <summary>
        /// Performs initialization<br/>
        /// [Called by BSIPA]
        /// </summary>
        [OnStart]
        public void OnApplicationStart() {
            BSEvents.OnLoad();
            BSEvents.lateMenuSceneLoadedFresh += InitAfterLoad;
            Patcher.Patch();
        }

        [OnExit]
        public void OnApplicationExit() {
            // Yes it is intentional ._.
        }


        /// <summary>
        /// Performs initialization steps after the game has loaded into the main menu for the first time
        /// </summary>
        private void InitAfterLoad(ScenesTransitionSetupDataSO ignored) {
            BSEvents.lateMenuSceneLoadedFresh -= InitAfterLoad;
            PlatformManager.Init();
        }
    }
}