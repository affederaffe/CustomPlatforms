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
            PlatformManager.Instance.SetPlatform(idx);
        }
        protected override void DidDeactivate(DeactivationType deactivationType) {
            PlatformManager.InternalTempChangeToPlatform(0);
            base.DidDeactivate(deactivationType);
        }
        protected override void DidActivate(bool firstActivation, ActivationType type) {
            PlatformManager.InternalTempChangeToPlatform(PlatformManager.Instance.currentPlatformIndex);
            base.DidActivate(firstActivation, type);
        }

        [UIAction("#post-parse")]
        internal void SetupPlatformsList() {
            Plugin.Log("Creating Settings UI");
            customListTableData.data.Clear();
            foreach(CustomPlatform platform in PlatformManager.Instance.GetPlatforms()) {
                customListTableData.data.Add(new CustomListTableData.CustomCellInfo(platform.platName, platform.platAuthor, platform.icon.texture));
            }
            customListTableData.tableView.ReloadData();
            int selectedPlatform = PlatformManager.Instance.currentPlatformIndex;
            customListTableData.tableView.ScrollToCellWithIdx(selectedPlatform, HMUI.TableViewScroller.ScrollPositionType.Beginning, false);
            customListTableData.tableView.SelectCellWithIdx(selectedPlatform);
            Plugin.Log("Done Creating Settings UI");
        }
    }
}