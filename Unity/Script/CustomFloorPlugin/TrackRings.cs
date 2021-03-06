using UnityEngine;


namespace CustomFloorPlugin
{
    public class TrackRings : MonoBehaviour
    {
        [Space]
        [Header("Rings")]
        public GameObject? trackLaneRingPrefab;
        public int ringCount = 10;
        public float ringPositionStep = 2f;
        [Space]
        [Header("Rotation Effect")]
        public bool useRotationEffect;
        public SongEventType rotationSongEventType = SongEventType.RingsRotationEffect;
        [Space]
        public float rotationStep = 5f;
        public int rotationPropagationSpeed = 1;
        public float rotationFlexySpeed = 1f;
        [Space]
        public float startupRotationAngle;
        public float startupRotationStep = 10f;
        public int startupRotationPropagationSpeed = 10;
        public float startupRotationFlexySpeed = 0.5f;
        [Space]
        [Header("Step Effect")]
        public bool useStepEffect;
        public SongEventType stepSongEventType = SongEventType.RingsStepEffect;
        [Space]
        public float minPositionStep = 1f;
        public float maxPositionStep = 2f;
        public float moveSpeed = 1f;

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.blue;
            for (int i = 0; i < ringCount; i++)
            {
                Vector3 vector = i * ringPositionStep * Vector3.forward;
                if (trackLaneRingPrefab is not null)
                {
                    foreach (Renderer renderer in trackLaneRingPrefab.GetComponentsInChildren<Renderer>())
                    {
                        Bounds bounds = renderer.bounds;
                        Gizmos.DrawCube(vector + bounds.center, bounds.size);
                    }
                }
                else
                {
                    Gizmos.DrawCube(vector, Vector3.one * 0.5f);
                }
            }
        }
    }
}