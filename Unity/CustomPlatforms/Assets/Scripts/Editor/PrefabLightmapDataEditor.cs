using System.Collections.Generic;

using CustomFloorPlugin;

using UnityEditor;
using UnityEngine;


public class PrefabLightmapDataEditor : MonoBehaviour
{
    [MenuItem("Assets/Bake Prefab Lightmaps")]
    static void GenerateLightmapInfo()
    {
        if (Lightmapping.giWorkflowMode != Lightmapping.GIWorkflowMode.OnDemand)
        {
            Debug.LogError("ExtractLightmapData requires that you have baked you lightmaps and Auto mode is disabled.");
            return;
        }
        Lightmapping.Bake();

        PrefabLightmapData[] prefabs = FindObjectsOfType<PrefabLightmapData>();

        foreach (var instance in prefabs)
        {
            var gameObject = instance.gameObject;
            var renderers = new List<Renderer>();
            var lightmapOffsetScales = new List<Vector4>();
            var lightmaps = new List<Texture2D>();

            GenerateLightmapInfo(gameObject, renderers, lightmapOffsetScales, lightmaps);

            instance.renderers = renderers.ToArray();
            instance.lightmapOffsetScales = lightmapOffsetScales.ToArray();
            instance.lightmaps = lightmaps.ToArray();

            var targetPrefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            if (targetPrefab != null)
            {
                PrefabUtility.ReplacePrefab(gameObject, targetPrefab);
            }
        }
    }

    static void GenerateLightmapInfo(GameObject root, List<Renderer> rendererList, List<Vector4> lightmapOffsetScaleList, List<Texture2D> lightmaps)
    {
        var renderers = root.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
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
