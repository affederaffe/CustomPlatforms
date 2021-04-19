using UnityEngine;


namespace CustomFloorPlugin 
{
    public class Spectrogram : MonoBehaviour 
    {
        public GameObject columnPrefab;
        public Vector3 separator = Vector3.forward;
        public float minHeight = 1f;
        public float maxHeight = 10f;
        public float columnWidth = 1f;
        public float columnDepth = 1f;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnDrawGizmos() 
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.green;
            for (int i = -64; i < 64; i++) 
            {
                Vector3 vector = i * separator;
                if (columnPrefab != null) 
                {
                    foreach (Renderer renderer in columnPrefab.GetComponentsInChildren<Renderer>()) 
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