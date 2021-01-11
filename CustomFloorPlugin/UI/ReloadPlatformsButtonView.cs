using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;

using HMUI;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin.UI {


    /// <summary>
    /// A <see cref="ViewController"/> generated and maintained by BSML at runtime.<br/>
    /// BSML uses the <see cref="ResourceName"/> to determine the Layout of the <see cref="GameObject"/>s and their <see cref="Component"/>s<br/>
    /// Tagged functions and variables from this class may be used/called by BSML if the .bsml file mentions them.<br/>
    /// </summary>
    internal class ReloadPlatformsButtonView : BSMLResourceViewController {

        [Inject]
        private readonly PlatformsListView platformsListView;

        /// <summary>
        /// Path to the embedded resource
        /// </summary>
        public override string ResourceName => "CustomFloorPlugin.Views.PlatformReloadButton.bsml";

        [UIAction("reloadPlatforms")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by BSML")]
        private void ReloadMaterials() {
            PlatformManager.Reload();
            platformsListView.SetupLists();
        }
    }
}
