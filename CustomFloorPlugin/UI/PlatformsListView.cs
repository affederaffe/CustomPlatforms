using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using System;
using UnityEngine;


namespace CustomPlatforms.UI {
    public class PlatformsListView:BSMLResourceViewController {
        public override string ResourceName => "CustomPlatforms.UI.PlatformList.bsml";
        [UIComponent("PlatformsList")]
        public CustomListTableData customListTableData;

        [UIAction("PlatformSelect")]
        private void PlatformSelect(TableView ignored1, int idx) {
            PlatformManager.Instance.SetPlatform(idx);
            try {
                Resources.FindObjectsOfTypeAll<PlayerDataModelSO>()[0].playerData.overrideEnvironmentSettings.overrideEnvironments = false;
            } catch(Exception e) {
                Plugin.Log(e);
            }

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
            customListTableData.data.Clear();
            foreach(CustomPlatform platform in PlatformManager.Instance.GetPlatforms()) {
                customListTableData.data.Add(new CustomListTableData.CustomCellInfo(platform.platName, platform.platAuthor, platform.icon.texture));
            }
            customListTableData.tableView.ReloadData();
            int selectedPlatform = PlatformManager.Instance.currentPlatformIndex;
            customListTableData.tableView.ScrollToCellWithIdx(selectedPlatform, HMUI.TableViewScroller.ScrollPositionType.Beginning, false);
            customListTableData.tableView.SelectCellWithIdx(selectedPlatform);
        }
    }
}