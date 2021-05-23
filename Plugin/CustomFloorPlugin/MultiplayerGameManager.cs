using System;

using Zenject;


namespace CustomFloorPlugin
{
    internal class MultiplayerGameManager : IInitializable, IDisposable
    {
        private readonly DiContainer _container;
        private readonly AssetLoader _assetLoader;
        private readonly PlatformManager _platformManager;
        private readonly PlatformSpawner _platformSpawner;
        private readonly MultiplayerPlayersManager _multiplayerPlayersManager;

        public MultiplayerGameManager(DiContainer container,
                                      AssetLoader assetLoader,
                                      PlatformManager platformManager,
                                      PlatformSpawner platformSpawner,
                                      MultiplayerPlayersManager multiplayerPlayersManager)
        {
            _container = container;
            _assetLoader = assetLoader;
            _platformManager = platformManager;
            _platformSpawner = platformSpawner;
            _multiplayerPlayersManager = multiplayerPlayersManager;
        }

        public async void Initialize()
        {
            _multiplayerPlayersManager.playerDidFinishEvent += OnPlayerDidFinish;
            await Helpers.AsyncHelper.WaitForEndOfFrameAsync();
            int index = _platformManager.GetIndexForType(PlatformType.Multiplayer);
            await _platformSpawner.SetContainerAndShowAsync(index, _container);
            _assetLoader.MultiplayerLightEffects.PlatformEnabled(_container);
        }

        public void Dispose()
        {
            _multiplayerPlayersManager.playerDidFinishEvent -= OnPlayerDidFinish;
        }

        /// <summary>
        /// Automatically change to the default platform when the player fails or finishes for a bette view
        /// </summary>
        private void OnPlayerDidFinish(LevelCompletionResults results)
        {
            _ = _platformSpawner.ChangeToPlatformAsync(0);
            _assetLoader.MultiplayerLightEffects.PlatformDisabled();
        }
    }
}