using HarmonyLib;


namespace CustomFloorPlugin.HarmonyPatches {


    /*/// <summary>
    /// A Harmony Patch that hides the CustomPlatform when the player fails or finished the level.
    /// </summary>
    [HarmonyPatch(typeof(MultiplayerPlayersManager))]
    [HarmonyPatch("SpawnPlayers")]
    internal class MultiplayerFinish_Patch {

        private static readonly AccessTools.FieldRef<MultiplayerPlayersManager, Action<LevelCompletionResults>> playerDidFinishEvent = AccessTools.FieldRefAccess<MultiplayerPlayersManager, Action<LevelCompletionResults>>("playerDidFinishEvent");

        public static void Postfix(MultiplayerPlayersManager __instance) {
            if (PluginConfig.Instance.UseInMultiplayer) {
                if (__instance.activeLocalPlayerFacade != null) {
                    playerDidFinishEvent(__instance) += delegate {
                        PlatformManager.ChangeToPlatform(0);

                        foreach (GameObject gameObject in MultiplayerIntroHidePlatform_Patch.constructions) {
                            gameObject?.SetActive(true);
                        }
                    };
                }
                else {
                    PlatformManager.ChangeToPlatform(0);
                }
            }
        }
    }


    /// <summary>
    /// A Harmony Patch that deactivates the Heart whenever the player joins a Lobby.
    /// </summary>
    [HarmonyPatch(typeof(MultiplayerLobbyController))]
    [HarmonyPatch("ActivateMultiplayerLobby")]
    internal class MultiplayerLobbyHeartDeactivate_Patch {

        public static void Postfix() {
            PlatformManager.Heart.SetActive(false);
        }
    }*/


    /// <summary>
    /// A Harmony Patch that deactivates the Intro Animation and hides the player's construction
    /// </summary>
    [HarmonyPatch(typeof(MultiplayerLocalActivePlayerIntroAnimator))]
    [HarmonyPatch("AnimateCoroutine")]
    internal class MultiplayerIntroHidePlatform_Patch {

        public static void Prefix(ref float animationDurationMultiplier) {
            animationDurationMultiplier = 0f;
        }
    }
}
