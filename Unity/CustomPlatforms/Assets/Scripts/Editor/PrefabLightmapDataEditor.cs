using System.Collections.Generic;

using CustomFloorPlugin;

using UnityEditor;
using UnityEngine;


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

        Lightmapping.Bake();

        PrefabLightmapData[] prefabs = FindObjectsOfType<PrefabLightmapData>();

        foreach (PrefabLightmapData instance in prefabs)
        {
            GameObject gameObject = instance.gameObject;
            List<Renderer> renderers = new List<Renderer>();
            List<Vector4> lightmapOffsetScales = new List<Vector4>();
            List<Texture2D> lightmaps = new List<Texture2D>();

            GenerateLightmapInfo(gameObject, renderers, lightmapOffsetScales, lightmaps);

            // ReSharper disable InconsistentNaming
            instance.m_Renderers = renderers.ToArray();
            instance.m_LightmapOffsetScales = lightmapOffsetScales.ToArray();
            instance.m_Lightmaps = lightmaps.ToArray();
            // ReSharper restore InconsistentNaming

            var targetPrefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            if (targetPrefab != null)
                PrefabUtility.ReplacePrefab(gameObject, targetPrefab);
        }
    }

    static void GenerateLightmapInfo(GameObject root, List<Renderer> rendererList, List<Vector4> lightmapOffsetScaleList, List<Texture2D> lightmaps)
    {
        foreach (MeshRenderer renderer in root.GetComponentsInChildren<MeshRenderer>())
        {
            if (renderer.lightmapIndex != -1)
            {
                Texture2D lightmap = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapColor;
                rendererList.Add(renderer);
                lightmapOffsetScaleList.Add(renderer.lightmapScaleOffset);
                lightmaps.Add(lightmap);
            }
        }
    }
}
