using BeatSaberMarkupLanguage.Attributes;

using CustomFloorPlugin.Extensions;

using System;
using System.Collections.Generic;

using UnityEngine;

using static CustomFloorPlugin.GlobalCollection;
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
    internal class Settings : PersistentSingleton<Settings> {


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
        public static readonly List<object> envArrs = EnvironmentArranger.RepositionModes().ToBoxedList();


        /// <summary>
        /// Override choice for platform base model/environment<br/>
        /// Forwards the current choice to the UI, and the new choice to the plugin
        /// </summary>
        [UIValue("env-ov")]
        public static EnvOverrideMode EnvOr {
            get {
                if (_EnvOr == null) {
                    //Wrapping value because 'Off' no longer exists
                    _EnvOr = (EnvOverrideMode)(CONFIG.GetInt("Settings", "EnvironmentOverrideMode", 0, true) % 6);
                }
                return _EnvOr.Value;
            }
            set {
                if (value != _EnvOr.Value) {
                    CONFIG.SetInt("Settings", "EnvironmentOverrideMode", (int)value);
                    _EnvOr = value;
                    EnvOrChanged(value);
                }
            }
        }
        private static EnvOverrideMode? _EnvOr;
        internal static event Action<EnvOverrideMode> EnvOrChanged = delegate (EnvOverrideMode value) {
            Log("EnvOr value changed. Notifying listeners.\nNew value: " + value);
        };


        /// <summary>
        /// Override choice for platform base model/environment<br/>
        /// Forwards the current choice to the UI, and the new choice to the plugin
        /// </summary>
        [UIValue("env-arr")]
        public static EnvironmentArranger.EnvArrangement EnvArr {
            get {
                if (_EnvArr == null) {
                    _EnvArr = (EnvironmentArranger.EnvArrangement)CONFIG.GetInt("Settings", "EnvironmentArrangement", 0, true);
                }
                return _EnvArr.Value;
            }
            set {
                if (value != _EnvArr.Value) {
                    CONFIG.SetInt("Settings", "EnvironmentArrangement", (int)value);
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
                if (_AlwaysShowFeet == null) {
                    _AlwaysShowFeet = CONFIG.GetBool("Settings", "AlwaysShowFeet", false, true);
                }
                return _AlwaysShowFeet.Value;
            }
            set {
                if (value != _AlwaysShowFeet.Value) {
                    CONFIG.SetBool("Settings", "AlwaysShowFeet", value);
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
                if (_ShowHeart == null) {
                    _ShowHeart = CONFIG.GetBool("Settings", "ShowHeart", true, true);
                }
                return _ShowHeart.Value;
            }
            set {
                if (value != _ShowHeart.Value) {
                    CONFIG.SetBool("Settings", "ShowHeart", value);
                    _ShowHeart = value;
                    ShowHeartChanged(value);
                }
            }
        }
        private static bool? _ShowHeart;
        internal static event Action<bool> ShowHeartChanged = delegate (bool value) {
            Log("ShowHeart value changed. Notifying listeners.\nNew value: " + value);
        };


        [UIValue("load-custom-scripts")]
        public static bool LoadCustomScripts {
            get {
                if (_LoadCustomScripts == null) {
                    _LoadCustomScripts = CONFIG.GetBool("Settings", "LoadCustomScripts", false, true);
                }
                return _LoadCustomScripts.Value;
            }
            set {
                if (value != _LoadCustomScripts.Value) {
                    CONFIG.SetBool("Settings", "LoadCustomScripts", value);
                    _LoadCustomScripts = value;
                    PlatformManager.Reload();
                }
            }
        }
        private static bool? _LoadCustomScripts;


        /// <summary>
        /// Should this Plugin spawn a Custom Platform in Multiplayer?
        /// Forwards the current choice to the UI, and the new choice to the plugin
        /// </summary>
        [UIValue("use-in-multiplayer")]
        public static bool UseInMultiplayer {
            get {
                if (_UseInMultiplayer == null) {
                    _UseInMultiplayer = CONFIG.GetBool("Settings", "UseInMultiplayer", false, true);
                }
                return _UseInMultiplayer.Value;
            }
            set {
                if (value != _UseInMultiplayer.Value) {
                    CONFIG.SetBool("Settings", "UseInMultiplayer", value);
                    _UseInMultiplayer = value;
                }
            }
        }
        private static bool? _UseInMultiplayer;


        /// <summary>
        /// This is a wrapper for Beat Saber's player data structure.<br/>
        /// </summary>
        internal static PlayerData PlayerData {
            get {
                if (_PlayerData == null) {
                    PlayerDataModel[] playerDataModels = Resources.FindObjectsOfTypeAll<PlayerDataModel>();
                    if (playerDataModels.Length >= 1) {
                        _PlayerData = Resources.FindObjectsOfTypeAll<PlayerDataModel>()[0].playerData;
                    }
                }
                return _PlayerData;
            }
        }
        private static PlayerData _PlayerData;
    }
}