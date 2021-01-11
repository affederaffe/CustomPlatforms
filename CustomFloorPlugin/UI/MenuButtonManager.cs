using System;

using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using BeatSaberMarkupLanguage.Settings;

using Zenject;


namespace CustomFloorPlugin.UI {


    /// <summary>
    /// UI Class, sets up MenuButton and Settings Section
    /// </summary>
    internal class MenuButtonManager : IInitializable, IDisposable {

        private readonly MenuButton _menuButton;
        private readonly PlatformListFlowCoordinator _platformListFlowCoordinator;
        private readonly Settings _settings;

        public MenuButtonManager(PlatformListFlowCoordinator platformListFlowCoordinator, Settings settings) {
            _platformListFlowCoordinator = platformListFlowCoordinator;
            _menuButton = new MenuButton("Custom Platforms", "Change your Platform here!", SummonFlowCoordinator);
            _settings = settings;
        }

        public void Initialize() {
            MenuButtons.instance.RegisterButton(_menuButton);
            BSMLSettings.instance.AddSettingsMenu("Custom Platforms", "CustomFloorPlugin.Views.Settings.bsml", _settings);
        }

        public void Dispose() {
            MenuButtons.instance.UnregisterButton(_menuButton);
            BSMLSettings.instance.RemoveSettingsMenu(_settings);
        }

        private void SummonFlowCoordinator() {
            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(_platformListFlowCoordinator);
        }
    }
}