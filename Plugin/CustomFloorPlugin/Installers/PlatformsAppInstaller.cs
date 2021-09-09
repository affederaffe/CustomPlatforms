using System.Collections.Generic;
using System.Linq;

using CustomFloorPlugin.Configuration;

using IPA.Loader;
using IPA.Logging;
using IPA.Utilities;

using SiraUtil;

using Zenject;


namespace CustomFloorPlugin.Installers
{
    internal class PlatformsAppInstaller : Installer
    {
        private readonly PluginMetadata _pluginMetadata;
        private readonly Logger _logger;
        private readonly PluginConfig _config;
        private readonly MirrorRendererSO _mirrorRenderer;
        private readonly BoolSO _postProcessEnabled;

        public PlatformsAppInstaller(PluginMetadata pluginMetadata, Logger logger, PluginConfig config, SceneContext sceneContext)
        {
            _pluginMetadata = pluginMetadata;
            _config = config;
            _logger = logger;
            MainSystemInit mainSystemInit = ((PCAppInit)sceneContext
                .GetField<List<MonoInstaller>, Context>("_monoInstallers")
                .First(x => x is PCAppInit))
                .GetField<MainSystemInit, PCAppInit>("_mainSystemInit");
            _mirrorRenderer = mainSystemInit.GetField<MirrorRendererSO, MainSystemInit>("_mirrorRenderer");
            MainEffectContainerSO mainEffectContainer = mainSystemInit.GetField<MainEffectContainerSO, MainSystemInit>("_mainEffectContainer");
            _postProcessEnabled = mainEffectContainer.GetField<BoolSO, MainEffectContainerSO>("_postProcessEnabled");
        }

        public override void InstallBindings()
        {
            Container.BindLoggerAsSiraLogger(_logger);
            Container.BindInstance(_config).AsSingle();
            Container.BindInstance(_mirrorRenderer).AsSingle().IfNotBound();
            Container.BindInstance(_postProcessEnabled).WithId("PostProcessEnabled").AsSingle().IfNotBound();
            Container.BindInterfacesAndSelfTo<MaterialSwapper>().AsSingle();
            Container.Bind<AssetLoader>().AsSingle().WithArguments(_pluginMetadata);
            Container.Bind<PlatformLoader>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlatformManager>().AsSingle();
            Container.Bind<EnvironmentHider>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlatformSpawner>().AsSingle();
            Container.BindInterfacesTo<ConnectionManager>().AsSingle();
        }
    }
}