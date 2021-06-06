using System.Collections.Generic;
using System.Linq;

using CustomFloorPlugin.Configuration;

using IPA.Logging;
using IPA.Utilities;

using SiraUtil;

using Zenject;


namespace CustomFloorPlugin.Installers
{
    internal class PlatformsAppInstaller : Installer
    {
        private readonly Logger _logger;
        private readonly PluginConfig _config;
        private readonly MainSystemInit _mainSystemInit;

        public PlatformsAppInstaller(Logger logger, PluginConfig config, SceneContext sceneContext)
        {
            _config = config;
            _logger = logger;
            _mainSystemInit = ((PCAppInit)sceneContext
                .GetField<List<MonoInstaller>, Context>("_monoInstallers")
                .First(x => x is PCAppInit))
                .GetField<MainSystemInit, PCAppInit>("_mainSystemInit");
        }

        public override void InstallBindings()
        {
            Container.BindLoggerAsSiraLogger(_logger);
            Container.BindInstance(_config).AsSingle();
            Container.BindInstance(_mainSystemInit.GetField<MirrorRendererSO, MainSystemInit>("_mirrorRenderer")).AsSingle();
            Container.BindInterfacesAndSelfTo<MaterialSwapper>().AsSingle();
            Container.Bind<AssetLoader>().AsSingle();
            Container.Bind<PlatformLoader>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlatformManager>().AsSingle();
            Container.Bind<EnvironmentHider>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlatformSpawner>().AsSingle();
            Container.BindInterfacesTo<InteractionManager>().AsSingle();
        }
    }
}