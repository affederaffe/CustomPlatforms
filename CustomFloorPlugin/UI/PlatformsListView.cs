using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;

using HMUI;
using System.Linq;
using UnityEngine;


namespace CustomFloorPlugin.UI {


    /// <summary>
    /// A <see cref="ViewController"/> generated and maintained by BSML at runtime.<br/>
    /// BSML uses the <see cref="ResourceName"/> to determine the Layout of the <see cref="GameObject"/>s and their <see cref="Component"/>s<br/>
    /// Tagged functions and variables from this class may be used/called by BSML if the .bsml file mentions them.<br/>
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1812:Avoid unistantiated internal classes", Justification = "Instantiated by Unity")]
    internal class PlatformsListView : BSMLResourceViewController {


        /// <summary>
        /// Path to the embedded resource
        /// </summary>
        public override string ResourceName => "CustomFloorPlugin.UI.PlatformList.bsml";


        /// <summary>
        /// The table of currently loaded Platforms
        /// </summary>
        [UIComponent("PlatformsList")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "BSML is not capable of finding private instance fields. lol")]
        public CustomListTableData PlatformListTable = null;


        /// <summary>
        /// Called when a <see cref="CustomPlatform"/> is selected by the user<br/>
        /// Passes the choice to the <see cref="PlatformManager"/> and deactivates Beat Sabers inbuilt Environment Override
        /// </summary>
        /// <param name="ignored1">I love how optimised BSML is</param>
        /// <param name="idx">Cell index of the users selection</param>
        [UIAction("PlatformSelect")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by BSML")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1801:Review unused parameters", Justification = "BSML")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "BSML")]
        private void PlatformSelect(TableView ignored1, int idx) {
            PlatformManager.SetPlatformAndShow(idx);
            Settings.PlayerData.overrideEnvironmentSettings.overrideEnvironments = false;
            EnvironmentSceneOverrider.SetEnabled(true);
        }
        [UIAction("reloadPlatforms")]
        public void ReloadMaterials() {
            PlatformManager.Reload();
        }


        /// <summary>
        /// Swapping back to the standard menu environment when the menu is closed<br/>
        /// [Called by Beat Saber]
        /// </summary>
        /// <param name="deactivationType">Type of deactivation</param>
        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
            PlatformManager.ChangeToPlatform(0);
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
        }


        /// <summary>
        /// Changing to the current platform when the menu is shown<br/>
        /// [Called by Beat Saber]
        /// </summary>
        /// <param name="firstActivation">Was this the first activation?</param>
        /// <param name="type">Type of activation</param>
        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            PlatformManager.ChangeToPlatform();
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
        }


        /// <summary>
        /// (Re-)Loading the table for the ListView of available platforms.<br/>
        /// [Called by BSML]
        /// </summary>
        [UIAction("#post-parse")]
        internal void SetupPlatformsList() {
            PlatformListTable.data.Clear();
            foreach (CustomPlatform platform in PlatformManager.AllPlatforms) {
                PlatformListTable.data.Add(new CustomListTableData.CustomCellInfo(platform.platName, platform.platAuthor, platform.icon));
            }
            PlatformListTable.tableView.ReloadData();
            int selectedPlatform = PlatformManager.CurrentPlatformIndex;
            if (!PlatformListTable.tableView.visibleCells.Any(x => x.selected))
                PlatformListTable.tableView.ScrollToCellWithIdx(selectedPlatform, HMUI.TableViewScroller.ScrollPositionType.Beginning, false);
            PlatformListTable.tableView.SelectCellWithIdx(selectedPlatform);
        }
    }
}