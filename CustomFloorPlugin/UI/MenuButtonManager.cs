using System;

using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;

using Zenject;


namespace CustomFloorPlugin.UI {


    /// <summary>
    /// UI Class, sets up MenuButton and Settings Section
    /// </summary>
    internal class MenuButtonManager : IInitializable, IDisposable {

        private readonly MenuButton _menuButton;
        private readonly PlatformListFlowCoordinator _platformListFlowCoordinator;

        public MenuButtonManager(PlatformListFlowCoordinator platformListFlowCoordinator) {
            _platformListFlowCoordinator = platformListFlowCoordinator;
            _menuButton = new MenuButton("Custom Platforms", "Change your Platform here!", SummonFlowCoordinator);
        }

        public void Initialize() {
            MenuButtons.instance.RegisterButton(_menuButton);
        }

        public void Dispose() {
            MenuButtons.instance.UnregisterButton(_menuButton);
        }

        private void SummonFlowCoordinator() {
            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(_platformListFlowCoordinator);
        }
    }
}