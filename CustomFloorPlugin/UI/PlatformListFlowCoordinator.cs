using BeatSaberMarkupLanguage;
using HMUI;
using IPA.Utilities;
using System;

namespace CustomPlatforms.UI {
    class PlatformListFlowCoordinator:FlowCoordinator {
        private PlatformsListView _platformsListView;
        public void Awake() {
            if(_platformsListView == null) {
                _platformsListView = BeatSaberUI.CreateViewController<UI.PlatformsListView>();
            }
        }
        protected override void DidActivate(bool firstActivation, ActivationType activationType) {
            try {
                if(firstActivation) {

                    title = "Custom Platforms";
                    showBackButton = true;
                    ProvideInitialViewControllers(_platformsListView);
                }
                if(activationType == ActivationType.AddedToHierarchy) {

                }
            } catch(Exception e) {
                Plugin.Log(e);
            }
        }
        protected override void BackButtonWasPressed(ViewController topViewController) {
            // dismiss ourselves
            Plugin.Log("Selected Environment:" + PlatformManager.currentPlatform.platName);
            var mainFlow = BeatSaberUI.MainFlowCoordinator;
            mainFlow.InvokePrivateMethod("DismissFlowCoordinator", this, null, false);
        }
    }
}