using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using BeatSaberMarkupLanguage.Settings;
using BS_Utils.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace CustomPlatforms {
    class PlatformUI:MonoBehaviour {
        public static PlatformUI _instance;

        public static UI.PlatformListFlowCoordinator _platformMenuFlowCoordinator;

        internal static void OnLoad() {
            if(_instance != null) {
                return;
            }
            new GameObject("PlatformUI").AddComponent<PlatformUI>();
        }

        private void Awake() {
            _instance = this;
            SceneManager.MoveGameObjectToScene(gameObject, SceneManager.CreateScene("PlatformUIDump", new CreateSceneParameters(LocalPhysicsMode.None)));

            Plugin.gsm.MarkSceneAsPersistent("PlatformUIDump");
        }


        public static void SetupMenuButtons(ScenesTransitionSetupDataSO ignored1 = null, DiContainer ignored2 = null) {
            MenuButtons.instance.RegisterButton(new MenuButton("Custom Platforms", "Change Custom Plaforms Here!", CustomPlatformsMenuButtonPressed, true));
            BSMLSettings.instance.AddSettingsMenu("Custom Platforms", "CustomPlatforms.UI.Settings.bsml", UI.Settings.instance);
        }
        private static void CustomPlatformsMenuButtonPressed() {
            if(_platformMenuFlowCoordinator == null) {
                _platformMenuFlowCoordinator = BeatSaberUI.CreateFlowCoordinator<UI.PlatformListFlowCoordinator>();
            }
            BeatSaberUI.MainFlowCoordinator.InvokeMethod("PresentFlowCoordinator", _platformMenuFlowCoordinator, null, false, false);
        }
    }
}