using Zenject;


namespace CustomFloorPlugin.Installers
{
    internal class PlatformsGameInstaller : Installer
    {
        public override void InstallBindings()
        {
            if (Container.HasBinding<GameplayCoreSceneSetupData>())
                Container.BindInterfacesAndSelfTo<BSEvents>().AsSingle();
            if (Container.HasBinding<MultiplayerPlayersManager>())
                Container.BindInterfacesTo<MultiplayerGameManager>().AsSingle();
        }
    }
}