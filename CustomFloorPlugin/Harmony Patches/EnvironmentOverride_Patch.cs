using HarmonyLib;

namespace CustomFloorPlugin.HarmonyPatches {


    [HarmonyPatch(typeof(BeatmapEnvironmentHelper))]
    [HarmonyPatch("GetEnvironmentInfo")]
    internal static class EnvironmentOverride_Patch {


        public static bool Prefix(ref EnvironmentInfoSO __result) {
            EnvironmentInfoSO environmentInfo = EnvironmentSceneOverrider.supportedEnvironmentInfos[UI.EnvironmentOverrideListView.EnvOr];
            if (environmentInfo != null) {
                __result = environmentInfo;
                return false;
            }
            else {
                return true;
            }
        }
    }
}
