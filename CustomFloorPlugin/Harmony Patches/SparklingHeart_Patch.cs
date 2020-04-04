using HarmonyLib;


namespace CustomFloorPlugin.HarmonyPatches {


    /// <summary>
    /// This is a patch for the sparkling heart, it makes sure to allow the heart to react to light events, but only allows it to be magenta
    /// </summary>
    [HarmonyPatch(typeof(InstancedMaterialLightWithId))]
    [HarmonyPatch("ColorWasSet")]
    internal class SparklingHeart_Patch {
        private static readonly float magenta_red = UnityEngine.Color.magenta.r;
        private static readonly float magenta_green = UnityEngine.Color.magenta.g;
        private static readonly float magenta_blue = UnityEngine.Color.magenta.b;

        public static void Prefix(InstancedMaterialLightWithId __instance, ref UnityEngine.Color color) {
            if(__instance.gameObject.name == "<3") {
                color.r = magenta_red;
                color.g = magenta_green;
                color.b = magenta_blue;
            }
        }
    }


    //This was needed to debug memory leak issues in Beat Saber 1.8.0
    //Turns out Unity is unreliable when calling OnDisables during object destruction.


    //using System.Collections.Generic;
    //using System.Runtime.CompilerServices;
    //using static CustomFloorPlugin.Utilities.Logging;


    //[HarmonyPatch(typeof(TubeBloomPrePassLight))]
    //[HarmonyPatch("FillMeshData")]
    //internal class Patchwork {


    //    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    //    public static bool Prefix(TubeBloomPrePassLight __instance, List<BloomPrePassLight.LightsDataItem> ____lightsDataItems) {
    //        if(__instance == null) {
    //            Log("Null instance found.");
    //            Log("\"Fixing it in post\" (tm)");
    //            foreach(BloomPrePassLight.LightsDataItem lights in ____lightsDataItems) {
    //                if(lights.lights.Contains(__instance)) {
    //                    lights.lights.Remove(__instance);
    //                }
    //            }
    //        }
    //        return true;
    //    }
    //}
}