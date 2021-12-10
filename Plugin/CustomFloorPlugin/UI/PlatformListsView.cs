using System.Collections.Specialized;

using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;

using CustomFloorPlugin.Configuration;

using HMUI;

using IPA.Utilities;

using Zenject;


namespace CustomFloorPlugin.UI
{
    /// <summary>
    /// A <see cref="BSMLAutomaticViewController"/> generated by SiraUtil and maintained by BSML at runtime.<br/>
    /// BSML uses the <see cref="ViewDefinitionAttribute"/> to determine the Layout of the GameObjects and their Components<br/>
    /// Tagged functions and variables from this class may be used/called by BSML if the .bsml file mentions them.<br/>
    /// </summary>
    [ViewDefinition("CustomFloorPlugin.Views.PlatformLists.bsml")]
    internal class PlatformListsView : BSMLAutomaticViewController
    {
        private PluginConfig _config = null!;
        private AssetLoader _assetLoader = null!;
        private PlatformManager _platformManager = null!;
        private PlatformSpawner _platformSpawner = null!;

        [UIComponent("singleplayer-platforms-list")]
        private readonly CustomListTableData _singleplayerPlatformListTable = null!;

        [UIComponent("multiplayer-platforms-list")]
        private readonly CustomListTableData _multiplayerPlatformListTable = null!;

        [UIComponent("a360-platforms-list")]
        private readonly CustomListTableData _a360PlatformListTable = null!;

        [UIComponent("menu-platforms-list")]
        private readonly CustomListTableData _menuPlatformListTable = null!;

        private CustomListTableData[] _listTables = null!;
        private ScrollView[] _scrollViews = null!;
        private int _tabIndex;

        [Inject]
        public void Construct(PluginConfig config, AssetLoader assetLoader, PlatformSpawner platformSpawner, PlatformManager platformManager)
        {
            _config = config;
            _assetLoader = assetLoader;
            _platformManager = platformManager;
            _platformSpawner = platformSpawner;
        }

        /// <summary>
        /// Called when a tab is selected by the user<br/>
        /// Changes to the <see cref="CustomPlatform"/> of the selected game mode
        /// </summary>
        /// <param name="segmentedControl">Used to gather the cell index</param>
        /// <param name="_">I love how optimised BSML is</param>
        [UIAction("select-tab")]
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        private void OnDidSelectTab(SegmentedControl segmentedControl, int _)
        {
            _tabIndex = segmentedControl.selectedCellNumber;
            int index = GetPlatformIndexForTabIndex(_tabIndex);
            _listTables[segmentedControl.selectedCellNumber].tableView.ScrollToCellWithIdx(index, TableView.ScrollPositionType.Beginning, false);
            _listTables[segmentedControl.selectedCellNumber].tableView.SelectCellWithIdx(index, true);
        }

        /// <summary>
        /// Called when a <see cref="CustomPlatform"/> is selected by the user
        /// </summary>
        /// <param name="_">I love how optimised BSML is</param>
        /// <param name="index">Cell index of the users selection</param>
        [UIAction("select-platform")]
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        private async void OnDidSelectPlatform(TableView _, int index)
        {
            await _platformSpawner.ChangeToPlatformAsync(_platformManager.AllPlatforms[index]);
            switch (_tabIndex)
            {
                case 0:
                    _platformManager.SingleplayerPlatform = _platformManager.ActivePlatform;
                    _config.SingleplayerPlatformPath = _platformManager.ActivePlatform.fullPath;
                    break;
                case 1:
                    _platformManager.MultiplayerPlatform = _platformManager.ActivePlatform;
                    _config.MultiplayerPlatformPath = _platformManager.ActivePlatform.fullPath;
                    break;
                case 2:
                    _platformManager.A360Platform = _platformManager.ActivePlatform;
                    _config.A360PlatformPath = _platformManager.ActivePlatform.fullPath;
                    break;
                case 3:
                    _platformManager.MenuPlatform = _platformManager.ActivePlatform;
                    _config.MenuPlatformPath = _platformManager.ActivePlatform.fullPath;
                    break;
            }
        }

        /// <summary>
        /// Changing to the current platform when the menu is shown<br/>
        /// [Called by Beat Saber]
        /// </summary>
        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (firstActivation) _platformManager.AllPlatforms.CollectionChanged += OnCollectionDidChange;
            CustomPlatform platform = GetPlatformForTabIndex(_tabIndex);
            _ = _platformSpawner.ChangeToPlatformAsync(platform);
        }

        /// <summary>
        /// Swapping back to the standard menu environment or to the selected singleplayer platform when the menu is closed
        /// [Called by Beat Saber]
        /// </summary>
        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            if (removedFromHierarchy) _platformManager.AllPlatforms.CollectionChanged -= OnCollectionDidChange;
            int index = GetPlatformIndexForTabIndex(_tabIndex);
            _listTables[_tabIndex].tableView.SelectCellWithIdx(index);
            _ = _platformSpawner.ChangeToPlatformAsync(_config.ShufflePlatforms ? _platformSpawner.RandomPlatform : _platformManager.MenuPlatform);
        }

        /// <summary>
        /// (Re-)Loading the tables for the ListView of available platforms
        /// [Called by BSML]
        /// </summary>
        [UIAction("#post-parse")]
        // ReSharper disable once UnusedMember.Local
        private void PostParse()
        {
            _listTables = new[] { _singleplayerPlatformListTable, _multiplayerPlatformListTable, _a360PlatformListTable, _menuPlatformListTable };
            _scrollViews = new ScrollView[_listTables.Length];
            for (int i = 0; i < _platformManager.AllPlatforms.Count; i++)
                AddCellForPlatform(_platformManager.AllPlatforms[i], i);
            for (int i = 0; i < _listTables.Length; i++)
            {
                int index = GetPlatformIndexForTabIndex(i);
                _listTables[i].tableView.ReloadData();
                _listTables[i].tableView.ScrollToCellWithIdx(index, TableView.ScrollPositionType.Beginning, false);
                _listTables[i].tableView.SelectCellWithIdx(index);
                _scrollViews[i] = _listTables[i].tableView.GetField<ScrollView, TableView>("_scrollView");
            }
        }

        /// <summary>
        /// Called when a <see cref="CustomPlatform"/> was added to the list<br/>
        /// Adds or Removes the corresponding cells and refreshes the UI
        /// </summary>
        private void OnCollectionDidChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (CustomPlatform platform in e.NewItems)
                        AddCellForPlatform(platform, e.NewStartingIndex);
                    RefreshListViews();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (CustomPlatform platform in e.OldItems)
                        RemoveCellForPlatform(platform, e.OldStartingIndex);
                    RefreshListViews();
                    break;
            }
        }

        /// <summary>
        /// Updates all <see cref="CustomListTableData"/>s
        /// </summary>
        private void RefreshListViews()
        {
            for (int i = 0; i < _listTables.Length; i++)
            {
                float pos = _scrollViews[i].GetField<float, ScrollView>("_destinationPos");
                _listTables[i].tableView.ReloadData();
                _scrollViews[i].ScrollTo(pos, false);
            }
        }

        /// <summary>
        /// Adds a cell to the UI for the given <see cref="CustomPlatform"/>
        /// </summary>
        /// <param name="platform">The platform to be added as a cell</param>
        /// <param name="index">The index the cell should be inserted at</param>
        private void AddCellForPlatform(CustomPlatform platform, int index)
        {
            CustomListTableData.CustomCellInfo cell = new(platform.platName, platform.platAuthor, platform.icon ? platform.icon : _assetLoader.FallbackCover);
            foreach (CustomListTableData listTable in _listTables)
                listTable.data.Insert(index, cell);
        }

        /// <summary>
        /// Removes the cell from the UI for the given <see cref="CustomPlatform"/>
        /// </summary>
        /// <param name="platform">The platform the cell was created for</param>
        /// <param name="index">The index the cell is located at</param>
        private void RemoveCellForPlatform(CustomPlatform platform, int index)
        {
            foreach (CustomListTableData listTable in _listTables)
            {
                listTable.data.RemoveAt(index);
                if (platform != GetPlatformForTabIndex(_tabIndex)) continue;
                listTable.tableView.SelectCellWithIdx(0);
                listTable.tableView.ScrollToCellWithIdx(0, TableView.ScrollPositionType.Beginning, false);
            }
        }

        private int GetPlatformIndexForTabIndex(int tabIndex)
        {
            CustomPlatform platform = GetPlatformForTabIndex(tabIndex);
            return _platformManager.AllPlatforms.IndexOf(platform);
        }

        private CustomPlatform GetPlatformForTabIndex(int tabIndex)
        {
            return tabIndex switch
            {
                0 => _platformManager.SingleplayerPlatform,
                1 => _platformManager.MultiplayerPlatform,
                2 => _platformManager.A360Platform,
                3 => _platformManager.MenuPlatform,
                _ => _platformManager.DefaultPlatform
            };
        }
    }
}