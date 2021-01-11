using System;
using System.Linq;

using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;

using CustomFloorPlugin.Configuration;

using HMUI;

using UnityEngine;


namespace CustomFloorPlugin.UI {


    /// <summary>
    /// A <see cref="ViewController"/> generated and maintained by BSML at runtime.<br/>
    /// BSML uses the <see cref="ResourceName"/> to determine the Layout of the <see cref="GameObject"/>s and their <see cref="Component"/>s<br/>
    /// Tagged functions and variables from this class may be used/called by BSML if the .bsml file mentions them.<br/>
    /// </summary>
    internal class EnvironmentOverrideListView : BSMLResourceViewController {

        /// <summary>
        /// Path to the embedded resource
        /// </summary>
        public override string ResourceName => "CustomFloorPlugin.Views.EnvironmentOverrideList.bsml";

        /// <summary>
        /// Override choice for platform base model/environment<br/>
        /// Forwards the current choice to the UI, and the new choice to the plugin
        /// </summary>
        public static EnvOverrideMode EnvOr {
            get => PluginConfig.Instance.EnvOverrideMode;
            set => PluginConfig.Instance.EnvOverrideMode = value;
        }

        /// <summary>
        /// The table of all available overrides
        /// </summary>
        [UIComponent("OverrideList")]
        public CustomListTableData OverrideListTable = null;

        /// <summary>
        /// Called when a <see cref="EnvOverrideMode"/> is selected by the user<br/>
        /// Passes the choice to <see cref="EnvOr"/>
        /// </summary>
        /// <param name="_1">I love how optimised BSML is</param>
        /// <param name="idx">Cell index of the users selection</param>
        [UIAction("OverrideSelect")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by BSML")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1801:Review unused parameters", Justification = "BSML")]
        private void OverrideSelect(TableView _1, int idx) {
            EnvOr = (EnvOverrideMode)idx;
        }

        /// <summary>
        /// (Re-)Loading the table for the ListView of available platforms and environments to override.<br/>
        /// [Called by BSML]
        /// </summary>
        [UIAction("#post-parse")]
        internal void SetupLists() {
            SetupOverrideList();
        }

        private void SetupOverrideList() {
            OverrideListTable.data.Clear();
            foreach (string name in Enum.GetNames(typeof(EnvOverrideMode))) {
                OverrideListTable.data.Add(new CustomListTableData.CustomCellInfo(name));
            }
            OverrideListTable.tableView.ReloadData();
            int selectedOverride = (int)EnvOr;
            if (!OverrideListTable.tableView.visibleCells.Any(x => x.selected)) {
                OverrideListTable.tableView.ScrollToCellWithIdx(selectedOverride, TableViewScroller.ScrollPositionType.Beginning, false);
            }
            OverrideListTable.tableView.SelectCellWithIdx(selectedOverride);
        }
    }
}
