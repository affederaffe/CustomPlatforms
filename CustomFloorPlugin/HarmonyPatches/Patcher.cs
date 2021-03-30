using System.Reflection;

using HarmonyLib;


namespace CustomFloorPlugin.HarmonyPatches
{
    internal static class Patcher
    {
        private const string kHarmonyId = "de.affederaffe.customplatforms";
        private static readonly Harmony harmony = new(kHarmonyId);

        internal static void Patch()
        {
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        internal static void Unpatch()
        {
            harmony.UnpatchAll(kHarmonyId);
        }
    }
}