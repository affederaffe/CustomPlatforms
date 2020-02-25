using Harmony;

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
}
