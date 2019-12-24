using HMUI;
using BS_Utils.Utilities;
using BeatSaberMarkupLanguage;
using System;
using IPA.Utilities;

namespace CustomFloorPlugin.UI {
    class PlatformListFlowCoordinator:FlowCoordinator {
        private UI.PlatformsListView _platformsListView;
        public void Awake() {
            if(_platformsListView == null) {
                _platformsListView = BeatSaberUI.CreateViewController<UI.PlatformsListView>();
            }
        }
        protected override void DidActivate(bool firstActivation, ActivationType activationType) {
            try {
                if(firstActivation) {

                    title = "Custom Notes";
                    showBackButton = true;
                    ProvideInitialViewControllers(_platformsListView);
                }
                if(activationType == ActivationType.AddedToHierarchy) {

                }
            } catch(Exception ex) {
                Logger.Log("CustomFloorPlugin", ex.Message);
            }
        }
        protected override void BackButtonWasPressed(ViewController topViewController) {
            // dismiss ourselves
            var mainFlow = BeatSaberMarkupLanguage.BeatSaberUI.MainFlowCoordinator;
            mainFlow.InvokePrivateMethod("DismissFlowCoordinator", this, null, false);
        }
    }
}