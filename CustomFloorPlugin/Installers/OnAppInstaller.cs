using CustomFloorPlugin.Configuration;

using IPA.Logging;

using SiraUtil;

using Zenject;


namespace CustomFloorPlugin.Installers
{
    internal class OnAppInstaller : Installer
    {
        private readonly PluginConfig _config;
        private readonly Logger _logger;

        public OnAppInstaller(Logger logger, PluginConfig config)
        {
            _config = config;
            _logger = logger;
        }

        public override void InstallBindings()
        {
            Container.BindLoggerAsSiraLogger(_logger);
            Container.BindInstance(_config).AsSingle();
            Container.BindInterfacesAndSelfTo<PlatformLoader>().AsSingle();
            PlatformManager manager = Container.InstantiateComponentOnNewGameObject<PlatformManager>("CustomPlatforms");
            Container.BindInstance(manager);
            Container.BindInterfacesAndSelfTo<EnvironmentHider>().AsSingle();
        }
    }
}
