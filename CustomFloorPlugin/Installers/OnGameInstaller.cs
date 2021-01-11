using Zenject;

namespace CustomFloorPlugin.Installers {


    internal class OnGameInstaller : Installer {

        public override void InstallBindings() {
            //GlobalCollection.BOCC = Container.Resolve<BeatmapObjectCallbackController>();
        }
    }
}
