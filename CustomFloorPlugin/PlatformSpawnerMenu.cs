using System;

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
            try {
                // On game restart, Unity will throw a NullReferenceException in UnityEngine.Object.get_name () and I have no idea why
                ChangeToPlatform(0);
            }
            catch (NullReferenceException e) {
                Utilities.Logging.Log(e);
            }
        }
    }
}
