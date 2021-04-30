using UnityEngine;
using UnityEngine.Serialization;


namespace CustomFloorPlugin
{
    public class PrefabLightmapData : MonoBehaviour
    {
        [FormerlySerializedAs("m_Renderers")] public Renderer[]? renderers;
        [FormerlySerializedAs("m_LightmapOffsetScales")] public Vector4[]? lightmapOffsetScales;
        [FormerlySerializedAs("m_Lightmaps")] public Texture2D[]? lightmaps;
    }
}