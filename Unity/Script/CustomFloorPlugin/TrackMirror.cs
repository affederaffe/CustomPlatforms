using UnityEngine;


namespace CustomFloorPlugin
{
    [RequireComponent(typeof(MeshRenderer))]
    public class TrackMirror : MonoBehaviour
    {
       public Texture? normalTexture;
       public float bumpIntensity;
       public bool enableDirt;
       public Texture? dirtTexture;
       public float dirtIntensity;
    }
}