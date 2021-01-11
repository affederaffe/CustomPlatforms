using System;

using HarmonyLib;

using UnityEngine;

using CustomFloorPlugin.Configuration;


namespace CustomFloorPlugin.HarmonyPatches {


    // @TODO In Multiplayer and Counters+ Settings, the Color[] of LightWithIdManager is empty causing all lights to stay black
    [HarmonyPatch(typeof(LightWithIdManager))]
    [HarmonyPatch(MethodType.Constructor)]
    internal class LightWithIdManagerSetColors_Patch {

        public static void Postfix(LightWithIdManager __instance) {
            string currentEnvironmentName = Utilities.BeatSaberSearching.GetCurrentEnvironment().name;
            if ((currentEnvironmentName.StartsWith("Multiplayer", StringComparison.Ordinal) && PluginConfig.Instance.UseInMultiplayer) || currentEnvironmentName.StartsWith("Tutorial", StringComparison.Ordinal)) {
                ColorScheme scheme = GlobalCollection.PDM.playerData.colorSchemesSettings.GetOverrideColorScheme();
                Color[] colors = new Color[] {
                    scheme.environmentColor0,
                    scheme.environmentColor0,
                    scheme.environmentColor0,
                    scheme.environmentColor0,
                    scheme.environmentColor1,
                    scheme.environmentColor1,
                    scheme.environmentColor1,
                    scheme.environmentColor1,
                    scheme.obstaclesColor,
                    scheme.obstaclesColor,
                    scheme.obstaclesColor,
                    scheme.obstaclesColor,
                    scheme.saberAColor,
                    scheme.saberAColor,
                    scheme.saberAColor,
                    scheme.saberAColor,
                    scheme.saberBColor,
                    scheme.saberBColor,
                    scheme.saberBColor,
                    scheme.saberBColor,
                };
                for (int i = 0; i < colors.Length; i++) {
                    __instance.SetColorForId(i, colors[i]);
                }
            }
        }
    }
}
