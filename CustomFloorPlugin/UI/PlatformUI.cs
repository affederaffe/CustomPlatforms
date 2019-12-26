using HMUI;
using UnityEngine;
using CustomFloorPlugin.Util;
using UnityEngine.SceneManagement;
using BS_Utils.Utilities;
using System.Linq;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using BeatSaberMarkupLanguage.Settings;
using Zenject;

namespace CustomFloorPlugin
{
    class PlatformUI : MonoBehaviour
    {   
        public static PlatformUI _instance;
                
        public static UI.PlatformListFlowCoordinator _platformMenuFlowCoordinator;

        internal static void OnLoad()
        {
            if (_instance != null)
            {
                return;
            }
            new GameObject("PlatformUI").AddComponent<PlatformUI>();
        }

        private void Awake()
        {
            _instance = this;
            SceneManager.MoveGameObjectToScene(gameObject, SceneManager.CreateScene("PlatformUIDump"));

            Plugin.gsm.MarkSceneAsPersistent("PlatformUIDump");
            //God I hate the new cheese -.-
            //Yeah... no kiddin'
            //AAAAAAAAAAAAAAAAAAAAAAAAAAAAARRRGH
        }
        

        public static void SetupMenuButtons(ScenesTransitionSetupDataSO ignored1 = null, DiContainer ignored2 = null) {
            MenuButtons.instance.RegisterButton(new MenuButton("Custom Platforms", "Change Custom Plaforms Here!", CustomPlatformsMenuButtonPressed, true));
            BSMLSettings.instance.AddSettingsMenu("Custom Platforms", "CustomFloorPlugin.UI.Settings.bsml", UI.Settings.instance);
            Debug.Log("Settings should exist now");
        }
        private static void CustomPlatformsMenuButtonPressed()
        {
            if (_platformMenuFlowCoordinator == null)
            {
                _platformMenuFlowCoordinator = BeatSaberUI.CreateFlowCoordinator<UI.PlatformListFlowCoordinator>();
            }
            BeatSaberUI.MainFlowCoordinator.InvokeMethod("PresentFlowCoordinator", _platformMenuFlowCoordinator, null, false, false);
        }
    }
}