﻿using System.Collections.Generic;

using HarmonyLib;


namespace CustomFloorPlugin.HarmonyPatches
{
    /// <summary>
    /// Fixes a base game bug
    /// Explanation: Everytime a light is unregistered, it's being added to <see cref="_lightsToUnregister"/> which is then iterated over in LateUpdate and removed from a List for every id.
    /// Problem: The lights are never removed from <see cref="_lightsToUnregister"/>, so the list gets longer and longer causing lags.
    /// Solution: Clear the list after LateUpdate is called.
    /// </summary>
    [HarmonyPatch(typeof(LightWithIdManager), nameof(LightWithIdManager.LateUpdate))]
    internal class LightWithIdManagerPatch
    {
        public static void Postfix(ref List<ILightWithId> ____lightsToUnregister)
        {
            ____lightsToUnregister.Clear();
        }
    }
}