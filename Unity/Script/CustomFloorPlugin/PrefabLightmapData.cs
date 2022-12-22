using UnityEngine;


namespace CustomFloorPlugin
{
    public class PrefabLightmapData : MonoBehaviour
    {
        public Renderer[]? renderInfoRenderer;
        public int[]? renderInfoLightmapIndex;
        public Vector4[]? renderInfoLightmapOffsetScale;

        public Texture2D[]? lightmaps;
        public Texture2D[]? lightmapsDir;
        public Texture2D[]? shadowMasks;

        public Light[]? lightInfoLight;
        public int[]? lightInfoLightmapBakeType;
        public int[]? lightInfoMixedLightingMode;

        public struct RendererInfo
        {
            public Renderer Renderer;
            public int LightmapIndex;
            public Vector4 LightmapOffsetScale;
        }

        public struct LightInfo
        {
            public Light Light;
            public int LightmapBakeType;
            public int MixedLightingMode;
        }
    }
}
