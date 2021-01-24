using UnityEngine;


namespace CustomFloorPlugin {


    public class TrackRings : MonoBehaviour {
        [Space]
        [Header("Rings")]
        public GameObject trackLaneRingPrefab;
        public int ringCount = 10;
        public float ringPositionStep = 2f;
        [Space]
        [Header("Rotation Effect")]
        public bool useRotationEffect = false;
        public SongEventType rotationSongEventType = SongEventType.RingsRotationEffect; // replace this with a easier to read enum
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
        public SongEventType stepSongEventType = SongEventType.RingsStepEffect; // replace this with a easier to read enum
        [Space]
        public float minPositionStep = 1f;
        public float maxPositionStep = 2f;
        public float moveSpeed = 1f;


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnDrawGizmos() {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.blue;
            Vector3 zOffset;
            for (int i = 0; i < ringCount; i++) {
                zOffset = i * ringPositionStep * Vector3.forward;
                if (trackLaneRingPrefab != null) {
                    foreach (Renderer r in trackLaneRingPrefab.GetComponentsInChildren<Renderer>()) {
                        Gizmos.DrawCube(zOffset + r.bounds.center, r.bounds.size);
                    }
                }
                else {
                    Gizmos.DrawCube(zOffset, Vector3.one * 0.5f);
                }
            }
        }
    }
}