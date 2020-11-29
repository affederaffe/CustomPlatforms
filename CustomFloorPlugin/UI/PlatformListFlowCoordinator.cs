using BeatSaberMarkupLanguage;

using CustomFloorPlugin.Utilities;

using HMUI;

namespace CustomFloorPlugin.UI {


    /// <summary>
    /// Instatiable custom <see cref="FlowCoordinator"/> used by BSML
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1812:Avoid unistantiated internal classes", Justification = "Instantiated by Unity")]
    internal class PlatformListFlowCoordinator : FlowCoordinator {


        /// <summary>
        /// Provides the <see cref="PlatformsListView"/>
        /// </summary>
        private static PlatformsListView PlatformsListView {
            get {
                if (_PlatformsListView == null) {
                    _PlatformsListView = BeatSaberUI.CreateViewController<UI.PlatformsListView>();
                }
                return _PlatformsListView;
            }
        }
        private static PlatformsListView _PlatformsListView;


        /// <summary>
        /// Set the window properties on first activation<br/>
        /// [Called by Beat Saber]
        /// </summary>
        /// <param name="firstActivation">Was this the first activation?</param>
        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            if (firstActivation) {
                SetTitle("Custom Platforms");
                showBackButton = true;
                ProvideInitialViewControllers(PlatformsListView);
            }
        }


        /// <summary>
        /// Transitions back to the main <see cref="FlowCoordinator"/><br/>
        /// [Called by Beat Saber]
        /// </summary>
        /// <param name="ignored1"></param>
        protected override void BackButtonWasPressed(ViewController ignored1) {
            Logging.Log("Selected Environment:" + PlatformManager.CurrentPlatform.platName);
            Logging.Log("Selected Override: " + PlatformsListView.EnvOr);
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this, null, ViewController.AnimationDirection.Horizontal, false);
        }
    }
}