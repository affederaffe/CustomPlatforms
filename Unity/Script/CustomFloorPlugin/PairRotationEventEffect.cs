using UnityEngine;


namespace CustomFloorPlugin
{
    public class PairRotationEventEffect : MonoBehaviour
    {
        [Header("Rotation Effect")]
        public SongEventType eventL;
        public SongEventType eventR;
        public SongEventType switchOverrideRandomValuesEvent;
        public Transform tranformL;
        public Transform tranformR;
        public Vector3 rotationVector;
        [Space]
        public bool useZPositionForAngleOffset;
        public float zPositionAngleOffsetScale = 1f;
    }
}
