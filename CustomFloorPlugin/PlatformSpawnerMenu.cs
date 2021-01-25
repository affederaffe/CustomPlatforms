using Zenject;


namespace CustomFloorPlugin {


    internal class PlatformSpawnerMenu : PlatformSpawner, IInitializable {

        [Inject]
        private readonly GameScenesManager _gameScenesManager;

        internal PlatformSpawnerMenu(DiContainer container) {
            _container = container;
        }

        public void Initialize() {
            _gameScenesManager.transitionDidStartEvent += HandleTransitionDidStart;
            if (_config.ShowInMenu) {
                ChangeToPlatform();
            }
        }

        private void HandleTransitionDidStart(float _1) {
            ChangeToPlatform(0);
        }
    }
}
