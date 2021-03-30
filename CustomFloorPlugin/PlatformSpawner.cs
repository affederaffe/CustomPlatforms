using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CustomFloorPlugin.Configuration;
using CustomFloorPlugin.Extensions;

using SiraUtil.Tools;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Handles platform spawing and despawning
    /// </summary>
    internal class PlatformSpawner : IInitializable, IDisposable
    {
        private readonly SiraLog _siraLog;
        private readonly PluginConfig _config;
        private readonly AssetLoader _assetLoader;
        private readonly EnvironmentHider _hider;
        private readonly PlatformLoader _platformLoader;
        private readonly PlatformManager _platformManager;
        private readonly GameScenesManager _gameScenesManager;
        private readonly System.Random _random;
        private DiContainer _container;
        internal bool isMultiplayer;

        /// <summary>
        /// Returns a random index of all platforms
        /// </summary>
        internal int RandomPlatformIndex => _random.Next(0, _platformManager.allPlatforms.Count);

        internal PlatformSpawner(SiraLog siraLog,
                                 PluginConfig config,
                                 AssetLoader assetLoader,
                                 EnvironmentHider hider,
                                 PlatformLoader platformLoader,
                                 PlatformManager platformManager,
                                 GameScenesManager gameScenesManager)
        {
            _siraLog = siraLog;
            _config = config;
            _assetLoader = assetLoader;
            _hider = hider;
            _platformLoader = platformLoader;
            _platformManager = platformManager;
            _gameScenesManager = gameScenesManager;
            _random = new();
        }

        public void Initialize()
        {
            _gameScenesManager.transitionDidStartEvent += HandleTransitionDidStart;
            _gameScenesManager.transitionDidFinishEvent += HandleTransistionDidFinish;
        }

        public void Dispose()
        {
            _gameScenesManager.transitionDidStartEvent -= HandleTransitionDidStart;
            _gameScenesManager.transitionDidFinishEvent -= HandleTransistionDidFinish;
        }

        /// <summary>
        /// Automaticly clean up all custom objects
        /// </summary>
        private async void HandleTransitionDidStart(float aheadTime)
        {
            await Task.Delay(TimeSpan.FromSeconds(aheadTime));
            ChangeToPlatform(0);
        }

        /// <summary>
        /// Decide which platform to change to based on the type of the <see cref="ScenesTransitionSetupDataSO"/>
        /// </summary>
        private void HandleTransistionDidFinish(ScenesTransitionSetupDataSO setupData, DiContainer container)
        {
            int platformIndex = 0;
            switch (setupData)
            {
                case null:
                case MenuScenesTransitionSetupDataSO:
                    if (isMultiplayer)
                        return;
                    _assetLoader.heart.SetActive(_config.ShowHeart);
                    _assetLoader.heart.GetComponent<InstancedMaterialLightWithId>().ColorWasSet(Color.magenta);
                    if (_config.ShowInMenu)
                        platformIndex = _config.ShufflePlatforms
                            ? RandomPlatformIndex
                            : _platformManager.GetIndexForType(PlatformType.Singleplayer);
                    break;
                case StandardLevelScenesTransitionSetupDataSO:
                case MissionLevelScenesTransitionSetupDataSO:
                case TutorialScenesTransitionSetupDataSO:
                    _assetLoader.heart.SetActive(false);
                    platformIndex = setupData.Is360Level()
                        ? _platformManager.GetIndexForType(PlatformType.A360)
                        : _config.ShufflePlatforms
                        ? RandomPlatformIndex
                        : _platformManager.GetIndexForType(PlatformType.Singleplayer);
                    break;
                case AppInitScenesTransitionSetupDataSO:
                case HealthWarningScenesTransitionSetupDataSO:
                case MultiplayerLevelScenesTransitionSetupDataSO:
                    // Multiplayer levels are handled by the MultiplayerGameHelper because this event doesn't provide the GameplayCore DiContainer for multiplayer levels.
                    return;
                default:
                    _assetLoader.heart.SetActive(false);
                    break;
            }

            // Handle possible API request
            if (_platformManager.apiRequestedPlatform != null)
            {
                int apiIndex = _platformManager.GetIndexForType(PlatformType.API);
                if (_platformManager.apiRequestedLevelId == setupData.GetLevelId())
                {
                    platformIndex = apiIndex;
                }
                else if (apiIndex == 0)
                {
                    platformIndex = apiIndex;
                    _platformManager.apiRequestedPlatform = null;
                }
            }

            SetContainerAndShow(platformIndex, container);
        }

        /// <summary>
        /// Changes to a specific <see cref="CustomPlatform"/>
        /// </summary>
        /// <param name="index">The index of the new <see cref="CustomPlatform"/> in the list</param>
        /// <param name="container">The container used to instantiate all custom objects</param>
        internal void SetContainerAndShow(int index, DiContainer container)
        {
            _container = container;
            ChangeToPlatform(index);
        }

        /// <summary>
        /// Changes to a specific <see cref="CustomPlatform"/> and saves the choice
        /// </summary>
        /// <param name="index">The index of the new <see cref="CustomPlatform"/> in the list</param>
        internal void SetPlatformAndShow(int index, PlatformType platformType)
        {
            switch (platformType)
            {
                case PlatformType.Singleplayer:
                    _platformManager.currentSingleplayerPlatform = _platformManager.allPlatforms[index];
                    _config.SingleplayerPlatformPath = _platformManager.currentSingleplayerPlatform.platName + _platformManager.currentSingleplayerPlatform.platAuthor;
                    break;
                case PlatformType.Multiplayer:
                    _platformManager.currentMultiplayerPlatform = _platformManager.allPlatforms[index];
                    _config.MultiplayerPlatformPath = _platformManager.currentMultiplayerPlatform.platName + _platformManager.currentMultiplayerPlatform.platAuthor;
                    break;
                case PlatformType.A360:
                    _platformManager.currentA360Platform = _platformManager.allPlatforms[index];
                    _config.A360PlatformPath = _platformManager.currentA360Platform.platName + _platformManager.currentA360Platform.platAuthor;
                    break;
            }
            ChangeToPlatform(index);
        }

        /// <summary>
        /// Changes to a specific <see cref="CustomPlatform"/>
        /// </summary>
        /// <param name="index">The index of the new <see cref="CustomPlatform"/> in the list of all platforms</param>
        internal async void ChangeToPlatform(int index)
        {
            _siraLog.Info("Switching to " + _platformManager.allPlatforms[index].name);
            DestroyCustomObjects();
            _platformManager.activePlatform?.gameObject.SetActive(false);
            _platformManager.activePlatform = _platformManager.allPlatforms[index];

            if (_platformManager.activePlatform.isDescriptor)
            {
                string platformPath = _platformManager.activePlatform.fullPath;
                await _platformLoader.LoadFromFileAsync(platformPath, _platformManager.HandlePlatformLoaded);
                // Check if another platform has been spawned in the meantime and abort if that's the case
                if (_platformManager.activePlatform?.fullPath != platformPath)
                    return;
            }

            if (index != 0)
            {
                _platformManager.activePlatform.gameObject.SetActive(true);
                SpawnCustomObjects();
            }
            else
            {
                _platformManager.activePlatform = null;
            }

            _hider.HideObjectsForPlatform(_platformManager.allPlatforms[index]);
        }

        /// <summary>
        /// Spawns all registered custom objects, as required by the selected <see cref="CustomPlatform"/>
        /// </summary>
        private void SpawnCustomObjects()
        {
            foreach (INotifyPlatformEnabled notifyEnable in _platformManager.activePlatform.GetComponentsInChildren<INotifyPlatformEnabled>(true))
            {
                notifyEnable.PlatformEnabled(_container);
            }
        }

        /// <summary>
        /// Despawns all registered custom objects, as required by the selected <see cref="CustomPlatform"/>
        /// </summary>
        private void DestroyCustomObjects()
        {
            if (_platformManager.activePlatform == null)
                return;

            foreach (INotifyPlatformDisabled notifyDisable in _platformManager.activePlatform.GetComponentsInChildren<INotifyPlatformDisabled>(true))
            {
                notifyDisable.PlatformDisabled();
            }

            while (_platformManager.spawnedObjects.Count != 0)
            {
                UnityEngine.Object gameObject = _platformManager.spawnedObjects[0];
                _platformManager.spawnedObjects.RemoveAt(0);
                UnityEngine.Object.Destroy(gameObject);
            }
        }
    }
}