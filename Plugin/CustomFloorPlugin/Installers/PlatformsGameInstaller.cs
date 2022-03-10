using Zenject;


namespace CustomFloorPlugin.Installers
{
    internal class PlatformsGameInstaller : Installer
    {
        private readonly PlatformSpawner _platformSpawner;
        private readonly MultiplayerPlayersManager? _multiplayerPlayersManager;

        public PlatformsGameInstaller(PlatformSpawner platformSpawner, [InjectOptional] MultiplayerPlayersManager? multiplayerPlayersManager)
        {
            _platformSpawner = platformSpawner;
            _multiplayerPlayersManager = multiplayerPlayersManager;
        }

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<BSEvents>().AsSingle();
            if (_multiplayerPlayersManager is not null)
                _multiplayerPlayersManager.playerSpawningDidFinishEvent += OnPlayerSpawningDidFinish;
        }

        private void OnPlayerSpawningDidFinish()
        {
            _multiplayerPlayersManager!.playerSpawningDidFinishEvent -= OnPlayerSpawningDidFinish;
            _platformSpawner.OnTransitionDidFinish(Container.Resolve<MultiplayerLevelScenesTransitionSetupDataSO>(), Container);
        }
    }
}