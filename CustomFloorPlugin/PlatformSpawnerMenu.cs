using System;

using Zenject;


namespace CustomFloorPlugin {


    internal class PlatformSpawnerMenu : PlatformSpawner, IInitializable, IDisposable {

        internal PlatformSpawnerMenu(DiContainer container) {
            _container = container;
        }

        public void Initialize() {
            if (_config.ShowInMenu) {
                ChangeToPlatform(PlatformType.Singleplayer);
            }
        }

        public void Dispose() {
            ChangeToPlatform(0);
        }
    }
}
