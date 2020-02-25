using UnityEngine;

namespace CustomFloorPlugin {


    /// <summary>
    /// Instatiable wrapper class for <see cref="LightRotationEventEffect"/>, to be used by mappers.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1812:Avoid unistantiated internal classes", Justification = "Instantiated by Unity")]
    public class RotationEventEffect:MonoBehaviour {


        /// <summary>
        /// What <see cref="SongEventType"/> to react to
        /// </summary>
        [SerializeField]
        internal SongEventType eventType = default;


        /// <summary>
        /// <see cref="Vector3"/> of the rotation
        /// </summary>
        [SerializeField]
        internal Vector3 rotationVector = default;
    }
}
