using CustomFloorPlugin.Configuration;

using IPA.Logging;

using SiraUtil;

using Zenject;


namespace CustomFloorPlugin.Installers {


    internal class OnAppInstaller : Installer {

        private readonly PluginConfig _config;
        private readonly Logger _logger;

        public OnAppInstaller(PluginConfig config) {
            _config = config;
        }

        public override void InstallBindings() {
            Container.BindInstance(_config).AsSingle();
            Container.BindLoggerAsSiraLogger(_logger);
            Container.BindInterfacesAndSelfTo<EnvironmentHider>().AsSingle();

            PlatformManager manager = Container.InstantiateComponentOnNewGameObject<PlatformManager>("CustomPlatforms");
            Container.BindInstance(manager);
        }
    }
}
