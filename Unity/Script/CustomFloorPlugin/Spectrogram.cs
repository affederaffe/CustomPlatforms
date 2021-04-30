using UnityEngine;


namespace CustomFloorPlugin 
{
    public class Spectrogram : MonoBehaviour 
    {
        public GameObject? columnPrefab;
        public Vector3 separator = Vector3.forward;
        public float minHeight = 1f;
        public float maxHeight = 10f;
        public float columnWidth = 1f;
        public float columnDepth = 1f;

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.green;

            for (int i = -64; i < 64; i++)
            {
                Vector3 zOffset = i * separator;
                if (columnPrefab != null)
                {
                    foreach (Renderer r in columnPrefab.GetComponentsInChildren<Renderer>())
                    {
                        Bounds bounds = r.bounds;
                        Gizmos.DrawCube(zOffset + bounds.center, bounds.size);
                    }
                }
                else
                {
                    Gizmos.DrawCube(zOffset, Vector3.one * 0.5f);
                }
            }
        }
    }
}