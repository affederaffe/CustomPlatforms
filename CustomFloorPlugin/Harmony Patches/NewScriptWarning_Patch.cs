using BeatSaberMarkupLanguage;

using HarmonyLib;

using HMUI;

using CustomFloorPlugin.UI;


namespace CustomFloorPlugin.HarmonyPatches {


    /// <summary>
    /// A Harmony Patch that presents the <see cref="NewScriptWarningFlowCoordinator"/> before the Main Menu if new Scripts are found
    /// </summary>
    [HarmonyPatch(typeof(MainMenuViewController))]
    [HarmonyPatch("DidActivate")]
    internal class NewScriptWarning_Patch {

        internal static NewScriptWarningFlowCoordinator _newScriptWarningFlowCoordinator;

        private static bool runOnce = false;

        public static void Postfix() {
            if (PlatformLoader.newScriptsFound && !runOnce) {
                runOnce = true;
                BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(_newScriptWarningFlowCoordinator, null, ViewController.AnimationDirection.Horizontal, true, false);
            }
        }
    }
}
