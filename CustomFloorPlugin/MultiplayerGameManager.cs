using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using IPA.Utilities.Async;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    internal class MultiplayerGameManager : IInitializable, IDisposable
    {
        private readonly AssetLoader _assetLoader;
        private readonly PlatformManager _platformManager;
        private readonly PlatformSpawner _platformSpawner;
        private readonly MultiplayerPlayersManager _multiplayerPlayersManager;
        private readonly DiContainer _container;

        public MultiplayerGameManager(AssetLoader assetLoader,
                                     PlatformManager platformManager,
                                     PlatformSpawner platformSpawner,
                                     MultiplayerPlayersManager multiplayerPlayersManager,
                                     DiContainer container)
        {
            _assetLoader = assetLoader;
            _platformManager = platformManager;
            _platformSpawner = platformSpawner;
            _multiplayerPlayersManager = multiplayerPlayersManager;
            _container = container;
        }

        public async void Initialize()
        {
            int platformIndex = _platformManager.GetIndexForType(PlatformType.Multiplayer);
            if (platformIndex != 0)
            {
                _multiplayerPlayersManager.playerDidFinishEvent += OnPlayerDidFinish;
                GameObject lightEffects = await SpawnLightEffects();
                await Coroutines.AsTask(WaitForEndOfFrameCoroutine());
                static IEnumerator<WaitForEndOfFrame> WaitForEndOfFrameCoroutine() { yield return new WaitForEndOfFrame(); }
                await _platformSpawner.SetContainerAndShowAsync(platformIndex, _container);
                _platformManager.spawnedObjects.Add(lightEffects);
            }
        }

        public void Dispose()
        {
            _multiplayerPlayersManager.playerDidFinishEvent -= OnPlayerDidFinish;
        }

        private async void OnPlayerDidFinish(LevelCompletionResults results)
        {
            await _platformSpawner.ChangeToPlatformAsync(0);
        }

        /// <summary>
        /// Instantiates the light effects prefab for multiplayer levels
        /// </summary>
        private async Task<GameObject> SpawnLightEffects()
        {
            await _assetLoader.loadAssetsTask!;
            GameObject lightEffects = _container.InstantiatePrefab(_assetLoader.lightEffects);
            lightEffects.SetActive(true);
            return lightEffects;
        }
    }
}