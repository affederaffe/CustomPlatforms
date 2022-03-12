using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CustomFloorPlugin.Configuration;

using IPA.Utilities;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin.Installers
{
    internal class PlatformsAppInstaller : Installer
    {
        private readonly Assembly _assembly;
        private readonly PluginConfig _config;
        private readonly MirrorRendererSO _mirrorRenderer;
        private readonly BloomPrePassRendererSO _bloomPrePassRenderer;
        private readonly BloomPrePassEffectContainerSO _bloomPrePassEffectContainer;
        private readonly BoolSO _postProcessEnabled;

        public PlatformsAppInstaller(Assembly assembly, PluginConfig config, SceneContext sceneContext)
        {
            _assembly = assembly;
            _config = config;
            MainSystemInit mainSystemInit = ((PCAppInit)sceneContext
                .GetField<List<MonoInstaller>, Context>("_monoInstallers")
                .First(static x => x is PCAppInit))
                .GetField<MainSystemInit, PCAppInit>("_mainSystemInit");
            _mirrorRenderer = mainSystemInit.GetField<MirrorRendererSO, MainSystemInit>("_mirrorRenderer");
            _bloomPrePassRenderer = _mirrorRenderer.GetField<BloomPrePassRendererSO, MirrorRendererSO>("_bloomPrePassRenderer");
            _bloomPrePassEffectContainer = mainSystemInit.GetField<BloomPrePassEffectContainerSO, MainSystemInit>("_bloomPrePassEffectContainer");
            MainEffectContainerSO mainEffectContainer = mainSystemInit.GetField<MainEffectContainerSO, MainSystemInit>("_mainEffectContainer");
            _postProcessEnabled = mainEffectContainer.GetField<BoolSO, MainEffectContainerSO>("_postProcessEnabled");
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_config).AsSingle();
            Container.BindInstance(_mirrorRenderer).AsSingle().IfNotBound();
            Container.BindInstance(_bloomPrePassRenderer).AsSingle().IfNotBound();
            Container.BindInstance(_bloomPrePassEffectContainer).AsSingle().IfNotBound();
            Container.BindInstance(_postProcessEnabled).WithId("PostProcessEnabled").AsSingle().IfNotBound();
            Container.BindInstance(new GameObject("CustomPlatforms").transform).WithId("CustomPlatforms").AsSingle();
            Container.Bind<MaterialSwapper>().AsSingle();
            Container.Bind<AssetLoader>().AsSingle().WithArguments(_assembly);
            Container.Bind<PlatformLoader>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlatformManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<EnvironmentHider>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlatformSpawner>().AsSingle();
            Container.BindInterfacesTo<ConnectionManager>().AsSingle();
        }
    }
}