using BeatSaberMarkupLanguage;
using HMUI;
using IPA.Utilities;

namespace CustomFloorPlugin.UI {


    /// <summary>
    /// Instatiable custom <see cref="FlowCoordinator"/> used by BSML
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1812:Avoid unistantiated internal classes", Justification = "Instantiated by Unity")]
    internal class PlatformListFlowCoordinator:FlowCoordinator {


        /// <summary>
        /// Provides the <see cref="PlatformsListView"/>
        /// </summary>
        private static PlatformsListView PlatformsListView {
            get {
                if(_PlatformsListView == null) {
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
        /// <param name="activationType">Was this call added to the hierachy.</param>
        protected override void DidActivate(bool firstActivation, ActivationType activationType) {
            if(firstActivation) {
                title = "Custom Platforms";
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
            Log("Selected Environment:" + PlatformManager.CurrentPlatform.platName);
            BeatSaberUI.MainFlowCoordinator.InvokeMethod<object, MainFlowCoordinator>("DismissFlowCoordinator", this, null, false);
        }
    }
}