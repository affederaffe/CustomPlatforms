using System;

using BeatSaberMarkupLanguage.Attributes;

using CustomFloorPlugin.Configuration;

using static CustomFloorPlugin.Utilities.Logging;


namespace CustomFloorPlugin.UI {


    /// <summary>
    /// Interface between the UI and the remainder of the plugin<br/>
    /// Abuses getters to inline showing values, and setters to perform relevant actions<br/>
    /// Not intended to hold extensive logic!<br/>
    /// Why is everything here public? I don't know... -.-
    /// </summary>
    internal class Settings {

        /// <summary>
        /// Hover hint of load-custom-scripts
        /// </summary>
        [UIValue("LoadingCustomScriptsText")]
        public const string loadingCustomScriptsText = "Loading Custom Scripts \nUse this at your own risk! \nOnly use scripts of trusted sources!";

        /// <summary>
        /// Hover hint for use-in-360
        /// </summary>
        [UIValue("UseIn360Text")]
        public const string useIn360Text = "Toggle if Custom Platforms is used in 360°-Levels \n!Not supported!";


        /// <summary>
        /// Hover hint of use-in-multiplayer
        /// </summary>
        [UIValue("UseInMultiplayerText")]
        public const string useInMultiplayerText = "Toggle if Custom Platforms is used in Multiplayer \n!Not supported!";


        /// <summary>
        /// Determines if the feet icon is shown even if the platform would normally hide them<br/>
        /// Forwards the current choice to the UI, and the new choice to the plugin
        /// </summary>
        [UIValue("always-show-feet")]
        public static bool AlwaysShowFeet {
            get => PluginConfig.Instance.AlwaysShowFeet;
            set => PluginConfig.Instance.AlwaysShowFeet = value;
        }


        /// <summary>
        /// Should the heart next to the logo be visible?<br/>
        /// Forwards the current choice to the UI, and the new choice to the plugin
        /// </summary>
        [UIValue("show-heart")]
        public static bool ShowHeart {
            get => PluginConfig.Instance.ShowHeart;
            set => PluginConfig.Instance.ShowHeart = value;
        }
        internal static event Action<bool> ShowHeartChanged = delegate (bool value) {
            Log("ShowHeart value changed. Notifying listeners.\nNew value: " + value);
        };


        /// <summary>
        /// Should this Plugin load CustomScripts?
        /// Forwards the current choice to the UI, and the new choice to the plugin
        /// </summary>
        [UIValue("load-custom-scripts")]
        public static bool LoadCustomScripts {
            get => PluginConfig.Instance.LoadCustomScripts;
            set => PluginConfig.Instance.LoadCustomScripts = value;
        }


            /// <summary>
            /// Should this Plugin spawn a Custom Platform in 360°-Levels?
            /// Forwards the current choice to the UI, and the new choice to the plugin
            /// </summary>
            [UIValue("use-in-360")]
        public static bool UseIn360 {
            get => PluginConfig.Instance.UseIn360;
            set => PluginConfig.Instance.UseIn360 = value;
        }


        /// <summary>
        /// Should this Plugin spawn a Custom Platform in Multiplayer?
        /// Forwards the current choice to the UI, and the new choice to the plugin
        /// </summary>
        [UIValue("use-in-multiplayer")]
        public static bool UseInMultiplayer {
            get => PluginConfig.Instance.UseInMultiplayer;
            set => PluginConfig.Instance.UseInMultiplayer = value;
        }
    }
}