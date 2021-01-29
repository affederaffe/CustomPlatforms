using CustomFloorPlugin.Extensions;

using Zenject;


namespace CustomFloorPlugin.Installers {


    internal class OnGameInstaller : Installer {

        public override void InstallBindings() {
            if (Container.HasBinding<GameplayCoreSceneSetupData>()) {
                GameplayCoreSceneSetupData sceneSetupData = Container.Resolve<GameplayCoreSceneSetupData>();
                float lastNoteTime = sceneSetupData.difficultyBeatmap.beatmapData.GetLastNoteTime();
                Container.Bind<float>().WithId("LastNoteId").FromInstance(lastNoteTime);
                Container.BindInterfacesAndSelfTo<BSEvents>().AsSingle();
            }

            Container.BindInterfacesAndSelfTo<PlatformSpawnerGame>().AsSingle().WithArguments(Container).NonLazy();
        }
    }
}