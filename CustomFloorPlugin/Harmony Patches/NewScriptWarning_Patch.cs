using HarmonyLib;

using BeatSaberMarkupLanguage;
using HMUI;

namespace CustomFloorPlugin.HarmonyPatches {

    [HarmonyPatch(typeof(MainMenuViewController))]
    [HarmonyPatch("DidActivate")]
    internal class NewScriptWarning_Patch {


        public static void Postfix(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            if (PlatformLoader.newScriptsFound) {
                Utilities.Logging.Log("FlowCoordinator should be presented");
                BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(UI.PlatformUI.NewScriptWarningFlowCoordinator, null, ViewController.AnimationDirection.Horizontal, true, false);
            }
        }
    }
}
