using Harmony;
using System.Reflection;
using UnityEngine;

namespace CustomFloorPlugin.HarmonyPatches {
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
    public class MenuTransitionsHelper_RestartGame_Patch {

        public static void Prefix() {
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
    /// <summary>
    /// After the settings have been applied for the first time, I see myself currently forced to move everthing into DontDestroyOnLoad.
    /// This is not reversed after loading, but shouldn't matter.
    /// Objects normally don't reside in there for transparency reasons
    /// 
    /// Note to self: removing them from the list of persistent scenes *should* get them off beatsabers radar
    /// </summary>
    [HarmonyPatch(typeof(GameScenesManager))]
    [HarmonyPatch("ClearAndOpenScenes")]
    public class GameScenesManager_ClearAndOpenScenes_Patch {
        static System.Collections.Generic.List<UnityEngine.GameObject> PlatformManagerDumpGameObjects = null, PlatformUIDumpGameObjects = null;
        public static void Prefix() {
            try {
                PlatformManagerDumpGameObjects = new System.Collections.Generic.List<UnityEngine.GameObject>();
                foreach(UnityEngine.GameObject gameObject in UnityEngine.SceneManagement.SceneManager.GetSceneByName("PlatformManagerDump").GetRootGameObjects()) {
                    PlatformManagerDumpGameObjects.Add(gameObject);
                    UnityEngine.Object.DontDestroyOnLoad(gameObject);
                }
                PlatformUIDumpGameObjects = new System.Collections.Generic.List<UnityEngine.GameObject>();
                foreach(UnityEngine.GameObject gameObject in UnityEngine.SceneManagement.SceneManager.GetSceneByName("PlatformUIDump").GetRootGameObjects()) {
                    PlatformUIDumpGameObjects.Add(gameObject);
                    UnityEngine.Object.DontDestroyOnLoad(gameObject);
                }
            } catch(System.ArgumentException e) {
                Plugin.Log(e);
            }
        }
    }
    [HarmonyPatch(typeof(InstancedMaterialLightWithId))]
    [HarmonyPatch("ColorWasSet")]
    public class InstancedMaterialLightWithId_ColorWasSet_Patch {
        static Color half_magenta = new Color(Color.magenta.r, Color.magenta.g, Color.magenta.b, 0.5f);
        public static bool Prefix(InstancedMaterialLightWithId __instance, MaterialPropertyBlockColorSetter ____materialPropertyBlockColorSetter, ref Color color) {
            if(__instance.gameObject.name == "<3(Clone)") {
                color = new Color(color.r * color.a, color.g * color.a, color.b * color.a, color.a);
                return true;
            }
            return true;
        }
    }
}
