using HMUI;
using UnityEngine;
using CustomFloorPlugin.Util;
using BeatSaberMarkupLanguage.MenuButtons;
using BeatSaberMarkupLanguage;
using System.Linq;

namespace CustomFloorPlugin
{
    internal class SettingsUI
    {
        public static PlatformsFlowCoordinator platformsFlowCoordinator;

        public static bool created = false;

        public static void CreateMenu()
        {
            if (!created)
            {
                MenuButton menuButton = new MenuButton("Custom Platform", "Change Custom Platforms Here!", ShowPlatformFlow, true);
                MenuButtons.instance.RegisterButton(menuButton);

                created = true;
            }
        }

        public static void ShowPlatformFlow()
        {
            if (platformsFlowCoordinator == null)
            {
                platformsFlowCoordinator = BeatSaberUI.CreateFlowCoordinator<PlatformsFlowCoordinator>();
            }

            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(platformsFlowCoordinator, null, false, false);
        }
    }
    /*class PlatformUI : MonoBehaviour
    {   
        public static PlatformUI _instance;
                
        public CustomMenu _platformMenu;


    /// <summary>
    /// Static, multifunctional, UI Class. Holds references to UI elements and provides UI relevant functions.
    /// </summary>
    internal static class PlatformUI {


        /// <summary>
        /// Static reference to the <see cref="PlatformListFlowCoordinator"/> singleton
        /// </summary>
        private static PlatformListFlowCoordinator PlatformMenuFlowCoordinator {
            get {
                if(_PlatformMenuFlowCoordinator == null) {
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
            if(!runOnce) {
                runOnce = true;
                MenuButtons.instance.RegisterButton(new MenuButton("Custom Platforms", "Change Custom Plaforms Here!", CustomPlatformsMenuButtonPressed, true));
                BSMLSettings.instance.AddSettingsMenu("Custom Platforms", "CustomFloorPlugin.UI.Settings.bsml", Settings.instance);
            }
        }


        /// <summary>
        /// Transitions to the CustomPlatforms selection menu
        /// </summary>
        private static void CustomPlatformsMenuButtonPressed() {
            BeatSaberUI.MainFlowCoordinator.InvokeMethod("PresentFlowCoordinator", PlatformMenuFlowCoordinator, null, false, false);
        }
    }*/
}