using System;

using CustomFloorPlugin.Configuration;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    internal class MultiplayerLobbyHandler : IInitializable, IDisposable
    {
        private readonly PluginConfig _config;
        private readonly AssetLoader _assetLoader;
        private readonly PlatformManager _platformManager;
        private readonly PlatformSpawner _platformSpawner;
        private readonly IMultiplayerSessionManager _multiplayerSessionManager;

        public MultiplayerLobbyHandler(PluginConfig config,
                                      AssetLoader assetLoader,
                                      PlatformManager platformManager,
                                      PlatformSpawner platformSpawner,
                                      IMultiplayerSessionManager multiplayerSessionManager)
        {
            _config = config;
            _assetLoader = assetLoader;
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

        private async void HandleConnected()
        {
            _platformSpawner.isMultiplayer = true;
            await _assetLoader.loadAssetsTask;
            _assetLoader.heart.SetActive(false);
            await _platformSpawner.ChangeToPlatformAsync(0);
        }

        private async void HandleDisconnected(DisconnectedReason reason)
        {
            _platformSpawner.isMultiplayer = false;
            await _assetLoader.loadAssetsTask;
            _assetLoader.heart.SetActive(_config.ShowHeart);
            _assetLoader.heart.GetComponent<InstancedMaterialLightWithId>().ColorWasSet(Color.magenta);
            if (_config.ShowInMenu)
            {
                int platformIndex = _config.ShufflePlatforms
                    ? await _platformSpawner.GetRandomPlatformIndexAsync()
                    : await _platformManager.GetIndexForTypeAsync(PlatformType.Singleplayer);
                await _platformSpawner.ChangeToPlatformAsync(platformIndex);
            }
        }
    }
}