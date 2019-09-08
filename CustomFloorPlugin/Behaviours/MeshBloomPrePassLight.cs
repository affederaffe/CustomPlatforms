using Harmony;
using UnityEngine;

// Token: 0x02000048 RID: 72
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
public class MeshBloomPrePassLight : TubeBloomPrePassLight
{
    public Renderer renderer;

    public void Init(Renderer renderer)
    {
        this.renderer = renderer;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _parametricBoxController.enabled = false;
    }

    public override void Refresh()
    {
        base.Refresh();
        renderer.material.color = color;

        _parametricBoxController.enabled = false;
    }
}

public class MeshBloomPrePassLightPatch
{
    [HarmonyPatch(typeof(TubeBloomPrePassLight))]
    [HarmonyPatch("color", MethodType.Setter)]
    public static bool Prefix(TubeBloomPrePassLight __instance, Color _color)
    {
        if (__instance is MeshBloomPrePassLight)
        {
            var mesh = (MeshBloomPrePassLight)__instance;
            __instance.color = _color;
            if (mesh.renderer.material != null) mesh.renderer.material.color = _color;
            return false;
        }
        return true;
    }
}