using UnityEngine;


namespace CustomFloorPlugin
{
    [RequireComponent(typeof(MeshRenderer))]
    public class TrackMirror : MonoBehaviour
    {
       public Texture? normalTexture;
       public Vector2 normalUVScale = Vector2.one;
       public Vector2 normalUVOffset = Vector2.one;
       public float bumpIntensity;
       public bool enableDirt;
       public Texture? dirtTexture;
       public Vector2 dirtUVScale = Vector2.one;
       public Vector2 dirtUVOffset = Vector2.one;
       public float dirtIntensity;
       public Color tintColor = Color.white;
    }
}