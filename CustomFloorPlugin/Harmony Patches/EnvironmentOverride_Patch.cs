using HarmonyLib;

namespace CustomFloorPlugin.HarmonyPatches {


    [HarmonyPatch(typeof(StandardLevelScenesTransitionSetupDataSO))]
    [HarmonyPatch("Init")]
    class EnvironmentOverride_Patch {


        public static void Prefix(ref OverrideEnvironmentSettings overrideEnvironmentSettings) {
            if (!UI.Settings.PlayerData.overrideEnvironmentSettings.overrideEnvironments) {
                EnvironmentSceneOverrider.OverrideEnvironment(UI.Settings.EnvOr);
                overrideEnvironmentSettings = UI.Settings.PlayerData.overrideEnvironmentSettings;
            }
        }
    }
}
