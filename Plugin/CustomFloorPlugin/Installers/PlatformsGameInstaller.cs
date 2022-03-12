using Zenject;


namespace CustomFloorPlugin.Installers
{
    internal class PlatformsGameInstaller : Installer
    {
        private readonly PlatformSpawner _platformSpawner;
        private readonly ObstacleSaberSparkleEffectManager? _obstacleSaberSparkleEffectManager;
        private readonly MultiplayerPlayersManager? _multiplayerPlayersManager;

        public PlatformsGameInstaller(PlatformSpawner platformSpawner, [InjectOptional] PlayerSpaceConvertor? playerSpaceConvertor, [InjectOptional] MultiplayerPlayersManager? multiplayerPlayersManager)
        {
            _platformSpawner = platformSpawner;
            _multiplayerPlayersManager = multiplayerPlayersManager;
            if (playerSpaceConvertor is not null)
                _obstacleSaberSparkleEffectManager = playerSpaceConvertor.GetComponentInChildren<ObstacleSaberSparkleEffectManager>();
        }

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<BSEvents>().AsSingle();
            if (_obstacleSaberSparkleEffectManager is not null && !Container.HasBinding<ObstacleSaberSparkleEffectManager>())
                Container.BindInstance(_obstacleSaberSparkleEffectManager).AsSingle();
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