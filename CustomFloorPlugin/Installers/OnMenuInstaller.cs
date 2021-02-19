using CustomFloorPlugin.UI;

using SiraUtil;

using Zenject;


namespace CustomFloorPlugin.Installers
{
    internal class OnMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PlatformSpawnerMenu>().AsSingle().WithArguments(Container).NonLazy();
            Container.Bind<PlatformListsView>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<ChangelogView>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<SettingsView>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<PlatformListFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesTo<MenuButtonManager>().AsSingle();
            Container.BindInterfacesTo<API>().AsSingle().NonLazy();
        }
    }
}
