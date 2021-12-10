using Zenject;


namespace CustomFloorPlugin.Installers
{
    internal class PlatformsGameInstaller : Installer
    {
        private readonly PlatformSpawner _platformSpawner;
        private readonly ObstacleSaberSparkleEffectManager _obstacleSaberSparkleEffectManager;
        private readonly MultiplayerPlayersManager? _multiplayerPlayersManager;

        public PlatformsGameInstaller(PlatformSpawner platformSpawner, PlayerSpaceConvertor playerSpaceConvertor, [InjectOptional] MultiplayerPlayersManager multiplayerPlayersManager)
        {
            _platformSpawner = platformSpawner;
            _obstacleSaberSparkleEffectManager = playerSpaceConvertor.GetComponentInChildren<ObstacleSaberSparkleEffectManager>();
            _multiplayerPlayersManager = multiplayerPlayersManager;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_obstacleSaberSparkleEffectManager).AsSingle().IfNotBound();
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