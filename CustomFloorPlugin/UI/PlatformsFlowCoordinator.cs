using BeatSaberMarkupLanguage;
using HMUI;
using System;
using UnityEngine;

namespace CustomFloorPlugin
{
    public class PlatformsFlowCoordinator : FlowCoordinator
    {

        private PlatformListViewController platformListView;
        private PlatformSettingsViewController platformSettingsView;

        public void Awake()
        {
            if (!platformSettingsView)
            {
                platformSettingsView = BeatSaberUI.CreateViewController<PlatformSettingsViewController>();
            }

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
                    ProvideInitialViewControllers(platformListView, platformSettingsView);
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