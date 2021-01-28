using System;

using IPA.Utilities;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin {


    internal class PlatformSpawnerGame : PlatformSpawner, IInitializable, IDisposable {

        [InjectOptional]
        private readonly BeatmapObjectCallbackController _beatmapObjectCallbackController;

        [InjectOptional]
        private readonly IDifficultyBeatmap _difficultyBeatmap;

        [InjectOptional]
        private readonly MultiplayerPlayersManager _multiplayerPlayersManager;

        private bool a360;
        private bool multiplayer;

        internal PlatformSpawnerGame(DiContainer container) {
            _container = container;
        }

        public void Initialize() {
            PlatformManager.Heart.SetActive(false);
            a360 = (_difficultyBeatmap?.parentDifficultyBeatmapSet.beatmapCharacteristic.requires360Movement).GetValueOrDefault();
            multiplayer = _multiplayerPlayersManager ? true : false;
            if ((a360 && _config.UseIn360) || (multiplayer && _config.UseInMultiplayer) || (!a360 && !multiplayer)) {
                ChangeToPlatform();
                if (multiplayer) {
                    _multiplayerPlayersManager.playerDidFinishEvent += HandlePlayerDidFinishEvent;
                    _multiplayerPlayersManager.activeLocalPlayerFacade?.introAnimator.StopAllCoroutines();
                    _multiplayerPlayersManager.inactivePlayerFacade?.introAnimator.StopAllCoroutines();
                    SpawnLightEffects();

                }
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

        private void SpawnLightEffects() {
            GameObject lightEffects = GameObject.Instantiate(PlatformManager.LightEffects);
            PlatformManager.SpawnedObjects.Add(lightEffects);
            foreach (LightSwitchEventEffect lightEffect in lightEffects.GetComponents<LightSwitchEventEffect>()) {
                lightEffect.SetField("_beatmapObjectCallbackController", _beatmapObjectCallbackController);
                lightEffect.SetField("_lightManager", _lightManager);
                lightEffect.SetField("_initialized", false);
            }
            lightEffects.SetActive(true);
        }
    }
}
