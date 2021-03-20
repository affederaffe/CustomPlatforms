using System;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    internal class MultiplayerGameHelper : IInitializable, IDisposable
    {
        private readonly PlatformManager _platformManager;
        private readonly PlatformSpawner _platformSpawner;
        private readonly MultiplayerPlayersManager _multiplayerPlayersManager;
        private readonly DiContainer _container;

        public MultiplayerGameHelper(PlatformManager platformManager,
                                     PlatformSpawner platformSpawner,
                                     MultiplayerPlayersManager multiplayerPlayersManager,
                                     DiContainer container)
        {
            _platformManager = platformManager;
            _platformSpawner = platformSpawner;
            _multiplayerPlayersManager = multiplayerPlayersManager;
            _container = container;
        }

        public void Initialize()
        {
            int platformIndex = _platformManager.GetIndexForType(PlatformType.Multiplayer);
            if (platformIndex != 0)
            {
                _multiplayerPlayersManager.playerDidFinishEvent += HandlePlayerDidFinish;
                SpawnLightEffects();
                _platformSpawner.SetContainerAndShow(platformIndex, _container);
            }
        }

        public void Dispose()
        {
            _multiplayerPlayersManager.playerDidFinishEvent -= HandlePlayerDidFinish;
        }

        private void HandlePlayerDidFinish(LevelCompletionResults results)
        {
            _platformSpawner.ChangeToPlatform(0);
        }

        /// <summary>
        /// Instantiates the light effects prefab for multiplayer levels
        /// </summary>
        private void SpawnLightEffects()
        {
            GameObject lightEffects = _container.InstantiatePrefab(AssetLoader.instance.lightEffects);
            _platformManager.spawnedObjects.Add(lightEffects);
            lightEffects.SetActive(true);
        }
    }
}
