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
        public Renderer Renderer;
        public int LightmapIndex;
        public Vector4 LightmapOffsetScale;
    }

    public struct LightInfo
    {
        public Light Light;
        public int LightmapBaketype;
        public int MixedLightingMode;
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

        public Renderer[]? renderInfoRenderer;
        public int[]? renderInfoLightmapIndex;
        public Vector4[]? renderInfoLightmapOffsetScale;

        public Texture2D[]? lightmaps;
        public Texture2D[]? lightmapsDir;
        public Texture2D[]? shadowMasks;

        public Light[]? lightInfoLight;
        public int[]? lightInfoLightmapBakeType;
        public int[]? lightInfoMixedLightingMode;

        private LightmapData[]? _oldLightmaps;

        public void PlatformEnabled(DiContainer container)
        {
            Init();
        }

        public void PlatformDisabled()
        {
            if (_oldLightmaps is null)
                return;

            // Restore the old lightmaps
            LightmapSettings.lightmaps = _oldLightmaps;
        }

        private void Init()
        {
            // Perform an exhaustive check to see if we have enough of the baked lightmap data to proceed
            // This data is supplied when the editor script is run to bake the lightmaps data into the platform
            if (renderInfoRenderer is null || renderInfoRenderer.Length == 0 ||
                renderInfoLightmapIndex is null || renderInfoLightmapOffsetScale is null ||
                lightInfoLight is null || lightInfoLightmapBakeType is null || lightInfoMixedLightingMode is null ||
                lightmaps is null || lightmapsDir is null || shadowMasks is null)
                return;

            // Save old LightmapData for restoration
            _oldLightmaps = LightmapSettings.lightmaps.Clone() as LightmapData[] ?? Array.Empty<LightmapData>();

            // Create a new combined LightmapData for all recorded lightmaps with their index
            LightmapData[] newLightmaps = lightmaps.Select((_, i) => new LightmapData
            {
                lightmapColor = lightmaps[i],
                lightmapDir = lightmapsDir[i],
                shadowMask = shadowMasks[i]
            }).ToArray();

            LightmapData[] combinedLightmaps = LightmapSettings.lightmaps.Concat(newLightmaps).ToArray();

            // Determine if directional lighting is used
            bool directional = lightmapsDir!.All(static t => t != null);
            LightmapSettings.lightmapsMode = lightmapsDir.Length == lightmaps.Length && directional ? LightmapsMode.CombinedDirectional : LightmapsMode.NonDirectional;

            // Apply lighting maps to the renderers
            RendererInfo[] rendererInfo = renderInfoRenderer.Select((r, i) => new RendererInfo // Use the struct from the original code in case future versions will succeed
            {
                Renderer = r,
                LightmapIndex = renderInfoLightmapIndex[i] + _oldLightmaps.Length,
                LightmapOffsetScale = renderInfoLightmapOffsetScale[i]
            }).ToArray();
            ApplyRendererInfo(rendererInfo);

            LightInfo[] lightInfo = lightInfoLight.Select((l, i) => new LightInfo // Use the struct from the original code in case future versions will succeed
            {
                Light = l,
                LightmapBaketype = lightInfoLightmapBakeType[i],
                MixedLightingMode = lightInfoMixedLightingMode[i]
            }).ToArray();
            ApplyLightInfo(lightInfo);

            // Finally set the new light maps to the global light settings
            LightmapSettings.lightmaps = combinedLightmaps;
        }

        /// <summary>
        /// Applies the light map data to the renderer associated in the given structs
        /// </summary>
        private static void ApplyRendererInfo(IEnumerable<RendererInfo> rendererInfos)
        {
            foreach (RendererInfo rendererInfo in rendererInfos)
            {
                rendererInfo.Renderer.lightmapIndex = rendererInfo.LightmapIndex;
                rendererInfo.Renderer.lightmapScaleOffset = rendererInfo.LightmapOffsetScale;

                // Find common shaders
                foreach (Material? sharedMaterial in rendererInfo.Renderer.sharedMaterials.Where(static sm => sm != null))
                {
                    Shader? commonShader = Shader.Find(sharedMaterial.shader!.name);
                    if (commonShader is not null)
                        sharedMaterial.shader = commonShader;
                }

            }
        }

        /// <summary>
        /// Create a new LightBakingOutput for the given light and apply the associated light map info in the given structs
        /// </summary>
        private static void ApplyLightInfo(IEnumerable<LightInfo> lightInfos)
        {
            foreach (LightInfo lightInfo in lightInfos)
            {
                LightBakingOutput bakingOutput = new()
                {
                    isBaked = true,
                    lightmapBakeType = (LightmapBakeType)lightInfo.LightmapBaketype,
                    mixedLightingMode = (MixedLightingMode)lightInfo.MixedLightingMode
                };

                lightInfo.Light.bakingOutput = bakingOutput;
            }
        }

    }
}
