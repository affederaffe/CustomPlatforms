using BeatSaberMarkupLanguage;

using CustomFloorPlugin.Utilities;

using HMUI;

using Zenject;


namespace CustomFloorPlugin.UI {


    /// <summary>
    /// Instatiable custom <see cref="FlowCoordinator"/> used by BSML
    /// </summary>
    internal class PlatformListFlowCoordinator : FlowCoordinator {

        [Inject]
        private readonly PlatformManager _platformManager;

        [Inject]
        private readonly PlatformsListView _platformsListView;

        [Inject]
        private readonly ChangelogView _changelogView;

        [Inject]
        private readonly SettingsView _settingsView;


        /// <summary>
        /// Set the window properties on first activation<br/>
        /// [Called by Beat Saber]
        /// </summary>
        /// <param name="firstActivation">Was this the first activation?</param>
        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            if (firstActivation) {
                SetTitle("Custom Platforms");
                showBackButton = true;
                ProvideInitialViewControllers(_platformsListView, _changelogView, _settingsView);
            }
        }


        /// <summary>
        /// Transitions back to the main <see cref="FlowCoordinator"/><br/>
        /// [Called by Beat Saber]
        /// </summary>
        /// <param name="_1"></param>
        protected override void BackButtonWasPressed(ViewController _1) {
            Logging.Log("Selected Platform: " + _platformManager.CurrentPlatform.platName);
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this, null, ViewController.AnimationDirection.Horizontal, false);
        }
    }
}
