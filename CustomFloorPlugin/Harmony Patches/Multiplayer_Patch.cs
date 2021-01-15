using System;
using System.Collections.Generic;
using System.Reflection;

using CustomFloorPlugin.Configuration;
using CustomFloorPlugin.Extensions;

using HarmonyLib;

using UnityEngine;

using static CustomFloorPlugin.GlobalCollection;
using static CustomFloorPlugin.Utilities.BeatSaberSearching;


namespace CustomFloorPlugin.HarmonyPatches {


    /// <summary>
    /// A Harmony Patch that hides the CustomPlatform when the player fails or finished the level.
    /// </summary>
    [HarmonyPatch(typeof(MultiplayerPlayersManager))]
    [HarmonyPatch("SpawnPlayers")]
    internal class MultiplayerFinish_Patch {

        private static readonly AccessTools.FieldRef<MultiplayerPlayersManager, Action<LevelCompletionResults>> playerDidFinishEvent = AccessTools.FieldRefAccess<MultiplayerPlayersManager, Action<LevelCompletionResults>>("playerDidFinishEvent");

        public static void Postfix(MultiplayerPlayersManager __instance) {
            if (PluginConfig.Instance.UseInMultiplayer) {
                playerDidFinishEvent(__instance) += (LevelCompletionResults results) => {
                    PlatformManager.ChangeToPlatform(0);

                    foreach (GameObject gameObject in MultiplayerIntroHidePlatform_Patch.constructions) {
                        gameObject?.SetActive(true);
                    }
                };
            }
        }
    }


    /// <summary>
    /// A Harmony Patch that deactivates the Heart whenever ther player joins a Lobby.
    /// </summary>
    [HarmonyPatch(typeof(MultiplayerLobbyController))]
    [HarmonyPatch("ActivateMultiplayerLobby")]
    internal class MultiplayerLobbyHeartDeactivate_Patch {

        public static void Postfix() {
            PlatformManager.Heart.SetActive(false);
        }
    }


    /// <summary>
    /// A little hacky Harmony Patch that unhides the Heart whenever the player leaves a Multiplayer Lobby or Game.<br></br>
    /// When this happens, the <see cref="MultiplayerModeSelectionFlowCoordinator"/> is called, and the Heart activated.
    /// </summary>
    [HarmonyPatch(typeof(MultiplayerModeSelectionFlowCoordinator))]
    [HarmonyPatch("DidActivate")]
    internal class MultiplayerLobbyHeartActivate_Patch {

        public static void Postfix() {
            typeof(LightWithIdMonoBehaviour).GetField("_lightManager", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(PlatformManager.Heart.GetComponent<InstancedMaterialLightWithId>(), FindLightWithIdManager(GetCurrentEnvironment()));
            PlatformManager.Heart.SetActive(PluginConfig.Instance.ShowHeart);
            PlatformManager.Heart.GetComponent<InstancedMaterialLightWithId>().ColorWasSet(Color.magenta);
        }
    }


    /// <summary>
    /// A Harmony Patch that deactivates the Intro Animation and hides the player's construction
    /// </summary>
    [HarmonyPatch(typeof(MultiplayerLocalActivePlayerIntroAnimator))]
    [HarmonyPatch("AnimateCoroutine")]
    internal class MultiplayerIntroHidePlatform_Patch {

        internal static List<GameObject> constructions;

        public static void Prefix(ref float animationDurationMultiplier) {
            if (PluginConfig.Instance.UseInMultiplayer) {
                animationDurationMultiplier = 0f;
                Hide();
            }
        }

        private static void Hide() {

            constructions = new List<GameObject> {

                // Duel Layout
                GameObject.Find("MultiplayerDuelConnectedPlayerController(Clone)/Construction"),
                GameObject.Find("MultiplayerDuelConnectedPlayerController(Clone)/Lasers"),

                GameObject.Find("MultiplayerDuelLocalActivePlayerController(Clone)/IsActiveObjects/Construction/ConstructionL"),
                GameObject.Find("MultiplayerDuelLocalActivePlayerController(Clone)/IsActiveObjects/Construction/ConstructionR"),
                GameObject.Find("MultiplayerDuelLocalActivePlayerController(Clone)/IsActiveObjects/Lasers"),

                // Normal Layout
                GameObject.Find("MultiplayerLocalActivePlayerController(Clone)/IsActiveObjects/Construction/ConstructionL"),
                GameObject.Find("MultiplayerLocalActivePlayerController(Clone)/IsActiveObjects/Construction/ConstructionR"),
                GameObject.Find("MultiplayerLocalActivePlayerController(Clone)/IsActiveObjects/Lasers"),
                GameObject.Find("MultiplayerLocalActivePlayerController(Clone)/IsActiveObjects/PlatformEnd"),
                GameObject.Find("MultiplayerLocalActivePlayerController(Clone)/IsActiveObjects/CenterRings"),
                GameObject.Find("MultiplayerLocalActivePlayerController(Clone)/IsActiveObjects/DirectionalLights")
            };

            if (PlatformManager.CurrentPlatform.hideDefaultPlatform) {
                constructions.Add(GameObject.Find("MultiplayerDuelConnectedPlayerController(Clone)/Construction/PlayersPlace"));
                constructions.Add(GameObject.Find("MultiplayerLocalActivePlayerController(Clone)/IsActiveObjects/Construction/PlayersPlace"));
            }

            foreach (GameObject gameObject in constructions) {
                gameObject?.SetActive(false);
            }
        }
    }

    // @TODO In Multiplayer and Counters+ Settings, the Color[] of LightWithIdManager is empty causing all lights to stay black
    /// <summary>
    /// A Harmony Patch that prevents lights to stay black in Multiplayer and the Tutorial / Counters+ Settings
    /// </summary>
    [HarmonyPatch(typeof(LightWithIdManager))]
    [HarmonyPatch(MethodType.Constructor)]
    internal class LightWithIdManagerSetColors_Patch {

        public static void Postfix(LightWithIdManager __instance) {
            string currentEnvironmentName = GetCurrentEnvironment().name;
            if ((currentEnvironmentName.StartsWith("Multiplayer", STR_INV) && PluginConfig.Instance.UseInMultiplayer) || currentEnvironmentName.StartsWith("Tutorial", STR_INV)) {
                __instance.FillManager();
            }
        }
    }
}
