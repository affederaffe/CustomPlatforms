using System.Linq;

using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;

using CustomFloorPlugin.Configuration;

using HMUI;

using Zenject;


namespace CustomFloorPlugin.UI {


    /// <summary>
    /// A <see cref="BSMLAutomaticViewController"/> generated by Zenject and maintained by BSML at runtime.<br/>
    /// BSML uses the <see cref="ViewDefinitionAttribute"/> to determine the Layout of the GameObjects and their Components<br/>
    /// Tagged functions and variables from this class may be used/called by BSML if the .bsml file mentions them.<br/>
    /// </summary>
    [ViewDefinition("CustomFloorPlugin.Views.PlatformLists.bsml")]
    internal class PlatformListsView : BSMLAutomaticViewController {

        [Inject]
        private readonly PluginConfig _config;

        [Inject]
        private readonly PlatformSpawnerMenu _platformSpawner;

        [Inject]
        private readonly PlatformManager _platformManager;


        /// <summary>
        /// The table of currently loaded Platforms, for singleplayer only because BSML can't use the same list for different tabs
        /// </summary>
        [UIComponent("singleplayerPlatformList")]
        public CustomListTableData singleplayerPlatformListTable;


        /// <summary>
        /// The table of currently loaded Platforms, for multiplayer only because BSML can't use the same list for different tabs
        /// </summary>
        [UIComponent("multiplayerPlatformList")]
        public CustomListTableData multiplayerPlatformListTable;


        /// <summary>
        /// The table of currently loaded Platforms, for multiplayer only because BSML can't use the same list for different tabs
        /// </summary>
        [UIComponent("a360PlatformList")]
        public CustomListTableData a360PlatformListTable;


        /// <summary>
        /// An <see cref="System.Array"/> holding all <see cref="CustomListTableData"/>s
        /// </summary>
        internal CustomListTableData[] allListTables;


        [UIAction("Select-cell")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by BSML")]
        private void TabSelect(SegmentedControl segmentedControl, int _1) {
            PlatformType type = (PlatformType)segmentedControl.selectedCellNumber;
            int index = _platformManager.GetIndexForType(type);
            singleplayerPlatformListTable.tableView.ScrollToCellWithIdx(index, TableViewScroller.ScrollPositionType.Beginning, false);
            _platformSpawner.ChangeToPlatform(index);
            _platformManager.currentPlatformType = type;
        }


        /// <summary>
        /// Called when a <see cref="CustomPlatform"/> is selected by the user<br/>
        /// Passes the choice to the <see cref="PlatformManager"/>
        /// </summary>
        /// <param name="_1">I love how optimised BSML is</param>
        /// <param name="idx">Cell index of the users selection</param>
        [UIAction("SingleplayerSelect")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by BSML")]
        private void SingleplayerSelect(TableView _1, int idx) {
            _platformSpawner.SetPlatformAndShow(idx, PlatformType.Singleplayer);
        }


        /// <summary>
        /// Called when a <see cref="CustomPlatform"/> is selected by the user<br/>
        /// Passes the choice to the <see cref="PlatformManager"/>
        /// </summary>
        /// <param name="_1">I love how optimised BSML is</param>
        /// <param name="idx">Cell index of the users selection</param>
        [UIAction("MultiplayerSelect")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by BSML")]
        private void MultiplayerSelect(TableView _1, int idx) {
            _platformSpawner.SetPlatformAndShow(idx, PlatformType.Multiplayer);
        }


        /// <summary>
        /// Called when a <see cref="CustomPlatform"/> is selected by the user<br/>
        /// Passes the choice to the <see cref="PlatformManager"/>
        /// </summary>
        /// <param name="_1">I love how optimised BSML is</param>
        /// <param name="idx">Cell index of the users selection</param>
        [UIAction("A360Select")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by BSML")]
        private void A360Select(TableView _1, int idx) {
            _platformSpawner.SetPlatformAndShow(idx, PlatformType.A360);
        }


        /// <summary>
        /// Changing to the current platform when the menu is shown<br/>
        /// [Called by Beat Saber]
        /// </summary>
        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            _platformSpawner.ChangeToPlatform(_platformManager.currentPlatformType);
            for (int i = 0; i < allListTables.Length; i++) {
                PlatformType type = (PlatformType)i;
                int index = _platformManager.GetIndexForType(type);
                allListTables[i].tableView.ScrollToCellWithIdx(index, TableViewScroller.ScrollPositionType.Beginning, false);
            }
        }


        /// <summary>
        /// Swapping back to the standard menu environment when the menu is closed<br/>
        /// [Called by Beat Saber]
        /// </summary>
        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            if (_config.ShowInMenu) {
                _platformSpawner.ChangeToPlatform(PlatformType.Singleplayer);
            }
            else {
                _platformSpawner.ChangeToPlatform(0);
            }
        }


        /// <summary>
        /// (Re-)Loading the tables for the ListView of available platforms.<br/>
        /// [Called by BSML]
        /// </summary>
        [UIAction("#post-parse")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by BSML")]
        private void SetupLists() {
            allListTables = new CustomListTableData[] { singleplayerPlatformListTable, multiplayerPlatformListTable, a360PlatformListTable };
            foreach (CustomPlatform platform in _platformManager.allPlatforms) {
                CustomListTableData.CustomCellInfo cell = new CustomListTableData.CustomCellInfo(platform.platName, platform.platAuthor, platform.icon);
                foreach (CustomListTableData listTable in allListTables) {
                    listTable.data.Add(cell);
                }
            }
            for (int i = 0; i < allListTables.Length; i++) {
                allListTables[i].tableView.ReloadData();
                int idx = _platformManager.GetIndexForType((PlatformType)i);
                if (!allListTables[i].tableView.visibleCells.Any(x => x.selected)) {
                    allListTables[i].tableView.ScrollToCellWithIdx(idx, TableViewScroller.ScrollPositionType.Beginning, false);
                }
                allListTables[i].tableView.SelectCellWithIdx(idx);
            }
        }
    }
}