using CustomFloorPlugin.Configuration;

using Zenject;


namespace CustomFloorPlugin.Installers {


    internal class OnAppInstaller : Installer {

        private readonly PluginConfig _config;

        public OnAppInstaller(PluginConfig config) {
            _config = config;
        }

        public override void InstallBindings() {
            Container.BindInstance(_config).AsSingle();
            Container.BindInterfacesAndSelfTo<PlatformLoader>().AsSingle();

            PlatformManager manager = Container.InstantiateComponentOnNewGameObject<PlatformManager>("CustomPlatforms");
            Container.BindInstance(manager);

            Container.BindInterfacesTo<API>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<EnvironmentHider>().AsSingle();
        }
    }
}
