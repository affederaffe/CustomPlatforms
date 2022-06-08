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

        private static readonly FieldAccessor<PCAppInit, MainSystemInit>.Accessor _pcAppInitAccessor = FieldAccessor<PCAppInit, MainSystemInit>.GetAccessor("_mainSystemInit");
        private static readonly FieldAccessor<MainSystemInit, MirrorRendererSO>.Accessor _mirrorRendererAccessor = FieldAccessor<MainSystemInit, MirrorRendererSO>.GetAccessor("_mirrorRenderer");
        private static readonly FieldAccessor<MirrorRendererSO, BloomPrePassRendererSO>.Accessor _bloomPrePassRendererAccessor = FieldAccessor<MirrorRendererSO, BloomPrePassRendererSO>.GetAccessor("_bloomPrePassRenderer");
        private static readonly FieldAccessor<MainSystemInit, BloomPrePassEffectContainerSO>.Accessor _bloomPrePassEffectContainerAccessor = FieldAccessor<MainSystemInit, BloomPrePassEffectContainerSO>.GetAccessor("_bloomPrePassEffectContainer");
        private static readonly FieldAccessor<MainSystemInit, MainEffectContainerSO>.Accessor _mainEffectContainerAccessor = FieldAccessor<MainSystemInit, MainEffectContainerSO>.GetAccessor("_mainEffectContainer");
        private static readonly FieldAccessor<MainEffectContainerSO, BoolSO>.Accessor _postProcessEnabledAccessor = FieldAccessor<MainEffectContainerSO, BoolSO>.GetAccessor("_postProcessEnabled");

        public PlatformsAppInstaller(Assembly assembly, PluginConfig config, SceneContext sceneContext)
        {
            _assembly = assembly;
            _config = config;
            PCAppInit? pcAppInit = sceneContext.Installers.First(static x => x is PCAppInit) as PCAppInit;
            MainSystemInit mainSystemInit = _pcAppInitAccessor(ref pcAppInit!);
            _mirrorRenderer = _mirrorRendererAccessor(ref mainSystemInit);
            _bloomPrePassRenderer = _bloomPrePassRendererAccessor(ref _mirrorRenderer);
            _bloomPrePassEffectContainer = _bloomPrePassEffectContainerAccessor(ref mainSystemInit);
            MainEffectContainerSO mainEffectContainer = _mainEffectContainerAccessor(ref mainSystemInit);
            _postProcessEnabled = _postProcessEnabledAccessor(ref mainEffectContainer);
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