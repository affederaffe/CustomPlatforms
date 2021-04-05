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
        private readonly AssetLoader _assetLoader;
        private readonly PlatformManager _platformManager;
        private readonly GameScenesManager _gameScenesManager;

        private string sceneName;
        private GameObject[] roots;

        private readonly List<GameObject> menuEnvironment = new();
        private readonly List<GameObject> multiplayerEnvironment = new();
        private readonly List<GameObject> playersPlace = new();
        private readonly List<GameObject> feet = new();
        private readonly List<GameObject> smallRings = new();
        private readonly List<GameObject> bigRings = new();
        private readonly List<GameObject> visualizer = new();
        private readonly List<GameObject> towers = new();
        private readonly List<GameObject> highway = new();
        private readonly List<GameObject> backColumns = new();
        private readonly List<GameObject> doubleColorLasers = new();
        private readonly List<GameObject> backLasers = new();
        private readonly List<GameObject> rotatingLasers = new();
        private readonly List<GameObject> trackLights = new();

        private const string renamedObjectSuffix = "renamed";
        private readonly List<GameObject> renamedObjects = new();

        private TrackLaneRing[] TrackLaneRings => _TrackLaneRings ??= Resources.FindObjectsOfTypeAll<TrackLaneRing>();
        private TrackLaneRing[] _TrackLaneRings;

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

        /// <summary>
        /// Hide and unhide world objects as required by the active platform
        /// </summary>
        internal void HideObjectsForPlatform()
        {
            FindEnvironment();
            if (menuEnvironment != null) SetCollectionHidden(menuEnvironment, _platformManager.GetIndexForType(PlatformType.Active) != 0);
            if (multiplayerEnvironment != null) SetCollectionHidden(multiplayerEnvironment, true);
            if (playersPlace != null) SetCollectionHidden(playersPlace, _platformManager.activePlatform.hideDefaultPlatform);
            if (feet != null) SetCollectionHidden(feet, _platformManager.activePlatform.hideDefaultPlatform && !_config.AlwaysShowFeet);
            if (smallRings != null) SetCollectionHidden(smallRings, _platformManager.activePlatform.hideSmallRings);
            if (bigRings != null) SetCollectionHidden(bigRings, _platformManager.activePlatform.hideBigRings);
            if (visualizer != null) SetCollectionHidden(visualizer, _platformManager.activePlatform.hideEQVisualizer);
            if (towers != null) SetCollectionHidden(towers, _platformManager.activePlatform.hideTowers);
            if (highway != null) SetCollectionHidden(highway, _platformManager.activePlatform.hideHighway);
            if (backColumns != null) SetCollectionHidden(backColumns, _platformManager.activePlatform.hideBackColumns);
            if (backLasers != null) SetCollectionHidden(backLasers, _platformManager.activePlatform.hideBackLasers);
            if (doubleColorLasers != null) SetCollectionHidden(doubleColorLasers, _platformManager.activePlatform.hideDoubleColorLasers);
            if (rotatingLasers != null) SetCollectionHidden(rotatingLasers, _platformManager.activePlatform.hideRotatingLasers);
            if (trackLights != null) SetCollectionHidden(trackLights, _platformManager.activePlatform.hideTrackLights);
            _assetLoader.playersPlace.SetActive(_platformManager.GetIndexForType(PlatformType.Active) != 0 && !_platformManager.activePlatform.hideDefaultPlatform && sceneName == "MenuEnvironment");
            CleanupEnvironment();
        }

        /// <summary>
        /// Finds all GameObjects that make up the default environment and groups them into lists
        /// </summary>
        private void FindEnvironment()
        {
            roots = GetRootGameObjects();
            if (roots == null) return;
            FindMenuEnvironmnet();
            FindMultiplayerEnvironment();
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
        /// Finds the currently loaded environment and gets the root GameObjects of the respective scene
        /// </summary>
        /// <returns>The root GameObjects of the environment scene</returns>
        private GameObject[] GetRootGameObjects()
        {
            sceneName = _gameScenesManager.GetCurrentlyLoadedSceneNames().LastOrDefault(x => x.EndsWith("Environment"));
            if (sceneName == "MultiplayerEnvironment") sceneName = "GameCore";
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid()) return null;
            return scene.GetRootGameObjects();
        }

        /// <summary>
        /// Set the active state of a Collection of GameObjects
        /// </summary>
        /// <param name="list">A <see cref="List{T}"/> of GameObjects</param>
        /// <param name="hidden">A boolean describing the desired hidden state</param>
        private static void SetCollectionHidden(List<GameObject> list, bool hidden)
        {
            foreach (GameObject go in list)
                go.SetActive(!hidden);
            list.Clear();
        }

        /// <summary>
        /// Resets the names of all renamed objects to it's default
        /// </summary>
        private void CleanupEnvironment()
        {
            foreach (GameObject go in renamedObjects)
                go.name = go.name.Remove(go.name.Length - renamedObjectSuffix.Length);
            _TrackLaneRings = null;
            roots = null;
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
            switch (sceneName)
            {
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
                    FindAddGameObject("IsActiveObjects/Construction/PlayersPlace", playersPlace);
                    break;
                default:
                    FindAddGameObject("PlayersPlace", playersPlace);
                    break;
            }
        }

        private void FindFeetIcon()
        {
            switch (sceneName)
            {
                case "GameCore":
                    FindAddGameObject("IsActiveObjects/Construction/PlayersPlace/Feet", feet);
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
            switch (sceneName)
            {
                default:
                    FindAddGameObject("Spectrograms", visualizer);
                    break;
            }
        }

        private void FindTowers()
        {
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
                case "TriangleEnvironment":
                    FindAddGameObject("NearBuildingLeft (3)", towers);
                    FindAddGameObject("NearBuildingRight (3)", towers);
                    break;
                case "NiceEnvironment":
                    FindAddGameObject("NearBuildingLeft (1)", towers);
                    FindAddGameObject("NearBuildingRight (1)", towers);
                    FindAddGameObject("NearBuildingLeft (2)", towers);
                    FindAddGameObject("NearBuildingRight (2)", towers);
                    break;
                case "BigMirrorEnvironment":
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
                    FindAddGameObject("MonstercatLogoL", highway);
                    FindAddGameObject("MonstercatLogoR", highway);
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