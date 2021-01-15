using HarmonyLib;

namespace CustomFloorPlugin.HarmonyPatches {


    /// <summary>
    /// A Harmony Patch that returns the correct <see cref="EnvironmentInfoSO"/>, as requested by the user through the UI.
    /// </summary>
    [HarmonyPatch(typeof(BeatmapEnvironmentHelper))]
    [HarmonyPatch("GetEnvironmentInfo")]
    internal static class EnvironmentOverride_Patch {


        internal static bool preventOverride;

        public static bool Prefix(ref EnvironmentInfoSO __result) {
            EnvironmentInfoSO environmentInfo = EnvironmentSceneOverrider.supportedEnvironmentInfos[UI.EnvironmentOverrideListView.EnvOr];
            if (environmentInfo != null && !preventOverride) {
                preventOverride = false;
                __result = environmentInfo;
                return false;
            }
            else {
                return true;
            }
        }
    }
}
