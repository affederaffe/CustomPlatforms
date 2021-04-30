using System;

using CustomFloorPlugin.Configuration;

using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// (Un-)Hides the menu environment when connecting to a multiplayer lobby
    /// </summary>
    internal class MenuEnvironmentManager : IInitializable, IDisposable
    {
        private readonly PluginConfig _config; 
        private readonly AssetLoader _assetLoader;
        private readonly PlatformManager _platformManager;
        private readonly PlatformSpawner _platformSpawner;
        private readonly IMultiplayerSessionManager _multiplayerSessionManager;

        public MenuEnvironmentManager(PluginConfig config,
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
            _multiplayerSessionManager.connectedEvent += OnConnected;
            _multiplayerSessionManager.disconnectedEvent += OnDisconnected;
        }

        public void Dispose()
        {
            _multiplayerSessionManager.connectedEvent -= OnConnected;
            _multiplayerSessionManager.disconnectedEvent -= OnDisconnected;
        }

        /// <summary>
        /// Deactivates the heart and changes to the default platform when connecting to a lobby
        /// because using a platform there looks terrible
        /// </summary>
        private async void OnConnected()
        {
            _platformSpawner.IsMultiplayer = true;
            _assetLoader.ToggleHeart(false);
            await _platformSpawner.ChangeToPlatformAsync(0);
        }

        /// <summary>
        /// Reactivates platform and heart if needed
        /// </summary>
        private async void OnDisconnected(DisconnectedReason reason)
        {
            _platformSpawner.IsMultiplayer = false;
            _assetLoader.ToggleHeart(_config.ShowHeart);
            if (_config.ShowInMenu)
            {
                int platformIndex = _config.ShufflePlatforms
                    ? _platformSpawner.RandomPlatformIndex
                    : _platformManager.GetIndexForType(PlatformType.Singleplayer);
                await _platformSpawner.ChangeToPlatformAsync(platformIndex);
            }
        }
    }
}