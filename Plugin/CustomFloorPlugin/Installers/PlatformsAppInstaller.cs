using CustomFloorPlugin.Configuration;

using IPA.Logging;

using SiraUtil;

using Zenject;


namespace CustomFloorPlugin.Installers
{
    internal class PlatformsAppInstaller : Installer
    {
        private readonly Logger _logger;
        private readonly PluginConfig _config;

        public PlatformsAppInstaller(Logger logger, PluginConfig config)
        {
            _config = config;
            _logger = logger;
        }

        public override void InstallBindings()
        {
            Container.BindLoggerAsSiraLogger(_logger);
            Container.BindInstance(_config).AsSingle();
            Container.BindInterfacesAndSelfTo<MaterialSwapper>().AsSingle();
            Container.BindInterfacesAndSelfTo<AssetLoader>().AsSingle();
            Container.Bind<PlatformLoader>().AsSingle();
            PlatformManager manager = Container.InstantiateComponentOnNewGameObject<PlatformManager>("CustomPlatforms");
            Container.BindInstance(manager).AsSingle();
            Container.Bind<EnvironmentHider>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlatformSpawner>().AsSingle();
        }
    }
}