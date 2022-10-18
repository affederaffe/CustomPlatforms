using System.Collections.Generic;
using System.Linq;
using CustomFloorPlugin;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Saves the light mapping data into all the PrefabLightmapData components
/// Adapted from https://github.com/Ayfel/PrefabLightmapping for use in Beat Saber
/// </summary>
public class PrefabLightmapDataEditor : MonoBehaviour
{
    [MenuItem("Assets/Bake Prefab Lightmaps")]
    public static void GenerateLightmapInfo()
    {
        if (Lightmapping.giWorkflowMode != Lightmapping.GIWorkflowMode.OnDemand)
        {
            Debug.LogError("ExtractLightmapData requires that you have baked you lightmaps and Auto mode is disabled.");
            return;
        }

        // Do not auto bake whenever this function is run since the user will manually bake
        // Lightmapping.Bake(); 

        PrefabLightmapData[] prefabs = FindObjectsOfType<PrefabLightmapData>();

        foreach (PrefabLightmapData instance in prefabs)
        {
            var gameObject = instance.gameObject;
            var rendererInfos = new List<PrefabLightmapData.RendererInfo>();
            var lightmaps = new List<Texture2D>();
            var lightmapsDir = new List<Texture2D>();
            var shadowMasks = new List<Texture2D>();
            var lightsInfos = new List<PrefabLightmapData.LightInfo>();


            GenerateLightmapInfo(gameObject, rendererInfos, lightmaps, lightmapsDir, shadowMasks, lightsInfos);

            // ReSharper disable InconsistentNaming
            instance.renderInfo_renderer = rendererInfos.Select(r => r.renderer).ToArray();
            instance.renderInfo_lightmapIndex = rendererInfos.Select(r => r.lightmapIndex).ToArray();
            instance.renderInfo_lightmapOffsetScale = rendererInfos.Select(r => r.lightmapOffsetScale).ToArray();

            instance.m_Lightmaps = lightmaps.ToArray();
            instance.m_LightmapsDir = lightmapsDir.ToArray();
            instance.m_ShadowMasks = shadowMasks.ToArray();

            instance.lightInfo_light = lightsInfos.Select(l => l.light).ToArray();
            instance.lightInfo_lightmapBaketype = lightsInfos.Select(l => l.lightmapBaketype).ToArray();
            instance.lightInfo_mixedLightingMode = lightsInfos.Select(l => l.mixedLightingMode).ToArray();
            // ReSharper restore InconsistentNaming


            var targetPrefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            if (targetPrefab != null)
            {
                PrefabUtility.ReplacePrefab(gameObject, targetPrefab);
            }

        }
    }

    static void GenerateLightmapInfo(GameObject root, List<PrefabLightmapData.RendererInfo> rendererInfos, List<Texture2D> lightmaps, List<Texture2D> lightmapsDir, List<Texture2D> shadowMasks, List<PrefabLightmapData.LightInfo> lightsInfo)
    {

        foreach (MeshRenderer renderer in root.GetComponentsInChildren<MeshRenderer>())
        {
            if (renderer.lightmapIndex != -1)
            {
                PrefabLightmapData.RendererInfo info = new PrefabLightmapData.RendererInfo();
                info.renderer = renderer;
                if (renderer.lightmapScaleOffset != Vector4.zero)
                {
                    //1ibrium's pointed out this issue : https://docs.unity3d.com/ScriptReference/Renderer-lightmapIndex.html
                    if (renderer.lightmapIndex < 0 || renderer.lightmapIndex == 0xFFFE) continue;
                    info.lightmapOffsetScale = renderer.lightmapScaleOffset;

                    Texture2D lightmap = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapColor;
                    Texture2D lightmapDir = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapDir;
                    Texture2D shadowMask = LightmapSettings.lightmaps[renderer.lightmapIndex].shadowMask;

                    info.lightmapIndex = lightmaps.IndexOf(lightmap);
                    if (info.lightmapIndex == -1)
                    {
                        info.lightmapIndex = lightmaps.Count;
                        lightmaps.Add(lightmap);
                        lightmapsDir.Add(lightmapDir);
                        shadowMasks.Add(shadowMask);
                    }

                    rendererInfos.Add(info);
                }

                var lights = root.GetComponentsInChildren<Light>(true);

                foreach (Light l in lights)
                {
                    PrefabLightmapData.LightInfo lightInfo = new PrefabLightmapData.LightInfo();
                    lightInfo.light = l;
                    lightInfo.lightmapBaketype = (int)l.lightmapBakeType;
                    lightInfo.mixedLightingMode = (int)UnityEditor.LightmapEditorSettings.mixedBakeMode;
                    lightsInfo.Add(lightInfo);
                }
            }
        }
    }
}
