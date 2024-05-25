using System.Linq;
using System.Reflection;

using CustomFloorPlugin.Configuration;

using JetBrains.Annotations;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin.Installers
{
    [UsedImplicitly]
    internal class PlatformsAppInstaller : Installer
    {
        private readonly Assembly _assembly;
        private readonly PluginConfig _config;
        private readonly MirrorRendererSO _mirrorRenderer;
        private readonly BloomPrePassRendererSO _bloomPrePassRenderer;
        private readonly BloomPrePassEffectContainerSO _bloomPrePassEffectContainer;
        private readonly BoolSO _postProcessEnabled;

        public PlatformsAppInstaller(Assembly assembly, PluginConfig config)
        {
            _assembly = assembly;
            _config = config;
            _mirrorRenderer = Resources.FindObjectsOfTypeAll<MirrorRendererSO>().FirstOrDefault();
            _bloomPrePassRenderer = _mirrorRenderer._bloomPrePassRenderer;
            _bloomPrePassEffectContainer = Resources.FindObjectsOfTypeAll<BloomPrePassEffectContainerSO>().FirstOrDefault();
            _postProcessEnabled = Resources.FindObjectsOfTypeAll<MainEffectContainerSO>().FirstOrDefault()._postProcessEnabled;
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
