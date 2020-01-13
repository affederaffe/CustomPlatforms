using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using System.Collections.Generic;

namespace CustomFloorPlugin.UI {
    class Settings:PersistentSingleton<Settings> {
#pragma warning disable CS0649
        [UIParams]
        public BSMLParserParams parserParams;
#pragma warning restore CS0649
        [UIValue("always-show-feet")]
        public bool alwaysShowFeet {
            get {
                return Plugin.config.GetBool("Settings", "AlwaysShowFeet", false, true);
            }
            set {
                EnvironmentHider.showFeetOverride = value;
                Plugin.config.SetBool("Settings", "AlwaysShowFeet", EnvironmentHider.showFeetOverride);
            }
        }
        [UIValue("env-ovs")]
        public List<object> envOrs = EnvironmentSceneOverrider.OverrideModes();
        [UIValue("env-arrs")]
        public List<object> envArrs = EnvironmentArranger.RepositionModes();

        [UIValue("env-ov")]
        public EnvironmentSceneOverrider.EnvOverrideMode envOr {
            get {
                return (EnvironmentSceneOverrider.EnvOverrideMode)Plugin.config.GetInt("Settings", "EnvironmentOverrideMode", 0, true);
            }
            set {
                EnvironmentSceneOverrider.overrideMode = value;
                EnvironmentSceneOverrider.OverrideEnvironmentScene();
                Plugin.config.SetInt("Settings", "EnvironmentOverrideMode", (int)EnvironmentSceneOverrider.overrideMode);
            }
        }
        [UIValue("env-arr")]
        public EnvironmentArranger.Arrangement envArr {
            get {
                return (EnvironmentArranger.Arrangement)Plugin.config.GetInt("Settings", "EnvironmentArrangement", 0, true);
            }
            set {
                EnvironmentArranger.arrangement = value;
                Plugin.config.SetInt("Settings", "EnvironmentArrangement", (int)EnvironmentArranger.arrangement);
            }
        }
        [UIValue("show-heart")]
        public bool showHeart {
            get {
                return Plugin.config.GetBool("Settings", "ShowHeart", true, true);
            }
            set {
                PlatformManager.showHeart = value;
                Plugin.config.SetBool("Settings", "ShowHeart", PlatformManager.showHeart);
                PlatformManager.Heart.SetActive(value);
            }
        }
    }
}