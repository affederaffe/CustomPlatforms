using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;

using CustomFloorPlugin.Configuration;

using Zenject;


namespace CustomFloorPlugin.UI {


    /// <summary>
    /// Interface between the UI and the remainder of the plugin<br/>
    /// Abuses getters to inline showing values, and setters to perform relevant actions<br/>
    /// Not intended to hold extensive logic!<br/>
    /// Why is everything here public? I don't know... -.-
    /// </summary>
    [ViewDefinition("CustomFloorPlugin.Views.Settings.bsml")]
    internal class SettingsView : BSMLAutomaticViewController {

        [Inject]
        private readonly PluginConfig _config;

        [Inject]
        private readonly PlatformManager _platformManager;

        [Inject]
        private readonly PlatformsListView _platformsListView;

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
        public bool AlwaysShowFeet {
            get => _config.AlwaysShowFeet;
            set => _config.AlwaysShowFeet = value;
        }


        /// <summary>
        /// Should the heart next to the logo be visible?<br/>
        /// Forwards the current choice to the UI, and the new choice to the plugin
        /// </summary>
        [UIValue("show-heart")]
        public bool ShowHeart {
            get => _config.ShowHeart;
            set => _config.ShowHeart = value;
        }


        /// <summary>
        /// Should this Plugin load CustomScripts?
        /// Forwards the current choice to the UI, and the new choice to the plugin
        /// </summary>
        [UIValue("load-custom-scripts")]
        public bool LoadCustomScripts {
            get => _config.LoadCustomScripts;
            set => _config.LoadCustomScripts = value;
        }


        /// <summary>
        /// Should this Plugin spawn a Custom Platform in 360°-Levels?
        /// Forwards the current choice to the UI, and the new choice to the plugin
        /// </summary>
        [UIValue("use-in-360")]
        public bool UseIn360 {
            get => _config.UseIn360;
            set => _config.UseIn360 = value;
        }


        /// <summary>
        /// Should this Plugin spawn a Custom Platform in Multiplayer?
        /// Forwards the current choice to the UI, and the new choice to the plugin
        /// </summary>
        [UIValue("use-in-multiplayer")]
        public bool UseInMultiplayer {
            get => _config.UseInMultiplayer;
            set => _config.UseInMultiplayer = value;
        }


        [UIAction("ReloadPlatforms")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by BSML")]
        private void ReloadButtonPressed() {
            _platformManager.Reload();
            _platformsListView.SetupPlatformsList();
        }
    }
}