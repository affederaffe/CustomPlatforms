using System;

using IPA.Utilities;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{

    internal class PlatformSpawnerGame : PlatformSpawner, IInitializable, IDisposable
    {
        [InjectOptional]
        private readonly BeatmapObjectCallbackController _beatmapObjectCallbackController;

        [InjectOptional]
        private readonly IDifficultyBeatmap _difficultyBeatmap;

        [InjectOptional]
        private readonly MultiplayerPlayersManager _multiplayerPlayersManager;

        private bool a360;
        private bool multiplayer;

        internal PlatformSpawnerGame(DiContainer container)
        {
            _container = container;
        }

        public void Initialize()
        {
            PlatformManager.Heart.SetActive(false);
            a360 = (_difficultyBeatmap?.parentDifficultyBeatmapSet.beatmapCharacteristic.requires360Movement).GetValueOrDefault();
            multiplayer = _multiplayerPlayersManager ? true : false;

            if (multiplayer)
            {
                if (_platformManager.GetIndexForType(PlatformType.Multiplayer) != 0)
                {
                    ChangeToPlatform(PlatformType.Multiplayer);
                    _multiplayerPlayersManager.playerDidFinishEvent += HandlePlayerDidFinishEvent;
                    _multiplayerPlayersManager.activeLocalPlayerFacade?.introAnimator.StopAllCoroutines();
                    _multiplayerPlayersManager.inactivePlayerFacade?.introAnimator.StopAllCoroutines();
                    SpawnLightEffects();
                }
            }
            else if (_platformManager.apiRequestIndex != -1 && (_platformManager.apiRequestedLevelId == _difficultyBeatmap.level.levelID || _platformManager.apiRequestIndex == 0))
            {
                ChangeToPlatform(_platformManager.apiRequestIndex);
            }
            else if (a360)
            {
                ChangeToPlatform(PlatformType.A360);
            }
            else if (!a360 && !multiplayer)
            {
                ChangeToPlatform(PlatformType.Singleplayer);
            }
            else
            {
                ChangeToPlatform(0);
            }
        }

        public void Dispose()
        {
            if (!multiplayer)
            {
                PlatformManager.Heart.SetActive(_config.ShowHeart);
                PlatformManager.Heart.GetComponent<InstancedMaterialLightWithId>().ColorWasSet(Color.magenta);
                if (_config.ShowInMenu)
                {
                    ChangeToPlatform(PlatformType.Singleplayer);
                }
                else
                {
                    ChangeToPlatform(0);
                }
                if (_platformManager.apiRequestIndex == 0)
                { // If a mod requests to disable the platform for the song, reset it when the level is finished
                    _platformManager.apiRequestIndex = -1;
                }
            }
        }

        private void HandlePlayerDidFinishEvent(LevelCompletionResults results)
        {
            ChangeToPlatform(0);
        }

        private void SpawnLightEffects()
        {
            GameObject lightEffects = GameObject.Instantiate(PlatformManager.LightEffects);
            PlatformManager.SpawnedObjects.Add(lightEffects);
            foreach (LightSwitchEventEffect lightEffect in lightEffects.GetComponents<LightSwitchEventEffect>())
            {
                lightEffect.SetField("_beatmapObjectCallbackController", _beatmapObjectCallbackController);
                lightEffect.SetField("_lightManager", _lightManager);
                lightEffect.SetField("_initialized", false);
            }
            lightEffects.SetActive(true);
        }
    }
}
