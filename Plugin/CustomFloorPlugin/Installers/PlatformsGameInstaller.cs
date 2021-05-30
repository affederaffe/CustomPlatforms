using Zenject;


namespace CustomFloorPlugin.Installers
{
    internal class PlatformsGameInstaller : Installer
    {
        private readonly PlatformSpawner _platformSpawner;

        public PlatformsGameInstaller(PlatformSpawner platformSpawner)
        {
            _platformSpawner = platformSpawner;
        }

        public override void InstallBindings()
        {
            _platformSpawner.Container = Container;
            if (Container.HasBinding<GameplayCoreSceneSetupData>())
                Container.BindInterfacesAndSelfTo<BSEvents>().AsSingle();
        }
    }
}