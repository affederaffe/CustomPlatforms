using BeatSaberMarkupLanguage;

using HarmonyLib;

using HMUI;

namespace CustomFloorPlugin.HarmonyPatches {

    [HarmonyPatch(typeof(MainMenuViewController))]
    [HarmonyPatch("DidActivate")]
    internal class NewScriptWarning_Patch {


        private static bool runOnce = false;

        public static void Postfix() {
            if (PlatformLoader.newScriptsFound && !runOnce) {
                runOnce = true;
                BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(UI.PlatformUI.NewScriptWarningFlowCoordinator, null, ViewController.AnimationDirection.Horizontal, true, false);
            }
        }
    }
}
