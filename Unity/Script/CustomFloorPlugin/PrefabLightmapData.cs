using UnityEngine;


namespace CustomFloorPlugin
{
    public class PrefabLightmapData : MonoBehaviour
    {
        // ReSharper disable InconsistentNaming
        public Renderer[]? m_Renderers;
        public Vector4[]? m_LightmapOffsetScales;
        public Texture2D[]? m_Lightmaps;
        // ReSharper restore InconsistentNaming
    }
}