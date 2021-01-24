using CustomFloorPlugin.Configuration;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin {


    internal class PlatformMultiplayerHandler : MonoBehaviour {

        [Inject]
        private readonly PluginConfig _config;

        [Inject]
        private readonly IMultiplayerSessionManager _multiplayerSessionManager;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void Start() {
            _multiplayerSessionManager.connectedEvent += HandleConnected;
            _multiplayerSessionManager.disconnectedEvent += HandleDisconnected;
        }

        private void HandleConnected() {
            PlatformManager.Heart.SetActive(false);
        }

        private void HandleDisconnected(DisconnectedReason reason) {
            PlatformManager.Heart.SetActive(_config.ShowHeart);
            PlatformManager.Heart.GetComponent<InstancedMaterialLightWithId>().ColorWasSet(Color.magenta);
            Destroy(this);
        }
    }
}
