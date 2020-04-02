using HarmonyLib;


namespace CustomFloorPlugin.HarmonyPatches {


    /// <summary>
    /// This patch notifies the <see cref="EnvironmentSceneOverrider"/> to deactivate if BeatSaber tries to load a specific environment
    /// </summary>
    [HarmonyPatch(typeof(EnvironmentOverrideSettingsPanelController))]
    [HarmonyPatch("HandleOverrideEnvironmentsToggleValueChanged")]
    internal class HandleOverrideEnvironmentsToggleValueChanged_EnvironmentOverrideSettingsPanelController_Patch {
        public static void Postfix(bool isOn) {
            EnvironmentSceneOverrider.SetEnabled(!isOn);
        }
    }
}
