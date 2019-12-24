using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;
namespace CustomFloorPlugin.Harmony_Patches
{
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
}
