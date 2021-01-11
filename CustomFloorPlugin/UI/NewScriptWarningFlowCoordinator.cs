using BeatSaberMarkupLanguage;

using HMUI;

using Zenject;


namespace CustomFloorPlugin.UI {

    /// <summary>
    /// 
    /// Instatiable custom <see cref="FlowCoordinator"/> used by BSML
    /// </summary>
    internal class NewScriptWarningFlowCoordinator : FlowCoordinator {

        /// <summary>
        /// Provides the <see cref="NewScriptWarningView"/>
        /// </summary>
        [Inject]
        private readonly NewScriptWarningView newScriptWarningView;


        /// <summary>
        /// Set the window properties on first activation<br/>
        /// [Called by Beat Saber]
        /// </summary>
        /// <param name="firstActivation">Was this the first activation?</param>
        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            if (firstActivation) {
                SetTitle("Warning");
                showBackButton = false;
                ProvideInitialViewControllers(newScriptWarningView);
            }
        }
    }
}
