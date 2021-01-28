using UnityEngine;


namespace CustomFloorPlugin {
    public static partial class PlatformManager {

        /// <summary>
        /// This changes the Material of <see cref="Spectrogram"/>s.
        /// Somehow it does not get replaced when the <see cref="CustomPlatform"/> is spawned.
        /// This should not be a permanent solution and is likely to be removed in future Updates.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1812:Avoid unistantiated internal classes", Justification = "Instantiated by Unity")]
        private class SpectrogramMaterialSwapper : MonoBehaviour {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
            private void Update() {
                if (activePlatform != null && activePlatform.GetComponentInChildren<Spectrogram>() != null) {
                    if (activePlatform.GetComponentInChildren<Spectrogram>().transform.childCount != 0) MaterialSwapper.ReplaceMaterials(activePlatform.gameObject);
                }
            }
        }
    }
}