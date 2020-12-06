using System;

using HarmonyLib;

using UnityEngine;

namespace CustomFloorPlugin.HarmonyPatches {


    [HarmonyPatch(typeof(MultiplayerPlayersManager))]
    [HarmonyPatch("SpawnPlayers")]
    internal class MultiplayerFinish_Patch {
        private static readonly AccessTools.FieldRef<MultiplayerPlayersManager, Action<LevelCompletionResults>> playerDidFinishEvent = AccessTools.FieldRefAccess<MultiplayerPlayersManager, Action<LevelCompletionResults>>("playerDidFinishEvent");

        public static void Postfix(MultiplayerPlayersManager __instance) {
            playerDidFinishEvent(__instance) += delegate { PlatformManager.ChangeToPlatform(0); };
        }
    }


    [HarmonyPatch(typeof(MultiplayerConnectedPlayerLevelFailController))]
    [HarmonyPatch("CheckIfPlayerFailed")]
    internal class MultiplayerFail_Patch {
        private static readonly AccessTools.FieldRef<MultiplayerConnectedPlayerLevelFailController, IConnectedPlayer> _connectedPlayer = AccessTools.FieldRefAccess<MultiplayerConnectedPlayerLevelFailController, IConnectedPlayer>("_connectedPlayer");
        private static readonly AccessTools.FieldRef<MultiplayerConnectedPlayerLevelFailController, bool> _wasActive = AccessTools.FieldRefAccess<MultiplayerConnectedPlayerLevelFailController, bool>("_wasActive");

        public static void Postfix(MultiplayerConnectedPlayerLevelFailController __instance, IConnectedPlayer player) {
            if (player.userId != _connectedPlayer(__instance).userId) {
                return;
            }
            if (_wasActive(__instance) && (!player.IsActiveOrFinished() || !player.isConnected)) {
                PlatformManager.ChangeToPlatform(0);
            }
        }

    }


    [HarmonyPatch(typeof(MultiplayerLobbyController))]
    [HarmonyPatch("ActivateMultiplayerLobby")]
    internal class MultiplayerLobbyHeartDeactivate_Patch {


        public static void Postfix() {
            PlatformManager.Heart.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(MultiplayerModeSelectionFlowCoordinator))]
    [HarmonyPatch("DidActivate")]
    internal class MultiplayerLobbyHeartActivate_Patch {


        public static void Prefix() {
            PlatformManager.Heart.SetActive(UI.Settings.ShowHeart);
            PlatformManager.Heart.GetComponent<InstancedMaterialLightWithId>().ColorWasSet(Color.magenta);
        }
    }
}
