using HMUI;
using BS_Utils.Utilities;
using BeatSaberMarkupLanguage;
using System;
using IPA.Utilities;
using UnityEngine;

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
            } catch(Exception e) {
                Plugin.Log(e);
            }
        }
        protected override void BackButtonWasPressed(ViewController topViewController) {
            // dismiss ourselves
            Plugin.Log("Selected Environment:" + PlatformManager.Instance.currentPlatform.platName);
            var mainFlow = BeatSaberMarkupLanguage.BeatSaberUI.MainFlowCoordinator;
            mainFlow.InvokePrivateMethod("DismissFlowCoordinator", this, null, false);
        }
    }
}