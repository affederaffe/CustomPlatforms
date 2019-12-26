using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using TMPro;

using HMUI;
using IPA.Utilities;
using System.Linq;
using UnityEngine;


namespace CustomFloorPlugin.UI {
    public class PlatformsListView:BSMLResourceViewController {
        public override string ResourceName => "CustomFloorPlugin.UI.PlatformList.bsml";
        [UIComponent("PlatformsList")]
        public CustomListTableData customListTableData;

        [UIAction("PlatformSelect")]
        private void PlatformSelect(TableView ignored1, int idx) {
            PlatformManager.Instance.ChangeToPlatform(idx);
        }
        protected override void DidDeactivate(DeactivationType deactivationType) {
            base.DidDeactivate(deactivationType);
        }
        [UIAction("#post-parse")]
        internal void SetupPlatformsList() {
            int i = 0;
            Debug.Log("Creating Settings UI");
            customListTableData.data.Clear();
            foreach(CustomPlatform platform in PlatformManager.Instance.GetPlatforms()) {
                customListTableData.data.Add(new CustomListTableData.CustomCellInfo(platform.platName, platform.platAuthor, platform.icon.texture));
            }
            i = 3;
            customListTableData.tableView.ReloadData();
            int selectedPlatform = PlatformManager.Instance.currentPlatformIndex;
            customListTableData.tableView.ScrollToCellWithIdx(selectedPlatform, HMUI.TableViewScroller.ScrollPositionType.Beginning, false);
            customListTableData.tableView.SelectCellWithIdx(selectedPlatform);
            Debug.Log("Done Creating Settings UI");
        }
    }
}