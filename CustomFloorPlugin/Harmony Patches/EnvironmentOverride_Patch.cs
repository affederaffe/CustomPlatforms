using HarmonyLib;

namespace CustomFloorPlugin.HarmonyPatches {


    [HarmonyPatch(typeof(StandardLevelScenesTransitionSetupDataSO))]
    [HarmonyPatch("Init")]
    internal class EnvironmentOverride_Patch {


        public static void Prefix(ref OverrideEnvironmentSettings overrideEnvironmentSettings) {
            UI.Settings.UpdatePlayerData();
            if (!UI.Settings.PlayerData.overrideEnvironmentSettings.overrideEnvironments) {
                EnvironmentSceneOverrider.OverrideEnvironment(UI.PlatformsListView.EnvOr);
                overrideEnvironmentSettings = UI.Settings.PlayerData.overrideEnvironmentSettings;
            }
        }
    }
}
