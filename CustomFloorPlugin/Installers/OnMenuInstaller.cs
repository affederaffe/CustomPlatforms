using SiraUtil;

using Zenject;

using CustomFloorPlugin.UI;


namespace CustomFloorPlugin.Installers {


    internal class OnMenuInstaller : Installer {


        public override void InstallBindings() {

            Container.Bind<EnvironmentOverrideListView>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<ReloadPlatformsButtonView>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<PlatformsListView>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<NewScriptWarningView>().FromNewComponentAsViewController().AsSingle();
            Container.BindFlowCoordinator<PlatformListFlowCoordinator>();
            Container.BindFlowCoordinator<NewScriptWarningFlowCoordinator>();

            Container.BindInterfacesAndSelfTo<Settings>().AsSingle();
            Container.BindInterfacesTo<MenuButtonManager>().AsSingle();

            HarmonyPatches.NewScriptWarning_Patch._newScriptWarningFlowCoordinator = Container.Resolve<NewScriptWarningFlowCoordinator>();
            GlobalCollection.PDM = Container.Resolve<PlayerDataModel>();
        }
    }
}
