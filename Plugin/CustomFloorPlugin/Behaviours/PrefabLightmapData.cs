using System;
using System.Collections.Generic;
using System.Linq;
using CustomFloorPlugin.Interfaces;
using UnityEngine;
using Zenject;


// ReSharper disable once CheckNamespace
namespace CustomFloorPlugin
{
    public struct RendererInfo
    {
        public Renderer renderer;
        public int lightmapIndex;
        public Vector4 lightmapOffsetScale;
    }
    public struct LightInfo
    {
        public Light light;
        public int lightmapBaketype;
        public int mixedLightingMode;
    }

    /// <summary>
    /// Enables the light mapping data saved in this PrefabLightmapData
    /// Adapted from https://github.com/Ayfel/PrefabLightmapping for use in Beat Saber
    /// </summary>
    public class PrefabLightmapData : MonoBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
    {
        // This version of Unity can't reconstruct structs or classes no matter how much you mark 
        // it up with SerializeField or Serializable. RendererInfo and LightInfo has to be expanded out.
        // Is this because of Unity's use of a flat-JSON serializer?! WTF

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

        private LightmapData[]? oldLightmaps;
        // ReSharper restore InconsistentNaming

        public void PlatformEnabled(DiContainer container)
        {
            Init();
        }

        public void PlatformDisabled()
        {
            if (this.oldLightmaps == null) 
                return;

            // Restore the old lightmaps
            LightmapSettings.lightmaps = this.oldLightmaps;
        }

        private void Init()
        {
            // Perform an exhaustive check to see if we have enough of the baked lightmap data to proceed
            // This data is supplied when the editor script is run to bake the lightmaps data into the platform
            if (renderInfo_renderer == null || renderInfo_renderer.Length == 0 ||
                renderInfo_lightmapIndex == null || renderInfo_lightmapOffsetScale == null ||
                lightInfo_light == null || lightInfo_lightmapBaketype == null || lightInfo_mixedLightingMode == null ||
                m_Lightmaps == null || m_LightmapsDir == null || m_ShadowMasks == null)
                return;

            // Save old LightmapData for restoration
            this.oldLightmaps = LightmapSettings.lightmaps.Clone() as LightmapData[] ?? Array.Empty<LightmapData>();

            // Create a new combined LightmapData for all recorded lightmaps with their index
            var newLightmaps = m_Lightmaps.Select((lm, i) => new LightmapData
            {
                lightmapColor = m_Lightmaps[i],
                lightmapDir = m_LightmapsDir[i],
                shadowMask = m_ShadowMasks[i],
            }).ToArray();

            var combinedLightmaps = LightmapSettings.lightmaps.Concat(newLightmaps).ToArray();

            // Determine if directional lighting is used
            bool directional = true;
            foreach (Texture2D t in m_LightmapsDir!)
            {
                if (t == null)
                {
                    directional = false;
                    break;
                }
            }
            LightmapSettings.lightmapsMode = (m_LightmapsDir.Length == m_Lightmaps.Length && directional) ? LightmapsMode.CombinedDirectional : LightmapsMode.NonDirectional;

            // Apply lighting maps to the renderers
            var rendererInfo = renderInfo_renderer.Select((r, i) => new RendererInfo // Use the struct from the original code in case future versions will succeeed
            {
                renderer = r,
                lightmapIndex = renderInfo_lightmapIndex[i] + this.oldLightmaps.Length,
                lightmapOffsetScale = renderInfo_lightmapOffsetScale[i],
            }).ToArray();
            ApplyRendererInfo(rendererInfo);

            var lightInfo = lightInfo_light.Select((l, i) => new LightInfo // Use the struct from the original code in case future versions will succeeed
            {
                light = l,
                lightmapBaketype = lightInfo_lightmapBaketype[i],
                mixedLightingMode = lightInfo_mixedLightingMode[i],
            }).ToArray();
            ApplyLightInfo(lightInfo);

            // Finally set the new light maps to the global light settings
            LightmapSettings.lightmaps = combinedLightmaps;
        }

        /// <summary>
        /// Applies the light map data to the renderer associated in the given structs
        /// </summary>
        private void ApplyRendererInfo(RendererInfo[] rendererInfos)
        {
            foreach (var rendererInfo in rendererInfos)
            {
                rendererInfo.renderer.lightmapIndex = rendererInfo.lightmapIndex;
                rendererInfo.renderer.lightmapScaleOffset = rendererInfo.lightmapOffsetScale;

                // Find common shaders
                foreach (var sharedMaterial in rendererInfo.renderer.sharedMaterials.Where(sm => sm != null))
                {
                    var commonShader = Shader.Find(sharedMaterial.shader!.name);
                    if (commonShader != null)
                    {
                        sharedMaterial.shader = commonShader;
                    }
                }

            }
        }

        /// <summary>
        /// Create a new LightBakingOutput for the given light and apply the associated light map info in the given structs
        /// </summary>
        private void ApplyLightInfo(LightInfo[] lightInfos)
        { 
            foreach (var lightInfo in lightInfos)
            {
                LightBakingOutput bakingOutput = new LightBakingOutput();
                bakingOutput.isBaked = true;
                bakingOutput.lightmapBakeType = (LightmapBakeType)lightInfo.lightmapBaketype;
                bakingOutput.mixedLightingMode = (MixedLightingMode)lightInfo.mixedLightingMode;

                lightInfo.light.bakingOutput = bakingOutput;
            }
        }

    }
}
