using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using CustomFloorPlugin.UI;
using static CustomFloorPlugin.GlobalCollection;
using static CustomFloorPlugin.Utilities.BeatSaberSearching;
using static CustomFloorPlugin.Utilities.UnityObjectSearching;

namespace CustomFloorPlugin {


    /// <summary> 
    /// Activates and deactivates world geometry in the active scene as required by the chosen custom platform<br/>
    /// Most documentation on this file is omited because it is a giant clusterfuck and I hate it... with a passion.
    /// </summary>
    internal static class EnvironmentHider {

        private static List<GameObject> menuEnvironment;
        private static List<GameObject> playersPlace;
        private static List<GameObject> feet;
        private static List<GameObject> smallRings;
        private static List<GameObject> bigRings;
        private static List<GameObject> visualizer;
        private static List<GameObject> towers;
        private static List<GameObject> highway;
        private static List<GameObject> backColumns;
        private static List<GameObject> doubleColorLasers;
        private static List<GameObject> backLasers;
        private static List<GameObject> rotatingLasers;
        private static List<GameObject> trackLights;

        //private static List<GameObject> MPPCs;

        private static bool ShowFeetOverride {
            get {
                return Settings.AlwaysShowFeet;
            }
        }


        /// <summary>
        /// Hide and unhide world objects as required by a platform<br/>
        /// Delayed by a frame because of order of operations after scene loading
        /// </summary>
        /// <param name="platform">A platform that defines which objects are to be hidden</param>
        internal static void HideObjectsForPlatform(CustomPlatform platform) {
            SharedCoroutineStarter.instance.StartCoroutine(InternalHideObjectsForPlatform(platform));
        }


        /// <summary>
        /// Hide and unhide world objects as required by a platform<br/>
        /// It is not practical to call this directly
        /// </summary>
        /// <param name="platform">A platform that defines which objects are to be hidden</param>
        private static IEnumerator<WaitForEndOfFrame> InternalHideObjectsForPlatform(CustomPlatform platform) {
            yield return new WaitForEndOfFrame();
            FindEnvironment();
            HandelEnvironment(platform);
            if (playersPlace != null) SetCollectionHidden(playersPlace, platform.hideDefaultPlatform);
            if (feet != null) SetCollectionHidden(feet, platform.hideDefaultPlatform && !ShowFeetOverride);
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
        }


        /// <summary>
        /// Finds all GameObjects that make up the default environment and groups them into lists
        /// </summary>
        private static void FindEnvironment() {
            if (IsNullZeroOrContainsNull(menuEnvironment)) FindMenuEnvironmnet();
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
        /// Set the active state of a Collection of GameObjects
        /// </summary>
        /// <param name="list">A <see cref="List{T}"/> of GameObjects</param>
        /// <param name="hidden">A boolean describing the desired hidden state</param>
        private static void SetCollectionHidden(List<GameObject> list, bool hidden) {
            if (list == null) return;
            foreach (GameObject go in list) {
                if (go != null) go.SetActive(!hidden);
            }
        }


        /// <summary>
        /// Finds a GameObject by name and adds it to the provided list
        /// </summary>
        /// <param name="name">The name of the desired GameObject</param>
        /// <param name="list">The list to be added to</param>
        private static bool FindAddGameObject(string name, List<GameObject> list) {
            GameObject[] roots = GetCurrentEnvironment().GetRootGameObjects();
            GameObject go;
            foreach (GameObject root in roots) {
                go = GameObject.Find(name);
                if (go != null) {
                    list.Add(go);
                    return true;
                }
                else if (root.name == name) {
                    list.Add(root);
                }
            }
            return false;
        }

        private static bool FindAddRenameGameObject(string name, List<GameObject> list) {
            GameObject[] roots = GetCurrentEnvironment().GetRootGameObjects();
            GameObject go;
            //foreach (GameObject root in roots) {
            for (int i = 0; i < roots.Length; i++) {
                go = GameObject.Find(name);
                if (go != null) {
                    go.name += i;
                    list.Add(go);
                    return true;
                }
                else if (roots[i].name == name) {
                    list.Add(roots[i]);
                }
            }
            return false;
        }

        private static void HandelEnvironment(CustomPlatform platform) {
            if (PlatformManager.PlayersPlace != null && PlatformManager.activePlatform != null && !platform.hideDefaultPlatform && (GetCurrentEnvironment().name.StartsWith("Menu", STR_INV) || GetCurrentEnvironment().name == "MultiplayerEnvironment")) PlatformManager.PlayersPlace.SetActive(true); // Handles Platforms which would normally use the default Platform...
            else if (PlatformManager.PlayersPlace != null) PlatformManager.PlayersPlace.SetActive(false); // Only in Menu
            if (menuEnvironment != null && PlatformManager.activePlatform != null) SetCollectionHidden(menuEnvironment, true); // Always hide the Menu Environment in Song...
            else SetCollectionHidden(menuEnvironment, false); // ...but not in Menu
        }

        private static void FindMenuEnvironmnet() {
            menuEnvironment = new List<GameObject>();
            FindAddGameObject("MenuEnvironment/DefaultEnvironment/Laser (1)", menuEnvironment);
            FindAddGameObject("MenuEnvironment/DefaultEnvironment/Laser (2)", menuEnvironment);
            FindAddGameObject("MenuEnvironment/DefaultEnvironment/Laser (3)", menuEnvironment);
            FindAddGameObject("MenuEnvironment/DefaultEnvironment/Laser (4)", menuEnvironment);
            FindAddGameObject("MenuEnvironment/DefaultEnvironment/NeonLights", menuEnvironment);
            FindAddGameObject("MenuEnvironment/DefaultEnvironment/Notes", menuEnvironment);
            FindAddGameObject("MenuEnvironment/Note (18)", menuEnvironment);
            FindAddGameObject("MenuEnvironment/Note (19)", menuEnvironment);
            FindAddGameObject("MenuEnvironment/Shadow (2)", menuEnvironment);
            FindAddGameObject("MenuEnvironment/Arrow", menuEnvironment);
            FindAddGameObject("MenuEnvironment/Arrow (1)", menuEnvironment);
            FindAddGameObject("MenuEnvironment/Arrow (2)", menuEnvironment);
            FindAddGameObject("MenuEnvironment/Arrow (3)", menuEnvironment);
            FindAddGameObject("MenuEnvironment/Ground", menuEnvironment);
            FindAddGameObject("MenuEnvironment/GroundCollider", menuEnvironment);
        }

        private static void FindPlayersPlace() {
            playersPlace = new List<GameObject>();
            FindAddGameObject("Environment/PlayersPlace", playersPlace);

            // LinkinPark
            FindAddGameObject("Environment/PlayersPlaceShadow", playersPlace);
        }

        private static void FindFeetIcon() {
            feet = new List<GameObject>();
            FindAddGameObject("MenuEnvironment/DefaultEnvironment/PlayersPlace/Feet", feet);
            FindAddGameObject("MenuEnvironment/DefaultEnvironment/PlayersPlace/Version", feet);
            FindAddGameObject("Feet", feet);
            FindAddGameObject("Version", feet);

            foreach (GameObject feet in feet) {
                feet.transform.SetParent(null, true); // remove from original platform 
            }
        }

        private static void FindSmallRings() {
            smallRings = new List<GameObject>();
            FindAddGameObject("TrackLaneRing", smallRings);
            FindAddGameObject("Environment/SmallTrackLaneRings", smallRings);
            FindAddGameObject("TriangleTrackLaneRing", smallRings);
            foreach (var trackLaneRing in Resources.FindObjectsOfTypeAll<TrackLaneRing>().Where(x => 
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
            FindAddGameObject("Environment/TentacleLeft", smallRings);
            FindAddGameObject("Environment/TentacleRight", smallRings);
        }

        private static void FindBigRings() {
            bigRings = new List<GameObject>();
            FindAddGameObject("Environment/BigLightsTrackLaneRings", bigRings);
            foreach (var trackLaneRing in Resources.FindObjectsOfTypeAll<TrackLaneRing>().Where(x => 
                x.name == "BigTrackLaneRing(Clone)" ||
                x.name == "BigCenterLightTrackLaneRing(Clone)"
                )) {
                bigRings.Add(trackLaneRing.gameObject);
            }
        }

        private static void FindVisualizers() {
            visualizer = new List<GameObject>();
            FindAddGameObject("Environment/Spectrograms", visualizer);
            FindAddGameObject("Environment/PillarPair", visualizer);
            FindAddGameObject("Environment/SmallPillarPair", visualizer);
            for (int i = 1; i < 5; i++) {
                FindAddGameObject($"Environment/PillarPair ({i})", visualizer);
                FindAddGameObject($"Environment/SmallPillarPair ({i})", visualizer);
            }
        }

        private static void FindTowers() {
            towers = new List<GameObject>();
            // Song Environments
            FindAddGameObject("Environment/Buildings", towers);

            // Monstercat
            FindAddGameObject("Environment/MonstercatLogoL", towers);
            FindAddGameObject("Environment/MonstercatLogoR", towers);
            FindAddGameObject("Environment/VConstruction", towers);
            FindAddGameObject("Environment/FarBuildings", towers);

            // CrabRave
            FindAddGameObject("Environment/NearBuildingLeft", towers);
            FindAddGameObject("Environment/NearBuildingRight", towers);

            // KDA
            FindAddGameObject("FloorL", towers);
            FindAddGameObject("FloorR", towers);
            if (FindAddGameObject($"GlowLine", towers)) {
                for (int i = 0; i < 100; i++) {
                    FindAddGameObject($"GlowLine ({i})", towers);
                }
            }

            // Rocket
            FindAddGameObject("Environment/RocketCar", towers);
            FindAddGameObject("Environment/RocketCar (1)", towers);
            FindAddGameObject("Environment/RocketArena", towers);

            // GreenDayGrenade
            FindAddGameObject("Environment/GreenDayCity", towers);

            // Timbaland
            FindAddGameObject("Environment/MainStructure", towers);
            FindAddGameObject("Environment/TopStructure", towers);

            // BTS
            FindAddGameObject("Environment/PillarTrackLaneRingsR", towers);
            FindAddGameObject("Environment/PillarTrackLaneRingsR (1)", towers);
        }

        private static void FindHighway() {
            highway = new List<GameObject>();
            FindAddGameObject("Environment/TrackConstruction", highway);
            FindAddGameObject("Environment/FloorConstruction", highway);
            FindAddGameObject("Environment/TrackMirror", highway);
            FindAddGameObject("Environment/Floor", highway);
            FindAddGameObject("Environment/FloorMirror", highway);
            FindAddGameObject("Environment/Construction", highway);
            FindAddGameObject("Environment/CombinedMesh", highway);

            // Dragons
            FindAddGameObject("Environment/TopConstruction", highway);
            FindAddGameObject("Environment/TopConstruction (1)", highway);
            FindAddGameObject("Environment/TopConstruction (2)", highway);
            FindAddGameObject("Environment/TopConstruction (3)", highway);
            FindAddGameObject("Environment/FloorGround (4)", highway);
            FindAddGameObject("Environment/FloorGround (5)", highway);
            FindAddGameObject("Environment/HallConstruction", highway);
            FindAddGameObject("Environment/Underground", highway);
            FindAddGameObject("Environment/Underground (18)", highway);
            FindAddGameObject("Environment/Underground (19)", highway);

            // Panic
            FindAddGameObject("Environment/BottomCones", highway);
            FindAddGameObject("Environment/TopCones", highway);

            // Rocket
            FindAddGameObject("Environment/Mirror", highway);

            // LinkinPark
            FindAddGameObject("Environment/Tunnel", highway);
            FindAddGameObject("Environment/TunnelRing", highway);
            for (int i = 1; i < 7; i++) {
                FindAddGameObject($"Environment/TunnelRing ({i})", highway);
            }
            FindAddGameObject("Environment/TunnelRingShadow", highway);
            FindAddGameObject("Environment/TunnelRingShadow (1)", highway);
            FindAddGameObject("Environment/LinkinParkTextLogoL", highway);
            FindAddGameObject("Environment/LinkinParkTextLogoR", highway);
            FindAddGameObject("Environment/TrackShadow", highway);
            FindAddGameObject("Environment/LinkinParkSoldier", highway);

            // BTS
            FindAddGameObject("Environment/Clouds", highway);

            // 360°
            FindAddGameObject("Environment/Collider", highway);
        }

        private static void FindBackColumns() {
            backColumns = new List<GameObject>();
            FindAddGameObject("Environment/BackColumns", backColumns);

            // 360°
            for (int i = 2; i < 24; i++) {
                FindAddGameObject($"Environment/GameObject ({i})", backColumns);
            }
        }

        private static void FindRotatingLasers() {
            rotatingLasers = new List<GameObject>();
            // Default, BigMirror, Triangle, Rocket
            FindAddGameObject("Environment/RotatingLasersPair", rotatingLasers);
            for (int i = 1; i < 19; i++) {
                FindAddGameObject($"Environment/RotatingLasersPair ({i})", rotatingLasers);
            }

            // Nice Env
            FindAddGameObject("Environment/RotatingLaserLeft0", rotatingLasers);
            FindAddGameObject("Environment/RotatingLaserLeft1", rotatingLasers);
            FindAddGameObject("Environment/RotatingLaserLeft2", rotatingLasers);
            FindAddGameObject("Environment/RotatingLaserLeft3", rotatingLasers);
            FindAddGameObject("Environment/RotatingLaserRight0", rotatingLasers);
            FindAddGameObject("Environment/RotatingLaserRight1", rotatingLasers);
            FindAddGameObject("Environment/RotatingLaserRight2", rotatingLasers);
            FindAddGameObject("Environment/RotatingLaserRight3", rotatingLasers);

            // 360°
            for (int i = 9; i < 19; i++) {
                FindAddGameObject($"Environment/LightPillar ({i})", rotatingLasers);
            }
        }

        private static void FindDoubleColorLasers() {
            doubleColorLasers = new List<GameObject>();

            // Default, BigMirror, Nice, 
            FindAddGameObject("Environment/DoubleColorLaser", doubleColorLasers);
            for (int i = 0; i < 9; i++) {
                FindAddGameObject($"Environment/DoubleColorLaser ({i+1})", doubleColorLasers);
            }

            // 360°
            for (int i = 4; i < 13; i++) {
                FindAddGameObject($"Environment/DownLaser ({i})", doubleColorLasers);
            }
        }

        private static void FindBackLasers() {
            backLasers = new List<GameObject>();
            FindAddGameObject("Environment/FrontLights", backLasers);

            // Panic
            FindAddRenameGameObject("Environment/Window", backLasers);
            FindAddRenameGameObject("Environment/Window", backLasers);

            // GreenDayGrenade
            FindAddGameObject("Environment/Logo", backLasers);

            // Timbaland
            FindAddGameObject("Environment/Light (4)", backLasers);
            FindAddGameObject("Environment/Light (5)", backLasers);
            FindAddGameObject("Environment/Light (6)", backLasers);
            FindAddGameObject("Environment/Light (7)", backLasers);

            // BTS
            for (int i = 0; i < 4; i++) {
                FindAddRenameGameObject("Environment/SideLaser", backLasers);
            }
            FindAddGameObject("Environment/GradientBackground", backLasers);
            FindAddGameObject("Environment/StarHemisphere", backLasers);

            // 360°
            //FindAddGameObject("Environment/SpawnRotationChevronManager", backLasers);
        }

        private static void FindTrackLights() {
            trackLights = new List<GameObject>();
            FindAddGameObject("Environment/GlowLineR", trackLights);
            FindAddGameObject("Environment/GlowLineL", trackLights);
            FindAddGameObject("Environment/GlowLineFarL", trackLights);
            FindAddGameObject("Environment/GlowLineFarR", trackLights);

            // Origins
            FindAddGameObject("Environment/SidePSL", trackLights);
            FindAddGameObject("Environment/SidePSR", trackLights);

            // KDA
            FindAddGameObject("GlowLineLVisible", trackLights);
            FindAddGameObject("GlowLineRVisible", trackLights);

            // KDA, Monstercat, CrabRave, GreenDayGrenade
            FindAddGameObject("Environment/Laser", trackLights);
            for (int i = 1; i < 20; i++) {
                FindAddGameObject($"Laser ({i})", trackLights);
            }
            FindAddGameObject("GlowTopLine", trackLights);
            for (int i = 1; i < 12; i++) {
                FindAddGameObject($"GlowTopLine ({i})", trackLights);
            }
            FindAddGameObject("Environment/GlowLineR (1)", trackLights);
            FindAddGameObject("Environment/GlowLineR (2)", trackLights);
            for (int i = 1; i < 25; i++) {
                FindAddGameObject($"Environment/GlowLineL ({i})", trackLights);
            }

            // Monstercat
            FindAddGameObject("GlowLineLHidden", trackLights);
            FindAddGameObject("GlowLineRHidden", trackLights);

            // Dragons, Panic
            FindAddGameObject("Environment/DragonsSidePSL", trackLights);
            FindAddGameObject("Environment/DragonsSidePSR", trackLights);
            for (int i = 0; i < 15; i++) {
                FindAddGameObject($"Environment/ConstructionGlowLine ({i+1})", trackLights);
            }

            // Rocket
            FindAddGameObject("Environment/RocketArenaLight", trackLights);
            FindAddGameObject("Environment/RocketGateLight", trackLights);
            FindAddGameObject("Environment/GateLight0", trackLights);
            FindAddGameObject("Environment/GateLight1", trackLights);
            FindAddGameObject("Environment/GateLight1 (4)", trackLights);
            FindAddGameObject("Environment/EnvLight0", trackLights);
            for (int i = 2; i < 10; i++) {
                FindAddGameObject($"Environment/EnvLight0 ({i})", trackLights);
            }

            // LinkinPark
            for (int i = 1; i < 13; i++) {
                FindAddGameObject($"Environment/LampShadow ({i})", trackLights);
            }
            for (int i = 2; i < 22; i++) {
                FindAddGameObject($"Environment/LaserTop ({i})", trackLights);
            }
            FindAddGameObject("Environment/TunnelRotatingLasersPair", trackLights);
            for (int i = 1; i < 18; i++) {
                FindAddGameObject($"Environment/TunnelRotatingLasersPair ({i})", trackLights);
            }
            FindAddGameObject("Environment/LaserFloor", trackLights);
            FindAddGameObject("Environment/LaserFloor (1)", trackLights);
            FindAddGameObject("Environment/LaserFloor (2)", trackLights);
            FindAddGameObject("Environment/LaserFloor (3)", trackLights);
            FindAddGameObject("Environment/LaserL", trackLights);
            FindAddGameObject("Environment/LarerR", trackLights); // this is not my typo
            FindAddGameObject("Environment/LaserL (2)", trackLights);
            FindAddGameObject("Environment/LarerR (2)", trackLights);
            FindAddGameObject("Environment/FloorLightShadowL", trackLights);
            FindAddGameObject("Environment/FloorLightShadowR", trackLights);
            FindAddGameObject("Environment/ArchShadow", trackLights);
            FindAddGameObject("Environment/ArchShadow (1)", trackLights);

            // BTS
            FindAddGameObject("Environment/GlowLineC", trackLights);
            FindAddGameObject("Environment/BottomGlow", trackLights);
            FindAddGameObject("Environment/LaserR", trackLights);

            // 360°
            FindAddGameObject("Environment/TopLaser", trackLights);
            for (int i = 1; i < 6; i++) {
                FindAddGameObject($"Environment/TopLaser ({i})", trackLights);
            }
            FindAddGameObject("Environment/Cube", trackLights);
            FindAddGameObject("Environment/Cube (82)", trackLights);
            for (int i = 82; i < 90; i++) {
                FindAddGameObject($"Environment/Cube ({i})", trackLights);
            }
        }
    }
}