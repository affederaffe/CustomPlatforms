using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using System;
using System.Collections.Generic;
using UnityEngine;
using static CustomFloorPlugin.Utilities.Logging;

namespace CustomFloorPlugin.UI {


    /// <summary>
    /// Interface between the UI and the remainder of the plugin<br/>
    /// Abuses getters to inline showing values, and setters to perform relevant actions<br/>
    /// Not intended to hold extensive logic!<br/>
    /// Why does this need to be a <see cref="PersistentSingleton{T}"/>? I don't know.<br/>
    /// Why is everything here public? I don't know... -.-
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1812:Avoid unistantiated internal classes", Justification = "Instantiated by Unity")]
    internal class Settings:PersistentSingleton<Settings> {


        /// <summary>
        /// I don't know why or where this is needed, oh well, it's BSML, let's not ask questions
        /// </summary>
        [UIParams]
        public static BSMLParserParams parserParams = null;


        /// <summary>
        /// The list of options the user can choose from for override modes
        /// </summary>
        [UIValue("env-ovs")]
        public static readonly List<object> envOrs = new List<object>() {
            EnvOverrideMode.Default,
            EnvOverrideMode.Nice,
            EnvOverrideMode.BigMirror,
            EnvOverrideMode.Triangle,
            EnvOverrideMode.KDA,
            EnvOverrideMode.Monstercat
        };


        /// <summary>
        /// The list of supported old environment configurations
        /// </summary>
        [UIValue("env-arrs")]
        public static readonly List<object> envArrs = EnvironmentArranger.RepositionModes();


        /// <summary>
        /// Override choice for platform base model/environment<br/>
        /// Forwards the current choice to the UI, and the new choice to the plugin
        /// </summary>
        [UIValue("env-ov")]
        public static EnvOverrideMode EnvOr {
            get {
                if(_EnvOr == null) {
                    //Wrapping value because 'Off' no longer exists
                    _EnvOr = (EnvOverrideMode)(Plugin.config.GetInt("Settings", "EnvironmentOverrideMode", 0, true) % 6);
                }
                return _EnvOr.Value;
            }
            set {
                if(value != _EnvOr.Value) {
                    Plugin.config.SetInt("Settings", "EnvironmentOverrideMode", (int)value);
                    _EnvOr = value;
                    EnvOrChanged(value);
                }
            }
        }
        private static EnvOverrideMode? _EnvOr;
        internal static Action<EnvOverrideMode> EnvOrChanged = delegate (EnvOverrideMode value) {
            Log("EnvOr value changed. Notifying listeners.\nNew value: " + value);
        };
        /// <summary>
        /// Override choice for platform base model/environment<br/>
        /// Forwards the current choice to the UI, and the new choice to the plugin
        /// </summary>
        [UIValue("env-arr")]
        public static EnvironmentArranger.EnvArrangement EnvArr {
            get {
                if(_EnvArr == null) {
                    _EnvArr = (EnvironmentArranger.EnvArrangement)Plugin.config.GetInt("Settings", "EnvironmentArrangement", 0, true);
                }
                return _EnvArr.Value;
            }
            set {
                if(value != _EnvArr.Value) {
                    Plugin.config.SetInt("Settings", "EnvironmentArrangement", (int)value);
                    _EnvArr = value;
                }
            }
        }
        private static EnvironmentArranger.EnvArrangement? _EnvArr;
        /// <summary>
        /// Determines if the feet icon is shown even if the platform would normally hide them<br/>
        /// Forwards the current choice to the UI, and the new choice to the plugin
        /// </summary>
        [UIValue("always-show-feet")]
        public static bool AlwaysShowFeet {
            get {
                if(_AlwaysShowFeet == null) {
                    _AlwaysShowFeet = Plugin.config.GetBool("Settings", "AlwaysShowFeet", false, true);
                }
                return _AlwaysShowFeet.Value;
            }
            set {
                if(value != _AlwaysShowFeet.Value) {
                    Plugin.config.SetBool("Settings", "AlwaysShowFeet", value);
                    _AlwaysShowFeet = value;
                }
            }
        }
        private static bool? _AlwaysShowFeet;
        /// <summary>
        /// Should the heart next to the logo be visible?<br/>
        /// Forwards the current choice to the UI, and the new choice to the plugin
        /// </summary>
        [UIValue("show-heart")]
        public static bool ShowHeart {
            get {
                if(_ShowHeart == null) {
                    _ShowHeart = Plugin.config.GetBool("Settings", "ShowHeart", true, true);
                }
                return _ShowHeart.Value;
            }
            set {
                if(value != _ShowHeart.Value) {
                    Plugin.config.SetBool("Settings", "ShowHeart", value);
                    _ShowHeart = value;
                    ShowHeartChanged(value);
                }
            }
        }
        private static bool? _ShowHeart;
        internal static Action<bool> ShowHeartChanged = delegate(bool value) {
            Log("ShowHeart value changed. Notifying listeners.\nNew value: " + value);
        };

        internal static PlayerData PlayerData {
            get {
                if(_PlayerData == null) {
                    _PlayerData = Resources.FindObjectsOfTypeAll<PlayerDataModelSO>()[0].playerData;
                }
                return _PlayerData;
            }
        }
        private static PlayerData _PlayerData;
    }
}