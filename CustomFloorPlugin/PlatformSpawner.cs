using System;
using System.Collections;
using System.Linq;

using CustomFloorPlugin.Configuration;
using CustomFloorPlugin.Extensions;

using IPA.Utilities;

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
        private readonly EnvironmentHider _hider;
        private readonly PlatformLoader _platformLoader;
        private readonly PlatformManager _platformManager;
        private readonly GameScenesManager _gameScenesManager;
        private readonly System.Random _random;
        private DiContainer _container;

        internal PlatformSpawner(SiraLog siraLog, PluginConfig config, EnvironmentHider hider, PlatformLoader platformLoader, PlatformManager platformManager, GameScenesManager gameScenesManager)
        {
            _siraLog = siraLog;
            _config = config;
            _hider = hider;
            _platformLoader = platformLoader;
            _platformManager = platformManager;
            _gameScenesManager = gameScenesManager;
            _random = new();
        }

        internal int RandomPlatformIndex => _random.Next(0, _platformManager.allPlatforms.Count);

        public void Initialize()
        {
            _gameScenesManager.transitionDidFinishEvent += HandleTransistionDidFinish;
        }

        public void Dispose()
        {
            _gameScenesManager.transitionDidFinishEvent -= HandleTransistionDidFinish;
        }

        private void HandleTransistionDidFinish(ScenesTransitionSetupDataSO setupData, DiContainer container)
        {
            _container = container;
            int platformIndex = 0;

            switch (setupData)
            {
                case null:
                case MenuScenesTransitionSetupDataSO:
                    _platformManager.heart?.SetActive(_config.ShowHeart);
                    _platformManager.heart?.GetComponent<InstancedMaterialLightWithId>().ColorWasSet(Color.magenta);
                    if (_config.ShowInMenu)
                        platformIndex = _config.ShufflePlatforms
                            ? RandomPlatformIndex
                            : _platformManager.GetIndexForType(PlatformType.Singleplayer);
                    break;
                case StandardLevelScenesTransitionSetupDataSO:
                case MissionLevelScenesTransitionSetupDataSO:
                case TutorialScenesTransitionSetupDataSO:
                    _platformManager.heart?.SetActive(false);
                    platformIndex = _config.ShufflePlatforms
                        ? RandomPlatformIndex
                        : setupData.Is360Level()
                        ? _platformManager.GetIndexForType(PlatformType.A360)
                        : _platformManager.GetIndexForType(PlatformType.Singleplayer);
                    break;
                case MultiplayerLevelScenesTransitionSetupDataSO:
                    _platformManager.heart?.SetActive(false);
                    platformIndex = _platformManager.GetIndexForType(PlatformType.Singleplayer);
                    SpawnLightEffects();
                    break;
                case CreditsScenesTransitionSetupDataSO:
                    _hider.HideObjectsForPlatform(_platformManager.activePlatform ?? _platformManager.allPlatforms[0]);
                    break;
                default:
                    _platformManager.heart?.SetActive(false);
                    break;
            }

            // Handle possible API request
            if (_platformManager.apiRequestIndex != -1 && (_platformManager.apiRequestedLevelId == setupData.GetLevelId() || _platformManager.apiRequestIndex == 0))
            {
                platformIndex = _platformManager.apiRequestIndex;
                if (_platformManager.apiRequestIndex == 0)
                    _platformManager.apiRequestIndex = -1;
            }

            ChangeToPlatform(platformIndex);
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
        /// Changes to the currently selected <see cref="CustomPlatform"/>
        /// </summary>
        internal void ChangeToPlatform(PlatformType platformType)
        {
            int index = _platformManager.GetIndexForType(platformType);
            ChangeToPlatform(index);
        }

        /// <summary>
        /// Changes to a specific <see cref="CustomPlatform"/>
        /// </summary>
        /// <param name="index">The index of the new <see cref="CustomPlatform"/> in the list <see cref="AllPlatforms"/></param>
        internal void ChangeToPlatform(int index)
        {
            if (!_platformManager.allPlatforms[index].requirements.All(x => _platformManager.allPluginNames.Contains(x)))
            {
                _siraLog.Warning("Missing requirement for platform " + _platformManager.allPlatforms[index].name);
                ChangeToPlatform(0);
                return;
            }

            _siraLog.Info("Switching to " + _platformManager.allPlatforms[index].name);
            _platformManager.activePlatform?.gameObject.SetActive(false);
            DestroyCustomObjects();
            _platformManager.activePlatform = _platformManager.allPlatforms[index];

            SharedCoroutineStarter.instance.StartCoroutine(WaitAndSpawn());
            IEnumerator WaitAndSpawn()
            {
                if (_platformManager.activePlatform?.transform.childCount == 0 && index != 0)
                {
                    string platformPath = _platformManager.activePlatform.fullPath;
                    yield return SharedCoroutineStarter.instance.StartCoroutine(_platformLoader.LoadFromFileAsync(platformPath, _platformManager.HandlePlatformLoaded));
                    // Check if another platform has been spawned in the meantime and abort if that's the case
                    if (_platformManager.activePlatform?.fullPath != platformPath)
                        yield break;
                }

                if (index != 0)
                {
                    yield return new WaitForEndOfFrame();
                    _platformManager.activePlatform?.gameObject.SetActive(true);
                    SpawnCustomObjects();
                }
                else
                {
                    _platformManager.activePlatform = null;
                }

                _hider.HideObjectsForPlatform(_platformManager.allPlatforms[index]);
            }
        }

        /// <summary>
        /// Spawns all registered custom objects, as required by the selected <see cref="CustomPlatform"/>
        /// </summary>
        private void SpawnCustomObjects()
        {
            INotifyPlatformEnabled[] notifyEnables = _platformManager.activePlatform?.GetComponentsInChildren<INotifyPlatformEnabled>(true);
            if (notifyEnables != null)
            {
                foreach (INotifyPlatformEnabled notifyEnable in notifyEnables)
                {
                    notifyEnable.PlatformEnabled(_container);
                }
            }
        }

        /// <summary>
        /// Despawns all registered custom objects, as required by the selected <see cref="CustomPlatform"/>
        /// </summary>
        private void DestroyCustomObjects()
        {
            INotifyPlatformDisabled[] notifyDisables = _platformManager.activePlatform?.GetComponentsInChildren<INotifyPlatformDisabled>(true);
            if (notifyDisables != null)
            {
                foreach (INotifyPlatformDisabled notifyDisable in notifyDisables)
                {
                    notifyDisable.PlatformDisabled();
                }
            }

            while (_platformManager.spawnedObjects.Count != 0)
            {
                GameObject gameObject = _platformManager.spawnedObjects[0];
                _platformManager.spawnedObjects.RemoveAt(0);
                GameObject.Destroy(gameObject);
                foreach (TubeBloomPrePassLight tubeBloomPrePassLight in gameObject.GetComponentsInChildren<TubeBloomPrePassLight>(true))
                {
                    //Unity requires this to be present, otherwise Unregister won't be called. Memory leaks may occour if this is removed.
                    tubeBloomPrePassLight.InvokeMethod<object, BloomPrePassLight>("UnregisterLight");
                }
            }

            while (_platformManager.spawnedComponents.Count != 0)
            {
                Component component = _platformManager.spawnedComponents[0];
                _platformManager.spawnedComponents.RemoveAt(0);
                GameObject.Destroy(component);
            }
        }

        /// <summary>
        /// Instantiates the light effects prefab for multiplayer levels
        /// </summary>
        private void SpawnLightEffects()
        {
            GameObject lightEffects = _container.InstantiatePrefab(_platformManager.lightEffects);
            _platformManager.spawnedObjects.Add(lightEffects);
            lightEffects.SetActive(true);
        }
    }
}
