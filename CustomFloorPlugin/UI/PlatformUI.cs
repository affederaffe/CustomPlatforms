using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using BeatSaberMarkupLanguage.Settings;
using HMUI;

namespace CustomFloorPlugin.UI {


    /// <summary>
    /// Static, multifunctional, UI Class. Holds references to UI elements and provides UI relevant functions.
    /// </summary>
    internal static class PlatformUI {


        /// <summary>
        /// Static reference to the <see cref="PlatformListFlowCoordinator"/> singleton
        /// </summary>
        private static PlatformListFlowCoordinator PlatformMenuFlowCoordinator {
            get {
                if (_PlatformMenuFlowCoordinator == null) {
                    _PlatformMenuFlowCoordinator = BeatSaberUI.CreateFlowCoordinator<PlatformListFlowCoordinator>();
                }
                return _PlatformMenuFlowCoordinator;
            }
        }
        private static PlatformListFlowCoordinator _PlatformMenuFlowCoordinator;


        /// <summary>
        /// Used to make sure setup is only performed once
        /// </summary>
        private static bool runOnce = false;


        /// <summary>
        /// Sets up the UI
        /// </summary>
        internal static void SetupMenuButtons() {
            if (!runOnce) {
                runOnce = true;
                MenuButtons.instance.RegisterButton(new MenuButton("Custom Platforms", "Change Custom Platforms Here!", CustomPlatformsMenuButtonPressed, true));
                BSMLSettings.instance.AddSettingsMenu("Custom Platforms", "CustomFloorPlugin.UI.Settings.bsml", Settings.instance);
            }
        }


        /// <summary>
        /// Transitions to the CustomPlatforms selection menu
        /// </summary>
        private static void CustomPlatformsMenuButtonPressed() {
            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(PlatformMenuFlowCoordinator, null, ViewController.AnimationDirection.Horizontal, true, false);
        }
    }

}