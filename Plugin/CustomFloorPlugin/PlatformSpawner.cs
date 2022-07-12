using System;
using System.Threading;
using System.Threading.Tasks;

using CustomFloorPlugin.Helpers;
using CustomFloorPlugin.Interfaces;

using JetBrains.Annotations;

using SiraUtil.Logging;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Handles platform spawning and despawning
    /// </summary>
    [UsedImplicitly]
    public sealed class PlatformSpawner : IInitializable, IDisposable
    {
        private readonly SiraLog _siraLog;
        private readonly System.Random _random;
        private readonly AssetLoader _assetLoader;
        private readonly MaterialSwapper _materialSwapper;
        private readonly EnvironmentHider _environmentHider;
        private readonly PlatformManager _platformManager;
        private readonly GameScenesManager _gameScenesManager;
        private readonly LobbyGameStateModel _lobbyGameStateModel;

        private MultiplayerGameState _prevGameState;
        private CancellationTokenSource? _cancellationTokenSource;
        private DiContainer _container = null!;

        public PlatformSpawner(SiraLog siraLog, System.Random random, MaterialSwapper materialSwapper, AssetLoader assetLoader, EnvironmentHider environmentHider, PlatformManager platformManager, GameScenesManager gameScenesManager, LobbyGameStateModel lobbyGameStateModel)
        {
            _siraLog = siraLog;
            _random = random;
            _materialSwapper = materialSwapper;
            _assetLoader = assetLoader;
            _environmentHider = environmentHider;
            _platformManager = platformManager;
            _gameScenesManager = gameScenesManager;
            _lobbyGameStateModel = lobbyGameStateModel;
        }

        private CustomPlatform RandomPlatform =>
            _platformManager.AllPlatforms.Count > PlatformManager.BuildInPlatformsCount
                ? _platformManager.AllPlatforms[_random.Next(PlatformManager.BuildInPlatformsCount, _platformManager.AllPlatforms.Count)]
                : _platformManager.DefaultPlatform;

        public void Initialize()
        {
            _gameScenesManager.transitionDidStartEvent += OnTransitionDidStart;
            _gameScenesManager.transitionDidFinishEvent += OnTransitionDidFinish;
            _lobbyGameStateModel.gameStateDidChangeAlwaysSentEvent += OnMultiplayerGameStateModelDidChange;
        }

        public void Dispose()
        {
            _gameScenesManager.transitionDidStartEvent -= OnTransitionDidStart;
            _gameScenesManager.transitionDidFinishEvent -= OnTransitionDidFinish;
            _lobbyGameStateModel.gameStateDidChangeAlwaysSentEvent -= OnMultiplayerGameStateModelDidChange;
        }

        /// <summary>
        /// Clean up before switching scenes
        /// </summary>
        // ReSharper disable once AsyncVoidMethod
        private async void OnTransitionDidStart(float aheadTime) => await ChangeToPlatformAsync(_platformManager.DefaultPlatform);

        /// <summary>
        /// Decide which platform to change to based on the type of the <see cref="ScenesTransitionSetupDataSO"/>
        /// </summary>
        // ReSharper disable once AsyncVoidMethod
        internal async void OnTransitionDidFinish(ScenesTransitionSetupDataSO? setupData, DiContainer container)
        {
            CustomPlatform platform = setupData switch
            {
                MenuScenesTransitionSetupDataSO or null when _lobbyGameStateModel.gameState == MultiplayerGameState.None => _platformManager.MenuPlatform,
                StandardLevelScenesTransitionSetupDataSO when _platformManager.APIRequestedPlatform is not null => _platformManager.APIRequestedPlatform,
                StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionSetupDataSO when standardLevelScenesTransitionSetupDataSO.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.requires360Movement => _platformManager.A360Platform,
                StandardLevelScenesTransitionSetupDataSO or MissionLevelScenesTransitionSetupDataSO or TutorialScenesTransitionSetupDataSO => _platformManager.SingleplayerPlatform,
                MultiplayerLevelScenesTransitionSetupDataSO when container.HasBinding<MultiplayerLocalActivePlayerFacade>() => _platformManager.MultiplayerPlatform,
                _ => _platformManager.DefaultPlatform
            };

            _container = container;
            _environmentHider.OnTransitionDidFinish(setupData, container);
            await ChangeToPlatformAsync(platform);
        }

        /// <summary>
        /// Despawns the current platform when entering a lobby and changing back when leaving
        /// </summary>
        // ReSharper disable once AsyncVoidMethod
        private async void OnMultiplayerGameStateModelDidChange(MultiplayerGameState multiplayerGameState)
        {
            CustomPlatform? platform = multiplayerGameState switch
            {
                MultiplayerGameState.None when _prevGameState == MultiplayerGameState.Lobby => _platformManager.MenuPlatform,
                MultiplayerGameState.Lobby when _prevGameState == MultiplayerGameState.None => _platformManager.DefaultPlatform,
                _ => null
            };

            _prevGameState = multiplayerGameState;
            if (platform is null) return;
            await ChangeToPlatformAsync(platform);
        }

        /// <summary>
        /// Changes to a specific <see cref="CustomPlatform"/>
        /// </summary>
        /// <param name="platform">The <see cref="CustomPlatform"/> to change to</param>
        public async Task ChangeToPlatformAsync(CustomPlatform platform)
        {
            if (platform == _platformManager.ActivePlatform) return;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = _cancellationTokenSource.Token;
            DestroyPlatform(_platformManager.ActivePlatform.gameObject);
            if (platform == _platformManager.RandomPlatform) platform = RandomPlatform;
            _platformManager.ActivePlatform = platform;
            if (platform.isDescriptor) platform = await ReplaceDescriptorAsync(platform);
            if (token.IsCancellationRequested) return;
            _platformManager.ActivePlatform = platform;
            _siraLog.Debug($"Switching to {platform.name}");
            _environmentHider.HideObjectsForPlatform(platform);
            SpawnPlatform(platform.gameObject);
        }

        /// <summary>
        /// Enables or spawns all registered custom objects, as required by the selected <see cref="CustomPlatform"/>
        /// </summary>
        private void SpawnPlatform(GameObject platform)
        {
            platform.gameObject.SetActive(true);
            _materialSwapper.ReplaceMaterials(platform.gameObject);
            if (_lobbyGameStateModel.gameState == MultiplayerGameState.Game)
                _assetLoader.MultiplayerLightEffects.PlatformEnabled(_container);
            foreach (INotifyPlatformEnabled notifyEnable in platform.GetComponentsInChildren<INotifyPlatformEnabled>(true))
                notifyEnable.PlatformEnabled(_container);
        }

        /// <summary>
        /// Disables all registered custom objects, as required by the selected <see cref="CustomPlatform"/>
        /// </summary>
        private void DestroyPlatform(GameObject platform)
        {
            if (_lobbyGameStateModel.gameState == MultiplayerGameState.Game)
                _assetLoader.MultiplayerLightEffects.PlatformDisabled();
            foreach (INotifyPlatformDisabled notifyDisable in platform.GetComponentsInChildren<INotifyPlatformDisabled>(true))
                notifyDisable.PlatformDisabled();
            platform.gameObject.SetActive(false);
        }

        private async Task<CustomPlatform> ReplaceDescriptorAsync(CustomPlatform descriptor)
        {
            CustomPlatform? platform = await _platformManager.CreatePlatformAsync(descriptor.fullPath);
            if (platform is null) return _platformManager.DefaultPlatform;
            _platformManager.AllPlatforms.Replace(descriptor, platform);
            UnityEngine.Object.Destroy(descriptor.gameObject);
            return platform;
        }
    }
}
