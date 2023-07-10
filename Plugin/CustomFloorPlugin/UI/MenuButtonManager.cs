﻿using System;

using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;

using JetBrains.Annotations;

using Zenject;


namespace CustomFloorPlugin.UI
{
    /// <summary>
    /// UI Class, sets up the menu button
    /// </summary>
    [UsedImplicitly]
    internal class MenuButtonManager : IInitializable, IDisposable
    {
        private readonly PlatformsFlowCoordinator _platformListFlowCoordinator;
        private readonly MainFlowCoordinator _mainFlowCoordinator;
        private readonly MenuButton _menuButton;

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
            MenuButtons.instance.UnregisterButton(_menuButton);
        }

        /// <summary>
        /// Shows the UI when the button is pressed
        /// </summary>
        private void SummonFlowCoordinator()
        {
            _mainFlowCoordinator.PresentFlowCoordinator(_platformListFlowCoordinator);
        }
    }
}
