using System;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin {


    internal class PlatformSpawnerGame : PlatformSpawner, IInitializable, IDisposable {

        [InjectOptional]
        private readonly MultiplayerPlayersManager _multiplayerPlayersManager;

        private bool isMultiplayer;

        internal PlatformSpawnerGame(DiContainer container) {
            _container = container;
        }

        public void Initialize() {
            isMultiplayer = _multiplayerPlayersManager ? true : false;
            if ((isMultiplayer && _config.UseInMultiplayer) || !isMultiplayer) {
                if (_multiplayerPlayersManager != null) _multiplayerPlayersManager.playerDidFinishEvent += HandlePlayerDidFinishEvent;
                ChangeToPlatform();
                PlatformManager.Heart.SetActive(false);
            }
        }

        public void Dispose() {
            if (!isMultiplayer) {
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
