using Zenject;


namespace CustomFloorPlugin {


    internal class PlatformSpawnerMenu : PlatformSpawner, IInitializable {

        internal PlatformSpawnerMenu(DiContainer container) {
            _container = container;
        }

        public void Initialize() {
            if (_config.ShowInMenu) {
                ChangeToPlatform(PlatformType.Singleplayer);
            }
        }
    }
}
