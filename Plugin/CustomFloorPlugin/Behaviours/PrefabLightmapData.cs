using System;

using UnityEngine;
using UnityEngine.Serialization;

using Zenject;


namespace CustomFloorPlugin
{
    public class PrefabLightmapData : MonoBehaviour, INotifyPlatformEnabled
    {
        [FormerlySerializedAs("m_Renderers")] public Renderer[]? renderers;
        [FormerlySerializedAs("m_LightmapOffsetScales")] public Vector4[]? lightmapOffsetScales;
        [FormerlySerializedAs("m_Lightmaps")] public Texture2D[]? lightmaps;

        public void PlatformEnabled(DiContainer container)
        {
            enabled = renderers != null && lightmapOffsetScales != null && lightmaps != null && renderers.Length > 0 &&
                      renderers.Length != lightmapOffsetScales.Length && renderers.Length != lightmaps.Length &&
                      lightmapOffsetScales.Length != lightmaps.Length &&
                      renderers[renderers.Length - 1].lightmapIndex >= LightmapSettings.lightmaps.Length;
        }

        private void Update()
        {
            LightmapData[] lightmapData = LightmapSettings.lightmaps;
            LightmapData[] combinedLightmaps = new LightmapData[lightmaps!.Length + lightmapData.Length];

            Array.Copy(lightmapData, combinedLightmaps, lightmapData.Length);
            for (int i = 0; i < lightmaps.Length; i++)
            {
                combinedLightmaps[lightmapData.Length + i] = new LightmapData
                {
                    lightmapColor = lightmaps[i]
                };
            }

            ApplyRendererInfo(renderers!, lightmapOffsetScales!, lightmapData.Length);
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