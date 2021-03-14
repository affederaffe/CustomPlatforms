using System;

using CustomFloorPlugin.Configuration;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    internal class MenuLobbyHelper : IInitializable, IDisposable
    {
        private readonly PluginConfig _config;
        private readonly PlatformManager _platformManager;
        private readonly PlatformSpawner _platformSpawner;
        private readonly IMultiplayerSessionManager _multiplayerSessionManager;

        internal MenuLobbyHelper(PluginConfig config, PlatformManager platformManager, PlatformSpawner platformSpawner, IMultiplayerSessionManager multiplayerSessionManager)
        {
            _config = config;
            _platformManager = platformManager;
            _platformSpawner = platformSpawner;
            _multiplayerSessionManager = multiplayerSessionManager;
        }

        public void Initialize()
        {
            _multiplayerSessionManager.connectedEvent += HandleConnected;
            _multiplayerSessionManager.disconnectedEvent += HandleDisconnected;
        }

        public void Dispose()
        {
            _multiplayerSessionManager.connectedEvent -= HandleConnected;
            _multiplayerSessionManager.disconnectedEvent -= HandleDisconnected;
        }

        private void HandleConnected()
        {
            _platformManager.heart.SetActive(false);
            _platformSpawner.ChangeToPlatform(0);
        }

        private void HandleDisconnected(DisconnectedReason reason)
        {
            _platformManager.heart.SetActive(_config.ShowHeart);
            _platformManager.heart.GetComponent<InstancedMaterialLightWithId>().ColorWasSet(Color.magenta);
            if (_config.ShowInMenu)
                _platformSpawner.ChangeToPlatform(PlatformType.Singleplayer);
        }
    }
}
