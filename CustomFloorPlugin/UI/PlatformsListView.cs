using System.Linq;

using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;

using HMUI;

using IPA.Utilities;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin.UI {


    /// <summary>
    /// A <see cref="ViewController"/> generated and maintained by BSML at runtime.<br/>
    /// BSML uses the <see cref="ResourceName"/> to determine the Layout of the <see cref="GameObject"/>s and their <see cref="Component"/>s<br/>
    /// Tagged functions and variables from this class may be used/called by BSML if the .bsml file mentions them.<br/>
    /// </summary>
    [ViewDefinition("CustomFloorPlugin.Views.PlatformList.bsml")]
    internal class PlatformsListView : BSMLAutomaticViewController {

        [Inject]
        private readonly PlatformSpawner _platformSpawner;

        [Inject]
        private readonly PlatformManager _platformManager;

        [Inject]
        private readonly LightWithIdManager _lightManager;

        [Inject]
        private readonly Color?[] _colors;

        private Color?[] _oldColors;


        /// <summary>
        /// The table of currently loaded Platforms
        /// </summary>
        [UIComponent("PlatformsList")]
        public CustomListTableData PlatformListTable = null;


        /// <summary>
        /// Called when a <see cref="CustomPlatform"/> is selected by the user<br/>
        /// Passes the choice to the <see cref="PlatformManager"/>
        /// </summary>
        /// <param name="_1">I love how optimised BSML is</param>
        /// <param name="idx">Cell index of the users selection</param>
        [UIAction("PlatformSelect")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by BSML")]
        private void PlatformSelect(TableView _1, int idx) {
            if (idx == 0) _lightManager.SetField("_colors", _oldColors);
            else _lightManager.SetField("_colors", _colors);
            _platformSpawner.SetPlatformAndShow(idx);
        }


        /// <summary>
        /// Swapping back to the standard menu environment when the menu is closed<br/>
        /// [Called by Beat Saber]
        /// </summary>
        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
            _lightManager.SetField("_colors", _oldColors);
            _platformSpawner.ChangeToPlatform(0);
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
        }


        /// <summary>
        /// Changing to the current platform when the menu is shown<br/>
        /// [Called by Beat Saber]
        /// </summary>
        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            _oldColors = _lightManager.colors;
            if (_platformManager.CurrentPlatformIndex != 0) _lightManager.SetField("_colors", _colors);
            _platformSpawner.ChangeToPlatform();
        }


        /// <summary>
        /// (Re-)Loading the table for the ListView of available platforms and environments to override.<br/>
        /// [Called by BSML]
        /// </summary>
        [UIAction("#post-parse")]
        internal void SetupPlatformsList() {
            PlatformListTable.data.Clear();
            foreach (CustomPlatform platform in _platformManager.AllPlatforms) {
                PlatformListTable.data.Add(new CustomListTableData.CustomCellInfo(platform.platName, platform.platAuthor, platform.icon));
            }
            PlatformListTable.tableView.ReloadData();
            int selectedPlatform = _platformManager.CurrentPlatformIndex;
            if (!PlatformListTable.tableView.visibleCells.Any(x => x.selected)) {
                PlatformListTable.tableView.ScrollToCellWithIdx(selectedPlatform, TableViewScroller.ScrollPositionType.Beginning, false);
            }
            PlatformListTable.tableView.SelectCellWithIdx(selectedPlatform);
        }
    }
}