using UnityEngine;


namespace CustomFloorPlugin 
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class TubeLight : MonoBehaviour 
    {
        public enum LightsID
        {
            Static,
            BackLights,
            BigRingLights,
            LeftLasers,
            RightLasers,
            TrackAndBottom,
            Unused5,
            Unused6,
            Unused7,
            RingsRotationEffect,
            RingsStepEffect,
            Unused10,
            Unused11,
            RingSpeedLeft,
            RingSpeedRight,
            Unused14,
            Unused15
        }

        public float width = 0.5f;
        public float length = 1f;
        [Range(0, 1)]
        public float center = 0.5f;
        public Color color = Color.white;
        public LightsID lightsID = LightsID.Static;

        private void OnDrawGizmos() 
        {
            Gizmos.color = color;
            Gizmos.matrix = transform.localToWorldMatrix;
            Vector3 vector = Vector3.up * (0.5f - center) * length;
            Gizmos.DrawCube(vector, new Vector3(2f * width, length, 2f * width));
        }
    }
}