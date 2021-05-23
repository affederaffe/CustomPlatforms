using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using CustomFloorPlugin.Configuration;

using UnityEngine;
using UnityEngine.SceneManagement;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Activates and deactivates world geometry in the active scene as required by the chosen custom platform<br />
    /// Most documentation on this file is omitted because it is a giant clusterfuck and I hate it... with a passion.
    /// </summary>
    internal class EnvironmentHider
    {
        private readonly PluginConfig _config;
        private readonly AssetLoader _assetLoader;
        private readonly PlatformManager _platformManager;
        private readonly GameScenesManager _gameScenesManager;

        private readonly List<GameObject> _menuEnvironment = new();
        private readonly List<GameObject> _playersPlace = new();
        private readonly List<GameObject> _feet = new();
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
        private GameObject? _root;

        public EnvironmentHider(PluginConfig config,
                                AssetLoader assetLoader,
                                PlatformManager platformManager,
                                GameScenesManager gameScenesManager)
        {
            _config = config;
            _assetLoader = assetLoader;
            _platformManager = platformManager;
            _gameScenesManager = gameScenesManager;
        }

        private TrackLaneRing[] TrackLaneRings => _trackLaneRings ??= Object.FindObjectsOfType<TrackLaneRing>();
        private TrackLaneRing[]? _trackLaneRings;

        /// <summary>
        /// Hide world objects as required by the active platform
        /// </summary>
        internal void HideObjectsForPlatform()
        {
            if (TryGetEnvironmentRoot(out _root))
            {
                FindEnvironment();
                SetCollectionHidden(_menuEnvironment, _platformManager.GetIndexForType(PlatformType.Active) != 0);
                SetCollectionHidden(_playersPlace, _platformManager.ActivePlatform.hideDefaultPlatform);
                SetCollectionHidden(_feet, _platformManager.ActivePlatform.hideDefaultPlatform && !_config.AlwaysShowFeet);
                SetCollectionHidden(_smallRings, _platformManager.ActivePlatform.hideSmallRings);
                SetCollectionHidden(_bigRings, _platformManager.ActivePlatform.hideBigRings);
                SetCollectionHidden(_visualizer, _platformManager.ActivePlatform.hideEQVisualizer);
                SetCollectionHidden(_towers, _platformManager.ActivePlatform.hideTowers);
                SetCollectionHidden(_highway, _platformManager.ActivePlatform.hideHighway);
                SetCollectionHidden(_backColumns, _platformManager.ActivePlatform.hideBackColumns);
                SetCollectionHidden(_backLasers, _platformManager.ActivePlatform.hideBackLasers);
                SetCollectionHidden(_doubleColorLasers, _platformManager.ActivePlatform.hideDoubleColorLasers);
                SetCollectionHidden(_rotatingLasers, _platformManager.ActivePlatform.hideRotatingLasers);
                SetCollectionHidden(_trackLights, _platformManager.ActivePlatform.hideTrackLights);
                CleanupEnvironment();
            }
        }

        /// <summary>
        /// Finds all GameObjects that make up the default environment and groups them into lists
        /// </summary>
        private void FindEnvironment()
        {
            FindMenuEnvironment();
            FindPlayersPlace();
            FindFeetIcon();
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
        /// (De-)Activates the PlayersPlace replacement as well as preparing for the next platform
        /// </summary>
        private void CleanupEnvironment()
        {
            _trackLaneRings = null;
            _root = null;
            _assetLoader.TogglePlayersPlace(!_platformManager.ActivePlatform.hideDefaultPlatform &&
                                            _sceneName == "MenuEnvironment" &&
                                            _platformManager.GetIndexForType(PlatformType.Active) != 0);
        }

        /// <summary>
        /// Gets the currently loaded <see cref="Scene" /> and returns the Environment root GameObject
        /// </summary>
        private bool TryGetEnvironmentRoot(out GameObject? root)
        {
            root = null;
            _sceneName = _gameScenesManager.GetCurrentlyLoadedSceneNames().Last(x => x.EndsWith("Environment", System.StringComparison.Ordinal));
            if (_sceneName == "MultiplayerEnvironment") _sceneName = "GameCore";
            Scene scene = SceneManager.GetSceneByName(_sceneName);
            if (!scene.IsValid()) return false;
            root = scene.GetRootGameObjects().First(x => x.name.EndsWith("Environment", System.StringComparison.Ordinal) || x.name.EndsWith("LocalActivePlayerController(Clone)", System.StringComparison.Ordinal));
            return true;
        }

        /// <summary>
        /// Set the active state of a Collection of GameObjects
        /// </summary>
        /// <param name="list">A <see cref="List{T}" /> of GameObjects</param>
        /// <param name="hidden">A boolean describing the desired hidden state</param>
        private void SetCollectionHidden(List<GameObject> list, bool hidden)
        {
            IEnumerable<GameObject> gameObjects = _sceneName == "MenuEnvironment" ? list : list.Where(x => x.activeSelf);
            foreach (GameObject gameObject in gameObjects)
                gameObject.SetActive(!hidden);
            list.Clear();
        }

        /// <summary>
        /// Finds a GameObject by name and adds it to the provided list
        /// </summary>
        /// <param name="name">The name of the desired GameObject</param>
        /// <param name="list">The list to be added to</param>
        /// <param name="rename">Whether the GameObject should be renamed or not</param>
        private bool FindAddGameObject(string name, List<GameObject> list, bool rename = false)
        {
            GameObject? go = _root!.transform.Find(name)?.gameObject;
            if (go == null) return false;
            if (rename)
                go.name += "renamed";
            list.Add(go);
            return true;
        }

        private void FindMenuEnvironment()
        {
            switch (_sceneName)
            {
                case "MenuEnvironment":
                case "Credits":
                    FindAddGameObject("NearBuildingLeft", _menuEnvironment);
                    FindAddGameObject("NearBuildingRight", _menuEnvironment);
                    FindAddGameObject("NearBuildingLeft (1)", _menuEnvironment);
                    FindAddGameObject("NearBuildingRight (1)", _menuEnvironment);
                    FindAddGameObject("GroundCollider", _menuEnvironment);
                    FindAddGameObject("DefaultEnvironment/Ground", _menuEnvironment);
                    FindAddGameObject("DefaultEnvironment/PlayersPlace", _menuEnvironment);
                    FindAddGameObject("DefaultEnvironment/PileOfNotes", _menuEnvironment);
                    FindAddGameObject("DefaultEnvironment/MenuFogRing", _menuEnvironment);
                    FindAddGameObject("DefaultEnvironment/NeonLights", _menuEnvironment);
                    FindAddGameObject("DefaultEnvironment/Notes", _menuEnvironment);
                    break;
            }
        }

        private void FindPlayersPlace()
        {
            switch (_sceneName)
            {
                case "GameCore":
                    FindAddGameObject("IsActiveObjects/Construction/PlayersPlace", _playersPlace);
                    break;
                case "GlassDesertEnvironment":
                    FindAddGameObject("PlayersPlace", _playersPlace);
                    FindAddGameObject("Collider", _playersPlace);
                    break;
                case "LinkinParkEnvironment":
                    FindAddGameObject("PlayersPlace", _playersPlace);
                    FindAddGameObject("PlayersPlaceShadow", _playersPlace);
                    break;
                default:
                    FindAddGameObject("PlayersPlace", _playersPlace);
                    break;
            }
        }

        private void FindFeetIcon()
        {
            switch (_sceneName)
            {
                case "GameCore":
                    FindAddGameObject("IsActiveObjects/Construction/PlayersPlace/Feet", _feet);
                    break;
                default:
                    FindAddGameObject("PlayersPlace/Feet", _feet);
                    break;
            }

            foreach (GameObject feet in _feet)
                feet.transform.SetParent(null); // Remove from original platform 
        }

        // ReSharper disable once CognitiveComplexity
        // ReSharper disable once CyclomaticComplexity
        private void FindSmallRings()
        {
            switch (_sceneName)
            {
                case "TutorialEnvironment":
                case "DefaultEnvironment":
                case "NiceEnvironment":
                case "MonstercatEnvironment":
                case "CrabRaveEnvironment":
                    FindAddGameObject("SmallTrackLaneRings", _smallRings);
                    foreach (TrackLaneRing ring in TrackLaneRings.Where(x => x.name == "SmallTrackLaneRing(Clone)"))
                        _smallRings.Add(ring.gameObject);
                    break;
                case "TriangleEnvironment":
                    FindAddGameObject("TriangleTrackLaneRings", _smallRings);
                    foreach (TrackLaneRing ring in TrackLaneRings.Where(x => x.name == "TriangleTrackLaneRing(Clone)"))
                        _smallRings.Add(ring.gameObject);
                    break;
                case "DragonsEnvironment":
                    FindAddGameObject("PanelsTrackLaneRings", _smallRings);
                    foreach (TrackLaneRing ring in TrackLaneRings.Where(x => x.name == "PanelsTrackLaneRing(Clone)"))
                        _smallRings.Add(ring.gameObject);
                    break;
                case "PanicEnvironment":
                    FindAddGameObject("Panels4TrackLaneRings", _smallRings);
                    foreach (TrackLaneRing ring in TrackLaneRings.Where(x => x.name == "Panels4TrackLaneRing(Clone)"))
                        _smallRings.Add(ring.gameObject);
                    break;
                case "GreenDayEnvironment":
                    FindAddGameObject("LightLinesTrackLaneRings", _smallRings);
                    foreach (TrackLaneRing ring in TrackLaneRings.Where(x => x.name == "LightLinesTrackLaneRing(Clone)"))
                        _smallRings.Add(ring.gameObject);
                    break;
                case "TimbalandEnvironment":
                    FindAddGameObject("PairLaserTrackLaneRings", _smallRings);
                    foreach (TrackLaneRing ring in TrackLaneRings.Where(x => x.name == "PairLaserTrackLaneRing(Clone)"))
                        _smallRings.Add(ring.gameObject);
                    break;
                case "FitBeatEnvironment":
                    FindAddGameObject("PanelsLightsTrackLaneRings", _smallRings);
                    foreach (TrackLaneRing ring in TrackLaneRings.Where(x => x.name == "PanelLightTrackLaneRing(Clone)"))
                        _smallRings.Add(ring.gameObject);
                    break;
                case "KaleidoscopeEnvironment":
                    FindAddGameObject("SmallTrackLaneRings", _smallRings);
                    foreach (TrackLaneRing ring in TrackLaneRings.Where(x => x.name == "ConeRing(Clone)"))
                        _smallRings.Add(ring.gameObject);
                    break;
            }
        }

        // ReSharper disable once CognitiveComplexity
        private void FindBigRings()
        {
            switch (_sceneName)
            {
                case "DefaultEnvironment":
                case "TriangleEnvironment":
                case "NiceEnvironment":
                case "BigMirrorEnvironment":
                    FindAddGameObject("BigTrackLaneRings", _bigRings);
                    foreach (TrackLaneRing ring in TrackLaneRings.Where(x => x.name == "BigTrackLaneRing(Clone)"))
                        _bigRings.Add(ring.gameObject);
                    break;
                case "OriginsEnvironment":
                    FindAddGameObject("BigLightsTrackLaneRings", _bigRings);
                    foreach (TrackLaneRing ring in TrackLaneRings.Where(x => x.name == "LightsTrackLaneRing(Clone)"))
                        _bigRings.Add(ring.gameObject);
                    break;
                case "FitBeatEnvironment":
                    FindAddGameObject("BigCenterLightsTrackLaneRings", _bigRings);
                    foreach (TrackLaneRing ring in TrackLaneRings.Where(x => x.name == "BigCenterLightTrackLaneRing(Clone)"))
                        _bigRings.Add(ring.gameObject);
                    break;
                case "KaleidoscopeEnvironment":
                    FindAddGameObject("DistantRings", _bigRings);
                    foreach (TrackLaneRing ring in TrackLaneRings.Where(x => x.name == "ConeRingBig(Clone)"))
                        _bigRings.Add(ring.gameObject);
                    break;
            }
        }

        private void FindVisualizers()
        {
            switch (_sceneName)
            {
                default:
                    FindAddGameObject("Spectrograms", _visualizer);
                    break;
            }
        }

        // ReSharper disable once CognitiveComplexity
        private void FindTowers()
        {
            switch (_sceneName)
            {
                case "GlassDesertEnvironment":
                    for (int i = 2; i < 25; i++)
                        FindAddGameObject($"GameObject ({i.ToString(NumberFormatInfo.InvariantInfo)})", _towers);
                    break;
                case "TutorialEnvironment":
                    FindAddGameObject("Buildings", _towers);
                    break;
                case "DefaultEnvironment":
                    FindAddGameObject("NearBuildingLeft (1)", _towers);
                    FindAddGameObject("NearBuildingRight (1)", _towers);
                    FindAddGameObject("NearBuildingLeft (2)", _towers);
                    FindAddGameObject("NearBuildingRight (2)", _towers);
                    break;
                case "TriangleEnvironment":
                    FindAddGameObject("NearBuildingLeft (3)", _towers);
                    FindAddGameObject("NearBuildingRight (3)", _towers);
                    break;
                case "NiceEnvironment":
                    FindAddGameObject("NearBuildingLeft (1)", _towers);
                    FindAddGameObject("NearBuildingRight (1)", _towers);
                    FindAddGameObject("NearBuildingLeft (2)", _towers);
                    FindAddGameObject("NearBuildingRight (2)", _towers);
                    break;
                case "BigMirrorEnvironment":
                    FindAddGameObject("NearBuildingLeft (2)", _towers);
                    FindAddGameObject("NearBuildingRight (2)", _towers);
                    break;
                case "DragonsEnvironment":
                    FindAddGameObject("HallConstruction", _towers);
                    break;
                case "KDAEnvironment":
                    FindAddGameObject("TentacleLeft", _towers);
                    FindAddGameObject("TentacleRight", _towers);
                    break;
                case "MonstercatEnvironment":
                    FindAddGameObject("NearBuildingLeft", _towers);
                    FindAddGameObject("NearBuildingRight", _towers);
                    FindAddGameObject("FarBuildings", _towers);

                    break;
                case "CrabRaveEnvironment":
                    FindAddGameObject("NearBuildingLeft", _towers);
                    FindAddGameObject("NearBuildingRight", _towers);
                    FindAddGameObject("FarBuildings", _towers);
                    break;
                case "PanicEnvironment":
                    FindAddGameObject("TopCones", _towers);
                    FindAddGameObject("BottomCones", _towers);
                    break;
                case "RocketEnvironment":
                    FindAddGameObject("RocketCarL", _towers);
                    FindAddGameObject("RocketCarR", _towers);
                    FindAddGameObject("RocketArena", _towers);
                    FindAddGameObject("RocketArenaLight", _towers);
                    FindAddGameObject("EnvLight0", _towers);
                    for (int i = 2; i < 10; i++)
                        FindAddGameObject($"EnvLight0 ({i.ToString(NumberFormatInfo.InvariantInfo)})", _towers);
                    break;
                case "GreenDayGrenadeEnvironment":
                    FindAddGameObject("GreenDayCity", _towers);
                    break;
                case "GreenDayEnvironment":
                    FindAddGameObject("GreenDayCity", _towers);
                    break;
                case "TimbalandEnvironment":
                    FindAddGameObject("Buildings", _towers);
                    FindAddGameObject("MainStructure", _towers);
                    FindAddGameObject("TopStructure", _towers);
                    break;
                case "BTSEnvironment":
                    FindAddGameObject("PillarTrackLaneRingsR", _towers);
                    FindAddGameObject("PillarTrackLaneRingsR (1)", _towers);
                    FindAddGameObject("PillarsMovementEffect", _towers);
                    FindAddGameObject("PillarPair", _towers);
                    FindAddGameObject("SmallPillarPair", _towers);
                    for (int i = 1; i < 5; i++)
                    {
                        string strI = i.ToString(NumberFormatInfo.InvariantInfo);
                        FindAddGameObject($"PillarPair ({strI})", _towers);
                        FindAddGameObject($"SmallPillarPair ({strI})", _towers);
                    }

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
                    FindAddGameObject("IsActiveObjects/Construction/ConstructionL", _highway);
                    FindAddGameObject("IsActiveObjects/Construction/ConstructionR", _highway);
                    FindAddGameObject("IsActiveObjects/Lasers", _highway);

                    // Only hide the other player's construction when in duel layout
                    if (FindAddGameObject("IsActiveObjects/CenterRings", _highway))
                    {
                        FindAddGameObject("IsActiveObjects/PlatformEnd", _highway);
                    }
                    else
                    {
                        FindAddGameObject("Construction", _highway);
                        FindAddGameObject("Lasers", _highway);
                    }

                    break;
                case "GlassDesertEnvironment":
                    FindAddGameObject("Cube", _highway);
                    FindAddGameObject("Floor", _highway);
                    break;
                case "TutorialEnvironment":
                    FindAddGameObject("Floor", _highway);
                    break;
                case "DefaultEnvironment":
                    FindAddGameObject("TrackMirror", _highway);
                    FindAddGameObject("TrackConstruction", _highway);
                    break;
                case "OriginsEnvironment":
                    FindAddGameObject("Construction", _highway);
                    FindAddGameObject("TrackMirror", _highway);
                    FindAddGameObject("CombinedMesh", _highway);
                    break;
                case "TriangleEnvironment":
                    FindAddGameObject("FloorConstruction", _highway);
                    FindAddGameObject("TrackMirror", _highway);
                    break;
                case "NiceEnvironment":
                    FindAddGameObject("Floor", _highway);
                    break;
                case "BigMirrorEnvironment":
                    FindAddGameObject("Floor", _highway);
                    FindAddGameObject("Construction", _highway);
                    break;
                case "DragonsEnvironment":
                    FindAddGameObject("TrackMirror", _highway);
                    FindAddGameObject("TrackConstruction", _highway);
                    FindAddGameObject("TopConstruction", _highway);
                    for (int i = 1; i < 4; i++)
                        FindAddGameObject($"TopConstruction ({i.ToString(NumberFormatInfo.InvariantInfo)})", _highway);
                    FindAddGameObject("FloorGround (4)", _highway);
                    FindAddGameObject("FloorGround (5)", _highway);
                    FindAddGameObject("Underground", _highway);
                    break;
                case "KDAEnvironment":
                    FindAddGameObject("Construction", _highway);
                    FindAddGameObject("FloorMirror", _highway);
                    break;
                case "MonstercatEnvironment":
                    FindAddGameObject("TrackMirror", _highway);
                    FindAddGameObject("VConstruction", _highway);
                    FindAddGameObject("MonstercatLogoL", _highway);
                    FindAddGameObject("MonstercatLogoR", _highway);
                    FindAddGameObject("Construction", _highway);
                    break;
                case "CrabRaveEnvironment":
                    FindAddGameObject("TrackMirror", _highway);
                    FindAddGameObject("VConstruction", _highway);
                    FindAddGameObject("Construction", _highway);
                    break;
                case "PanicEnvironment":
                    FindAddGameObject("TrackMirror", _highway);
                    FindAddGameObject("TrackConstruction", _highway);
                    break;
                case "RocketEnvironment":
                    FindAddGameObject("Mirror", _highway);
                    FindAddGameObject("Construction", _highway);
                    break;
                case "GreenDayGrenadeEnvironment":
                    FindAddGameObject("TrackMirror", _highway);
                    FindAddGameObject("TrackConstruction", _highway);
                    FindAddGameObject("Cube", _highway);
                    FindAddGameObject("Cube (1)", _highway);
                    break;
                case "GreenDayEnvironment":
                    FindAddGameObject("TrackMirror", _highway);
                    FindAddGameObject("TrackConstruction", _highway);
                    FindAddGameObject("Cube", _highway);
                    FindAddGameObject("Cube (1)", _highway);
                    break;
                case "TimbalandEnvironment":
                    FindAddGameObject("TrackMirror", _highway);
                    FindAddGameObject("TrackConstruction", _highway);
                    break;
                case "FitBeatEnvironment":
                    FindAddGameObject("TrackMirror", _highway);
                    FindAddGameObject("TrackConstruction", _highway);
                    break;
                case "LinkinParkEnvironment":
                    FindAddGameObject("TrackMirror", _highway);
                    FindAddGameObject("TrackConstruction", _highway);
                    FindAddGameObject("TrackShadow", _highway);
                    FindAddGameObject("Tunnel", _highway);
                    FindAddGameObject("TunnelRings", _highway);
                    FindAddGameObject("LinkinParkSoldier", _highway);
                    FindAddGameObject("LinkinParkTextLogoL", _highway);
                    FindAddGameObject("LinkinParkTextLogoR", _highway);
                    FindAddGameObject("FloorLightShadowL", _highway);
                    FindAddGameObject("FloorLightShadowR", _highway);
                    break;
                case "BTSEnvironment":
                    FindAddGameObject("TrackMirror", _highway);
                    FindAddGameObject("Construction", _highway);
                    FindAddGameObject("Clouds", _highway);
                    FindAddGameObject("StarHemisphere", _highway);
                    FindAddGameObject("StarEmitterPS", _highway);
                    FindAddGameObject("BTSStarTextEffectEvent", _highway);
                    FindAddGameObject("GradientBackground", _highway);
                    break;
                case "KaleidoscopeEnvironment":
                    FindAddGameObject("TrackMirror", _highway);
                    FindAddGameObject("Construction", _highway);
                    FindAddGameObject("GradientBackground", _highway);
                    break;
            }
        }

        private void FindBackColumns()
        {
            switch (_sceneName)
            {
                case "GlassDesertEnvironment":
                    FindAddGameObject("SeparatorWall", _backColumns);
                    for (int i = 1; i < 16; i++)
                        FindAddGameObject($"SeparatorWall ({i.ToString(NumberFormatInfo.InvariantInfo)})", _backColumns);
                    break;
                case "OriginsEnvironment":
                    FindAddGameObject("SpectrogramEnd", _backColumns, true);
                    FindAddGameObject("SpectrogramEnd", _backColumns);
                    break;
                case "DragonsEnvironment":
                    FindAddGameObject("Underground (18)", _backColumns);
                    FindAddGameObject("Underground (19)", _backColumns);
                    break;
                case "MonstercatEnvironment":
                    FindAddGameObject("SpectrogramEnd", _backColumns);
                    break;
                default:
                    FindAddGameObject("BackColumns", _backColumns);
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
                        FindAddGameObject($"LightPillar ({i.ToString(NumberFormatInfo.InvariantInfo)})", _rotatingLasers);
                    for (int i = 19; i < 26; i++)
                        FindAddGameObject($"LightPillar ({i.ToString(NumberFormatInfo.InvariantInfo)})", _rotatingLasers);
                    break;
                case "DefaultEnvironment":
                    FindAddGameObject("RotatingLasersPair", _rotatingLasers);
                    for (int i = 1; i < 4; i++)
                        FindAddGameObject($"RotatingLasersPair ({i.ToString(NumberFormatInfo.InvariantInfo)})", _rotatingLasers);
                    break;
                case "OriginsEnvironment":
                    FindAddGameObject("RotatingLasersPair", _rotatingLasers);
                    for (int i = 1; i < 6; i++)
                        FindAddGameObject($"RotatingLasersPair ({i.ToString(NumberFormatInfo.InvariantInfo)})", _rotatingLasers);
                    break;
                case "TriangleEnvironment":
                    FindAddGameObject("RotatingLasersPair", _rotatingLasers);
                    for (int i = 1; i < 7; i++)
                        FindAddGameObject($"RotatingLasersPair ({i.ToString(NumberFormatInfo.InvariantInfo)})", _rotatingLasers);
                    break;
                case "NiceEnvironment":
                    FindAddGameObject("RotatingLaserLeft", _rotatingLasers);
                    FindAddGameObject("RotatingLaserRight", _rotatingLasers);
                    for (int i = 0; i < 4; i++)
                    {
                        string strI = i.ToString(NumberFormatInfo.InvariantInfo);
                        FindAddGameObject($"RotatingLaserLeft ({strI})", _rotatingLasers);
                        FindAddGameObject($"RotatingLaserRight ({strI})", _rotatingLasers);
                    }

                    break;
                case "BigMirrorEnvironment":
                    FindAddGameObject("RotatingLasersPair", _rotatingLasers);
                    for (int i = 1; i < 4; i++)
                        FindAddGameObject($"RotatingLasersPair ({i.ToString(NumberFormatInfo.InvariantInfo)})", _rotatingLasers);
                    break;
                case "DragonsEnvironment":
                    FindAddGameObject("RotatingLasersPair", _rotatingLasers);
                    for (int i = 1; i < 5; i++)
                        FindAddGameObject($"RotatingLasersPair ({i.ToString(NumberFormatInfo.InvariantInfo)})", _rotatingLasers);
                    break;
                case "KDAEnvironment":
                    FindAddGameObject("RotatingLasersPair", _rotatingLasers);
                    for (int i = 1; i < 7; i++)
                        FindAddGameObject($"RotatingLasersPair ({i.ToString(NumberFormatInfo.InvariantInfo)})", _rotatingLasers);
                    break;
                case "MonstercatEnvironment":
                    FindAddGameObject("RotatingLasersPair", _rotatingLasers);
                    for (int i = 1; i < 5; i++)
                        FindAddGameObject($"RotatingLasersPair ({i.ToString(NumberFormatInfo.InvariantInfo)})", _rotatingLasers);
                    break;
                case "CrabRaveEnvironment":
                    FindAddGameObject("RotatingLasersPair", _rotatingLasers);
                    for (int i = 1; i < 5; i++)
                        FindAddGameObject($"RotatingLasersPair ({i.ToString(NumberFormatInfo.InvariantInfo)})", _rotatingLasers);
                    break;
                case "PanicEnvironment":
                    FindAddGameObject("RotatingLasersPair", _rotatingLasers);
                    for (int i = 1; i < 7; i++)
                        FindAddGameObject($"RotatingLasersPair ({i.ToString(NumberFormatInfo.InvariantInfo)})", _rotatingLasers);
                    break;
                case "RocketEnvironment":
                    for (int i = 7; i < 14; i++)
                        FindAddGameObject($"RotatingLasersPair ({i.ToString(NumberFormatInfo.InvariantInfo)})", _rotatingLasers);
                    break;
                case "GreenDayGrenadeEnvironment":
                    FindAddGameObject("RotatingLasersPair", _rotatingLasers);
                    for (int i = 1; i < 6; i++)
                        FindAddGameObject($"RotatingLasersPair ({i.ToString(NumberFormatInfo.InvariantInfo)})", _rotatingLasers);
                    break;
                case "GreenDayEnvironment":
                    FindAddGameObject("RotatingLasersPair", _rotatingLasers);
                    for (int i = 1; i < 6; i++)
                        FindAddGameObject($"RotatingLasersPair ({i.ToString(NumberFormatInfo.InvariantInfo)})", _rotatingLasers);
                    break;
                case "FitBeatEnvironment":
                    FindAddGameObject("RotatingLasersPair", _rotatingLasers);
                    for (int i = 1; i < 8; i++)
                        FindAddGameObject($"RotatingLasersPair ({i.ToString(NumberFormatInfo.InvariantInfo)})", _rotatingLasers);
                    break;
                case "LinkinParkEnvironment":
                    FindAddGameObject("TunnelRotatingLasersPair", _rotatingLasers);
                    for (int i = 1; i < 18; i++)
                        FindAddGameObject($"TunnelRotatingLasersPair ({i.ToString(NumberFormatInfo.InvariantInfo)})", _rotatingLasers);
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
                        FindAddGameObject($"DoubleColorLaser ({i.ToString(NumberFormatInfo.InvariantInfo)})", _doubleColorLasers);
                    break;
                case "DefaultEnvironment":
                    FindAddGameObject("DoubleColorLaserL", _doubleColorLasers);
                    FindAddGameObject("DoubleColorLaserR", _doubleColorLasers);
                    for (int i = 1; i < 5; i++)
                    {
                        FindAddGameObject($"DoubleColorLaserL ({i.ToString(NumberFormatInfo.InvariantInfo)})", _doubleColorLasers);
                        FindAddGameObject($"DoubleColorLaserR ({i.ToString(NumberFormatInfo.InvariantInfo)})", _doubleColorLasers);
                    }

                    break;
                case "OriginsEnvironment":
                    FindAddGameObject("Laser", _doubleColorLasers);
                    for (int i = 1; i < 4; i++)
                        FindAddGameObject($"Laser ({i.ToString(NumberFormatInfo.InvariantInfo)})", _doubleColorLasers);
                    break;
                case "TriangleEnvironment":
                    FindAddGameObject("DoubleColorLaser", _doubleColorLasers);
                    for (int i = 1; i < 10; i++)
                        FindAddGameObject($"DoubleColorLaser ({i.ToString(NumberFormatInfo.InvariantInfo)})", _doubleColorLasers);
                    break;
                case "NiceEnvironment":
                    FindAddGameObject("DoubleColorLaser", _doubleColorLasers);
                    for (int i = 1; i < 8; i++)
                        FindAddGameObject($"DoubleColorLaser ({i.ToString(NumberFormatInfo.InvariantInfo)})", _doubleColorLasers);
                    break;
                case "BigMirrorEnvironment":
                    FindAddGameObject("DoubleColorLaser", _doubleColorLasers);
                    for (int i = 1; i < 10; i++)
                        FindAddGameObject($"DoubleColorLaser ({i.ToString(NumberFormatInfo.InvariantInfo)})", _doubleColorLasers);
                    break;
                case "KDAEnvironment":
                    for (int i = 2; i < 14; i++)
                        FindAddGameObject($"Laser ({i.ToString(NumberFormatInfo.InvariantInfo)})", _doubleColorLasers);
                    break;
                case "MonstercatEnvironment":
                    for (int i = 4; i < 13; i++)
                        FindAddGameObject($"Laser ({i.ToString(NumberFormatInfo.InvariantInfo)})", _doubleColorLasers);
                    break;
                case "CrabRaveEnvironment":
                    for (int i = 4; i < 13; i++)
                        FindAddGameObject($"Laser ({i.ToString(NumberFormatInfo.InvariantInfo)})", _doubleColorLasers);
                    break;
                case "GreenDayGrenadeEnvironment":
                    FindAddGameObject("Laser", _doubleColorLasers);
                    for (int i = 1; i < 20; i++)
                        FindAddGameObject($"Laser ({i.ToString(NumberFormatInfo.InvariantInfo)})", _doubleColorLasers);
                    break;
                case "GreenDayEnvironment":
                    FindAddGameObject("Laser", _doubleColorLasers);
                    for (int i = 1; i < 20; i++)
                        FindAddGameObject($"Laser ({i.ToString(NumberFormatInfo.InvariantInfo)})", _doubleColorLasers);
                    break;
            }
        }

        private void FindBackLasers()
        {
            switch (_sceneName)
            {
                case "PanicEnvironment":
                    FindAddGameObject("FrontLights", _backLasers);
                    FindAddGameObject("Window", _backLasers, true);
                    FindAddGameObject("Window", _backLasers);
                    break;
                case "RocketEnvironment":
                    FindAddGameObject("FrontLights", _backLasers);
                    FindAddGameObject("RocketGateLight", _backLasers);
                    FindAddGameObject("GateLight0", _backLasers);
                    FindAddGameObject("GateLight1", _backLasers);
                    FindAddGameObject("GateLight1 (4)", _backLasers);
                    break;
                case "TimbalandEnvironment":
                    FindAddGameObject("FrontLights", _backLasers);
                    for (int i = 4; i < 8; i++)
                        FindAddGameObject($"Light ({i.ToString(NumberFormatInfo.InvariantInfo)})", _backLasers);
                    break;
                case "LinkinParkEnvironment":
                    FindAddGameObject("Logo", _backLasers);
                    FindAddGameObject("LogoLight", _backLasers);
                    break;
                case "BTSEnvironment":
                    FindAddGameObject("MagicDoorSprite", _backLasers);
                    break;
                default:
                    FindAddGameObject("FrontLights", _backLasers);
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
                    FindAddGameObject("TopLaser", _trackLights);
                    for (int i = 1; i < 6; i++)
                        FindAddGameObject($"TopLaser ({i.ToString(NumberFormatInfo.InvariantInfo)})", _trackLights);
                    for (int i = 4; i < 13; i++)
                        FindAddGameObject($"DownLaser ({i.ToString(NumberFormatInfo.InvariantInfo)})", _trackLights);
                    for (int i = 0; i < 7; i++)
                        FindAddGameObject("TopLightMesh", _trackLights, true);
                    break;
                case "TutorialEnvironment":
                    FindAddGameObject("GlowLines", _trackLights);
                    break;
                case "DefaultEnvironment":
                    FindAddGameObject("NeonTubeL", _trackLights);
                    FindAddGameObject("NeonTubeR", _trackLights);
                    break;
                case "OriginsEnvironment":
                    FindAddGameObject("NeonTube", _trackLights);
                    FindAddGameObject("NeonTube (1)", _trackLights);
                    FindAddGameObject("LightAreaL", _trackLights);
                    FindAddGameObject("LightAreaR", _trackLights);
                    FindAddGameObject("SidePSL", _trackLights);
                    FindAddGameObject("SidePSR", _trackLights);
                    break;
                case "TriangleEnvironment":
                    FindAddGameObject("NeonTubeDirectionalL", _trackLights);
                    FindAddGameObject("NeonTubeDirectionalR", _trackLights);
                    break;
                case "NiceEnvironment":
                    FindAddGameObject("GlowLineL", _trackLights);
                    FindAddGameObject("GlowLineR", _trackLights);
                    FindAddGameObject("GlowLineFarL", _trackLights);
                    FindAddGameObject("GlowLineFarR", _trackLights);
                    break;
                case "BigMirrorEnvironment":
                    FindAddGameObject("NeonTubeDirectionalL", _trackLights);
                    FindAddGameObject("NeonTubeDirectionalR", _trackLights);
                    FindAddGameObject("NeonTubeDirectionalFL", _trackLights);
                    FindAddGameObject("NeonTubeDirectionalFR", _trackLights);
                    break;
                case "DragonsEnvironment":
                    FindAddGameObject("GlowLineL", _trackLights);
                    FindAddGameObject("GlowLineR", _trackLights);
                    FindAddGameObject("ConstructionGlowLine (1)", _trackLights);
                    FindAddGameObject("ConstructionGlowLine (4)", _trackLights);
                    FindAddGameObject("ConstructionGlowLine (5)", _trackLights);
                    FindAddGameObject("ConstructionGlowLine (6)", _trackLights);
                    FindAddGameObject("DragonsSidePSL", _trackLights);
                    FindAddGameObject("DragonsSidePSR", _trackLights);
                    break;
                case "KDAEnvironment":
                    FindAddGameObject("GlowLineLVisible", _trackLights);
                    FindAddGameObject("GlowLineRVisible", _trackLights);
                    FindAddGameObject("GlowTopLine", _trackLights);
                    FindAddGameObject("GlowLineL", _trackLights);
                    FindAddGameObject("GlowLineR", _trackLights);
                    FindAddGameObject("GlowLineFarL", _trackLights);
                    FindAddGameObject("GlowLineFarR", _trackLights);
                    for (int i = 0; i < 5; i++)
                        FindAddGameObject($"GlowTopLine ({i.ToString(NumberFormatInfo.InvariantInfo)})", _trackLights);
                    FindAddGameObject("GlowLine", _trackLights);
                    for (int i = 0; i < 100; i++)
                        FindAddGameObject($"GlowLine ({i.ToString(NumberFormatInfo.InvariantInfo)})", _trackLights);
                    break;
                case "MonstercatEnvironment":
                    FindAddGameObject("GlowLineL", _trackLights);
                    FindAddGameObject("GlowLineL (1)", _trackLights);
                    FindAddGameObject("GlowLineR", _trackLights);
                    FindAddGameObject("GlowLineR (1)", _trackLights);
                    for (int i = 5; i < 12; i++)
                        FindAddGameObject($"GlowTopLine ({i.ToString(NumberFormatInfo.InvariantInfo)})", _trackLights);
                    break;
                case "CrabRaveEnvironment":
                    FindAddGameObject("GlowLineL", _trackLights);
                    FindAddGameObject("GlowLineL (1)", _trackLights);
                    FindAddGameObject("GlowLineR", _trackLights);
                    FindAddGameObject("GlowLineR (1)", _trackLights);
                    for (int i = 5; i < 12; i++)
                        FindAddGameObject($"GlowTopLine ({i.ToString(NumberFormatInfo.InvariantInfo)})", _trackLights);
                    break;
                case "PanicEnvironment":
                    FindAddGameObject("Light (5)", _trackLights);
                    FindAddGameObject("ConstructionGlowLine (15)", _trackLights);
                    for (int i = 4; i < 9; i++)
                        FindAddGameObject($"ConstructionGlowLine ({i.ToString(NumberFormatInfo.InvariantInfo)})", _trackLights);
                    break;
                case "RocketEnvironment":
                    FindAddGameObject("GlowLineR (1)", _trackLights);
                    for (int i = 1; i < 10; i++)
                        FindAddGameObject($"GlowLineL ({i})", _trackLights);
                    break;
                case "GreenDayGrenadeEnvironment":
                    FindAddGameObject("GlowLineL", _trackLights);
                    FindAddGameObject("GlowLineR", _trackLights);
                    FindAddGameObject("GlowLineL (1)", _trackLights);
                    FindAddGameObject("GlowLineR (1)", _trackLights);
                    FindAddGameObject("GlowLineL (2)", _trackLights);
                    FindAddGameObject("GlowLineL (4)", _trackLights);
                    for (int i = 7; i < 25; i++)
                        FindAddGameObject($"GlowLineL ({i.ToString(NumberFormatInfo.InvariantInfo)})", _trackLights);
                    break;
                case "GreenDayEnvironment":
                    FindAddGameObject("GlowLineL", _trackLights);
                    FindAddGameObject("GlowLineR", _trackLights);
                    FindAddGameObject("GlowLineL (1)", _trackLights);
                    FindAddGameObject("GlowLineR (1)", _trackLights);
                    FindAddGameObject("GlowLineL (2)", _trackLights);
                    FindAddGameObject("GlowLineL (4)", _trackLights);
                    for (int i = 7; i < 25; i++)
                        FindAddGameObject($"GlowLineL ({i.ToString(NumberFormatInfo.InvariantInfo)})", _trackLights);
                    break;
                case "TimbalandEnvironment":
                    FindAddGameObject("GlowLineL", _trackLights);
                    FindAddGameObject("GlowLineR", _trackLights);
                    break;
                case "LinkinParkEnvironment":
                    FindAddGameObject("LaserFloor", _trackLights);
                    for (int i = 1; i < 4; i++)
                        FindAddGameObject($"LaserFloor ({i.ToString(NumberFormatInfo.InvariantInfo)})", _trackLights);
                    for (int i = 2; i < 22; i++)
                        FindAddGameObject($"LaserTop ({i.ToString(NumberFormatInfo.InvariantInfo)})", _trackLights);
                    FindAddGameObject("LaserL", _trackLights);
                    // ReSharper disable once StringLiteralTypo
                    FindAddGameObject("LarerR", _trackLights);
                    FindAddGameObject("LaserL (2)", _trackLights);
                    // ReSharper disable once StringLiteralTypo
                    FindAddGameObject("LarerR (2)", _trackLights);
                    break;
                case "BTSEnvironment":
                    FindAddGameObject("GlowLineL", _trackLights);
                    FindAddGameObject("GlowLineR", _trackLights);
                    FindAddGameObject("GlowLineH", _trackLights);
                    FindAddGameObject("GlowLineH (2)", _trackLights);
                    FindAddGameObject("LaserL", _trackLights);
                    FindAddGameObject("LaserR", _trackLights);
                    FindAddGameObject("GlowLineC", _trackLights);
                    FindAddGameObject("BottomGlow", _trackLights);
                    for (int i = 0; i < 4; i++)
                        FindAddGameObject("SideLaser", _trackLights, true);
                    break;
            }
        }
    }
}