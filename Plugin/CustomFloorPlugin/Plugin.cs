using CustomFloorPlugin.Configuration;
using CustomFloorPlugin.Installers;

using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Logging;

using SiraUtil.Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Main Plugin executable, loaded and instantiated by BSIPA before the game starts<br/>
    /// Different callbacks will be notified throughout the games lifespan, and can be used as hooks.
    /// </summary>
    [Plugin(RuntimeOptions.DynamicInit)]
    // ReSharper disable once UnusedType.Global
    public class Plugin
    {
        /// <summary>
        /// Initializes the Plugin 
        /// </summary>
        /// <param name="logger">The instance of the IPA logger that BSIPA hands to plugins on initialization</param>
        /// <param name="config">The config BSIPA provides</param>
        /// <param name="zenjector">The zenjector that SiraUtil passes to this plugin</param>
        [Init]
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once CA1822
        public void Init(Logger logger, Config config, Zenjector zenjector)
        {
            zenjector.OnApp<PlatformsAppInstaller>().WithParameters(logger, config.Generated<PluginConfig>());
            zenjector.OnMenu<PlatformsMenuInstaller>();
            zenjector.OnGame<PlatformsGameInstaller>(false);
        }
    }
}