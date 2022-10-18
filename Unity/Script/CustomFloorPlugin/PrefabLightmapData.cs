using UnityEngine;


namespace CustomFloorPlugin
{
    public class PrefabLightmapData : MonoBehaviour
    {
        // ReSharper disable InconsistentNaming
        public Renderer[]? renderInfo_renderer;
        public int[]? renderInfo_lightmapIndex;
        public Vector4[]? renderInfo_lightmapOffsetScale;

        public Texture2D[]? m_Lightmaps;
        public Texture2D[]? m_LightmapsDir;
        public Texture2D[]? m_ShadowMasks;

        public Light[]? lightInfo_light;
        public int[]? lightInfo_lightmapBaketype;
        public int[]? lightInfo_mixedLightingMode;
        // ReSharper restore InconsistentNaming
    }
}