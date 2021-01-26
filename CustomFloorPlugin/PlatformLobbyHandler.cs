using CustomFloorPlugin.Configuration;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin {


    internal class PlatformLobbyHandler : IInitializable {

        [Inject]
        private readonly PluginConfig _config;

        [Inject]
        private readonly PlatformSpawnerMenu _platformSpawner;

        [Inject]
        private readonly IMultiplayerSessionManager _multiplayerSessionManager;

        public void Initialize() {
            _multiplayerSessionManager.connectedEvent += HandleConnected;
            _multiplayerSessionManager.disconnectedEvent += HandleDisconnected;
        }

        private void HandleConnected() {
            _platformSpawner.ChangeToPlatform(0);
            PlatformManager.Heart.SetActive(false);
        }

        private void HandleDisconnected(DisconnectedReason reason) {
            PlatformManager.Heart.SetActive(_config.ShowHeart);
            PlatformManager.Heart.GetComponent<InstancedMaterialLightWithId>().ColorWasSet(Color.magenta);
            if (_config.ShowInMenu) {
                _platformSpawner.ChangeToPlatform();
            }
        }
    }
}
