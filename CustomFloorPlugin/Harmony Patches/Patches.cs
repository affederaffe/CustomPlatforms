using Harmony;
using System.Reflection;

namespace CustomFloorPlugin.HarmonyPatches
{
    internal static class Patcher {
        private static bool _runOnce = false;
        internal static void Patch() {
            if(_runOnce) {
                return;
            }
            HarmonyInstance.Create("com.rolopogo.customplatforms").PatchAll(Assembly.GetExecutingAssembly());
        }
    }
    [HarmonyPatch(typeof(MenuTransitionsHelper))]
    [HarmonyPatch("RestartGame", MethodType.Normal)]
    public class SettingsRefreshPatch
    {

        public static void Prefix()
        {
            System.Console.WriteLine("Restart of Game");
            PlatformManager.Instance.TempChangeToPlatform(0);
        }
    }
    [HarmonyPatch(typeof(EnvironmentOverrideSettingsPanelController))]
    [HarmonyPatch("HandleOverrideEnvironmentsToggleValueChanged")]
    public class EnviromentOverideSettings_Patch {
        static public void Postfix(OverrideEnvironmentSettings ____overrideEnvironmentSettings) {
            if(____overrideEnvironmentSettings.overrideEnvironments == true) {
                Plugin.Log("Enviroment Override On");
            }

            if(____overrideEnvironmentSettings.overrideEnvironments == false) {
                Plugin.Log("Enviroment Override Off");
            }
        }
    }

}
