using System;
using CustomFloorPlugin.Interfaces;
using UnityEngine;

using Zenject;


// ReSharper disable once CheckNamespace
namespace CustomFloorPlugin
{
    public class PrefabLightmapData : MonoBehaviour, INotifyPlatformEnabled
    {
        // ReSharper disable InconsistentNaming
        public Renderer[]? m_Renderers;
        public Vector4[]? m_LightmapOffsetScales;
        public Texture2D[]? m_Lightmaps;
        // ReSharper restore InconsistentNaming

        public void PlatformEnabled(DiContainer container)
        {
            enabled = m_Renderers is not null && m_LightmapOffsetScales is not null && m_Lightmaps is not null && m_Renderers.Length > 0 &&
                      m_Renderers.Length != m_LightmapOffsetScales.Length && m_Renderers.Length != m_Lightmaps.Length &&
                      m_LightmapOffsetScales.Length != m_Lightmaps.Length &&
                      m_Renderers[m_Renderers.Length - 1].lightmapIndex >= LightmapSettings.lightmaps.Length;
        }

        private void Update()
        {
            LightmapData[] lightmapData = LightmapSettings.lightmaps;
            LightmapData[] combinedLightmaps = new LightmapData[m_Lightmaps!.Length + lightmapData.Length];

            Array.Copy(lightmapData, combinedLightmaps, lightmapData.Length);
            for (int i = 0; i < m_Lightmaps.Length; i++)
            {
                combinedLightmaps[lightmapData.Length + i] = new LightmapData
                {
                    lightmapColor = m_Lightmaps[i]
                };
            }

            ApplyRendererInfo(m_Renderers!, m_LightmapOffsetScales!, lightmapData.Length);
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