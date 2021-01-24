using Zenject;


namespace CustomFloorPlugin.Installers {


    internal class OnMultiplayerMenuInstaller : Installer {

        public override void InstallBindings() {
            Container.InstantiateComponentOnNewGameObject<PlatformMultiplayerHandler>("CustomPlatforms MultiplayerHandler");
        }
    }
}
