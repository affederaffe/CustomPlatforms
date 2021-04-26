using System;

using UnityEngine;
using UnityEngine.Serialization;


namespace CustomFloorPlugin
{
    public class PrefabLightmapData : MonoBehaviour
    {
        [FormerlySerializedAs("m_Renderers")] public Renderer[]? renderers;
        [FormerlySerializedAs("m_LightmapOffsetScales")] public Vector4[]? lightmapOffsetScales;
        [FormerlySerializedAs("m_Lightmaps")] public Texture2D[]? lightmaps;

        private void Start()
        {
            ApplyLightmaps();
        }

        private void Update()
        {
            if (renderers != null && renderers.Length > 0 && renderers[renderers.Length - 1].lightmapIndex >= LightmapSettings.lightmaps.Length)
            {
                ApplyLightmaps();
            }
        }

        private void ApplyLightmaps()
        {
            if (renderers == null || lightmapOffsetScales == null || lightmaps == null ||
                renderers.Length <= 0 ||
                renderers.Length != lightmapOffsetScales.Length ||
                renderers.Length != lightmaps.Length ||
                lightmapOffsetScales.Length != lightmaps.Length)
            {
                return;
            }

            LightmapData[] lightmapDatas = LightmapSettings.lightmaps;
            LightmapData[] combinedLightmaps = new LightmapData[lightmaps.Length + lightmapDatas.Length];

            Array.Copy(lightmapDatas, combinedLightmaps, lightmapDatas.Length);
            for (int i = 0; i < lightmaps.Length; i++)
            {
                combinedLightmaps[lightmapDatas.Length + i] = new LightmapData
                {
                    lightmapColor = lightmaps[i]
                };
            }

            ApplyRendererInfo(renderers, lightmapOffsetScales, lightmapDatas.Length);
            LightmapSettings.lightmaps = combinedLightmaps;
        }


        private static void ApplyRendererInfo(Renderer[] renderers, Vector4[] lightmapOffsetScales, int lightmapIndexOffset)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                renderer.lightmapIndex = i + lightmapIndexOffset;
                renderer.lightmapScaleOffset = lightmapOffsetScales[i];
            }
        }
    }
}