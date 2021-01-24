using CustomFloorPlugin.HarmonyPatches;
using CustomFloorPlugin.Installers;

using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Logging;

using SiraUtil.Zenject;


namespace CustomFloorPlugin {


    /// <summary>
    /// Main Plugin executable, loaded and instantiated by BSIPA before the game starts<br/>
    /// Different callbacks will be notified throughout the games lifespan, and can be used as hooks.
    /// </summary>
    [Plugin(RuntimeOptions.SingleStartInit)]
    internal class Plugin {


        /// <summary>
        /// Initializes the Plugin and everything about it
        /// </summary>
        /// <param name="logger">The instance of the IPA logger that BSIPA hands to plugins on initialization</param>
        /// <param name="config">The config BSIPA provides</param>
        /// <param name="zenjector">The holy zenjector that SiraUtil passes to this plugin</param>
        [Init]
        public void Init(Logger logger, Config config, Zenjector zenjector) {
            Utilities.Logger.logger = logger;
            zenjector.OnApp<OnAppInstaller>().WithParameters(config.Generated<Configuration.PluginConfig>());
            zenjector.OnMenu<OnMenuInstaller>();
            zenjector.OnMenu<OnMultiplayerMenuInstaller>().OnlyForMultiplayer();
            zenjector.OnGame<OnGameInstaller>();
        }


        /// <summary>
        /// Performs initialization<br/>
        /// [Called by BSIPA]
        /// </summary>
        [OnStart]
        public void OnApplicationStart() {
            Patcher.Patch();
        }

        /// <summary>
        /// Performs teardown<br/>
        /// [Called by BSIPA]
        /// </summary>
        [OnExit]
        public void OnApplicationExit() {
            Patcher.Unpatch();
        }
    }
}