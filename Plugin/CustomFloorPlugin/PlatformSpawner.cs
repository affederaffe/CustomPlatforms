using System;
using System.Threading;
using System.Threading.Tasks;

using CustomFloorPlugin.Configuration;
using CustomFloorPlugin.Helpers;
using CustomFloorPlugin.Interfaces;
using SiraUtil.Tools;

using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Handles platform spawning and despawning
    /// </summary>
    public sealed class PlatformSpawner : IInitializable, IDisposable
    {
        private readonly SiraLog _siraLog;
        private readonly PluginConfig _config;
        private readonly AssetLoader _assetLoader;
        private readonly EnvironmentHider _environmentHider;
        private readonly PlatformManager _platformManager;
        private readonly GameScenesManager _gameScenesManager;
        private readonly LobbyGameState _lobbyGameState;

        private readonly Random _random;

        private bool _didTransition;
        private CancellationTokenSource? _cancellationTokenSource;

        internal DiContainer Container { private get; set; }

        internal CustomPlatform RandomPlatform => _platformManager.AllPlatforms[_random.Next(0, _platformManager.AllPlatforms.Count)];

        public PlatformSpawner(DiContainer container,
                               SiraLog siraLog,
                               PluginConfig config,
                               AssetLoader assetLoader,
                               EnvironmentHider environmentHider,
                               PlatformManager platformManager,
                               GameScenesManager gameScenesManager,
                               LobbyGameState lobbyGameState)
        {
            Container = container;
            _siraLog = siraLog;
            _config = config;
            _assetLoader = assetLoader;
            _environmentHider = environmentHider;
            _platformManager = platformManager;
            _gameScenesManager = gameScenesManager;
            _lobbyGameState = lobbyGameState;
            _random = new Random();
        }

        public void Initialize()
        {
            _gameScenesManager.transitionDidFinishEvent += OnTransitionDidFinish;
            _lobbyGameState.gameStateDidChangeAlwaysSentEvent += OnMultiplayerGameStateDidChange;
        }

        public void Dispose()
        {
            _gameScenesManager.transitionDidFinishEvent -= OnTransitionDidFinish;
            _lobbyGameState.gameStateDidChangeAlwaysSentEvent -= OnMultiplayerGameStateDidChange;
        }

        /// <summary>
        /// Decide which platform to change to based on the type of the <see cref="ScenesTransitionSetupDataSO"/>
        /// </summary> 
        private void OnTransitionDidFinish(ScenesTransitionSetupDataSO? setupData, DiContainer container)
        {
            _didTransition = true;
            CustomPlatform platform;
            switch (setupData)
            {
                case null when _lobbyGameState.gameState is not MultiplayerGameState.Lobby:
                case MenuScenesTransitionSetupDataSO:
                    Container = container;
                    _assetLoader.ToggleHeart(_config.ShowHeart);
                    platform = _config.ShowInMenu
                        ? _config.ShufflePlatforms
                            ? RandomPlatform
                            : _platformManager.SingleplayerPlatform
                        : _platformManager.DefaultPlatform;
                    break;
                case StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionSetupDataSO when standardLevelScenesTransitionSetupDataSO.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.requires360Movement:
                    Container = container;
                    _assetLoader.ToggleHeart(false);
                    platform = _platformManager.A360Platform;
                    break;
                case StandardLevelScenesTransitionSetupDataSO when _platformManager.APIRequestedPlatform is not null:
                    Container = container;
                    _assetLoader.ToggleHeart(false);
                    platform = _platformManager.APIRequestedPlatform;
                    break;
                case StandardLevelScenesTransitionSetupDataSO:
                case MissionLevelScenesTransitionSetupDataSO:
                case TutorialScenesTransitionSetupDataSO:
                    Container = container;
                    _assetLoader.ToggleHeart(false);
                    platform = _config.ShufflePlatforms
                        ? RandomPlatform
                        : _platformManager.SingleplayerPlatform;
                    break;
                case MultiplayerLevelScenesTransitionSetupDataSO:
                    platform = _platformManager.MultiplayerPlatform;
                    break;
                case BeatmapEditorScenesTransitionSetupDataSO:
                    _assetLoader.ToggleHeart(false);
                    platform = _platformManager.DefaultPlatform;
                    break;
                default:
                    return;
            }

            _ = ChangeToPlatformAsync(platform);
        }

        /// <summary>
        /// Despawns the current platform when entering a lobby and changing back when leaving
        /// </summary>
        private void OnMultiplayerGameStateDidChange(MultiplayerGameState multiplayerGameState)
        {
            CustomPlatform platform;
            switch (multiplayerGameState)
            {
                case MultiplayerGameState.None:
                    _assetLoader.ToggleHeart(_config.ShowHeart);
                    platform = _config.ShowInMenu
                        ? _config.ShufflePlatforms
                            ? RandomPlatform
                            : _platformManager.SingleplayerPlatform
                        : _platformManager.DefaultPlatform;
                    break;
                case MultiplayerGameState.Lobby:
                    _assetLoader.ToggleHeart(false);
                    platform = _platformManager.DefaultPlatform;
                    break;
                default:
                    return;
            }

            _ = ChangeToPlatformAsync(platform);
        }

        /// <summary>
        /// Changes to a specific <see cref="CustomPlatform"/>
        /// </summary>
        /// <param name="platform">The <see cref="CustomPlatform"/> to change to</param>
        public async Task ChangeToPlatformAsync(CustomPlatform platform)
        {
            try
            {
                if (platform == _platformManager.ActivePlatform && !_didTransition) return;

                _didTransition = false;
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
                CancellationToken cancellationToken = _cancellationTokenSource.Token;

                DestroyCustomObjects();
                _platformManager.ActivePlatform.gameObject.SetActive(false);
                _platformManager.ActivePlatform = platform;

                if (platform.isDescriptor)
                {
                    CustomPlatform? newPlatform = await _platformManager.CreatePlatformAsync(platform.fullPath);
                    if (newPlatform is not null)
                    {
                        _platformManager.AllPlatforms.Replace(platform, newPlatform);
                        UnityEngine.Object.Destroy(platform.gameObject);
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    if (newPlatform is null)
                    {
                        _ = ChangeToPlatformAsync(_platformManager.DefaultPlatform);
                        return;
                    }

                    _platformManager.ActivePlatform = newPlatform;
                }

                _siraLog.Info($"Switching to {_platformManager.ActivePlatform.name}");
                _environmentHider.HideObjectsForActivePlatform(Container);
                _platformManager.ActivePlatform.gameObject.SetActive(true);
                SpawnCustomObjects();
            }
            catch (OperationCanceledException) { }
        }

        /// <summary>
        /// Enables or spawns all registered custom objects, as required by the selected <see cref="CustomPlatform"/>
        /// </summary>
        private void SpawnCustomObjects()
        {
            if (_lobbyGameState.gameState is MultiplayerGameState.Game) _assetLoader.MultiplayerLightEffects.PlatformEnabled(Container);
            foreach (INotifyPlatformEnabled notifyEnable in _platformManager.ActivePlatform.GetComponentsInChildren<INotifyPlatformEnabled>(true))
                notifyEnable.PlatformEnabled(Container);
        }

        /// <summary>
        /// Disables all registered custom objects, as required by the selected <see cref="CustomPlatform"/>
        /// </summary>
        private void DestroyCustomObjects()
        {
            if (_lobbyGameState.gameState is MultiplayerGameState.Game) _assetLoader.MultiplayerLightEffects.PlatformDisabled();
            foreach (INotifyPlatformDisabled notifyDisable in _platformManager.ActivePlatform.GetComponentsInChildren<INotifyPlatformDisabled>(true))
                notifyDisable.PlatformDisabled();
        }
    }
}