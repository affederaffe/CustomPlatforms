using Zenject;


namespace CustomFloorPlugin.Installers {


    internal class OnMultiplayerMenuInstaller : Installer {

        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<PlatformMultiplayerHandler>().AsSingle().NonLazy();
        }
    }
}
