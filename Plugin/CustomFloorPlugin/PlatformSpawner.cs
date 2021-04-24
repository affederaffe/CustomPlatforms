using System;
using System.Threading.Tasks;

using CustomFloorPlugin.Configuration;
using CustomFloorPlugin.Extensions;

using SiraUtil.Tools;

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
        private readonly EnvironmentHider _environmentHider;
        private readonly PlatformManager _platformManager;
        private readonly GameScenesManager _gameScenesManager;
        private readonly Random _random;
        private DiContainer _container;
        internal bool IsMultiplayer;

        internal PlatformSpawner(DiContainer container,
                                 SiraLog siraLog,
                                 PluginConfig config,
                                 AssetLoader assetLoader,
                                 EnvironmentHider environmentHider,
                                 PlatformManager platformManager,
                                 GameScenesManager gameScenesManager)
        {
            _container = container;
            _siraLog = siraLog;
            _config = config;
            _assetLoader = assetLoader;
            _environmentHider = environmentHider;
            _platformManager = platformManager;
            _gameScenesManager = gameScenesManager;
            _random = new Random();
        }

        public void Initialize()
        {
            _gameScenesManager.transitionDidStartEvent += OnTransitionDidStart;
            _gameScenesManager.transitionDidFinishEvent += OnTransitionDidFinish;
        }

        public void Dispose()
        {
            _gameScenesManager.transitionDidStartEvent -= OnTransitionDidStart;
            _gameScenesManager.transitionDidFinishEvent -= OnTransitionDidFinish;
        }

        /// <summary>
        /// Returns a random index of all platforms
        /// </summary>
        internal int RandomPlatformIndex => _random.Next(0, _platformManager.LoadPlatformsTask!.Result.Count);

        /// <summary>
        /// Automaticly clean up all custom objects
        /// </summary>
        private async void OnTransitionDidStart(float aheadTime)
        {
            await Task.Delay(TimeSpan.FromSeconds(aheadTime));
            await ChangeToPlatformAsync(0);
        }

        /// <summary>
        /// Decide which platform to change to based on the type of the <see cref="ScenesTransitionSetupDataSO"/>
        /// </summary> 
        private async void OnTransitionDidFinish(ScenesTransitionSetupDataSO setupData, DiContainer container)
        {
            await _platformManager.LoadPlatformsTask!;
            int platformIndex = 0;
            switch (setupData)
            {
                case null:
                case MenuScenesTransitionSetupDataSO:
                    if (IsMultiplayer) return;
                    _assetLoader.SetHeartActive(_config.ShowHeart);
                    if (_config.ShowInMenu)
                        platformIndex = _config.ShufflePlatforms
                            ? RandomPlatformIndex
                            : _platformManager.GetIndexForType(PlatformType.Singleplayer);
                    break;
                case StandardLevelScenesTransitionSetupDataSO:
                case MissionLevelScenesTransitionSetupDataSO:
                case TutorialScenesTransitionSetupDataSO:
                    _assetLoader.SetHeartActive(false);
                    platformIndex = setupData.Is360Level()
                        ? _platformManager.GetIndexForType(PlatformType.A360)
                        : _config.ShufflePlatforms
                        ? RandomPlatformIndex
                        : _platformManager.GetIndexForType(PlatformType.Singleplayer);
                    break;
                case AppInitScenesTransitionSetupDataSO:
                case HealthWarningScenesTransitionSetupDataSO:
                case MultiplayerLevelScenesTransitionSetupDataSO:
                    return;
                default:
                    _assetLoader.SetHeartActive(false);
                    break;
            }

            // Handle possible API request
            if (_platformManager.APIRequestedPlatform != null)
            {
                int apiIndex = _platformManager.GetIndexForType(PlatformType.API);
                if (_platformManager.APIRequestedLevelId == setupData!.GetLevelId())
                {
                    platformIndex = apiIndex;
                }
                else if (apiIndex == 0)
                {
                    platformIndex = apiIndex;
                    _platformManager.APIRequestedPlatform = null;
                }
            }

            await SetContainerAndShowAsync(platformIndex, container);
        }

        /// <summary>
        /// Changes to a specific <see cref="CustomPlatform"/>
        /// </summary>
        /// <param name="index">The index of the new <see cref="CustomPlatform"/> in the list</param>
        /// <param name="container">The container used to instantiate all custom objects</param>
        internal async Task SetContainerAndShowAsync(int index, DiContainer container)
        {
            _container = container;
            await ChangeToPlatformAsync(index);
        }

        /// <summary>
        /// Changes to a specific <see cref="CustomPlatform"/> and saves the choice
        /// </summary>
        /// <param name="index">The index of the new <see cref="CustomPlatform"/> in the list</param>
        /// <param name="platformType">Specify for which game mode the platform should be used</param>
        internal async Task SetPlatformAndShowAsync(int index, PlatformType platformType)
        {
            await _platformManager.LoadPlatformsTask!;
            switch (platformType)
            {
                case PlatformType.Singleplayer:
                    _platformManager.CurrentSingleplayerPlatform = _platformManager.LoadPlatformsTask.Result[index];
                    _config.SingleplayerPlatformPath = _platformManager.CurrentSingleplayerPlatform.platName + _platformManager.CurrentSingleplayerPlatform.platAuthor;
                    break;
                case PlatformType.Multiplayer:
                    _platformManager.CurrentMultiplayerPlatform = _platformManager.LoadPlatformsTask.Result[index];
                    _config.MultiplayerPlatformPath = _platformManager.CurrentMultiplayerPlatform.platName + _platformManager.CurrentMultiplayerPlatform.platAuthor;
                    break;
                case PlatformType.A360:
                    _platformManager.CurrentA360Platform = _platformManager.LoadPlatformsTask.Result[index];
                    _config.A360PlatformPath = _platformManager.CurrentA360Platform.platName + _platformManager.CurrentA360Platform.platAuthor;
                    break;
            }
            await ChangeToPlatformAsync(index);
        }

        /// <summary>
        /// Changes to a specific <see cref="CustomPlatform"/>
        /// </summary>
        /// <param name="index">The index of the new <see cref="CustomPlatform"/> in the list of all platforms</param>
        internal async Task ChangeToPlatformAsync(int index)
        {
            await _platformManager.LoadPlatformsTask!;
            // Avoid changing the platform unnecessarily
            if (_platformManager.ActivePlatform == _platformManager.LoadPlatformsTask.Result[index])
                return;
            _siraLog.Info("Switching to " + _platformManager.LoadPlatformsTask.Result[index].name);
            DestroyCustomObjects();
            _platformManager.ActivePlatform!.gameObject.SetActive(false);
            _platformManager.ActivePlatform = _platformManager.LoadPlatformsTask.Result[index];

            if (_platformManager.ActivePlatform.isDescriptor)
            {
                CustomPlatform? platform = await _platformManager.CreatePlatformAsync(_platformManager.ActivePlatform.fullPath);
                if (platform == null) return;
                // Check if another platform has been spawned in the meantime and abort if that's the case
                if (_platformManager.ActivePlatform.fullPath != platform.fullPath) return;
                _platformManager.ActivePlatform = platform;
            }

            _platformManager.ActivePlatform.gameObject.SetActive(true);
            SpawnCustomObjects();
            _environmentHider.HideObjectsForPlatform();
        }

        /// <summary>
        /// Enables or spawns all registered custom objects, as required by the selected <see cref="CustomPlatform"/>
        /// </summary>
        private void SpawnCustomObjects()
        {
            foreach (INotifyPlatformEnabled notifyEnable in _platformManager.ActivePlatform!.GetComponentsInChildren<INotifyPlatformEnabled>(true))
            {
                notifyEnable.PlatformEnabled(_container);
            }
        }

        /// <summary>
        /// Disables all registered custom objects, as required by the selected <see cref="CustomPlatform"/>
        /// </summary>
        private void DestroyCustomObjects()
        {
            foreach (INotifyPlatformDisabled notifyDisable in _platformManager.ActivePlatform!.GetComponentsInChildren<INotifyPlatformDisabled>(true))
            {
                notifyDisable.PlatformDisabled();
            }
        }
    }
}