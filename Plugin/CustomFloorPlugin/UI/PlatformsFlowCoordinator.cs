using BeatSaberMarkupLanguage;

using HMUI;

using Zenject;


namespace CustomFloorPlugin.UI
{
    /// <summary>
    /// Instantiable custom <see cref="FlowCoordinator"/> used by BSML
    /// </summary>
    internal class PlatformsFlowCoordinator : FlowCoordinator
    {
        private PlatformListsView _platformsListView = null!;
        private ChangelogView _changelogView = null!;
        private SettingsView _settingsView = null!;
        private MainFlowCoordinator _mainFlowCoordinator = null!;

        [Inject]
        public void Construct(PlatformListsView platformsListView,
                              ChangelogView changelogView,
                              SettingsView settingsView,
                              MainFlowCoordinator mainFlowCoordinator)
        {
            _platformsListView = platformsListView;
            _changelogView = changelogView;
            _settingsView = settingsView;
            _mainFlowCoordinator = mainFlowCoordinator;
        }

        /// <summary>
        /// Set the window properties on first activation<br/>
        /// [Called by Beat Saber]
        /// </summary>
        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                showBackButton = true;
                SetTitle("Custom Platforms");
                ProvideInitialViewControllers(_platformsListView, _changelogView, _settingsView);
            }
        }

        /// <summary>
        /// Transitions back to the main <see cref="FlowCoordinator"/><br/>
        /// [Called by Beat Saber]
        /// </summary>
        protected override void BackButtonWasPressed(ViewController viewController)
        {
            _mainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}