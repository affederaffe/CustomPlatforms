using Harmony;
using UnityEngine;

// Token: 0x02000048 RID: 72
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
public class MeshBloomPrePassLight:TubeBloomPrePassLight {
    public Renderer renderer;

    public void Init(Renderer renderer) {
        this.renderer = renderer;
    }

    protected override void OnEnable() {
        base.OnEnable();
        _parametricBoxController.enabled = false;
    }

    public override void Refresh() {
        base.Refresh();
        renderer.material.color = color;
        _parametricBoxController.enabled = false;
    }

}
//Don't use this, it's broken, it's here for reference
//[HarmonyPatch(typeof(MeshBloomPrePassLight))]
//[HarmonyPatch("color", MethodType.Setter)]
//public class MeshBloomPrePassLightPatch {
//    public static bool Prefix(MeshBloomPrePassLight __instance, Color ____color) {
//        __instance.color = ____color;
//        if(__instance.renderer.material != null) __instance.renderer.material.color = ____color;
//        return false;
//    }
//}
[HarmonyPatch(typeof(MeshBloomPrePassLight))]
[HarmonyPatch("color", MethodType.Setter)]
public class MeshBloomPrePassLightPatch {
    public static bool Prefix(MeshBloomPrePassLight __instance, Color value) {
        if(__instance.GetType() == typeof(MeshBloomPrePassLight)) {
            if(__instance.renderer.material != null) __instance.renderer.material.color = value;
            return false;
        }
        return true;
    }
}