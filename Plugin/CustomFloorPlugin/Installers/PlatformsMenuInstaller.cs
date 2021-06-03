using CustomFloorPlugin.UI;

using SiraUtil;

using Zenject;


namespace CustomFloorPlugin.Installers
{
    internal class PlatformsMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<PlatformListsView>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<ChangelogView>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<SettingsView>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<PlatformsFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesTo<MenuButtonManager>().AsSingle();
        }
    }
}