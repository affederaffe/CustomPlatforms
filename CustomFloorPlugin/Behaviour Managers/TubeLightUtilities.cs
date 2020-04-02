using BS_Utils.Utilities;

using System.Linq;

using UnityEngine;


namespace CustomFloorPlugin {


    /// <summary>
    /// Utility Class. Provides functionality to create more <see cref="LightSwitchEventEffect"/>s and update their <see cref="LightManager"/> references<br/>
    /// [Potential Redundancy!]<br/>
    /// [Possible Semantic Errors!]
    /// </summary>
    internal static class TubeLightUtilities {


        /// <summary>
        /// Creates Additional <see cref="LightSwitchEventEffect"/>s<br/>
        /// [May create redundant/duplicate <see cref="LightSwitchEventEffect"/>s!]<br/>
        /// [May cover <see cref="BeatmapEventType"/>s that aren't supposed to be supported anymore!]<br/>
        /// [Possible Semantic Error!]
        /// </summary>
        internal static void CreateAdditionalLightSwitchControllers(LightWithIdManager lightManager) {
            LightSwitchEventEffect templateSwitchEffect = Resources.FindObjectsOfTypeAll<LightSwitchEventEffect>().FirstOrDefault();

            for(int i = 6; i < 16; i++) {
                //This Component is spawned onto a base game object and not cleaned up by Custom Platforms (in good faith that the game does so for us by unloading the environment scene)
                LightSwitchEventEffect newSwitchEffect = ReflectionUtil.CopyComponent(templateSwitchEffect, typeof(LightSwitchEventEffect), typeof(LightSwitchEventEffect), templateSwitchEffect.gameObject) as LightSwitchEventEffect;
                newSwitchEffect.SetPrivateField("_lightManager", lightManager);
                newSwitchEffect.SetPrivateField("_lightsID", i);
                newSwitchEffect.SetPrivateField("_event", (BeatmapEventType)(i - 1));
            }
        }
    }
}