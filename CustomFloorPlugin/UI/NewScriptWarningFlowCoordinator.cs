using BeatSaberMarkupLanguage;

using HMUI;

namespace CustomFloorPlugin.UI {

    /// <summary>
    /// 
    /// Instatiable custom <see cref="FlowCoordinator"/> used by BSML
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1812:Avoid unistantiated internal classes", Justification = "Instantiated by Unity")]
    internal class NewScriptWarningFlowCoordinator : FlowCoordinator {

        /// <summary>
        /// Provides the <see cref="NewScriptWarningView"/>
        /// </summary>
        private static NewScriptWarningView NewScriptWarningView {
            get {
                if (_NewScriptWarningView == null) {
                    _NewScriptWarningView = BeatSaberUI.CreateViewController<UI.NewScriptWarningView>();
                }
                return _NewScriptWarningView;
            }
        }
        private static NewScriptWarningView _NewScriptWarningView;


        /// <summary>
        /// Set the window properties on first activation<br/>
        /// [Called by Beat Saber]
        /// </summary>
        /// <param name="firstActivation">Was this the first activation?</param>
        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            if (firstActivation) {
                SetTitle("Warning");
                showBackButton = false;
                ProvideInitialViewControllers(NewScriptWarningView);
            }
        }
    }
}
