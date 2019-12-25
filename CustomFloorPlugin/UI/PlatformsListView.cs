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
            Debug.Log("" + ++i);
            customListTableData.data.Clear();
            Debug.Log("" + ++i);
            foreach(CustomPlatform platform in PlatformManager.Instance.GetPlatforms()) {
                Debug.Log("before " + ++i);
                Debug.Log("PlatName: " + platform.platName ?? "null");
                if(platform.icon == null) {
                    Debug.Log("Texture is null");
                }

                customListTableData.data.Add(new CustomListTableData.CustomCellInfo(platform.platName, platform.platAuthor, platform.icon.texture));
                Debug.Log("after " + i);
            }
            i = 3;
            Debug.Log("" + ++i);
            customListTableData.tableView.ReloadData();
            Debug.Log("" + ++i);
            int selectedPlatform = PlatformManager.Instance.currentPlatformIndex;
            Debug.Log("" + ++i);
            customListTableData.tableView.ScrollToCellWithIdx(selectedPlatform, HMUI.TableViewScroller.ScrollPositionType.Beginning, false);
            Debug.Log("" + ++i);
            customListTableData.tableView.SelectCellWithIdx(selectedPlatform);
            Debug.Log("" + ++i);
        }
    }
}