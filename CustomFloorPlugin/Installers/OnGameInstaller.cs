using UnityEngine;

using Zenject;


namespace CustomFloorPlugin.Installers {


    internal class OnGameInstaller : Installer {

        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<PlatformSpawnerGame>().AsSingle().WithArguments(Container).NonLazy();

            ColorScheme scheme = Container.Resolve<PlayerDataModel>().playerData.colorSchemesSettings.GetSelectedColorScheme();
            Color?[] colors = new Color?[] {
                    scheme.environmentColor0,
                    scheme.environmentColor1,
                    scheme.obstaclesColor,
                    scheme.saberAColor,
                    scheme.saberBColor,
                };
            Container.Bind<Color?[]>().FromInstance(colors).AsSingle();
        }
    }
}