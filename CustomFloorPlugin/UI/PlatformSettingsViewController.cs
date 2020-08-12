using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomFloorPlugin
{
    internal class PlatformSettingsViewController : BSMLResourceViewController
    {

        public override string ResourceName => "CustomFloorPlugin.UI.PlatformSettings.bsml";

        [UIValue("env-arrangement")]
        public string Arrangement
        {
            get => EnvironmentArranger.arrangement.ToString();
            set {
                EnvironmentArranger.arrangement = Enum.TryParse(value, out EnvironmentArranger.Arrangement arrangement) ? arrangement : EnvironmentArranger.arrangement;
                Plugin.config.SetInt("Settings", "EnvironmentArrangement", (int)EnvironmentArranger.arrangement);
            }
        }

        [UIValue("arrangement-list")]
        public List<object> arrangements = Enum.GetNames(typeof(EnvironmentArranger.Arrangement)).ToList<object>();

        [UIValue("always-show-feet")]
        public bool AlwaysShowFeet
        {
            get => EnvironmentHider.showFeetOverride;
            set {
                EnvironmentHider.showFeetOverride = value;
                Plugin.config.SetBool("Settings", "AlwaysShowFeet", EnvironmentHider.showFeetOverride);
            }
        }

        [UIValue("environment-override")]
        public string EnvOverride
        {
            get => EnvironmentSceneOverrider.overrideMode.ToString();
            set
            {
                EnvironmentSceneOverrider.overrideMode = Enum.TryParse(value, out EnvironmentSceneOverrider.EnvOverrideMode envOverride) ? envOverride : EnvironmentSceneOverrider.overrideMode;

                EnvironmentSceneOverrider.OverrideEnvironmentScene();
                Plugin.config.SetInt("Settings", "EnvironmentOverrideMode", (int)EnvironmentSceneOverrider.overrideMode);
            }
        }

        [UIValue("environment-override-list")]
        public List<object> envOverrideModes = Enum.GetNames(typeof(EnvironmentSceneOverrider.EnvOverrideMode)).ToList<object>();

    }
}