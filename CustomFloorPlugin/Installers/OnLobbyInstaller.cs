using Zenject;


namespace CustomFloorPlugin.Installers
{
    internal class OnLobbyInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PlatformLobbyHandler>().AsSingle().NonLazy();
        }
    }
}
