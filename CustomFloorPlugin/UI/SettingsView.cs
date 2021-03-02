using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;

using CustomFloorPlugin.Configuration;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin.UI
{
    /// <summary>
    /// A <see cref="BSMLAutomaticViewController"/> generated by Zenject and maintained by BSML at runtime.<br/>
    /// BSML uses the <see cref="ViewDefinitionAttribute"/> to determine the Layout of the GameObjects and their Components<br/>
    /// Tagged functions and variables from this class may be used/called by BSML if the .bsml file mentions them.<br/>
    /// Interface between the UI and the remainder of the plugin<br/>
    /// Abuses getters to inline showing values, and setters to perform relevant actions<br/>
    /// </summary>
    [ViewDefinition("CustomFloorPlugin.Views.Settings.bsml")]
    internal class SettingsView : BSMLAutomaticViewController
    {
        [Inject]
        private readonly PluginConfig _config;

        /// <summary>
        /// Determines if the feet icon is shown even if the platform would normally hide them<br/>
        /// Forwards the current choice to the UI, and the new choice to the plugin
        /// </summary>
        [UIValue("always-show-feet")]
        public bool AlwaysShowFeet
        {
            get => _config.AlwaysShowFeet;
            set => _config.AlwaysShowFeet = value;
        }

        /// <summary>
        /// Should the heart next to the logo be visible?<br/>
        /// Forwards the current choice to the UI, and the new choice to the plugin
        /// </summary>
        [UIValue("show-heart")]
        public bool ShowHeart
        {
            get => _config.ShowHeart;
            set
            {
                _config.ShowHeart = value;
                if (PlatformManager.Heart != null)
                {
                    PlatformManager.Heart.SetActive(value);
                    PlatformManager.Heart.GetComponent<InstancedMaterialLightWithId>().ColorWasSet(Color.magenta);
                }
            }
        }

        /// <summary>
        /// Should the selected Platform also be shown in menu?<br/>
        /// Forwards the current choice to the UI, and the new choice to the plugin
        /// </summary>
        [UIValue("show-in-menu")]
        public bool ShowInMenu
        {
            get => _config.ShowInMenu;
            set => _config.ShowInMenu = value;
        }
    }
}