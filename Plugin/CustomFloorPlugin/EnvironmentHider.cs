using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Activates and deactivates world geometry in the active scene as required by the chosen custom platform<br />
    /// Most documentation on this file is omitted because it is a giant clusterfuck and I hate it.
    /// </summary>
    [UsedImplicitly]
    public sealed class EnvironmentHider
    {
        private readonly AssetLoader _assetLoader;
        private readonly PlatformManager _platformManager;

        private readonly List<GameObject> _menuEnvironment = new();
        private readonly List<GameObject> _playersPlace = new();
        private readonly List<GameObject> _smallRings = new();
        private readonly List<GameObject> _bigRings = new();
        private readonly List<GameObject> _visualizer = new();
        private readonly List<GameObject> _towers = new();
        private readonly List<GameObject> _highway = new();
        private readonly List<GameObject> _backColumns = new();
        private readonly List<GameObject> _doubleColorLasers = new();
        private readonly List<GameObject> _backLasers = new();
        private readonly List<GameObject> _rotatingLasers = new();
        private readonly List<GameObject> _trackLights = new();

        private string? _sceneName;
        private Transform? _envRoot;
        private Transform? _menuRoot;

        public EnvironmentHider(AssetLoader assetLoader, PlatformManager platformManager)
        {
            _assetLoader = assetLoader;
            _platformManager = platformManager;
        }

        internal void OnTransitionDidFinish(ScenesTransitionSetupDataSO? setupData, DiContainer container)
        {
            switch (setupData)
            {
                case MenuScenesTransitionSetupDataSO:
                    _menuRoot = container.Resolve<MenuEnvironmentManager>().transform;
                    _envRoot = container.Resolve<LightWithIdManager>().transform.parent;
                    _sceneName = _envRoot.gameObject.scene.name;
                    break;
                case null when container.HasBinding<LightWithIdManager>():
                case StandardLevelScenesTransitionSetupDataSO:
                case TutorialScenesTransitionSetupDataSO:
                case MissionLevelScenesTransitionSetupDataSO:
                    _envRoot = container.Resolve<LightWithIdManager>().transform.parent;
                    _sceneName = _envRoot.gameObject.scene.name;
                    break;
                case MultiplayerLevelScenesTransitionSetupDataSO when container.HasBinding<MultiplayerLocalActivePlayerFacade>():
                    _envRoot = container.Resolve<MultiplayerPlayersManager>().localPlayerTransform;
                    _sceneName = _envRoot.gameObject.scene.name;
                    break;
            }
        }

        /// <summary>
        /// Hide world objects as required by the active platform
        /// </summary>
        internal void HideObjectsForPlatform(CustomPlatform platform)
        {
            if (_menuRoot is null && _envRoot is null) return;
            FindEnvironment();
            SetCollectionHidden(_menuEnvironment, platform != _platformManager.DefaultPlatform);
            SetCollectionHidden(_playersPlace, platform.hideDefaultPlatform);
            SetCollectionHidden(_smallRings, platform.hideSmallRings);
            SetCollectionHidden(_bigRings, platform.hideBigRings);
            SetCollectionHidden(_visualizer, platform.hideEQVisualizer);
            SetCollectionHidden(_towers, platform.hideTowers);
            SetCollectionHidden(_highway, platform.hideHighway);
            SetCollectionHidden(_backColumns, platform.hideBackColumns);
            SetCollectionHidden(_backLasers, platform.hideBackLasers);
            SetCollectionHidden(_doubleColorLasers, platform.hideDoubleColorLasers);
            SetCollectionHidden(_rotatingLasers, platform.hideRotatingLasers);
            SetCollectionHidden(_trackLights, platform.hideTrackLights);
            bool showPlayersPlace = _sceneName == "MainMenu" && !platform.hideDefaultPlatform && platform != _platformManager.DefaultPlatform;
            _assetLoader.PlayersPlace.SetActive(showPlayersPlace);
        }

        /// <summary>
        /// Finds all GameObjects that make up the default environment and groups them into lists
        /// </summary>
        private void FindEnvironment()
        {
            FindMenuEnvironment();
            FindPlayersPlace();
            FindSmallRings();
            FindBigRings();
            FindVisualizers();
            FindTowers();
            FindHighway();
            FindBackColumns();
            FindBackLasers();
            FindRotatingLasers();
            FindDoubleColorLasers();
            FindTrackLights();
        }

        /// <summary>
        /// Set the active state of a Collection of GameObjects
        /// </summary>
        /// <param name="list">A <see cref="ICollection{T}" /> of GameObjects</param>
        /// <param name="hidden">A boolean describing the desired hidden state</param>
        private static void SetCollectionHidden(ICollection<GameObject> list, bool hidden)
        {
            foreach (GameObject gameObject in list)
                gameObject.SetActive(!hidden);
            list.Clear();
        }

        /// <summary>
        /// Finds a GameObject by name and adds it to the provided list
        /// </summary>
        /// <param name="root">The Transform the GameObject is a child of</param>
        /// <param name="name">The name of the desired GameObject</param>
        /// <param name="list">The list to be added to</param>
        /// <param name="rename">Whether the GameObject should be renamed or not</param>
        private void FindAddGameObject(Transform root, string name, ICollection<GameObject> list, bool rename = false)
        {
            GameObject? go = root.Find(name)?.gameObject;
            if (go is null || (!go.activeSelf && _sceneName != "MainMenu")) return;
            if (rename) go.name += "renamed";
            list.Add(go);
        }

        private void FindMenuEnvironment()
        {
            switch (_sceneName)
            {
                case "MainMenu":
                case "Credits":
                    FindAddGameObject(_menuRoot!, "DefaultMenuEnvironment/MenuFogRing", _menuEnvironment);
                    FindAddGameObject(_menuRoot!, "DefaultMenuEnvironment/BasicMenuGround", _menuEnvironment);
                    FindAddGameObject(_menuRoot!, "DefaultMenuEnvironment/Notes", _menuEnvironment);
                    FindAddGameObject(_menuRoot!, "DefaultMenuEnvironment/PileOfNotes", _menuEnvironment);
                    break;
            }
        }

        private void FindPlayersPlace()
        {
            switch (_sceneName)
            {
                case "GameCore":
                    FindAddGameObject(_envRoot!, "IsActiveObjects/Construction/PlayersPlace/Mirror", _playersPlace);
                    FindAddGameObject(_envRoot!, "IsActiveObjects/Construction/PlayersPlace/Construction", _playersPlace);
                    FindAddGameObject(_envRoot!, "IsActiveObjects/Construction/PlayersPlace/RectangleFakeGlow", _playersPlace);
                    FindAddGameObject(_envRoot!, "IsActiveObjects/Construction/PlayersPlace/Frame", _playersPlace);
                    FindAddGameObject(_envRoot!, "IsActiveObjects/Construction/PlayersPlace/SaberBurnMarksParticles", _playersPlace);
                    FindAddGameObject(_envRoot!, "IsActiveObjects/Construction/PlayersPlace/SaberBurnMarksArea", _playersPlace);
                    FindAddGameObject(_envRoot!, "IsActiveObjects/Construction/PlayersPlace/Collider", _playersPlace);
                    break;
                default:
                    FindAddGameObject(_envRoot!, "PlayersPlace/Mirror", _playersPlace);
                    FindAddGameObject(_envRoot!, "PlayersPlace/Construction", _playersPlace);
                    FindAddGameObject(_envRoot!, "PlayersPlace/RectangleFakeGlow", _playersPlace);
                    FindAddGameObject(_envRoot!, "PlayersPlace/Frame", _playersPlace);
                    FindAddGameObject(_envRoot!, "PlayersPlace/SaberBurnMarksParticles", _playersPlace);
                    FindAddGameObject(_envRoot!, "PlayersPlace/SaberBurnMarksArea", _playersPlace);
                    FindAddGameObject(_envRoot!, "PlayersPlace/Collider", _playersPlace);
                    break;
            }
        }

        // ReSharper disable once CognitiveComplexity
        // ReSharper disable once CyclomaticComplexity
        private void FindSmallRings()
        {
            TrackLaneRingsManager? ringsManager = null;
            switch (_sceneName)
            {
                case "TutorialEnvironment":
                case "DefaultEnvironment":
                case "NiceEnvironment":
                case "MonstercatEnvironment":
                case "CrabRaveEnvironment":
                    ringsManager = _envRoot!.Find("SmallTrackLaneRings").GetComponent<TrackLaneRingsManager>();
                    break;
                case "TriangleEnvironment":
                    ringsManager = _envRoot!.Find("TriangleTrackLaneRings").GetComponent<TrackLaneRingsManager>();
                    break;
                case "DragonsEnvironment":
                    ringsManager = _envRoot!.Find("PanelsTrackLaneRings").GetComponent<TrackLaneRingsManager>();
                    break;
                case "PanicEnvironment":
                    ringsManager = _envRoot!.Find("Panels4TrackLaneRings").GetComponent<TrackLaneRingsManager>();
                    break;
                case "GreenDayEnvironment":
                    ringsManager = _envRoot!.Find("LightLinesTrackLaneRings").GetComponent<TrackLaneRingsManager>();
                    break;
                case "TimbalandEnvironment":
                    ringsManager = _envRoot!.Find("PairLaserTrackLaneRings").GetComponent<TrackLaneRingsManager>();
                    break;
                case "FitBeatEnvironment":
                    ringsManager = _envRoot!.Find("PanelsLightsTrackLaneRings").GetComponent<TrackLaneRingsManager>();
                    break;
                case "KaleidoscopeEnvironment":
                    ringsManager = _envRoot!.Find("SmallTrackLaneRings").GetComponent<TrackLaneRingsManager>();
                    break;
                case "SkrillexEnvironment":
                    ringsManager = _envRoot!.Find("TrackLaneRings1").GetComponent<TrackLaneRingsManager>();
                    break;
                case "TheSecondEnvironment":
                    FindAddGameObject(_envRoot!, "SmallTrackLaneRingsGroup", _smallRings);
                    return;
            }

            if (ringsManager is null) return;
            _smallRings.Add(ringsManager.gameObject);
            _smallRings.AddRange(ringsManager.Rings.Select(static x => x.gameObject));
        }

        private void FindBigRings()
        {
            TrackLaneRingsManager? ringsManager = null;
            switch (_sceneName)
            {
                case "DefaultEnvironment":
                case "TriangleEnvironment":
                case "NiceEnvironment":
                case "BigMirrorEnvironment":
                case "DragonsEnvironment":
                    ringsManager = _envRoot!.Find("BigTrackLaneRings").GetComponent<TrackLaneRingsManager>();
                    break;
                case "OriginsEnvironment":
                    ringsManager = _envRoot!.Find("BigLightsTrackLaneRings").GetComponent<TrackLaneRingsManager>();
                    break;
                case "FitBeatEnvironment":
                    ringsManager = _envRoot!.Find("BigCenterLightsTrackLaneRings").GetComponent<TrackLaneRingsManager>();
                    break;
                case "KaleidoscopeEnvironment":
                    ringsManager = _envRoot!.Find("DistantRings").GetComponent<TrackLaneRingsManager>();
                    break;
                case "SkrillexEnvironment":
                    ringsManager = _envRoot!.Find("TrackLaneRings2").GetComponent<TrackLaneRingsManager>();
                    break;
                case "TheSecondEnvironment":
                    FindAddGameObject(_envRoot!, "BigTrackLaneRingsGroup", _bigRings);
                    return;
            }

            if (ringsManager is null) return;
            _bigRings.Add(ringsManager.gameObject);
            _bigRings.AddRange(ringsManager.Rings.Select(static x => x.gameObject));
        }

        private void FindVisualizers()
        {
            switch (_sceneName)
            {
                case "TheSecondEnvironment":
                    FindAddGameObject(_envRoot!, "SpectrogramsTheSecond", _visualizer);
                    break;
                default:
                    FindAddGameObject(_envRoot!, "Spectrograms", _visualizer);
                    break;
            }
        }

        // ReSharper disable once CognitiveComplexity
        // ReSharper disable once CyclomaticComplexity
        private void FindTowers()
        {
            switch (_sceneName)
            {
                case "GlassDesertEnvironment":
                    for (int i = 2; i < 25; i++)
                        FindAddGameObject(_envRoot!, $"GameObject ({i})", _towers);
                    break;
                case "TutorialEnvironment":
                    FindAddGameObject(_envRoot!, "Buildings", _towers);
                    break;
                case "DefaultEnvironment":
                    FindAddGameObject(_envRoot!, "NearBuildingLeft (1)", _towers);
                    FindAddGameObject(_envRoot!, "NearBuildingRight (1)", _towers);
                    FindAddGameObject(_envRoot!, "NearBuildingLeft (2)", _towers);
                    FindAddGameObject(_envRoot!, "NearBuildingRight (2)", _towers);
                    break;
                case "TriangleEnvironment":
                    FindAddGameObject(_envRoot!, "NearBuildingLeft", _towers);
                    FindAddGameObject(_envRoot!, "NearBuildingRight", _towers);
                    break;
                case "NiceEnvironment":
                    FindAddGameObject(_envRoot!, "NearBuildingLeft (1)", _towers);
                    FindAddGameObject(_envRoot!, "NearBuildingRight (1)", _towers);
                    FindAddGameObject(_envRoot!, "NearBuildingLeft (2)", _towers);
                    FindAddGameObject(_envRoot!, "NearBuildingRight (2)", _towers);
                    break;
                case "BigMirrorEnvironment":
                    FindAddGameObject(_envRoot!, "NearBuildingLeft", _towers);
                    FindAddGameObject(_envRoot!, "NearBuildingRight", _towers);
                    break;
                case "DragonsEnvironment":
                    FindAddGameObject(_envRoot!, "HallConstruction", _towers);
                    break;
                case "KDAEnvironment":
                    FindAddGameObject(_envRoot!, "TentacleLeft", _towers);
                    FindAddGameObject(_envRoot!, "TentacleRight", _towers);
                    break;
                case "MonstercatEnvironment":
                    FindAddGameObject(_envRoot!, "NearBuildingLeft", _towers);
                    FindAddGameObject(_envRoot!, "NearBuildingRight", _towers);
                    FindAddGameObject(_envRoot!, "FarBuildings", _towers);
                    break;
                case "CrabRaveEnvironment":
                    FindAddGameObject(_envRoot!, "NearBuildingLeft", _towers);
                    FindAddGameObject(_envRoot!, "NearBuildingRight", _towers);
                    FindAddGameObject(_envRoot!, "FarBuildings", _towers);
                    break;
                case "PanicEnvironment":
                    FindAddGameObject(_envRoot!, "TopCones", _towers);
                    FindAddGameObject(_envRoot!, "BottomCones", _towers);
                    break;
                case "RocketEnvironment":
                    FindAddGameObject(_envRoot!, "RocketCarL", _towers);
                    FindAddGameObject(_envRoot!, "RocketCarR", _towers);
                    FindAddGameObject(_envRoot!, "RocketArena", _towers);
                    FindAddGameObject(_envRoot!, "RocketArenaLight", _towers);
                    FindAddGameObject(_envRoot!, "EnvLight0", _towers);
                    for (int i = 2; i < 10; i++)
                        FindAddGameObject(_envRoot!, $"EnvLight0 ({i})", _towers);
                    break;
                case "GreenDayEnvironment":
                case "GreenDayGrenadeEnvironment":
                    FindAddGameObject(_envRoot!, "GreenDayCity", _towers);
                    break;
                case "TimbalandEnvironment":
                    FindAddGameObject(_envRoot!, "Buildings", _towers);
                    FindAddGameObject(_envRoot!, "MainStructure", _towers);
                    FindAddGameObject(_envRoot!, "TopStructure", _towers);
                    FindAddGameObject(_envRoot!, "TimbalandLogo", _towers);
                    for (int i = 0; i < 4; i++)
                        FindAddGameObject(_envRoot!, $"TimbalandLogo ({i})", _towers);
                    break;
                case "BTSEnvironment":
                    FindAddGameObject(_envRoot!, "PillarTrackLaneRingsR", _towers);
                    FindAddGameObject(_envRoot!, "PillarTrackLaneRingsR (1)", _towers);
                    FindAddGameObject(_envRoot!, "PillarsMovementEffect", _towers);
                    FindAddGameObject(_envRoot!, "PillarPair", _towers);
                    for (int i = 1; i < 5; i++)
                        FindAddGameObject(_envRoot!, $"PillarPair ({i})", _towers);
                    FindAddGameObject(_envRoot!, "SmallPillarPair", _towers);
                    for (int i = 1; i < 4; i++)
                        FindAddGameObject(_envRoot!, $"SmallPillarPair ({i})", _towers);
                    break;
                case "BillieEnvironment":
                    FindAddGameObject(_envRoot!, "Mountains", _towers);
                    FindAddGameObject(_envRoot!, "Clouds", _towers);
                    break;
                case "GagaEnvironment":
                    FindAddGameObject(_envRoot!, "TeslaTower1L", _towers);
                    FindAddGameObject(_envRoot!, "TeslaTower1R", _towers);
                    FindAddGameObject(_envRoot!, "TeslaTower2L", _towers);
                    FindAddGameObject(_envRoot!, "TeslaTower2R", _towers);
                    FindAddGameObject(_envRoot!, "TeslaTower3L", _towers);
                    FindAddGameObject(_envRoot!, "TeslaTower3R", _towers);
                    FindAddGameObject(_envRoot!, "TubeR", _towers);
                    FindAddGameObject(_envRoot!, "TubeL", _towers);
                    FindAddGameObject(_envRoot!, "TubeL (1)", _towers);
                    break;
                case "TheSecondEnvironment":
                    FindAddGameObject(_envRoot!, "Buildings", _towers);
                    break;
            }
        }

        // ReSharper disable once CognitiveComplexity
        // ReSharper disable once CyclomaticComplexity
        private void FindHighway()
        {
            switch (_sceneName)
            {
                case "GameCore":
                    FindAddGameObject(_envRoot!, "IsActiveObjects/Construction/ConstructionL", _highway);
                    FindAddGameObject(_envRoot!, "IsActiveObjects/Construction/ConstructionR", _highway);
                    FindAddGameObject(_envRoot!, "IsActiveObjects/Lasers", _highway);
                    if (_envRoot!.transform.Find("IsActiveObjects/CenterRings"))
                    {
                        FindAddGameObject(_envRoot!, "IsActiveObjects/CenterRings", _highway);
                        FindAddGameObject(_envRoot!, "IsActiveObjects/PlatformEnd", _highway);
                    }
                    else
                    {
                        FindAddGameObject(_envRoot!, "Construction", _highway);
                        FindAddGameObject(_envRoot!, "Lasers", _highway);
                    }

                    break;
                case "GlassDesertEnvironment":
                    FindAddGameObject(_envRoot!, "Cube", _highway);
                    FindAddGameObject(_envRoot!, "Floor", _highway);
                    break;
                case "TutorialEnvironment":
                    FindAddGameObject(_envRoot!, "Floor", _highway);
                    break;
                case "DefaultEnvironment":
                case "PanicEnvironment":
                case "GreenDayEnvironment":
                case "GreenDayGrenadeEnvironment":
                case "TimbalandEnvironment":
                case "FitBeatEnvironment":
                    FindAddGameObject(_envRoot!, "TrackMirror", _highway);
                    FindAddGameObject(_envRoot!, "TrackConstruction", _highway);
                    break;
                case "OriginsEnvironment":
                    FindAddGameObject(_envRoot!, "Construction", _highway);
                    FindAddGameObject(_envRoot!, "TrackMirror", _highway);
                    FindAddGameObject(_envRoot!, "TrackConstruction", _highway);
                    break;
                case "TriangleEnvironment":
                    FindAddGameObject(_envRoot!, "FloorConstruction", _highway);
                    FindAddGameObject(_envRoot!, "TrackMirror", _highway);
                    break;
                case "NiceEnvironment":
                    FindAddGameObject(_envRoot!, "Floor", _highway);
                    break;
                case "BigMirrorEnvironment":
                    FindAddGameObject(_envRoot!, "Floor", _highway);
                    FindAddGameObject(_envRoot!, "Construction", _highway);
                    break;
                case "DragonsEnvironment":
                    FindAddGameObject(_envRoot!, "TrackMirror", _highway);
                    FindAddGameObject(_envRoot!, "TrackConstruction", _highway);
                    FindAddGameObject(_envRoot!, "Underground", _highway);
                    break;
                case "KDAEnvironment":
                    FindAddGameObject(_envRoot!, "Construction", _highway);
                    FindAddGameObject(_envRoot!, "FloorMirror", _highway);
                    break;
                case "MonstercatEnvironment":
                    FindAddGameObject(_envRoot!, "TrackMirror", _highway);
                    FindAddGameObject(_envRoot!, "VConstruction", _highway);
                    FindAddGameObject(_envRoot!, "MonstercatLogoL", _highway);
                    FindAddGameObject(_envRoot!, "MonstercatLogoR", _highway);
                    FindAddGameObject(_envRoot!, "Construction", _highway);
                    break;
                case "CrabRaveEnvironment":
                    FindAddGameObject(_envRoot!, "TrackMirror", _highway);
                    FindAddGameObject(_envRoot!, "VConstruction", _highway);
                    FindAddGameObject(_envRoot!, "Construction", _highway);
                    break;
                case "RocketEnvironment":
                    FindAddGameObject(_envRoot!, "Mirror", _highway);
                    FindAddGameObject(_envRoot!, "Construction", _highway);
                    break;
                case "LinkinParkEnvironment":
                    FindAddGameObject(_envRoot!, "TrackMirror", _highway);
                    FindAddGameObject(_envRoot!, "TrackConstruction", _highway);
                    FindAddGameObject(_envRoot!, "Tunnel", _highway);
                    for (int i = 1; i < 11; i++)
                        FindAddGameObject(_envRoot!, $"Tunnel ({i})", _highway);
                    FindAddGameObject(_envRoot!, "TunnelRings", _highway);
                    FindAddGameObject(_envRoot!, "LinkinParkSoldier", _highway);
                    FindAddGameObject(_envRoot!, "LinkinParkTextLogoL", _highway);
                    FindAddGameObject(_envRoot!, "LinkinParkTextLogoR", _highway);
                    break;
                case "BTSEnvironment":
                    FindAddGameObject(_envRoot!, "TrackMirror", _highway);
                    FindAddGameObject(_envRoot!, "Construction", _highway);
                    FindAddGameObject(_envRoot!, "Clouds", _highway);
                    FindAddGameObject(_envRoot!, "StarHemisphere", _highway);
                    FindAddGameObject(_envRoot!, "StarEmitterPS", _highway);
                    FindAddGameObject(_envRoot!, "BTSStarTextEffectEvent", _highway);
                    break;
                case "SkrillexEnvironment":
                    FindAddGameObject(_envRoot!, "TrackBL", _highway);
                    FindAddGameObject(_envRoot!, "TrackBR", _highway);
                    FindAddGameObject(_envRoot!, "TrackTR", _highway);
                    FindAddGameObject(_envRoot!, "TrackTL", _highway);
                    break;
                case "KaleidoscopeEnvironment":
                    FindAddGameObject(_envRoot!, "TrackMirror", _highway);
                    FindAddGameObject(_envRoot!, "Construction", _highway);
                    break;
                case "InterscopeEnvironment":
                    FindAddGameObject(_envRoot!, "Logo", _highway);
                    FindAddGameObject(_envRoot!, "Floor", _highway);
                    FindAddGameObject(_envRoot!, "Spectrograms", _highway);
                    FindAddGameObject(_envRoot!, "Pillar/PillarL", _highway);
                    FindAddGameObject(_envRoot!, "Pillar/PillarR", _highway);
                    for (int i = 1; i < 5; i++)
                    {
                        string baseName = $"Pillar ({i})";
                        FindAddGameObject(_envRoot!, $"{baseName}/PillarL", _highway);
                        FindAddGameObject(_envRoot!, $"{baseName}/PillarR", _highway);
                    }

                    FindAddGameObject(_envRoot!, "RearPillar", _highway);
                    for (int i = 1; i < 6; i++)
                    {
                        FindAddGameObject(_envRoot!, $"RearPillar ({i})", _highway);
                        FindAddGameObject(_envRoot!, $"Plane ({i})", _highway);
                    }

                    for (int i = 1; i < 5; i++)
                    {
                        FindAddGameObject(_envRoot!, $"Car{i}", _highway);
                        FindAddGameObject(_envRoot!, $"FarCar{i}", _highway);
                    }

                    break;
                case "BillieEnvironment":
                    FindAddGameObject(_envRoot!, "Rain", _highway);
                    FindAddGameObject(_envRoot!, "Waterfall", _highway);
                    FindAddGameObject(_envRoot!, "LeftRail", _highway);
                    FindAddGameObject(_envRoot!, "RightRail", _highway);
                    FindAddGameObject(_envRoot!, "LeftFarRail1", _highway);
                    FindAddGameObject(_envRoot!, "LeftFarRail2", _highway);
                    FindAddGameObject(_envRoot!, "RightFarRail1", _highway);
                    FindAddGameObject(_envRoot!, "RightFarRail2", _highway);
                    FindAddGameObject(_envRoot!, "RailingFullBack", _highway);
                    FindAddGameObject(_envRoot!, "RailingFullFront", _highway);
                    FindAddGameObject(_envRoot!, "LastRailingCurve", _highway);
                    FindAddGameObject(_envRoot!, "WaterRainRipples", _highway);
                    break;
                case "HalloweenEnvironment":
                    FindAddGameObject(_envRoot!, "Ground", _highway);
                    for (int i = 1; i < 92; i++)
                        FindAddGameObject(_envRoot!, $"GroundStone ({i})", _highway);
                    FindAddGameObject(_envRoot!, "Fence", _highway, true);
                    FindAddGameObject(_envRoot!, "Fence", _highway);
                    for (int i = 1; i < 25; i++)
                        FindAddGameObject(_envRoot!, $"Fence ({i})", _highway);
                    FindAddGameObject(_envRoot!, "Grave", _highway, true);
                    FindAddGameObject(_envRoot!, "Grave", _highway, true);
                    FindAddGameObject(_envRoot!, "Grave0", _highway);
                    FindAddGameObject(_envRoot!, "Grave0 (1)", _highway);
                    FindAddGameObject(_envRoot!, "Grave0 (2)", _highway);
                    FindAddGameObject(_envRoot!, "Grave1", _highway);
                    FindAddGameObject(_envRoot!, "Grave1 (1)", _highway);
                    FindAddGameObject(_envRoot!, "Castle", _highway);
                    for (int i = 1; i < 4; i++)
                        FindAddGameObject(_envRoot!, $"Castle ({i})", _highway);
                    FindAddGameObject(_envRoot!, "Tree1 (1)", _highway);
                    FindAddGameObject(_envRoot!, "Tree2", _highway);
                    FindAddGameObject(_envRoot!, "Tree3", _highway);
                    FindAddGameObject(_envRoot!, "Tree5", _highway);
                    for (int i = 1; i < 4; i++)
                    {
                        FindAddGameObject(_envRoot!, $"Tree3 ({i})", _highway);
                        FindAddGameObject(_envRoot!, $"Tree2 ({i})", _highway);
                    }

                    for (int i = 2; i < 25; i++)
                        FindAddGameObject(_envRoot!, $"TombStone ({i})", _highway);
                    FindAddGameObject(_envRoot!, "ZombieHand", _highway);
                    for (int i = 1; i < 7; i++)
                        FindAddGameObject(_envRoot!, $"ZombieHand ({i})", _highway);
                    FindAddGameObject(_envRoot!, "Crow", _highway);
                    for (int i = 1; i < 4; i++)
                        FindAddGameObject(_envRoot!, $"Crow ({i})", _highway);
                    FindAddGameObject(_envRoot!, "Bats", _highway);
                    FindAddGameObject(_envRoot!, "GroundFog", _highway);
                    break;
                case "GagaEnvironment":
                    FindAddGameObject(_envRoot!, "Construction", _highway);
                    FindAddGameObject(_envRoot!, "Runway", _highway);
                    FindAddGameObject(_envRoot!, "RunwayPillar", _highway);
                    FindAddGameObject(_envRoot!, "RunwayPillarLow (1)", _highway);
                    FindAddGameObject(_envRoot!, "RunwayPillarLow (2)", _highway);
                    FindAddGameObject(_envRoot!, "BackCube", _highway);
                    FindAddGameObject(_envRoot!, "Logo", _highway);
                    break;
                case "PyroEnvironment":
                    FindAddGameObject(_envRoot!, "PlayerSetup", _highway);
                    FindAddGameObject(_envRoot!, "Runway", _highway);
                    FindAddGameObject(_envRoot!, "Fire", _highway);
                    FindAddGameObject(_envRoot!, "SmokeLeft", _highway);
                    FindAddGameObject(_envRoot!, "CrowdFlipbookGroup", _highway);
                    FindAddGameObject(_envRoot!, "ScreenSetupLeft", _highway);
                    FindAddGameObject(_envRoot!, "ScreenSetupRight", _highway);
                    FindAddGameObject(_envRoot!, "StageRing", _highway);
                    FindAddGameObject(_envRoot!, "FrontScaffolding", _highway);
                    FindAddGameObject(_envRoot!, "ProjectorArray", _highway);
                    break;
                case "EDMEnvironment":
                    FindAddGameObject(_envRoot!, "Spectrograms", _highway);
                    FindAddGameObject(_envRoot!, "Spectrograms (1)", _highway);
                    break;
                case "TheSecondEnvironment":
                    FindAddGameObject(_envRoot!, "TrackMirror", _highway);
                    FindAddGameObject(_envRoot!, "TrackConstruction", _highway);
                    break;
            }
        }

        private void FindBackColumns()
        {
            switch (_sceneName)
            {
                case "GlassDesertEnvironment":
                    FindAddGameObject(_envRoot!, "SeparatorWall", _backColumns);
                    for (int i = 1; i < 16; i++)
                        FindAddGameObject(_envRoot!, $"SeparatorWall ({i})", _backColumns);
                    break;
                case "MonstercatEnvironment":
                    FindAddGameObject(_envRoot!, "SpectrogramEnd", _backColumns);
                    break;
                default:
                    FindAddGameObject(_envRoot!, "BackColumns", _backColumns);
                    break;
            }
        }

        // ReSharper disable once CognitiveComplexity
        // ReSharper disable once CyclomaticComplexity
        private void FindRotatingLasers()
        {
            switch (_sceneName)
            {
                case "GlassDesertEnvironment":
                    for (int i = 9; i < 13; i++)
                        FindAddGameObject(_envRoot!, $"LightPillar ({i})", _rotatingLasers);
                    for (int i = 19; i < 26; i++)
                        FindAddGameObject(_envRoot!, $"LightPillar ({i})", _rotatingLasers);
                    break;
                case "DefaultEnvironment":
                    FindAddGameObject(_envRoot!, "RotatingLasersPair", _rotatingLasers);
                    for (int i = 1; i < 4; i++)
                        FindAddGameObject(_envRoot!, $"RotatingLasersPair ({i})", _rotatingLasers);
                    break;
                case "OriginsEnvironment":
                    FindAddGameObject(_envRoot!, "RotatingLasersPair", _rotatingLasers);
                    for (int i = 1; i < 5; i++)
                        FindAddGameObject(_envRoot!, $"RotatingLasersPair ({i})", _rotatingLasers);
                    break;
                case "TriangleEnvironment":
                    FindAddGameObject(_envRoot!, "RotatingLasersPair", _rotatingLasers);
                    for (int i = 1; i < 7; i++)
                        FindAddGameObject(_envRoot!, $"RotatingLasersPair ({i})", _rotatingLasers);
                    break;
                case "NiceEnvironment":
                    FindAddGameObject(_envRoot!, "RotatingLaserLeft", _rotatingLasers);
                    FindAddGameObject(_envRoot!, "RotatingLaserRight", _rotatingLasers);
                    for (int i = 1; i < 4; i++)
                    {
                        FindAddGameObject(_envRoot!, $"RotatingLaserLeft ({i})", _rotatingLasers);
                        FindAddGameObject(_envRoot!, $"RotatingLaserRight ({i})", _rotatingLasers);
                    }

                    break;
                case "BigMirrorEnvironment":
                    FindAddGameObject(_envRoot!, "RotatingLasersPair", _rotatingLasers);
                    for (int i = 1; i < 4; i++)
                        FindAddGameObject(_envRoot!, $"RotatingLasersPair ({i})", _rotatingLasers);
                    break;
                case "DragonsEnvironment":
                    FindAddGameObject(_envRoot!, "RotatingLasersPair", _rotatingLasers);
                    for (int i = 1; i < 5; i++)
                        FindAddGameObject(_envRoot!, $"RotatingLasersPair ({i})", _rotatingLasers);
                    break;
                case "KDAEnvironment":
                    FindAddGameObject(_envRoot!, "RotatingLasersPair", _rotatingLasers);
                    for (int i = 1; i < 7; i++)
                        FindAddGameObject(_envRoot!, $"RotatingLasersPair ({i})", _rotatingLasers);
                    break;
                case "MonstercatEnvironment":
                    FindAddGameObject(_envRoot!, "RotatingLasersPair", _rotatingLasers);
                    for (int i = 1; i < 5; i++)
                        FindAddGameObject(_envRoot!, $"RotatingLasersPair ({i})", _rotatingLasers);
                    break;
                case "CrabRaveEnvironment":
                    FindAddGameObject(_envRoot!, "RotatingLasersPair", _rotatingLasers);
                    for (int i = 1; i < 5; i++)
                        FindAddGameObject(_envRoot!, $"RotatingLasersPair ({i})", _rotatingLasers);
                    break;
                case "PanicEnvironment":
                    FindAddGameObject(_envRoot!, "RotatingLasersPair", _rotatingLasers);
                    for (int i = 1; i < 7; i++)
                        FindAddGameObject(_envRoot!, $"RotatingLasersPair ({i})", _rotatingLasers);
                    break;
                case "RocketEnvironment":
                    for (int i = 7; i < 14; i++)
                        FindAddGameObject(_envRoot!, $"RotatingLasersPair ({i})", _rotatingLasers);
                    break;
                case "GreenDayEnvironment":
                case "GreenDayGrenadeEnvironment":
                    FindAddGameObject(_envRoot!, "RotatingLasersPair", _rotatingLasers);
                    for (int i = 1; i < 6; i++)
                        FindAddGameObject(_envRoot!, $"RotatingLasersPair ({i})", _rotatingLasers);
                    break;
                case "FitBeatEnvironment":
                    FindAddGameObject(_envRoot!, "RotatingLasersPair", _rotatingLasers);
                    for (int i = 1; i < 8; i++)
                        FindAddGameObject(_envRoot!, $"RotatingLasersPair ({i})", _rotatingLasers);
                    break;
                case "LinkinParkEnvironment":
                    FindAddGameObject(_envRoot!, "TunnelRotatingLasersPair", _rotatingLasers);
                    for (int i = 1; i < 18; i++)
                        FindAddGameObject(_envRoot!, $"TunnelRotatingLasersPair ({i})", _rotatingLasers);
                    break;
                case "BillieEnvironment":
                    for (int i = 4; i < 15; i++)
                        FindAddGameObject(_envRoot!, $"TunnelRotatingLasersPair ({i})", _rotatingLasers);
                    break;
                case "HalloweenEnvironment":
                    for (int i = 7; i < 24; i++)
                        FindAddGameObject(_envRoot!, $"RotatingLasersPair ({i})", _rotatingLasers);
                    break;
                case "TheSecondEnvironment":
                    FindAddGameObject(_envRoot!, "SpotlightGroupLeft", _rotatingLasers);
                    FindAddGameObject(_envRoot!, "SpotlightGroupRight", _rotatingLasers);
                    break;
            }
        }

        // ReSharper disable once CognitiveComplexity
        // ReSharper disable once CyclomaticComplexity
        private void FindDoubleColorLasers()
        {
            switch (_sceneName)
            {
                case "TutorialEnvironment":
                    for (int i = 10; i < 20; i++)
                        FindAddGameObject(_envRoot!, $"DoubleColorLaser ({i})", _doubleColorLasers);
                    break;
                case "DefaultEnvironment":
                    FindAddGameObject(_envRoot!, "DoubleColorLaserL", _doubleColorLasers);
                    FindAddGameObject(_envRoot!, "DoubleColorLaserR", _doubleColorLasers);
                    for (int i = 1; i < 5; i++)
                    {
                        FindAddGameObject(_envRoot!, $"DoubleColorLaserL ({i})", _doubleColorLasers);
                        FindAddGameObject(_envRoot!, $"DoubleColorLaserR ({i})", _doubleColorLasers);
                    }

                    break;
                case "OriginsEnvironment":
                    FindAddGameObject(_envRoot!, "Laser", _doubleColorLasers);
                    for (int i = 1; i < 4; i++)
                        FindAddGameObject(_envRoot!, $"Laser ({i})", _doubleColorLasers);
                    break;
                case "TriangleEnvironment":
                    FindAddGameObject(_envRoot!, "DoubleColorLaser", _doubleColorLasers);
                    for (int i = 1; i < 10; i++)
                        FindAddGameObject(_envRoot!, $"DoubleColorLaser ({i})", _doubleColorLasers);
                    break;
                case "NiceEnvironment":
                    FindAddGameObject(_envRoot!, "DoubleColorLaser", _doubleColorLasers);
                    for (int i = 1; i < 8; i++)
                        FindAddGameObject(_envRoot!, $"DoubleColorLaser ({i})", _doubleColorLasers);
                    break;
                case "BigMirrorEnvironment":
                    FindAddGameObject(_envRoot!, "DoubleColorLaser", _doubleColorLasers);
                    for (int i = 1; i < 10; i++)
                        FindAddGameObject(_envRoot!, $"DoubleColorLaser ({i})", _doubleColorLasers);
                    break;
                case "KDAEnvironment":
                    for (int i = 2; i < 5; i++)
                        FindAddGameObject(_envRoot!, $"Laser ({i})", _doubleColorLasers);
                    for (int i = 7; i < 10; i++)
                        FindAddGameObject(_envRoot!, $"Laser ({i})", _doubleColorLasers);
                    break;
                case "MonstercatEnvironment":
                    for (int i = 4; i < 13; i++)
                        FindAddGameObject(_envRoot!, $"Laser ({i})", _doubleColorLasers);
                    break;
                case "CrabRaveEnvironment":
                    for (int i = 4; i < 13; i++)
                        FindAddGameObject(_envRoot!, $"Laser ({i})", _doubleColorLasers);
                    break;
                case "BillieEnvironment":
                    FindAddGameObject(_envRoot!, "BottomPairLasers", _doubleColorLasers);
                    for (int i = 1; i < 9; i++)
                        FindAddGameObject(_envRoot!, $"BottomPairLasers ({i})", _doubleColorLasers);
                    break;
                case "TheSecondEnvironment":
                    FindAddGameObject(_envRoot!, "Main Lasers Top", _doubleColorLasers);
                    FindAddGameObject(_envRoot!, "Main Lasers Bottom", _doubleColorLasers);
                    break;
            }
        }

        // ReSharper disable once CognitiveComplexity
        // ReSharper disable once CyclomaticComplexity
        private void FindBackLasers()
        {
            switch (_sceneName)
            {
                case "PanicEnvironment":
                    FindAddGameObject(_envRoot!, "Window", _backLasers, true);
                    FindAddGameObject(_envRoot!, "Window", _backLasers);
                    break;
                case "RocketEnvironment":
                    FindAddGameObject(_envRoot!, "FrontLights", _backLasers);
                    FindAddGameObject(_envRoot!, "RocketGateLight", _backLasers);
                    FindAddGameObject(_envRoot!, "GateLight0", _backLasers);
                    FindAddGameObject(_envRoot!, "GateLight1", _backLasers);
                    FindAddGameObject(_envRoot!, "GateLight1 (4)", _backLasers);
                    break;
                case "GreenDayEnvironment":
                case "GreenDayGrenadeEnvironment":
                    FindAddGameObject(_envRoot!, "Logo", _backLasers);
                    FindAddGameObject(_envRoot!, "FrontLight", _backLasers);
                    break;
                case "TimbalandEnvironment":
                    FindAddGameObject(_envRoot!, "FrontLights", _backLasers);
                    for (int i = 4; i < 8; i++)
                        FindAddGameObject(_envRoot!, $"Light ({i})", _backLasers);
                    break;
                case "LinkinParkEnvironment":
                    FindAddGameObject(_envRoot!, "Logo", _backLasers);
                    FindAddGameObject(_envRoot!, "LogoLight", _backLasers);
                    break;
                case "BTSEnvironment":
                    FindAddGameObject(_envRoot!, "MagicDoorSprite", _backLasers);
                    break;
                case "SkrillexEnvironment":
                    FindAddGameObject(_envRoot!, "SkrillexLogo", _backLasers);
                    FindAddGameObject(_envRoot!, "SkrillexLogo (1)", _backLasers);
                    break;
                case "BillieEnvironment":
                    FindAddGameObject(_envRoot!, "DayAndNight/Day", _backLasers);
                    FindAddGameObject(_envRoot!, "DayAndNight/Night", _backLasers);
                    break;
                case "HalloweenEnvironment":
                    FindAddGameObject(_envRoot!, "Moon", _backLasers);
                    FindAddGameObject(_envRoot!, "GateLight", _backLasers);
                    break;
                case "GagaEnvironment":
                    FindAddGameObject(_envRoot!, "FrontLasers", _backLasers);
                    break;
                case "PyroEnvironment":
                    FindAddGameObject(_envRoot!, "PyroLogo", _backLasers);
                    break;
                case "TheSecondEnvironment":
                    FindAddGameObject(_envRoot!, "FrontLogo", _backLasers);
                    break;
                default:
                    FindAddGameObject(_envRoot!, "FrontLights", _backLasers);
                    break;
            }
        }

        // ReSharper disable once CognitiveComplexity
        // ReSharper disable once CyclomaticComplexity
        private void FindTrackLights()
        {
            switch (_sceneName)
            {
                case "GlassDesertEnvironment":
                    FindAddGameObject(_envRoot!, "TopLaser", _trackLights);
                    for (int i = 1; i < 6; i++)
                        FindAddGameObject(_envRoot!, $"TopLaser ({i})", _trackLights);
                    for (int i = 4; i < 13; i++)
                        FindAddGameObject(_envRoot!, $"DownLaser ({i})", _trackLights);
                    for (int i = 0; i < 7; i++)
                        FindAddGameObject(_envRoot!, "TopLightMesh", _trackLights, true);
                    break;
                case "TutorialEnvironment":
                    FindAddGameObject(_envRoot!, "GlowLines", _trackLights);
                    break;
                case "DefaultEnvironment":
                    FindAddGameObject(_envRoot!, "NeonTubeL", _trackLights);
                    FindAddGameObject(_envRoot!, "NeonTubeR", _trackLights);
                    break;
                case "OriginsEnvironment":
                    FindAddGameObject(_envRoot!, "NeonTube", _trackLights);
                    FindAddGameObject(_envRoot!, "NeonTube (1)", _trackLights);
                    FindAddGameObject(_envRoot!, "LightAreaL", _trackLights);
                    FindAddGameObject(_envRoot!, "LightAreaR", _trackLights);
                    FindAddGameObject(_envRoot!, "SidePSL", _trackLights);
                    FindAddGameObject(_envRoot!, "SidePSR", _trackLights);
                    break;
                case "TriangleEnvironment":
                    FindAddGameObject(_envRoot!, "NeonTubeDirectionalL", _trackLights);
                    FindAddGameObject(_envRoot!, "NeonTubeDirectionalR", _trackLights);
                    break;
                case "NiceEnvironment":
                    FindAddGameObject(_envRoot!, "GlowLineL", _trackLights);
                    FindAddGameObject(_envRoot!, "GlowLineR", _trackLights);
                    FindAddGameObject(_envRoot!, "GlowLineFarL", _trackLights);
                    FindAddGameObject(_envRoot!, "GlowLineFarR", _trackLights);
                    break;
                case "BigMirrorEnvironment":
                    FindAddGameObject(_envRoot!, "NeonTubeDirectionalL", _trackLights);
                    FindAddGameObject(_envRoot!, "NeonTubeDirectionalR", _trackLights);
                    FindAddGameObject(_envRoot!, "NeonTubeDirectionalFL", _trackLights);
                    FindAddGameObject(_envRoot!, "NeonTubeDirectionalFR", _trackLights);
                    break;
                case "DragonsEnvironment":
                    FindAddGameObject(_envRoot!, "GlowLineL", _trackLights);
                    FindAddGameObject(_envRoot!, "GlowLineR", _trackLights);
                    FindAddGameObject(_envRoot!, "ConstructionGlowLine (1)", _trackLights);
                    FindAddGameObject(_envRoot!, "ConstructionGlowLine (4)", _trackLights);
                    FindAddGameObject(_envRoot!, "ConstructionGlowLine (5)", _trackLights);
                    FindAddGameObject(_envRoot!, "ConstructionGlowLine (6)", _trackLights);
                    FindAddGameObject(_envRoot!, "DragonsSidePSL", _trackLights);
                    FindAddGameObject(_envRoot!, "DragonsSidePSR", _trackLights);
                    break;
                case "KDAEnvironment":
                    FindAddGameObject(_envRoot!, "GlowLineLVisible", _trackLights);
                    FindAddGameObject(_envRoot!, "GlowLineRVisible", _trackLights);
                    FindAddGameObject(_envRoot!, "GlowTopLine", _trackLights);
                    FindAddGameObject(_envRoot!, "GlowLineFarL", _trackLights);
                    FindAddGameObject(_envRoot!, "GlowLineFarR", _trackLights);
                    for (int i = 1; i < 5; i++)
                        FindAddGameObject(_envRoot!, $"GlowTopLine ({i})", _trackLights);
                    FindAddGameObject(_envRoot!, "GlowLine", _trackLights);
                    for (int i = 1; i < 77; i++)
                        FindAddGameObject(_envRoot!, $"GlowLine ({i})", _trackLights);
                    break;
                case "MonstercatEnvironment":
                    FindAddGameObject(_envRoot!, "GlowLineL", _trackLights);
                    FindAddGameObject(_envRoot!, "GlowLineL (1)", _trackLights);
                    FindAddGameObject(_envRoot!, "GlowLineR", _trackLights);
                    FindAddGameObject(_envRoot!, "GlowLineR (1)", _trackLights);
                    for (int i = 5; i < 12; i++)
                        FindAddGameObject(_envRoot!, $"GlowTopLine ({i})", _trackLights);
                    break;
                case "CrabRaveEnvironment":
                    FindAddGameObject(_envRoot!, "GlowLineL", _trackLights);
                    FindAddGameObject(_envRoot!, "GlowLineL (1)", _trackLights);
                    FindAddGameObject(_envRoot!, "GlowLineR", _trackLights);
                    FindAddGameObject(_envRoot!, "GlowLineR (1)", _trackLights);
                    for (int i = 5; i < 12; i++)
                        FindAddGameObject(_envRoot!, $"GlowTopLine ({i})", _trackLights);
                    break;
                case "PanicEnvironment":
                    FindAddGameObject(_envRoot!, "Light (5)", _trackLights);
                    FindAddGameObject(_envRoot!, "ConstructionGlowLine (15)", _trackLights);
                    for (int i = 4; i < 9; i++)
                        FindAddGameObject(_envRoot!, $"ConstructionGlowLine ({i})", _trackLights);
                    break;
                case "RocketEnvironment":
                    FindAddGameObject(_envRoot!, "GlowLineR (1)", _trackLights);
                    for (int i = 1; i < 10; i++)
                        FindAddGameObject(_envRoot!, $"GlowLineL ({i})", _trackLights);
                    break;
                case "GreenDayEnvironment":
                case "GreenDayGrenadeEnvironment":
                    FindAddGameObject(_envRoot!, "GlowLineL", _trackLights);
                    FindAddGameObject(_envRoot!, "GlowLineR", _trackLights);
                    FindAddGameObject(_envRoot!, "GlowLineL (2)", _trackLights);
                    FindAddGameObject(_envRoot!, "GlowLineL (4)", _trackLights);
                    FindAddGameObject(_envRoot!, "GlowLineL (7)", _trackLights);
                    FindAddGameObject(_envRoot!, "GlowLineL (8)", _trackLights);
                    for (int i = 13; i < 25; i++)
                        FindAddGameObject(_envRoot!, $"GlowLineL ({i})", _trackLights);
                    break;
                case "TimbalandEnvironment":
                    FindAddGameObject(_envRoot!, "GlowLineL", _trackLights);
                    FindAddGameObject(_envRoot!, "GlowLineR", _trackLights);
                    break;
                case "LinkinParkEnvironment":
                    for (int i = 2; i < 5; i++)
                        FindAddGameObject(_envRoot!, $"LaserFloor ({i})", _trackLights);
                    FindAddGameObject(_envRoot!, "LaserTop", _trackLights);
                    for (int i = 1; i < 8; i++)
                        FindAddGameObject(_envRoot!, $"LaserTop ({i})", _trackLights);
                    FindAddGameObject(_envRoot!, "LaserL", _trackLights);
                    // ReSharper disable once StringLiteralTypo
                    FindAddGameObject(_envRoot!, "LarerR", _trackLights);
                    FindAddGameObject(_envRoot!, "LaserL (2)", _trackLights);
                    // ReSharper disable once StringLiteralTypo
                    FindAddGameObject(_envRoot!, "LarerR (2)", _trackLights);
                    break;
                case "BTSEnvironment":
                    FindAddGameObject(_envRoot!, "GlowLineL", _trackLights);
                    FindAddGameObject(_envRoot!, "GlowLineR", _trackLights);
                    FindAddGameObject(_envRoot!, "GlowLineH", _trackLights);
                    FindAddGameObject(_envRoot!, "GlowLineH (2)", _trackLights);
                    FindAddGameObject(_envRoot!, "LaserL", _trackLights);
                    FindAddGameObject(_envRoot!, "LaserR", _trackLights);
                    FindAddGameObject(_envRoot!, "GlowLineC", _trackLights);
                    FindAddGameObject(_envRoot!, "BottomGlow", _trackLights);
                    for (int i = 0; i < 4; i++)
                        FindAddGameObject(_envRoot!, "SideLaser", _trackLights, true);
                    break;
                case "SkrillexEnvironment":
                    FindAddGameObject(_envRoot!, "LeftLaser", _trackLights);
                    FindAddGameObject(_envRoot!, "RightLaser", _trackLights);
                    FindAddGameObject(_envRoot!, "NeonSide", _trackLights);
                    for (int i = 1; i < 18; i++)
                        FindAddGameObject(_envRoot!, $"NeonSide ({i})", _trackLights);
                    break;
                case "InterscopeEnvironment":
                    FindAddGameObject(_envRoot!, "NeonTop", _trackLights);
                    FindAddGameObject(_envRoot!, "Pillar/NeonLightL", _trackLights);
                    FindAddGameObject(_envRoot!, "Pillar/NeonLightR", _trackLights);
                    for (int i = 1; i < 5; i++)
                    {
                        FindAddGameObject(_envRoot!, $"NeonTop ({i})", _trackLights);
                        string baseName = $"Pillar ({i})";
                        FindAddGameObject(_envRoot!, $"{baseName}/NeonLightL", _trackLights);
                        FindAddGameObject(_envRoot!, $"{baseName}/NeonLightR", _trackLights);
                    }

                    break;
                case "BillieEnvironment":
                    FindAddGameObject(_envRoot!, "LightRailingSegment", _trackLights);
                    for (int i = 1; i < 4; i++)
                        FindAddGameObject(_envRoot!, $"LightRailingSegment ({i})", _trackLights);
                    break;
                case "HalloweenEnvironment":
                    FindAddGameObject(_envRoot!, "NeonTubeL", _trackLights);
                    FindAddGameObject(_envRoot!, "NeonTubeL (1)", _trackLights);
                    FindAddGameObject(_envRoot!, "NeonTubeR", _trackLights);
                    FindAddGameObject(_envRoot!, "NeonTubeR (1)", _trackLights);
                    FindAddGameObject(_envRoot!, "NeonTubeC", _trackLights);
                    for (int i = 6; i < 10; i++)
                        FindAddGameObject(_envRoot!, $"GlowLineL ({i})", _trackLights);
                    break;
                case "GagaEnvironment":
                    FindAddGameObject(_envRoot!, "Aurora", _trackLights);
                    break;
                case "WeaveEnvironment":
                    for (int i = 0; i < 16; i++)
                        FindAddGameObject(_envRoot!, $"LightGroup{i}", _trackLights);
                    break;
                case "PyroEnvironment":
                    FindAddGameObject(_envRoot!, "Behind", _highway);
                    FindAddGameObject(_envRoot!, "Video", _highway);
                    FindAddGameObject(_envRoot!, "MainLasers", _highway);
                    FindAddGameObject(_envRoot!, "Stairs", _highway);
                    FindAddGameObject(_envRoot!, "MainStageSetup", _highway);
                    FindAddGameObject(_envRoot!, "LightBoxesScaffoldingLeft", _highway);
                    FindAddGameObject(_envRoot!, "LightBoxesScaffoldingRight", _highway);
                    break;
                case "EDMEnvironment":
                    FindAddGameObject(_envRoot!, "CloseCircle", _trackLights);
                    FindAddGameObject(_envRoot!, "DistantCircle1", _trackLights);
                    FindAddGameObject(_envRoot!, "DistantCircle2", _trackLights);
                    FindAddGameObject(_envRoot!, "Laser", _trackLights);
                    FindAddGameObject(_envRoot!, "TopCircle", _trackLights);
                    FindAddGameObject(_envRoot!, "SingleSourceCircularLY", _trackLights);
                    FindAddGameObject(_envRoot!, "SingleSourceCircularRY", _trackLights);
                    FindAddGameObject(_envRoot!, "SingleSourceLaserUp", _trackLights);
                    FindAddGameObject(_envRoot!, "SingleSourceLaserLeftMid", _trackLights);
                    FindAddGameObject(_envRoot!, "SingleSourceLaserDown", _trackLights);
                    FindAddGameObject(_envRoot!, "SingleSourceLaserUp", _trackLights);
                    FindAddGameObject(_envRoot!, "SingleSourceLaserRightMid", _trackLights);
                    FindAddGameObject(_envRoot!, "SingleSourceLaserDown", _trackLights);
                    for (int i = 1; i < 5; i++)
                        FindAddGameObject(_envRoot!, $"Laser ({i})", _trackLights);
                    break;
                case "TheSecondEnvironment":
                    FindAddGameObject(_envRoot!, "RunwayLasers", _trackLights);
                    break;
            }
        }
    }
}
