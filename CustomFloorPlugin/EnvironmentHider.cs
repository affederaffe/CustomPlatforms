using System.Collections.Generic;
using System.Linq;

using CustomFloorPlugin.Configuration;

using UnityEngine;
using UnityEngine.SceneManagement;


namespace CustomFloorPlugin
{
    /// <summary> 
    /// Activates and deactivates world geometry in the active scene as required by the chosen custom platform<br/>
    /// Most documentation on this file is omited because it is a giant clusterfuck and I hate it... with a passion.
    /// </summary>
    internal class EnvironmentHider
    {
        private readonly PluginConfig _config;
        private readonly PlatformManager _platformManager;
        private readonly GameScenesManager _gameScenesManager;

        private string sceneName;
        private GameObject[] roots;

        private List<GameObject> menuEnvironment;
        private List<GameObject> multiplayerEnvironment;
        private List<GameObject> playersPlace;
        private List<GameObject> feet;
        private List<GameObject> smallRings;
        private List<GameObject> bigRings;
        private List<GameObject> visualizer;
        private List<GameObject> towers;
        private List<GameObject> highway;
        private List<GameObject> backColumns;
        private List<GameObject> doubleColorLasers;
        private List<GameObject> backLasers;
        private List<GameObject> rotatingLasers;
        private List<GameObject> trackLights;

        private const string renamedObjectSuffix = "renamed";
        private List<GameObject> renamedObjects;

        private TrackLaneRing[] TrackLaneRings
        {
            get
            {
                if (_TrackLaneRings == null)
                {
                    _TrackLaneRings = Resources.FindObjectsOfTypeAll<TrackLaneRing>();
                }
                return _TrackLaneRings;
            }
        }
        private TrackLaneRing[] _TrackLaneRings;

        public EnvironmentHider(PluginConfig config, PlatformManager platformManager, GameScenesManager gameScenesManager)
        {
            _config = config;
            _platformManager = platformManager;
            _gameScenesManager = gameScenesManager;
        }

        /// <summary>
        /// Hide and unhide world objects as required by a platform<br/>
        /// </summary>
        /// <param name="platform">A platform that defines which objects are to be hidden</param>
        internal void HideObjectsForPlatform(CustomPlatform platform)
        {
            FindEnvironment();
            if (menuEnvironment != null) SetCollectionHidden(menuEnvironment, _platformManager.activePlatform != null);
            if (multiplayerEnvironment != null) SetCollectionHidden(multiplayerEnvironment, true);
            if (feet != null) SetCollectionHidden(feet, platform.hideDefaultPlatform && !_config.AlwaysShowFeet);
            if (playersPlace != null) SetCollectionHidden(playersPlace, platform.hideDefaultPlatform);
            if (smallRings != null) SetCollectionHidden(smallRings, platform.hideSmallRings);
            if (bigRings != null) SetCollectionHidden(bigRings, platform.hideBigRings);
            if (visualizer != null) SetCollectionHidden(visualizer, platform.hideEQVisualizer);
            if (towers != null) SetCollectionHidden(towers, platform.hideTowers);
            if (highway != null) SetCollectionHidden(highway, platform.hideHighway);
            if (backColumns != null) SetCollectionHidden(backColumns, platform.hideBackColumns);
            if (backLasers != null) SetCollectionHidden(backLasers, platform.hideBackLasers);
            if (doubleColorLasers != null) SetCollectionHidden(doubleColorLasers, platform.hideDoubleColorLasers);
            if (rotatingLasers != null) SetCollectionHidden(rotatingLasers, platform.hideRotatingLasers);
            if (trackLights != null) SetCollectionHidden(trackLights, platform.hideTrackLights);
            AssetLoader.instance.playersPlace?.SetActive(_platformManager.activePlatform != null && !platform.hideDefaultPlatform && (sceneName == "MenuEnvironment" || sceneName == "Credits" || sceneName == "HealthWarning"));
            _TrackLaneRings = null;
        }

        /// <summary>
        /// Finds all GameObjects that make up the default environment and groups them into lists
        /// </summary>
        private void FindEnvironment()
        {
            sceneName = _gameScenesManager.GetCurrentlyLoadedSceneNames().LastOrDefault(x => x.EndsWith("Environment") || x == "Credits" || x == "HealthWarning");
            sceneName = sceneName == "MultiplayerEnvironment" ? sceneName = "GameCore" : sceneName;
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid())
                return;
            roots = scene.GetRootGameObjects();
            renamedObjects = new List<GameObject>();

            FindMenuEnvironmnet();

            // If no platform is selected, don't hide anything other than the menu environent to prevent showing objects disabled by chroma
            if (_platformManager.allPlatforms.IndexOf(_platformManager.activePlatform) <= 0)
            {
                multiplayerEnvironment = null;
                feet = null;
                playersPlace = null;
                smallRings = null;
                bigRings = null;
                visualizer = null;
                towers = null;
                highway = null;
                backColumns = null;
                backLasers = null;
                rotatingLasers = null;
                doubleColorLasers = null;
                trackLights = null;
                return;
            }

            FindMultiplayerEnvironment();
            FindPlayersPlace();
            FindFeetIcon();
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

            foreach (GameObject go in renamedObjects)
                go.name = go.name.Remove(go.name.Length - renamedObjectSuffix.Length);
        }

        /// <summary>
        /// Set the active state of a Collection of GameObjects
        /// </summary>
        /// <param name="list">A <see cref="List{T}"/> of GameObjects</param>
        /// <param name="hidden">A boolean describing the desired hidden state</param>
        private void SetCollectionHidden(List<GameObject> list, bool hidden)
        {
            foreach (GameObject go in list)
                go.SetActive(!hidden);
        }

        /// <summary>
        /// Finds a GameObject by name and adds it to the provided list
        /// </summary>
        /// <param name="name">The name of the desired GameObject</param>
        /// <param name="list">The list to be added to</param>
        private bool FindAddGameObject(string name, List<GameObject> list, bool rename = false)
        {
            GameObject go;
            foreach (GameObject root in roots)
            {
                go = root.transform.Find(name)?.gameObject;
                if (go != null)
                {
                    list.Add(go);
                    if (rename)
                    {
                        go.name += renamedObjectSuffix;
                        renamedObjects.Add(go);
                    }
                    return true;
                }
                else if (root.name == name)
                {
                    list.Add(root);
                }
            }
            return false;
        }

        private void FindMenuEnvironmnet()
        {
            menuEnvironment = new List<GameObject>();
            switch (sceneName)
            {
                case "HealthWarning":
                case "MenuEnvironment":
                case "Credits":
                    FindAddGameObject("MenuFogRing", menuEnvironment);
                    FindAddGameObject("NearBuildingLeft", menuEnvironment);
                    FindAddGameObject("NearBuildingRight", menuEnvironment);
                    FindAddGameObject("NearBuildingLeft (1)", menuEnvironment);
                    FindAddGameObject("NearBuildingRight (1)", menuEnvironment);
                    FindAddGameObject("GroundCollider", menuEnvironment);
                    FindAddGameObject("DefaultEnvironment/Ground", menuEnvironment);
                    FindAddGameObject("DefaultEnvironment/playersPlace", menuEnvironment);
                    FindAddGameObject("DefaultEnvironment/PileOfNotes", menuEnvironment);
                    FindAddGameObject("DefaultEnvironment/NeonLights", menuEnvironment);
                    FindAddGameObject("DefaultEnvironment/Notes", menuEnvironment);
                    break;
            }
        }

        private void FindMultiplayerEnvironment()
        {
            multiplayerEnvironment = new List<GameObject>();
            switch (sceneName)
            {
                case "GameCore":
                    FindAddGameObject("IsActiveObjects/Construction/ConstructionL", multiplayerEnvironment);
                    FindAddGameObject("IsActiveObjects/Construction/ConstructionR", multiplayerEnvironment);
                    FindAddGameObject("IsActiveObjects/Lasers", multiplayerEnvironment);

                    // Only hide the other player's construction when in duel layout
                    if (FindAddGameObject("IsActiveObjects/CenterRings", multiplayerEnvironment))
                        FindAddGameObject("IsActiveObjects/PlatformEnd", multiplayerEnvironment);
                    else
                    {
                        FindAddGameObject("Construction", multiplayerEnvironment);
                        FindAddGameObject("Lasers", multiplayerEnvironment);
                    }
                    break;
            }
        }

        private void FindPlayersPlace()
        {
            playersPlace = new List<GameObject>();
            switch (sceneName)
            {
                case "GlassDesertEnvironment":
                    FindAddGameObject("PlayersPlace", playersPlace);
                    FindAddGameObject("Collider", playersPlace);
                    break;
                case "LinkinParkEnvironment":
                    FindAddGameObject("PlayersPlace", playersPlace);
                    FindAddGameObject("PlayersPlaceShadow", playersPlace);
                    break;
                case "GameCore":
                    FindAddGameObject("IsActiveObjects/Construction/playersPlace", playersPlace);
                    break;
                default:
                    FindAddGameObject("PlayersPlace", playersPlace);
                    break;
            }
        }

        private void FindFeetIcon()
        {
            feet = new List<GameObject>();
            switch (sceneName)
            {
                case "GameCore":
                    FindAddGameObject("IsActiveObjects/Construction/playersPlace/Feet", feet);
                    break;
                default:
                    FindAddGameObject("PlayersPlace/Feet", feet);
                    break;
            }

            foreach (GameObject feet in feet)
                feet.transform.SetParent(null); // remove from original platform 
        }

        private void FindSmallRings()
        {
            smallRings = new List<GameObject>();
            FindAddGameObject("SmallTrackLaneRings", smallRings);
            FindAddGameObject("TrackLaneRing", smallRings);
            FindAddGameObject("TriangleTrackLaneRings", smallRings);
            FindAddGameObject("PanelsTrackLaneRing", smallRings);
            FindAddGameObject("Panels4TrackLaneRing", smallRings);
            FindAddGameObject("PairLaserTrackLaneRing", smallRings);
            FindAddGameObject("PanelLightTrackLaneRing", smallRings);
            FindAddGameObject("LightLinesTrackLaneRing", smallRings);
            FindAddGameObject("DistantRings", smallRings);
            foreach (TrackLaneRing trackLaneRing in TrackLaneRings.Where(x =>
                x.name is "TrackLaneRing(Clone)" or
                "SmallTrackLaneRing(Clone)" or
                "TriangleTrackLaneRing(Clone)" or
                "PanelsTrackLaneRing(Clone)" or
                "Panels4TrackLaneRing(Clone)" or
                "PairLaserTrackLaneRing(Clone)" or
                "PanelLightTrackLaneRing(Clone)" or
                "LightLinesTrackLaneRing(Clone)" or
                "ConeRingBig(Clone)"
                ))
            {
                smallRings.Add(trackLaneRing.gameObject);
            }
        }

        private void FindBigRings()
        {
            bigRings = new List<GameObject>();
            FindAddGameObject("BigTrackLaneRings", bigRings);
            FindAddGameObject("BigLightsTrackLaneRings", bigRings);
            FindAddGameObject("BigCenterLightTrackLaneRing", bigRings);
            FindAddGameObject("LightsTrackLaneRing", bigRings);

            foreach (TrackLaneRing trackLaneRing in TrackLaneRings.Where(x =>
                x.name is "BigTrackLaneRing(Clone)" or
                "BigCenterLightTrackLaneRing(Clone)" or
                "LightsTrackLaneRing(Clone)"
                ))
            {
                bigRings.Add(trackLaneRing.gameObject);
            }
        }

        private void FindVisualizers()
        {
            visualizer = new List<GameObject>();
            switch (sceneName)
            {
                default:
                    FindAddGameObject("Spectrograms", visualizer);
                    break;
            }
        }

        private void FindTowers()
        {
            towers = new List<GameObject>();
            switch (sceneName)
            {
                case "GlassDesertEnvironment":
                    for (int i = 2; i < 25; i++)
                        FindAddGameObject($"GameObject ({i})", towers);
                    break;
                case "TutorialEnvironment":
                    FindAddGameObject("Buildings", towers);
                    break;
                case "DefaultEnvironment":
                    FindAddGameObject("NearBuildingLeft (1)", towers);
                    FindAddGameObject("NearBuildingRight (1)", towers);
                    FindAddGameObject("NearBuildingLeft (2)", towers);
                    FindAddGameObject("NearBuildingRight (2)", towers);
                    break;
                case "NiceEnvironment":
                    FindAddGameObject("NearBuildingLeft (1)", towers);
                    FindAddGameObject("NearBuildingRight (1)", towers);
                    FindAddGameObject("NearBuildingLeft (2)", towers);
                    FindAddGameObject("NearBuildingRight (2)", towers);
                    break;
                case "DragonsEnvironment":
                    FindAddGameObject("HallConstruction", towers);
                    break;
                case "KDAEnvironment":
                    FindAddGameObject("TentacleLeft", towers);
                    FindAddGameObject("TentacleRight", towers);
                    break;
                case "MonstercatEnvironment":
                    FindAddGameObject("NearBuildingLeft", towers);
                    FindAddGameObject("NearBuildingRight", towers);
                    FindAddGameObject("FarBuildings", towers);
                    break;
                case "CrabRaveEnvironment":
                    FindAddGameObject("NearBuildingLeft", towers);
                    FindAddGameObject("NearBuildingRight", towers);
                    FindAddGameObject("FarBuildings", towers);
                    break;
                case "PanicEnvironment":
                    FindAddGameObject("TopCones", towers);
                    FindAddGameObject("BottomCones", towers);
                    break;
                case "RocketEnvironment":
                    FindAddGameObject("RocketCar", towers);
                    FindAddGameObject("RocketCar (1)", towers);
                    FindAddGameObject("RocketArena", towers);
                    FindAddGameObject("RocketArenaLight", towers);
                    FindAddGameObject("EnvLight0", towers);
                    for (int i = 2; i < 10; i++)
                        FindAddGameObject($"EnvLight0 ({i})", towers);
                    break;
                case "GreenDayGrenadeEnvironment":
                    FindAddGameObject("GreenDayCity", towers);
                    break;
                case "GreenDayEnvironment":
                    FindAddGameObject("GreenDayCity", towers);
                    break;
                case "TimbalandEnvironment":
                    FindAddGameObject("Buildings", towers);
                    FindAddGameObject("MainStructure", towers);
                    FindAddGameObject("TopStructure", towers);
                    break;
                case "BTSEnvironment":
                    FindAddGameObject("PillarTrackLaneRingsR", towers);
                    FindAddGameObject("PillarTrackLaneRingsR (1)", towers);
                    FindAddGameObject("PillarsMovementEffect", towers);
                    FindAddGameObject("PillarPair", towers);
                    FindAddGameObject("SmallPillarPair", towers);
                    for (int i = 1; i < 5; i++)
                    {
                        FindAddGameObject($"PillarPair ({i})", towers);
                        FindAddGameObject($"SmallPillarPair ({i})", towers);
                    }
                    break;
            }
        }

        private void FindHighway()
        {
            highway = new List<GameObject>();
            switch (sceneName)
            {
                case "GlassDesertEnvironment":
                    FindAddGameObject("Cube", highway);
                    FindAddGameObject("Floor", highway);
                    break;
                case "TutorialEnvironment":
                    FindAddGameObject("Floor", highway);
                    break;
                case "DefaultEnvironment":
                    FindAddGameObject("TrackMirror", highway);
                    FindAddGameObject("TrackConstruction", highway);
                    break;
                case "OriginsEnvironment":
                    FindAddGameObject("Construction", highway);
                    FindAddGameObject("TrackMirror", highway);
                    FindAddGameObject("CombinedMesh", highway);
                    break;
                case "TriangleEnvironment":
                    FindAddGameObject("FloorConstruction", highway);
                    FindAddGameObject("TrackMirror", highway);
                    break;
                case "NiceEnvironment":
                    FindAddGameObject("Floor", highway);
                    break;
                case "BigMirrorEnvironment":
                    FindAddGameObject("Floor", highway);
                    FindAddGameObject("Construction", highway);
                    break;
                case "DragonsEnvironment":
                    FindAddGameObject("TrackMirror", highway);
                    FindAddGameObject("TrackConstruction", highway);
                    FindAddGameObject("TopConstruction", highway);
                    for (int i = 1; i < 4; i++)
                        FindAddGameObject($"TopConstruction ({i})", highway);
                    FindAddGameObject("FloorGround (4)", highway);
                    FindAddGameObject("FloorGround (5)", highway);
                    FindAddGameObject("Underground", highway);
                    break;
                case "KDAEnvironment":
                    FindAddGameObject("Construction", highway);
                    FindAddGameObject("FloorMirror", highway);
                    break;
                case "MonstercatEnvironment":
                    FindAddGameObject("TrackMirror", highway);
                    FindAddGameObject("VConstruction", highway);
                    FindAddGameObject("Construction", highway);
                    break;
                case "CrabRaveEnvironment":
                    FindAddGameObject("TrackMirror", highway);
                    FindAddGameObject("VConstruction", highway);
                    FindAddGameObject("Construction", highway);
                    break;
                case "RocketEnvironment":
                    FindAddGameObject("Mirror", highway);
                    FindAddGameObject("Construction", highway);
                    break;
                case "GreenDayGrenadeEnvironment":
                    FindAddGameObject("TrackMirror", highway);
                    FindAddGameObject("TrackConstruction", highway);
                    FindAddGameObject("Cube", highway);
                    FindAddGameObject("Cube (1)", highway);
                    break;
                case "GreenDayEnvironment":
                    FindAddGameObject("TrackMirror", highway);
                    FindAddGameObject("TrackConstruction", highway);
                    FindAddGameObject("Cube", highway);
                    FindAddGameObject("Cube (1)", highway);
                    break;
                case "TimbalandEnvironment":
                    FindAddGameObject("TrackMirror", highway);
                    FindAddGameObject("TrackConstruction", highway);
                    break;
                case "FitBeatEnvironment":
                    FindAddGameObject("TrackMirror", highway);
                    FindAddGameObject("TrackConstruction", highway);
                    break;
                case "LinkinParkEnvironment":
                    FindAddGameObject("TrackMirror", highway);
                    FindAddGameObject("TrackConstruction", highway);
                    FindAddGameObject("TrackShadow", highway);
                    FindAddGameObject("Tunnel", highway);
                    FindAddGameObject("TunnelRing", highway);
                    for (int i = 1; i < 7; i++)
                        FindAddGameObject($"TunnelRing ({i})", highway);
                    FindAddGameObject("TunnelRingShadow", highway);
                    FindAddGameObject("TunnelRingShadow (1)", highway);
                    for (int i = 1; i < 13; i++)
                        FindAddGameObject($"LampShadow ({i})", highway);
                    FindAddGameObject("ArchShadow", highway);
                    FindAddGameObject("ArchShadow (1)", highway);
                    FindAddGameObject("LinkinParkSoldier", highway);
                    FindAddGameObject("LinkinParkTextLogoL", highway);
                    FindAddGameObject("LinkinParkTextLogoR", highway);
                    FindAddGameObject("FloorLightShadowL", highway);
                    FindAddGameObject("FloorLightShadowR", highway);
                    break;
                case "BTSEnvironment":
                    FindAddGameObject("TrackMirror", highway);
                    FindAddGameObject("Construction", highway);
                    FindAddGameObject("Clouds", highway);
                    FindAddGameObject("StarHemisphere", highway);
                    FindAddGameObject("StarEmitterPS", highway);
                    FindAddGameObject("BTSStarTextEffectEvent", highway);
                    FindAddGameObject("GradientBackground", highway);
                    break;
                case "KaleidoscopeEnvironment":
                    FindAddGameObject("TrackMirror", highway);
                    FindAddGameObject("Construction", highway);
                    FindAddGameObject("GradientBackground", highway);
                    break;
            }
        }

        private void FindBackColumns()
        {
            backColumns = new List<GameObject>();
            switch (sceneName)
            {
                case "OriginsEnvironment":
                    FindAddGameObject("SpectrogramEnd", backColumns, true);
                    FindAddGameObject("SpectrogramEnd", backColumns);
                    break;
                case "DragonsEnvironment":
                    FindAddGameObject("Underground (18)", backColumns);
                    FindAddGameObject("Underground (19)", backColumns);
                    break;
                case "MonstercatEnvironment":
                    FindAddGameObject("SpectrogramEnd", backColumns);
                    break;
                default:
                    FindAddGameObject("BackColumns", backColumns);
                    break;
            }
        }

        private void FindRotatingLasers()
        {
            rotatingLasers = new List<GameObject>();
            switch (sceneName)
            {
                case "GlassDesertEnvironment":
                    for (int i = 9; i < 19; i++)
                        FindAddGameObject($"LightPillar ({i})", rotatingLasers);
                    break;
                case "DefaultEnvironment":
                    FindAddGameObject("RotatingLasersPair", rotatingLasers);
                    for (int i = 1; i < 4; i++)
                        FindAddGameObject($"RotatingLasersPair ({i})", rotatingLasers);
                    break;
                case "OriginsEnvironment":
                    FindAddGameObject("RotatingLasersPair", rotatingLasers);
                    for (int i = 1; i < 6; i++)
                        FindAddGameObject($"RotatingLasersPair ({i})", rotatingLasers);
                    break;
                case "TriangleEnvironment":
                    FindAddGameObject("RotatingLasersPair", rotatingLasers);
                    for (int i = 1; i < 7; i++)
                        FindAddGameObject($"RotatingLasersPair ({i})", rotatingLasers);
                    break;
                case "NiceEnvironment":
                    FindAddGameObject("RotatingLaserLeft", rotatingLasers);
                    FindAddGameObject("RotatingLaserRight", rotatingLasers);
                    for (int i = 0; i < 4; i++)
                    {
                        FindAddGameObject($"RotatingLaserLeft ({i})", rotatingLasers);
                        FindAddGameObject($"RotatingLaserRight ({i})", rotatingLasers);
                    }
                    break;
                case "BigMirrorEnvironment":
                    FindAddGameObject("RotatingLasersPair", rotatingLasers);
                    for (int i = 1; i < 4; i++)
                        FindAddGameObject($"RotatingLasersPair ({i})", rotatingLasers);
                    break;
                case "DragonsEnvironment":
                    FindAddGameObject("RotatingLasersPair", rotatingLasers);
                    for (int i = 1; i < 5; i++)
                        FindAddGameObject($"RotatingLasersPair ({i})", rotatingLasers);
                    break;
                case "KDAEnvironment":
                    FindAddGameObject("RotatingLasersPair", rotatingLasers);
                    for (int i = 1; i < 7; i++)
                        FindAddGameObject($"RotatingLasersPair ({i})", rotatingLasers);
                    break;
                case "MonstercatEnvironment":
                    FindAddGameObject("RotatingLasersPair", rotatingLasers);
                    for (int i = 1; i < 5; i++)
                        FindAddGameObject($"RotatingLasersPair ({i})", rotatingLasers);
                    break;
                case "CrabRaveEnvironment":
                    FindAddGameObject("RotatingLasersPair", rotatingLasers);
                    for (int i = 1; i < 5; i++)
                        FindAddGameObject($"RotatingLasersPair ({i})", rotatingLasers);
                    break;
                case "PanicEnvironment":
                    FindAddGameObject("RotatingLasersPair", rotatingLasers);
                    for (int i = 1; i < 7; i++)
                        FindAddGameObject($"RotatingLasersPair ({i})", rotatingLasers);
                    break;
                case "RocketEnvironment":
                    for (int i = 7; i < 14; i++)
                        FindAddGameObject($"RotatingLasersPair ({i})", rotatingLasers);
                    break;
                case "GreenDayGrenadeEnvironment":
                    FindAddGameObject("RotatingLasersPair", rotatingLasers);
                    for (int i = 1; i < 6; i++)
                        FindAddGameObject($"RotatingLasersPair ({i})", rotatingLasers);
                    break;
                case "GreenDayEnvironment":
                    FindAddGameObject("RotatingLasersPair", rotatingLasers);
                    for (int i = 1; i < 6; i++)
                        FindAddGameObject($"RotatingLasersPair ({i})", rotatingLasers);
                    break;
                case "FitBeatEnvironment":
                    FindAddGameObject("RotatingLasersPair", rotatingLasers);
                    for (int i = 1; i < 8; i++)
                        FindAddGameObject($"RotatingLasersPair ({i})", rotatingLasers);
                    break;
                case "LinkinParkEnvironment":
                    FindAddGameObject("TunnelRotatingLasersPair", rotatingLasers);
                    for (int i = 1; i < 18; i++)
                        FindAddGameObject($"TunnelRotatingLasersPair ({i})", rotatingLasers);
                    break;
            }
        }

        private void FindDoubleColorLasers()
        {
            doubleColorLasers = new List<GameObject>();
            switch (sceneName)
            {
                case "TutorialEnvironment":
                    for (int i = 10; i < 20; i++)
                        FindAddGameObject($"DoubleColorLaser ({i})", doubleColorLasers);
                    break;
                case "DefaultEnvironment":
                    FindAddGameObject("DoubleColorLaserL", doubleColorLasers);
                    FindAddGameObject("DoubleColorLaserR", doubleColorLasers);
                    for (int i = 1; i < 5; i++)
                    {
                        FindAddGameObject($"DoubleColorLaserL ({i})", doubleColorLasers);
                        FindAddGameObject($"DoubleColorLaserR ({i})", doubleColorLasers);
                    }
                    break;
                case "OriginsEnvironment":
                    FindAddGameObject("Laser", doubleColorLasers);
                    for (int i = 1; i < 4; i++)
                        FindAddGameObject($"Laser ({i})", doubleColorLasers);
                    break;
                case "TriangleEnvironment":
                    FindAddGameObject("DoubleColorLaser", doubleColorLasers);
                    for (int i = 1; i < 10; i++)
                        FindAddGameObject($"DoubleColorLaser ({i})", doubleColorLasers);
                    break;
                case "NiceEnvironment":
                    FindAddGameObject("DoubleColorLaser", doubleColorLasers);
                    for (int i = 1; i < 8; i++)
                        FindAddGameObject($"DoubleColorLaser ({i})", doubleColorLasers);
                    break;
                case "BigMirrorEnvironment":
                    FindAddGameObject("DoubleColorLaser", doubleColorLasers);
                    for (int i = 1; i < 10; i++)
                        FindAddGameObject($"DoubleColorLaser ({i})", doubleColorLasers);
                    break;
                case "KDAEnvironment":
                    for (int i = 2; i < 14; i++)
                        FindAddGameObject($"Laser ({i})", doubleColorLasers);
                    break;
                case "MonstercatEnvironment":
                    for (int i = 4; i < 13; i++)
                        FindAddGameObject($"Laser ({i})", doubleColorLasers);
                    break;
                case "CrabRaveEnvironment":
                    for (int i = 4; i < 13; i++)
                        FindAddGameObject($"Laser ({i})", doubleColorLasers);
                    break;
                case "GreenDayGrenadeEnvironment":
                    FindAddGameObject("Laser", doubleColorLasers);
                    for (int i = 1; i < 20; i++)
                        FindAddGameObject($"Laser ({i})", doubleColorLasers);
                    break;
                case "GreenDayEnvironment":
                    FindAddGameObject("Laser", doubleColorLasers);
                    for (int i = 1; i < 20; i++)
                        FindAddGameObject($"Laser ({i})", doubleColorLasers);
                    break;
            }
        }

        private void FindBackLasers()
        {
            backLasers = new List<GameObject>();
            switch (sceneName)
            {
                case "PanicEnvironment":
                    FindAddGameObject("FrontLights", backLasers);
                    FindAddGameObject("Window", backLasers, true);
                    FindAddGameObject("Window", backLasers);
                    break;
                case "RocketEnvironment":
                    FindAddGameObject("RocketGateLight", backLasers);
                    FindAddGameObject("GateLight0", backLasers);
                    FindAddGameObject("GateLight1", backLasers);
                    FindAddGameObject("GateLight1 (4)", backLasers);
                    break;
                case "TimbalandEnvironment":
                    FindAddGameObject("FrontLights", backLasers);
                    for (int i = 4; i < 8; i++)
                        FindAddGameObject($"Light ({i})", backLasers);
                    break;
                case "LinkinParkEnvironment":
                    FindAddGameObject("FrontLights", backLasers);
                    FindAddGameObject("Logo", backLasers);
                    break;
                case "BTSEnvironment":
                    FindAddGameObject("MagicDoorSprite", backLasers);
                    break;
                default:
                    FindAddGameObject("FrontLights", backLasers);
                    break;
            }
        }

        private void FindTrackLights()
        {
            trackLights = new List<GameObject>();
            switch (sceneName)
            {
                case "GlassDesertEnvironment":
                    FindAddGameObject("Cube (82)", trackLights);
                    for (int i = 85; i < 90; i++)
                        FindAddGameObject($"Cube ({i})", trackLights);
                    FindAddGameObject("TopLaser", trackLights);
                    for (int i = 1; i < 6; i++)
                        FindAddGameObject($"TopLaser ({i})", trackLights);
                    for (int i = 4; i < 13; i++)
                        FindAddGameObject($"DownLaser ({i})", trackLights);
                    break;
                case "TutorialEnvironment":
                    FindAddGameObject("GlowLines", trackLights);
                    break;
                case "DefaultEnvironment":
                    FindAddGameObject("NeonTubeL", trackLights);
                    FindAddGameObject("NeonTubeR", trackLights);
                    break;
                case "OriginsEnvironment":
                    FindAddGameObject("NeonTube", trackLights);
                    FindAddGameObject("NeonTube (1)", trackLights);
                    FindAddGameObject("LightAreaL", trackLights);
                    FindAddGameObject("LightAreaR", trackLights);
                    FindAddGameObject("SidePSL", trackLights);
                    FindAddGameObject("SidePSR", trackLights);
                    break;
                case "TriangleEnvironment":
                    FindAddGameObject("NeonTubeDirectionalL", trackLights);
                    FindAddGameObject("NeonTubeDirectionalR", trackLights);
                    break;
                case "NiceEnvironment":
                    FindAddGameObject("GlowLineL", trackLights);
                    FindAddGameObject("GlowLineR", trackLights);
                    FindAddGameObject("GlowLineFarL", trackLights);
                    FindAddGameObject("GlowLineFarR", trackLights);
                    break;
                case "BigMirrorEnvironment":
                    FindAddGameObject("NeonTubeDirectionalL", trackLights);
                    FindAddGameObject("NeonTubeDirectionalR", trackLights);
                    FindAddGameObject("NeonTubeDirectionalFL", trackLights);
                    FindAddGameObject("NeonTubeDirectionalFR", trackLights);
                    break;
                case "DragonsEnvironment":
                    FindAddGameObject("GlowLineL", trackLights);
                    FindAddGameObject("GlowLineR", trackLights);
                    FindAddGameObject("ConstructionGlowLine (1)", trackLights);
                    FindAddGameObject("ConstructionGlowLine (4)", trackLights);
                    FindAddGameObject("ConstructionGlowLine (5)", trackLights);
                    FindAddGameObject("ConstructionGlowLine (6)", trackLights);
                    FindAddGameObject("DragonsSidePSL", trackLights);
                    FindAddGameObject("DragonsSidePSR", trackLights);
                    break;
                case "KDAEnvironment":
                    FindAddGameObject("GlowLineLVisible", trackLights);
                    FindAddGameObject("GlowLineRVisible", trackLights);
                    FindAddGameObject("GlowTopLine", trackLights);
                    FindAddGameObject("GlowLineL", trackLights);
                    FindAddGameObject("GlowLineR", trackLights);
                    FindAddGameObject("GlowLineFarL", trackLights);
                    FindAddGameObject("GlowLineFarR", trackLights);
                    for (int i = 0; i < 5; i++)
                        FindAddGameObject($"GlowTopLine ({i})", trackLights);
                    FindAddGameObject("GlowLine", trackLights);
                    for (int i = 0; i < 100; i++)
                        FindAddGameObject($"GlowLine ({i})", trackLights);
                    break;
                case "MonstercatEnvironment":
                    FindAddGameObject("GlowLineL", trackLights);
                    FindAddGameObject("GlowLineL (1)", trackLights);
                    FindAddGameObject("GlowLineR", trackLights);
                    FindAddGameObject("GlowLineR (1)", trackLights);
                    for (int i = 5; i < 12; i++)
                        FindAddGameObject($"GlowTopLine ({i})", trackLights);
                    break;
                case "CrabRaveEnvironment":
                    FindAddGameObject("GlowLineL", trackLights);
                    FindAddGameObject("GlowLineL (1)", trackLights);
                    FindAddGameObject("GlowLineR", trackLights);
                    FindAddGameObject("GlowLineR (1)", trackLights);
                    for (int i = 5; i < 12; i++)
                        FindAddGameObject($"GlowTopLine ({i})", trackLights);
                    break;
                case "PanicEnvironment":
                    FindAddGameObject("Light (5)", trackLights);
                    FindAddGameObject("ConstructionGlowLine (15)", trackLights);
                    for (int i = 4; i < 9; i++)
                        FindAddGameObject($"ConstructionGlowLine ({i})", trackLights);
                    break;
                case "RocketEnvironment":
                    FindAddGameObject("GlowLineR (1)", trackLights);
                    for (int i = 1; i < 10; i++)
                        FindAddGameObject($"GlowLineL ({i})", trackLights);
                    break;
                case "GreenDayGrenadeEnvironment":
                    FindAddGameObject("GlowLineL", trackLights);
                    FindAddGameObject("GlowLineR", trackLights);
                    FindAddGameObject("GlowLineL (1)", trackLights);
                    FindAddGameObject("GlowLineR (1)", trackLights);
                    FindAddGameObject("GlowLineL (2)", trackLights);
                    FindAddGameObject("GlowLineL (4)", trackLights);
                    for (int i = 7; i < 25; i++)
                        FindAddGameObject($"GlowLineL ({i})", trackLights);
                    break;
                case "GreenDayEnvironment":
                    FindAddGameObject("GlowLineL", trackLights);
                    FindAddGameObject("GlowLineR", trackLights);
                    FindAddGameObject("GlowLineL (1)", trackLights);
                    FindAddGameObject("GlowLineR (1)", trackLights);
                    FindAddGameObject("GlowLineL (2)", trackLights);
                    FindAddGameObject("GlowLineL (4)", trackLights);
                    for (int i = 7; i < 25; i++)
                        FindAddGameObject($"GlowLineL ({i})", trackLights);
                    break;
                case "TimbalandEnvironment":
                    FindAddGameObject("GlowLineL", trackLights);
                    FindAddGameObject("GlowLineR", trackLights);
                    break;
                case "LinkinParkEnvironment":
                    FindAddGameObject("LaserFloor", trackLights);
                    for (int i = 1; i < 4; i++)
                        FindAddGameObject($"LaserFloor ({i})", trackLights);
                    for (int i = 2; i < 22; i++)
                        FindAddGameObject($"LaserTop ({i})", trackLights);
                    FindAddGameObject("LaserL", trackLights);
                    FindAddGameObject("LarerR", trackLights);
                    FindAddGameObject("LaserL (2)", trackLights);
                    FindAddGameObject("LarerR (2)", trackLights);
                    break;
                case "BTSEnvironment":
                    FindAddGameObject("GlowLineL", trackLights);
                    FindAddGameObject("GlowLineR", trackLights);
                    FindAddGameObject("GlowLineH", trackLights);
                    FindAddGameObject("GlowLineH (2)", trackLights);
                    FindAddGameObject("LaserL", trackLights);
                    FindAddGameObject("LaserR", trackLights);
                    FindAddGameObject("GlowLineC", trackLights);
                    FindAddGameObject("BottomGlow", trackLights);
                    for (int i = 0; i < 4; i++)
                        FindAddGameObject("SideLaser", trackLights, true);
                    break;
            }
        }
    }
}