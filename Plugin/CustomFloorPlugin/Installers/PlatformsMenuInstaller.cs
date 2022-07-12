using CustomFloorPlugin.UI;

using JetBrains.Annotations;

using Zenject;


namespace CustomFloorPlugin.Installers
{
    [UsedImplicitly]
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
