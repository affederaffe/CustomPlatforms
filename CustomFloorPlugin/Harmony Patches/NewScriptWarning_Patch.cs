using BeatSaberMarkupLanguage;

using HarmonyLib;

using HMUI;

using Zenject;

using CustomFloorPlugin.UI;


namespace CustomFloorPlugin.HarmonyPatches {

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
