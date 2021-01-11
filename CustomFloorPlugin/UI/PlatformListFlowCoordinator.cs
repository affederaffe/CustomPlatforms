using BeatSaberMarkupLanguage;

using HMUI;

using Zenject;

using CustomFloorPlugin.Utilities;


namespace CustomFloorPlugin.UI {


    /// <summary>
    /// Instatiable custom <see cref="FlowCoordinator"/> used by BSML
    /// </summary>
    internal class PlatformListFlowCoordinator : FlowCoordinator {

        [Inject]
        private readonly PlatformsListView platformsListView;

        [Inject]
        private readonly EnvironmentOverrideListView environmentOverrideListView;

        [Inject]
        private readonly ReloadPlatformsButtonView reloadPlatformsButtonView;


        /// <summary>
        /// Set the window properties on first activation<br/>
        /// [Called by Beat Saber]
        /// </summary>
        /// <param name="firstActivation">Was this the first activation?</param>
        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            if (firstActivation) {
                SetTitle("Custom Platforms");
                showBackButton = true;
                ProvideInitialViewControllers(platformsListView, environmentOverrideListView, null, reloadPlatformsButtonView);
            }
        }


        /// <summary>
        /// Transitions back to the main <see cref="FlowCoordinator"/><br/>
        /// [Called by Beat Saber]
        /// </summary>
        /// <param name="ignored1"></param>
        protected override void BackButtonWasPressed(ViewController ignored1) {
            Logging.Log("Selected Environment: " + PlatformManager.CurrentPlatform.platName);
            Logging.Log("Selected Override: " + EnvironmentOverrideListView.EnvOr);
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this, null, ViewController.AnimationDirection.Horizontal, false);
        }
    }
}