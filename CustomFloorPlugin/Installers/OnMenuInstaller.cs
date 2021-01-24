using CustomFloorPlugin.UI;

using SiraUtil;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin.Installers {


    internal class OnMenuInstaller : Installer {


        public override void InstallBindings() {

            Container.BindInterfacesAndSelfTo<PlatformSpawner>().AsSingle().WithArguments(Container).NonLazy();

            Container.Bind<PlatformsListView>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<SettingsView>().FromNewComponentAsViewController().AsSingle();
            Container.BindFlowCoordinator<PlatformListFlowCoordinator>();
            Container.BindInterfacesTo<MenuButtonManager>().AsSingle();

            ColorScheme scheme = Container.Resolve<PlayerDataModel>().playerData.colorSchemesSettings.GetSelectedColorScheme();
            Color?[] colors = GetColors(scheme);
            Container.Bind<Color?[]>().FromInstance(colors).AsSingle();
        }

        private Color?[] GetColors(ColorScheme scheme) {
            Color?[] colors = new Color?[] {
                    scheme.environmentColor0,
                    scheme.environmentColor1,
                    scheme.obstaclesColor,
                    scheme.saberAColor,
                    scheme.saberBColor,
                    scheme.environmentColor0,
                    scheme.environmentColor1,
                    scheme.obstaclesColor,
                    scheme.saberAColor,
                    scheme.saberBColor,
                    scheme.environmentColor0,
                    scheme.environmentColor1,
                    scheme.obstaclesColor,
                    scheme.saberAColor,
                    scheme.saberBColor,
                    scheme.environmentColor0,
                    scheme.environmentColor1,
                    scheme.obstaclesColor,
                    scheme.saberAColor,
                    scheme.saberBColor,
                };
            return colors;
        }
    }
}
