using Harmony;

namespace CustomFloorPlugin.HarmonyPatches {
    [HarmonyPatch(typeof(EnvironmentOverrideSettingsPanelController))]
    [HarmonyPatch("HandleOverrideEnvironmentsToggleValueChanged")]
    internal class HandleOverrideEnvironmentsToggleValueChanged_EnvironmentOverrideSettingsPanelController_Patch {
        public static void Postfix() {
            EnvironmentSceneOverrider.OverrideEnvironmentScene();
        }
    }
}
