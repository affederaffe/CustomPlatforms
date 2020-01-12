using Harmony;
using System.Reflection;
using UnityEngine;

namespace CustomPlatforms.HarmonyPatches {
    internal static class Patcher {
        private static bool _runOnce = false;
        internal static void Patch() {
            if(_runOnce) {
                return;
            }
            HarmonyInstance.Create("com.rolopogo.customplatforms").PatchAll(Assembly.GetExecutingAssembly());
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
        public static void Prefix(InstancedMaterialLightWithId __instance, MaterialPropertyBlockColorSetter ____materialPropertyBlockColorSetter, ref Color color) {
            if(__instance.gameObject.name == "<3(Clone)") {
                color.r *= color.a;
                color.g *= color.a;
                color.b *= color.a;
            }
        }
    }
}
