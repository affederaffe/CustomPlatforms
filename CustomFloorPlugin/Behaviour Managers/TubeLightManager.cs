using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using CustomFloorPlugin.Util;
using CustomUI.Utilities;
using Harmony;

namespace CustomFloorPlugin
{
    public class TubeLightManager : MonoBehaviour
    {
        public static void CreateAdditionalLightSwitchControllers()
        {
            LightSwitchEventEffect templateSwitchEffect = Resources.FindObjectsOfTypeAll<LightSwitchEventEffect>().FirstOrDefault();

            for (int i = 6; i < 16; i++)
            {
                LightSwitchEventEffect newSwitchEffect = ReflectionUtil.CopyComponent(templateSwitchEffect, typeof(LightSwitchEventEffect), typeof(LightSwitchEventEffect), templateSwitchEffect.gameObject) as LightSwitchEventEffect;
                
                newSwitchEffect.SetPrivateField("_lightManager", Plugin.LightWithId_Start_Patch.GameLightManager);
                newSwitchEffect.SetPrivateField("_lightsID", i);
                newSwitchEffect.SetPrivateField("_event", (BeatmapEventType)(i - 1));
            }
            UpdateEventTubeLightList();
        }
        public static void UpdateEventTubeLightList()
        {
            
            // Annotation 1


            LightSwitchEventEffect[] lightSwitchEvents = Resources.FindObjectsOfTypeAll<LightSwitchEventEffect>();
            foreach (LightSwitchEventEffect switchEffect in lightSwitchEvents)
            {
                switchEffect.SetPrivateField("_lightManager", Plugin.LightWithId_Start_Patch.GameLightManager);
            }
        }
    }
}
