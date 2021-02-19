using UnityEngine;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Instatiable wrapper class for <see cref="LightRotationEventEffect"/>, to be used by mappers.
    /// </summary>
    public class RotationEventEffect : MonoBehaviour
    {
        public SongEventType eventType;
        public Vector3 rotationVector;
    }
}