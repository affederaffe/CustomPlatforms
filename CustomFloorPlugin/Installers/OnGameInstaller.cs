using Zenject;


namespace CustomFloorPlugin.Installers {


    internal class OnGameInstaller : Installer {

        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<PlatformSpawner>().AsSingle().WithArguments(Container).NonLazy();
        }
    }
}