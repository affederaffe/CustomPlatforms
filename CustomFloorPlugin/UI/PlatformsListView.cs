using System.Linq;

using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;

using CustomFloorPlugin.Extensions;

using HMUI;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin.UI {


    /// <summary>
    /// A <see cref="ViewController"/> generated and maintained by BSML at runtime.<br/>
    /// BSML uses the <see cref="ResourceName"/> to determine the Layout of the <see cref="GameObject"/>s and their <see cref="Component"/>s<br/>
    /// Tagged functions and variables from this class may be used/called by BSML if the .bsml file mentions them.<br/>
    /// </summary>
    internal class PlatformsListView : BSMLResourceViewController {


        /// <summary>
        /// Path to the embedded resource
        /// </summary>
        public override string ResourceName => "CustomFloorPlugin.Views.PlatformList.bsml";


        /// <summary>
        /// Holds the old Color[] to switch back to when leaving the Preview.
        /// </summary>
        private Color?[] oldColors;


        [Inject]
        private readonly LightWithIdManager manager;


        /// <summary>
        /// The table of currently loaded Platforms
        /// </summary>
        [UIComponent("PlatformsList")]
        public CustomListTableData PlatformListTable = null;


        /// <summary>
        /// Called when a <see cref="CustomPlatform"/> is selected by the user<br/>
        /// Passes the choice to the <see cref="PlatformManager"/>
        /// </summary>
        /// <param name="ignored1">I love how optimised BSML is</param>
        /// <param name="idx">Cell index of the users selection</param>
        [UIAction("PlatformSelect")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by BSML")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1801:Review unused parameters", Justification = "BSML")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "BSML")]
        private void PlatformSelect(TableView ignored1, int idx) {
            if (idx == 0) manager.FillManager(oldColors);
            else manager.FillManager();
            PlatformManager.SetPlatformAndShow(idx);
            PlatformManager.Heart.SetActive(Configuration.PluginConfig.Instance.ShowHeart);
            PlatformManager.Heart.GetComponent<InstancedMaterialLightWithId>().ColorWasSet(Color.magenta);
        }


        /// <summary>
        /// Swapping back to the standard menu environment when the menu is closed<br/>
        /// [Called by Beat Saber]
        /// </summary>
        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
            manager.FillManager(oldColors);
            PlatformManager.ChangeToPlatform(0);
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
        }


        /// <summary>
        /// Changing to the current platform when the menu is shown<br/>
        /// [Called by Beat Saber]
        /// </summary>
        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            oldColors = manager.colors;
            if (PlatformManager.CurrentPlatformIndex != 0) manager.FillManager();
            PlatformManager.ChangeToPlatform();
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
        }


        /// <summary>
        /// (Re-)Loading the table for the ListView of available platforms and environments to override.<br/>
        /// [Called by BSML]
        /// </summary>
        [UIAction("#post-parse")]
        internal void SetupLists() {
            SetupPlatformsList();
        }

        private void SetupPlatformsList() {
            PlatformListTable.data.Clear();
            foreach (CustomPlatform platform in PlatformManager.AllPlatforms) {
                PlatformListTable.data.Add(new CustomListTableData.CustomCellInfo(platform.platName, platform.platAuthor, platform.icon));
            }
            PlatformListTable.tableView.ReloadData();
            int selectedPlatform = PlatformManager.CurrentPlatformIndex;
            if (!PlatformListTable.tableView.visibleCells.Any(x => x.selected)) {
                PlatformListTable.tableView.ScrollToCellWithIdx(selectedPlatform, TableViewScroller.ScrollPositionType.Beginning, false);
            }

            PlatformListTable.tableView.SelectCellWithIdx(selectedPlatform);
        }
    }
}