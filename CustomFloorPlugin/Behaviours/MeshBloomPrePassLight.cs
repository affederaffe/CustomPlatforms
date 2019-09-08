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

//public class MeshBloomPrePassLightPatch
//{
//    [HarmonyPatch(typeof(MeshBloomPrePassLight))]
//    [HarmonyPatch("color", MethodType.Setter)]
//    public static bool Prefix(MeshBloomPrePassLight __instance, Color _color)
//    {
//        __instance.color = _color;
//        if (__instance.renderer.material != null) __instance.renderer.material.color = _color;
//        return false;
//    }
//}