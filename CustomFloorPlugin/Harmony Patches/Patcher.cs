/*using System.Reflection;

using HarmonyLib;


namespace CustomFloorPlugin.HarmonyPatches {


    /// <summary>
    /// Harmony Patcher, set to auto-detect
    /// </summary>
    internal static class Patcher {

        /// <summary>
        /// Tracks if the Patcher has run or not
        /// </summary>
        private static bool runOnce;

        /// <summary>
        /// The Harmony ID this Plugin uses
        /// </summary>
        private const string ID = "com.rolopogo.CustomFloorPlugin";

        /// <summary>
        /// The Harmony Instance for this Plugin
        /// </summary>
        private static Harmony Harmony;


        /// <summary>
        /// Used to patch the game, applies all patches.
        /// </summary>
        internal static void Patch() {
            if (!runOnce) {
                Harmony = new Harmony(ID);
                Harmony.PatchAll(Assembly.GetExecutingAssembly());
                runOnce = true;
            }
        }

        internal static void Unpatch() {
            Harmony.UnpatchAll(ID);
            runOnce = false;
        }
    }
}
*/