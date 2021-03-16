using CustomFloorPlugin.Extensions;

using Zenject;


namespace CustomFloorPlugin.Installers
{
    internal class PlatformsGameInstaller : Installer
    {

        public override void InstallBindings()
        {
            if (Container.HasBinding<GameplayCoreSceneSetupData>())
            {
                GameplayCoreSceneSetupData sceneSetupData = Container.Resolve<GameplayCoreSceneSetupData>();
                float lastNoteTime = sceneSetupData.difficultyBeatmap.beatmapData.GetLastNoteTime();
                Container.BindInterfacesAndSelfTo<BSEvents>().AsSingle().WithArguments(lastNoteTime);
                if (sceneSetupData.environmentInfo.environmentName == "Multiplayer")
                {
                    Container.BindInterfacesTo<MultiplayerGameHelper>().AsSingle().NonLazy();
                }
            }
        }
    }
}