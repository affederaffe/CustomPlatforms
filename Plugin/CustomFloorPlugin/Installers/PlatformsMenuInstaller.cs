using CustomFloorPlugin.UI;

using Zenject;


namespace CustomFloorPlugin.Installers
{
    internal class PlatformsMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<PlatformListsView>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<PlatformsFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesTo<MenuButtonManager>().AsSingle();
        }
    }
}