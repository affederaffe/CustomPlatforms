using HMUI;
using CustomUI.BeatSaber;
using CustomUI.Utilities;
using TMPro;

namespace CustomFloorPlugin
{
    class PlatformListViewController : CustomListViewController
    {
        public override void __Activate(ActivationType activationType)
        {
            base.__Activate(activationType);
            _customListTableView.SelectCellWithIdx(PlatformManager.Instance.currentPlatformIndex);
        }

        public override int NumberOfCells()
        {
            return PlatformManager.Instance.GetPlatforms().Length;
        }

        public override TableCell CellForIdx(TableView t, int idx)
        {
            CustomPlatform platform = PlatformManager.Instance.GetPlatform(idx);
            LevelListTableCell _tableCell = GetTableCell(false);
            _tableCell.GetPrivateField<TextMeshProUGUI>("_songNameText").text = platform.platName;
            _tableCell.GetPrivateField<TextMeshProUGUI>("_authorText").text = platform.platAuthor;
            _tableCell.GetPrivateField<UnityEngine.UI.RawImage>("_coverRawImage").texture = platform.icon != null? platform.icon.texture : UnityEngine.Texture2D.blackTexture;
            _tableCell.reuseIdentifier = "PlatformListCell";
            return _tableCell;
        }
    }
}