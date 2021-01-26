using System;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin {


    internal class PlatformSpawnerGame : PlatformSpawner, IInitializable, IDisposable {

        [Inject]
        private readonly Color?[] _colors;

        [Inject]
        private readonly IDifficultyBeatmap _difficultyBeatmap;

        [InjectOptional]
        private readonly MultiplayerPlayersManager _multiplayerPlayersManager;

        private bool a360;
        private bool multiplayer;

        internal PlatformSpawnerGame(DiContainer container) {
            _container = container;
        }

        public void Initialize() {
            a360 = _difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.requires360Movement;
            multiplayer = _multiplayerPlayersManager ? true : false;
            if ((a360 && _config.UseIn360) || (multiplayer && _config.UseInMultiplayer) || (!a360 && !multiplayer)) {
                if (_multiplayerPlayersManager != null) {
                    _multiplayerPlayersManager.playerDidFinishEvent += HandlePlayerDidFinishEvent;
                    _multiplayerPlayersManager.activeLocalPlayerFacade?.introAnimator.StopAllCoroutines();
                    _multiplayerPlayersManager.inactivePlayerFacade?.introAnimator.StopAllCoroutines();
                    for (int i = 0; i < _colors.Length; i++) {
                        _lightManager.SetColorForId(i, _colors[i].Value);
                    }
                }
                ChangeToPlatform();
                PlatformManager.Heart.SetActive(false);
            }
        }

        public void Dispose() {
            if (!multiplayer) {
                PlatformManager.Heart.SetActive(_config.ShowHeart);
                PlatformManager.Heart.GetComponent<InstancedMaterialLightWithId>().ColorWasSet(Color.magenta);
                if (_config.ShowInMenu) {
                    ChangeToPlatform();
                }
            }
        }

        private void HandlePlayerDidFinishEvent(LevelCompletionResults results) {
            ChangeToPlatform(0);
        }
    }
}
