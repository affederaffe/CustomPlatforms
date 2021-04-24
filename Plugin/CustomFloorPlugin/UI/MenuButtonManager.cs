using System;

using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;

using Zenject;


namespace CustomFloorPlugin.UI
{
    /// <summary>
    /// UI Class, sets up the menu button and everything about it
    /// </summary>
    internal class MenuButtonManager : IInitializable, IDisposable
    {
        private readonly MenuButton _menuButton;
        private readonly PlatformsFlowCoordinator _platformListFlowCoordinator;
        private readonly MainFlowCoordinator _mainFlowCoordinator;

        public MenuButtonManager(PlatformsFlowCoordinator platformListFlowCoordinator, MainFlowCoordinator mainFlowCoordinator)
        {
            _platformListFlowCoordinator = platformListFlowCoordinator;
            _mainFlowCoordinator = mainFlowCoordinator;
            _menuButton = new MenuButton("Custom Platforms", "Change your Platform here!", SummonFlowCoordinator);
        }

        public void Initialize()
        {
            MenuButtons.instance.RegisterButton(_menuButton);
        }

        public void Dispose()
        {
            if (MenuButtons.IsSingletonAvailable)
            {
                MenuButtons.instance.UnregisterButton(_menuButton);
            }
        }

        private void SummonFlowCoordinator()
        {
            _mainFlowCoordinator.PresentFlowCoordinator(_platformListFlowCoordinator);
        }
    }
}