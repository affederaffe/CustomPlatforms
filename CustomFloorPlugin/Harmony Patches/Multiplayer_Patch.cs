/*using System;
using System.Collections.Generic;

using CustomFloorPlugin.Configuration;

using UnityEngine;

using HarmonyLib;


namespace CustomFloorPlugin.HarmonyPatches {


    /// <summary>
    /// A Harmony Patch that hides the CustomPlatform when the player fails or finished the level.
    /// </summary>
    [HarmonyPatch(typeof(MultiplayerPlayersManager))]
    [HarmonyPatch("SpawnPlayers")]
    internal class MultiplayerFinish_Patch {

        internal static PluginConfig config;
        internal static PlatformSpawner spawner;

        private static readonly AccessTools.FieldRef<MultiplayerPlayersManager, Action<LevelCompletionResults>> playerDidFinishEvent = AccessTools.FieldRefAccess<MultiplayerPlayersManager, Action<LevelCompletionResults>>("playerDidFinishEvent");

        public static void Postfix(MultiplayerPlayersManager __instance) {
            if (config.UseInMultiplayer) {
                if (__instance.activeLocalPlayerFacade != null) {
                    playerDidFinishEvent(__instance) += delegate {
                        spawner.ChangeToPlatform(0);

                        foreach (GameObject gameObject in MultiplayerIntroHidePlatform_Patch.constructions) {
                            gameObject?.SetActive(true);
                        }
                    };
                }
                else {
                    spawner.ChangeToPlatform(0);
                }
            }
        }
    }

    /// <summary>
    /// A Harmony Patch that deactivates the Intro Animation and hides the player's construction
    /// </summary>
    [HarmonyPatch(typeof(MultiplayerLocalActivePlayerIntroAnimator))]
    [HarmonyPatch("AnimateCoroutine")]
    internal class MultiplayerIntroHidePlatform_Patch {

        internal static PluginConfig config;
        internal static PlatformManager platformManager;
        internal static List<GameObject> constructions;

        public static void Prefix(ref float animationDurationMultiplier) {
            if (config.UseInMultiplayer) {
                animationDurationMultiplier = 0f;
                constructions = new List<GameObject>() {
                    GameObject.Find("MultiplayerDuelConnectedPlayerController(Clone)/Construction"),
                    GameObject.Find("MultiplayerDuelConnectedPlayerController(Clone)/Lasers"),
                    GameObject.Find("MultiplayerDuelLocalActivePlayerController(Clone)/IsActiveObjects/Construction/ConstructionL"),
                    GameObject.Find("MultiplayerDuelLocalActivePlayerController(Clone)/IsActiveObjects/Construction/ConstructionR"),
                    GameObject.Find("MultiplayerDuelLocalActivePlayerController(Clone)/IsActiveObjects/Lasers"),

                    GameObject.Find("MultiplayerLocalActivePlayerController(Clone)/IsActiveObjects/Construction/ConstructionL"),
                    GameObject.Find("MultiplayerLocalActivePlayerController(Clone)/IsActiveObjects/Construction/ConstructionR"),
                    GameObject.Find("MultiplayerLocalActivePlayerController(Clone)/IsActiveObjects/Lasers"),
                    GameObject.Find("MultiplayerLocalActivePlayerController(Clone)/IsActiveObjects/PlatformEnd"),
                    GameObject.Find("MultiplayerLocalActivePlayerController(Clone)/IsActiveObjects/CenterRings"),
                    GameObject.Find("MultiplayerLocalActivePlayerController(Clone)/IsActiveObjects/DirectionalLights"),
                };

                if (platformManager.CurrentPlatform.hideDefaultPlatform) {
                    constructions.Add(GameObject.Find("MultiplayerDuelConnectedPlayerController(Clone)/Construction/PlayersPlace"));
                    constructions.Add(GameObject.Find("MultiplayerLocalActivePlayerController(Clone)/IsActiveObjects/Construction/PlayersPlace"));
                }

                foreach (GameObject gameObject in constructions) {
                    gameObject?.SetActive(false);
                }
            }
        }
    }
}
*/