using System.Collections.Generic;
using System.Linq;

using CustomFloorPlugin.Configuration;
using CustomFloorPlugin.Utilities;

using UnityEngine;
using UnityEngine.SceneManagement;

using Zenject;


namespace CustomFloorPlugin {


    /// <summary> 
    /// Activates and deactivates world geometry in the active scene as required by the chosen custom platform<br/>
    /// Most documentation on this file is omited because it is a giant clusterfuck and I hate it... with a passion.
    /// </summary>
    internal class EnvironmentHider {

        [Inject]
        private readonly PluginConfig _config;

        [Inject]
        private readonly PlatformManager _platformManager;

        private Scene currentEnvironment;
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


        /// <summary>
        /// Hide and unhide world objects as required by a platform<br/>
        /// Delayed by a frame because of order of operations after scene loading
        /// </summary>
        /// <param name="platform">A platform that defines which objects are to be hidden</param>
        internal void HideObjectsForPlatform(CustomPlatform platform) {
            SharedCoroutineStarter.instance.StartCoroutine(InternalHideObjectsForPlatform(platform));
        }


        /// <summary>
        /// Hide and unhide world objects as required by a platform<br/>
        /// It is not practical to call this directly
        /// </summary>
        /// <param name="platform">A platform that defines which objects are to be hidden</param>
        private IEnumerator<WaitForEndOfFrame> InternalHideObjectsForPlatform(CustomPlatform platform) {
            yield return new WaitForEndOfFrame();
            FindEnvironment();
            if (menuEnvironment != null) SetCollectionHidden(menuEnvironment, _platformManager.activePlatform != null);
            if (multiplayerEnvironment != null) SetCollectionHidden(multiplayerEnvironment, _config.UseInMultiplayer);
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
            bool showDefaultPlatform = _platformManager.activePlatform != null && !platform.hideDefaultPlatform && currentEnvironment.name.StartsWith("Menu");
            PlatformManager.PlayersPlace.SetActive(showDefaultPlatform);
            PlatformManager.Feet.SetActive(_config.AlwaysShowFeet);
        }


        /// <summary>
        /// Finds all GameObjects that make up the default environment and groups them into lists
        /// </summary>
        private void FindEnvironment() {
            currentEnvironment = Searching.GetCurrentEnvironment();
            roots = currentEnvironment.GetRootGameObjects();
            if (currentEnvironment.name == "MenuEnvironment") {
                FindMenuEnvironmnetAndFeet();
            }
            if (currentEnvironment.name == "GameCore") {
                FindMultiplayerEnvironment();
                FindPlayersPlace();
            }
            else {
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
            }
        }


        /// <summary>
        /// Set the active state of a Collection of GameObjects
        /// </summary>
        /// <param name="list">A <see cref="List{T}"/> of GameObjects</param>
        /// <param name="hidden">A boolean describing the desired hidden state</param>
        private void SetCollectionHidden(List<GameObject> list, bool hidden) {
            if (list == null) {
                return;
            }

            foreach (GameObject go in list) {
                go?.SetActive(!hidden);
            }
        }


        /// <summary>
        /// Finds a GameObject by name and adds it to the provided list
        /// </summary>
        /// <param name="name">The name of the desired GameObject</param>
        /// <param name="list">The list to be added to</param>
        private bool FindAddGameObject(string name, List<GameObject> list, bool rename = false) {
            GameObject go;
            foreach (GameObject root in roots) {
                go = root?.transform.Find(name)?.gameObject;
                if (go != null) {
                    list.Add(go);
                    if (rename) {
                        go.name += "renamed";
                    }
                    return true;
                }
                else if (root?.name == name) {
                    list.Add(root);
                }
            }
            return false;
        }

        private void FindMenuEnvironmnetAndFeet() {
            if (menuEnvironment == null || menuEnvironment?.Count == 0) {
                menuEnvironment = new List<GameObject>();
                FindAddGameObject("MenuCoreLighting/SkyGradient", menuEnvironment);
                FindAddGameObject("NearBuildingLeft", menuEnvironment);
                FindAddGameObject("NearBuildingRight", menuEnvironment);
                FindAddGameObject("NearBuildingLeft (1)", menuEnvironment);
                FindAddGameObject("NearBuildingRight (1)", menuEnvironment);
                FindAddGameObject("DefaultEnvironment/GroundCollider", menuEnvironment);
                FindAddGameObject("DefaultEnvironment/Ground", menuEnvironment);
                FindAddGameObject("DefaultEnvironment/PlayersPlace", menuEnvironment);
                FindAddGameObject("DefaultEnvironment/NotesBehindPlayer", menuEnvironment);
                FindAddGameObject("DefaultEnvironment/NeonLights", menuEnvironment);
                FindAddGameObject("DefaultEnvironment/Notes", menuEnvironment);

                // Find the feet icon in the menu, only the first time, and reparent it, otherwise it's destroyed on scene change
                feet = new List<GameObject>();
                FindAddGameObject("DefaultEnvironment/PlayersPlace/Feet", feet);
                FindAddGameObject("DefaultEnvironment/PlayersPlace/Version", feet);

                PlatformManager.Feet = new GameObject("Feet");
                foreach (GameObject feet in feet) {
                    feet.transform.SetParent(PlatformManager.Feet.transform);
                }
                feet = null;
            }
        }

        private void FindMultiplayerEnvironment() {
            multiplayerEnvironment = new List<GameObject>();
            FindAddGameObject("IsActiveObjects/Construction/ConstructionL", multiplayerEnvironment);
            FindAddGameObject("IsActiveObjects/Construction/ConstructionR", multiplayerEnvironment);
            FindAddGameObject("IsActiveObjects/Lasers", multiplayerEnvironment);

            if (FindAddGameObject("IsActiveObjects/PlatformEnd", multiplayerEnvironment)) {
                FindAddGameObject("IsActiveObjects/CenterRings", multiplayerEnvironment);
            }
            else {
                FindAddGameObject("Construction", multiplayerEnvironment);
                FindAddGameObject("Lasers", multiplayerEnvironment);
            }
        }

        private void FindPlayersPlace() {
            playersPlace = new List<GameObject>();
            FindAddGameObject("PlayersPlace", playersPlace);

            // LinkinPark
            FindAddGameObject("PlayersPlaceShadow", playersPlace);

            // Multiplayer
            FindAddGameObject("IsActiveObjects/Construction/PlayersPlace", playersPlace);
        }

        private void FindFeetIcon() {
            feet = new List<GameObject>();

            // Song
            FindAddGameObject("PlayersPlace/Feet", feet);

            // Multiplayer
            FindAddGameObject("IsActiveObjects/Construction/PlayersPlace/Feet", feet);

            foreach (GameObject feet in feet) {
                feet?.transform.SetParent(null); // remove from original platform 
            }
        }

        private void FindSmallRings() {
            smallRings = new List<GameObject>();
            FindAddGameObject("SmallTrackLaneRings", smallRings);
            FindAddGameObject("TrackLaneRing", smallRings);
            FindAddGameObject("TriangleTrackLaneRing", smallRings);
            FindAddGameObject("PanelsTrackLaneRing", smallRings);
            FindAddGameObject("Panels4TrackLaneRing", smallRings);
            FindAddGameObject("PairLaserTrackLaneRing", smallRings);
            FindAddGameObject("PanelLightTrackLaneRing", smallRings);
            foreach (TrackLaneRing trackLaneRing in Resources.FindObjectsOfTypeAll<TrackLaneRing>().Where(x =>
                x.name == "TrackLaneRing(Clone)" ||
                x.name == "TriangleTrackLaneRing(Clone)" ||
                x.name == "PanelsTrackLaneRing(Clone)" ||
                x.name == "Panels4TrackLaneRing(Clone)" ||
                x.name == "PairLaserTrackLaneRing(Clone)" ||
                x.name == "PanelLightTrackLaneRing(Clone)"
                )) {
                smallRings.Add(trackLaneRing.gameObject);
            }

            // KDA
            FindAddGameObject("TentacleLeft", smallRings);
            FindAddGameObject("TentacleRight", smallRings);
        }

        private void FindBigRings() {
            bigRings = new List<GameObject>();
            FindAddGameObject("BigTrackLaneRings", bigRings);
            FindAddGameObject("BigLightsTrackLaneRings", bigRings);
            FindAddGameObject("BigCenterLightTrackLaneRing", bigRings);
            FindAddGameObject("LightsTrackLaneRing", bigRings);

            foreach (TrackLaneRing trackLaneRing in Resources.FindObjectsOfTypeAll<TrackLaneRing>().Where(x =>
                x.name == "BigTrackLaneRing(Clone)" ||
                x.name == "BigCenterLightTrackLaneRing(Clone)" ||
                x.name == "LightsTrackLaneRing(Clone)"
                )) {
                bigRings.Add(trackLaneRing.gameObject);
            }
        }

        private void FindVisualizers() {
            visualizer = new List<GameObject>();
            FindAddGameObject("Spectrograms", visualizer);
            FindAddGameObject("PillarPair", visualizer);
            if (FindAddGameObject("SmallPillarPair", visualizer)) {
                for (int i = 1; i < 5; i++) {
                    FindAddGameObject($"PillarPair ({i})", visualizer);
                    FindAddGameObject($"SmallPillarPair ({i})", visualizer);
                }
            }
        }

        private void FindTowers() {
            towers = new List<GameObject>();
            // Song Environments
            FindAddGameObject("Buildings", towers);

            // Monstercat
            FindAddGameObject("MonstercatLogoL", towers);
            FindAddGameObject("MonstercatLogoR", towers);
            FindAddGameObject("VConstruction", towers);
            FindAddGameObject("FarBuildings", towers);

            // CrabRave
            FindAddGameObject("NearBuildingLeft", towers);
            FindAddGameObject("NearBuildingRight", towers);

            // KDA
            FindAddGameObject("FloorL", towers);
            FindAddGameObject("FloorR", towers);
            if (FindAddGameObject($"GlowLine", towers)) {
                for (int i = 0; i < 100; i++) {
                    FindAddGameObject($"GlowLine ({i})", towers);
                }
            }

            // Rocket
            FindAddGameObject("RocketCar", towers);
            FindAddGameObject("RocketCar (1)", towers);
            FindAddGameObject("RocketArena", towers);

            // GreenDayGrenade
            FindAddGameObject("GreenDayCity", towers);

            // Timbaland
            FindAddGameObject("MainStructure", towers);
            FindAddGameObject("TopStructure", towers);
            if (FindAddGameObject("TimbalandLogo", towers)) {
                for (int i = 0; i < 4; i++) {
                    FindAddGameObject($"TimbalandLogo ({i})", towers);
                }
            }

            // BTS
            FindAddGameObject("PillarTrackLaneRingsR", towers);
            FindAddGameObject("PillarTrackLaneRingsR (1)", towers);
        }

        private void FindHighway() {
            highway = new List<GameObject>();
            FindAddGameObject("TrackConstruction", highway);
            FindAddGameObject("FloorConstruction", highway);
            FindAddGameObject("TrackMirror", highway);
            FindAddGameObject("Floor", highway);
            FindAddGameObject("FloorMirror", highway);
            FindAddGameObject("Construction", highway);
            FindAddGameObject("CombinedMesh", highway);

            // Dragons
            FindAddGameObject("TopConstruction", highway);
            FindAddGameObject("TopConstruction (1)", highway);
            FindAddGameObject("TopConstruction (2)", highway);
            FindAddGameObject("TopConstruction (3)", highway);
            FindAddGameObject("FloorGround (4)", highway);
            FindAddGameObject("FloorGround (5)", highway);
            FindAddGameObject("HallConstruction", highway);
            FindAddGameObject("Underground", highway);
            FindAddGameObject("Underground (18)", highway);
            FindAddGameObject("Underground (19)", highway);

            // Panic
            FindAddGameObject("BottomCones", highway);
            FindAddGameObject("TopCones", highway);

            // Rocket
            FindAddGameObject("Mirror", highway);

            // LinkinPark
            FindAddGameObject("Tunnel", highway);
            FindAddGameObject("TunnelRing", highway);
            for (int i = 1; i < 7; i++) {
                FindAddGameObject($"TunnelRing ({i})", highway);
            }
            FindAddGameObject("TunnelRingShadow", highway);
            FindAddGameObject("TunnelRingShadow (1)", highway);
            FindAddGameObject("LinkinParkTextLogoL", highway);
            FindAddGameObject("LinkinParkTextLogoR", highway);
            FindAddGameObject("TrackShadow", highway);
            FindAddGameObject("LinkinParkSoldier", highway);

            // BTS
            FindAddGameObject("Clouds", highway);

            // 360°
            FindAddGameObject("Collider", highway);
        }

        private void FindBackColumns() {
            backColumns = new List<GameObject>();
            FindAddGameObject("BackColumns", backColumns);

            // 360°
            for (int i = 2; i < 24; i++) {
                FindAddGameObject($"GameObject ({i})", backColumns);
            }
        }

        private void FindRotatingLasers() {
            rotatingLasers = new List<GameObject>();
            // Default, BigMirror, Triangle, Rocket
            if (FindAddGameObject("RotatingLasersPair", rotatingLasers)) {
                for (int i = 1; i < 19; i++) {
                    FindAddGameObject($"RotatingLasersPair ({i})", rotatingLasers);
                }
            }

            // KDA
            if (FindAddGameObject("RotatingLasersPair (3)", rotatingLasers)) {
                for (int i = 4; i < 7; i++) {
                    FindAddGameObject($"RotatingLasersPair ({i})", rotatingLasers);
                }
            }

            // Nice Env
            if (FindAddGameObject("RotatingLaserLeft0", rotatingLasers) && FindAddGameObject("RotatingLaserRight0", rotatingLasers)) {
                for (int i = 1; i < 4; i++) {
                    FindAddGameObject($"RotatingLaserLeft{i}", rotatingLasers);
                    FindAddGameObject($"RotatingLaserRight{i}", rotatingLasers);
                }
            }

            // 360°
            if (FindAddGameObject("LightPillar (9)", rotatingLasers)) {
                for (int i = 10; i < 19; i++) {
                    FindAddGameObject($"LightPillar ({i})", rotatingLasers);
                }
            }
        }

        private void FindDoubleColorLasers() {
            doubleColorLasers = new List<GameObject>();

            // Default, BigMirror, Nice
            if (FindAddGameObject("DoubleColorLaser", doubleColorLasers)) {
                for (int i = 1; i < 10; i++) {
                    FindAddGameObject($"DoubleColorLaser ({i})", doubleColorLasers);
                }
            }

            // Tutorial
            if (FindAddGameObject("DoubleColorLaser (10)", doubleColorLasers)) {
                for (int i = 11; i < 20; i++) {
                    FindAddGameObject($"DoubleColorLaser ({i})", doubleColorLasers);
                }
            }

            // 360°
            if (FindAddGameObject("DownLaser (4)", doubleColorLasers)) {
                for (int i = 5; i < 13; i++) {
                    FindAddGameObject($"DownLaser ({i})", doubleColorLasers);
                }
            }
        }

        private void FindBackLasers() {
            backLasers = new List<GameObject>();
            FindAddGameObject("FrontLights", backLasers);

            // Panic
            FindAddGameObject("Window", backLasers, true);
            FindAddGameObject("Window", backLasers, true);

            // GreenDayGrenade
            FindAddGameObject("Logo", backLasers);

            // Timbaland
            if (FindAddGameObject("Light (4)", backLasers)) {
                for (int i = 5; i < 8; i++) {
                    FindAddGameObject($"Light {i}", backLasers);
                }
            }

            // BTS
            if (FindAddGameObject("SideLaser", backLasers, true)) {
                for (int i = 0; i < 3; i++) {
                    FindAddGameObject("SideLaser", backLasers, true);
                }
            }
            FindAddGameObject("GradientBackground", backLasers);
            FindAddGameObject("StarHemisphere", backLasers);
            FindAddGameObject("MagicDoorSprite", backColumns);

            // 360°
            //FindAddGameObject("SpawnRotationChevronManager", backLasers);
        }

        private void FindTrackLights() {
            trackLights = new List<GameObject>();
            FindAddGameObject("GlowLineR", trackLights);
            FindAddGameObject("GlowLineL", trackLights);
            FindAddGameObject("GlowLineFarL", trackLights);
            FindAddGameObject("GlowLineFarR", trackLights);

            // Origins
            FindAddGameObject("SidePSL", trackLights);
            FindAddGameObject("SidePSR", trackLights);

            // BigMirror
            FindAddGameObject("GlowLineL2", trackLights);
            FindAddGameObject("GlowLineR2", trackLights);

            // Tutorial
            FindAddGameObject("GlowLines", trackLights);

            // KDA
            FindAddGameObject("GlowLineLVisible", trackLights);
            FindAddGameObject("GlowLineRVisible", trackLights);

            // KDA, Monstercat, CrabRave, GreenDayGrenade
            if (FindAddGameObject("Laser", trackLights)) {
                for (int i = 1; i < 20; i++) {
                    FindAddGameObject($"Laser ({i})", trackLights);
                }
            }

            // Monstercat
            if (FindAddGameObject("Laser (4)", trackLights)) {
                for (int i = 5; i < 13; i++) {
                    FindAddGameObject($"Laser ({i})", trackLights);
                }
            }

            if (FindAddGameObject("GlowTopLine", trackLights)) {
                for (int i = 1; i < 5; i++) {
                    FindAddGameObject($"GlowTopLine ({i})", trackLights);
                }
            }

            // Dragons
            if (FindAddGameObject("GlowTopLine (5)", trackLights)) {
                for (int i = 6; i < 12; i++) {
                    FindAddGameObject($"GlowTopLine ({i})", trackLights);
                }
            }

            FindAddGameObject("GlowLineR (1)", trackLights);
            FindAddGameObject("GlowLineR (2)", trackLights);
            if (FindAddGameObject($"GlowLineL (1)", trackLights)) {
                for (int i = 2; i < 25; i++) {
                    FindAddGameObject($"GlowLineL ({i})", trackLights);
                }
            }

            // Monstercat
            FindAddGameObject("GlowLineLHidden", trackLights);
            FindAddGameObject("GlowLineRHidden", trackLights);

            // Dragons, Panic
            FindAddGameObject("DragonsSidePSL", trackLights);
            FindAddGameObject("DragonsSidePSR", trackLights);
            for (int i = 0; i < 15; i++) {
                FindAddGameObject($"ConstructionGlowLine ({i + 1})", trackLights);
            }

            // Rocket
            FindAddGameObject("RocketArenaLight", trackLights);
            FindAddGameObject("RocketGateLight", trackLights);
            FindAddGameObject("GateLight0", trackLights);
            FindAddGameObject("GateLight1", trackLights);
            FindAddGameObject("GateLight1 (4)", trackLights);
            if (FindAddGameObject("EnvLight0", trackLights)) {
                for (int i = 2; i < 10; i++) {
                    FindAddGameObject($"EnvLight0 ({i})", trackLights);
                }
            }

            // LinkinPark
            if (FindAddGameObject($"LampShadow (1)", trackLights)) {
                for (int i = 2; i < 13; i++) {
                    FindAddGameObject($"LampShadow ({i})", trackLights);
                }
            }
            if (FindAddGameObject($"LaserTop (2)", trackLights)) {
                for (int i = 3; i < 22; i++) {
                    FindAddGameObject($"LaserTop ({i})", trackLights);
                }
            }
            if (FindAddGameObject("TunnelRotatingLasersPair", trackLights)) {
                for (int i = 1; i < 18; i++) {
                    FindAddGameObject($"TunnelRotatingLasersPair ({i})", trackLights);
                }
            }
            FindAddGameObject("LaserFloor", trackLights);
            FindAddGameObject("LaserFloor (1)", trackLights);
            FindAddGameObject("LaserFloor (2)", trackLights);
            FindAddGameObject("LaserFloor (3)", trackLights);
            FindAddGameObject("LaserL", trackLights);
            FindAddGameObject("LarerR", trackLights); // this is not my typo
            FindAddGameObject("LaserL (2)", trackLights);
            FindAddGameObject("LarerR (2)", trackLights);
            FindAddGameObject("FloorLightShadowL", trackLights);
            FindAddGameObject("FloorLightShadowR", trackLights);
            FindAddGameObject("ArchShadow", trackLights);
            FindAddGameObject("ArchShadow (1)", trackLights);

            // BTS
            FindAddGameObject("GlowLineC", trackLights);
            FindAddGameObject("BottomGlow", trackLights);
            FindAddGameObject("LaserR", trackLights);

            // 360°
            if (FindAddGameObject("TopLaser", trackLights)) {
                for (int i = 1; i < 6; i++) {
                    FindAddGameObject($"TopLaser ({i})", trackLights);
                }
            }
            if (FindAddGameObject("Cube", trackLights) && FindAddGameObject("Cube (1)", trackLights)) {
                for (int i = 82; i < 90; i++) {
                    FindAddGameObject($"Cube ({i})", trackLights);
                }
            }
        }
    }
}