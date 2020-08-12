using BeatSaberMarkupLanguage;
using HMUI;
using System;
using UnityEngine;

namespace CustomFloorPlugin
{
    public class PlatformsFlowCoordinator : FlowCoordinator
    {

        private PlatformListViewController platformListView;
        //private SaberPreviewViewController saberPreviewView;
        //private SaberSettingsViewController saberSettingsView;

        public void Awake()
        {
            /*if (!saberPreviewView)
            {
                saberPreviewView = BeatSaberUI.CreateViewController<SaberPreviewViewController>();
            }

            if (!saberSettingsView)
            {
                saberSettingsView = BeatSaberUI.CreateViewController<SaberSettingsViewController>();
            }*/

            if (!platformListView)
            {
                platformListView = BeatSaberUI.CreateViewController<PlatformListViewController>();
            }
        }

        protected override void DidActivate(bool firstActivation, ActivationType activationType)
        {
            try
            {
                if (firstActivation)
                {
                    title = "Custom Platforms";
                    showBackButton = true;
                    ProvideInitialViewControllers(platformListView);
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            // Dismiss ourselves
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this, null, false);
        }

    }
}