using UnityEngine;


namespace CustomFloorPlugin 
{
    public class TrackRings : MonoBehaviour 
    {
        [Space]
        [Header("Rings")]
        public GameObject trackLaneRingPrefab;
        public int ringCount = 10;
        public float ringPositionStep = 2f;
        [Space]
        [Header("Rotation Effect")]
        public bool useRotationEffect = false;
        public SongEventType rotationSongEventType = SongEventType.RingsRotationEffect;
        [Space]
        public float rotationStep = 5f;
        public int rotationPropagationSpeed = 1;
        public float rotationFlexySpeed = 1f;
        [Space]
        public float startupRotationAngle = 0f;
        public float startupRotationStep = 10f;
        public int startupRotationPropagationSpeed = 10;
        public float startupRotationFlexySpeed = 0.5f;
        [Space]
        [Header("Step Effect")]
        public bool useStepEffect = false;
        public SongEventType stepSongEventType = SongEventType.RingsStepEffect;
        [Space]
        public float minPositionStep = 1f;
        public float maxPositionStep = 2f;
        public float moveSpeed = 1f;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnDrawGizmos() 
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.blue;
            for (int i = 0; i < ringCount; i++) 
            {
                Vector3 vector = i * ringPositionStep * Vector3.forward;
                if (trackLaneRingPrefab != null) 
                {
                    foreach (Renderer renderer in trackLaneRingPrefab.GetComponentsInChildren<Renderer>()) 
                    {
                        Gizmos.DrawCube(vector + renderer.bounds.center, renderer.bounds.size);
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