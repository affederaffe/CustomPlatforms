using System;

using HarmonyLib;

using UnityEngine;

using CustomFloorPlugin.Configuration;


namespace CustomFloorPlugin.HarmonyPatches {


    [HarmonyPatch(typeof(MultiplayerPlayersManager))]
    [HarmonyPatch("SpawnPlayers")]
    internal class MultiplayerFinish_Patch {

        private static readonly AccessTools.FieldRef<MultiplayerPlayersManager, Action<LevelCompletionResults>> playerDidFinishEvent = AccessTools.FieldRefAccess<MultiplayerPlayersManager, Action<LevelCompletionResults>>("playerDidFinishEvent");

        public static void Postfix(MultiplayerPlayersManager __instance) {
            if (PluginConfig.Instance.UseInMultiplayer) {
                playerDidFinishEvent(__instance) += (LevelCompletionResults results) => {
                    PlatformManager.PlayersPlace.SetActive(false);
                    PlatformManager.ChangeToPlatform(0);
                };
            }
        }
    }

    [HarmonyPatch(typeof(MultiplayerLobbyController))]
    [HarmonyPatch("ActivateMultiplayerLobby")]
    internal class MultiplayerLobbyHeartDeactivate_Patch {

        public static void Postfix() {
            PlatformManager.Heart.SetActive(false);
            PlatformManager.PlayersPlace.SetActive(false);
        }
    }


    [HarmonyPatch(typeof(MultiplayerModeSelectionFlowCoordinator))]
    [HarmonyPatch("DidActivate")]
    internal class MultiplayerLobbyHeartActivate_Patch {

        public static void Postfix() {
            PlatformManager.Heart.SetActive(PluginConfig.Instance.ShowHeart);
            PlatformManager.Heart.GetComponent<InstancedMaterialLightWithId>().ColorWasSet(Color.magenta);
        }
    }

    [HarmonyPatch(typeof(MultiplayerLocalActivePlayerIntroAnimator))]
    [HarmonyPatch("AnimateCoroutine")]
    internal class MultiplayerIntroHidePlatform_Patch {

        public static void Prefix(ref float animationDurationMultiplier) {
            if (PluginConfig.Instance.UseInMultiplayer) {
                animationDurationMultiplier = 0f;
                Hide();
            }
        }

        private static void Hide() {
            GameObject[] constructions = new GameObject[] {

                // Duel Layout
                GameObject.Find("MultiplayerDuelConnectedPlayerController(Clone)/Construction"),
                GameObject.Find("MultiplayerDuelConnectedPlayerController(Clone)/Lasers"),
                GameObject.Find("MultiplayerDuelLocalActivePlayerController(Clone)/IsActiveObjects/Construction"),
                GameObject.Find("MultiplayerDuelLocalActivePlayerController(Clone)/IsActiveObjects/Lasers"),

                // Normal Layout
                GameObject.Find("MultiplayerLocalActivePlayerController(Clone)/IsActiveObjects/Construction"),
                GameObject.Find("MultiplayerLocalActivePlayerController(Clone)/IsActiveObjects/Lasers"),
                GameObject.Find("MultiplayerLocalActivePlayerController(Clone)/IsActiveObjects/PlatformEnd"),
                GameObject.Find("MultiplayerLocalActivePlayerController(Clone)/IsActiveObjects/CenterRings")
                //GameObject.Find("MultiplayerLocalActivePlayerController(Clone)/IsActiveObjects/DirectionalLights")
            };

            foreach (GameObject gameObject in constructions) {
                gameObject?.SetActive(false);
            }
        }
    }
}
