using BS_Utils.Utilities;
using System.Linq;
using UnityEngine;


namespace CustomPlatforms {
    public class TubeLightManager:MonoBehaviour {
        public static void CreateAdditionalLightSwitchControllers() {
            LightSwitchEventEffect templateSwitchEffect = Resources.FindObjectsOfTypeAll<LightSwitchEventEffect>().FirstOrDefault();

            for(int i = 6; i < 16; i++) {
                LightSwitchEventEffect newSwitchEffect = ReflectionUtil.CopyComponent(templateSwitchEffect, typeof(LightSwitchEventEffect), typeof(LightSwitchEventEffect), templateSwitchEffect.gameObject) as LightSwitchEventEffect;

                newSwitchEffect.SetPrivateField("_lightManager", PlatformManager.LightManager);
                newSwitchEffect.SetPrivateField("_lightsID", i);
                newSwitchEffect.SetPrivateField("_event", (BeatmapEventType)(i - 1));
            }
            UpdateEventTubeLightList();
        }
        public static void UpdateEventTubeLightList() {

            // Annotation 1


            LightSwitchEventEffect[] lightSwitchEvents = Resources.FindObjectsOfTypeAll<LightSwitchEventEffect>();
            foreach(LightSwitchEventEffect switchEffect in lightSwitchEvents) {
                switchEffect.SetPrivateField("_lightManager", PlatformManager.LightManager);
            }
        }
    }
}
