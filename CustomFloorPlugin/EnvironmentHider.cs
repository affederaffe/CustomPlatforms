using CustomFloorPlugin.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
            //MultiplayerFinder("Construction/PlayersPlace", playersPlace);
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
            foreach (var trackLaneRing in Resources.FindObjectsOfTypeAll<TrackLaneRing>().Where(x => x.name == "TrackLaneRing(Clone)" || x.name == "TriangleTrackLaneRing(Clone)")) {
                smallRings.Add(trackLaneRing.gameObject);
            }
            FindAddGameObject("TriangleTrackLaneRings", smallRings); // Triangle Rings from TriangleEnvironment
            // KDA
            FindAddGameObject("Environment/TentacleLeft", smallRings);
            FindAddGameObject("Environment/TentacleRight", smallRings);
        }

        private static void FindBigRings() {
            bigRings = new List<GameObject>();
            FindAddGameObject("BigTrackLaneRings", bigRings);
            foreach (var trackLaneRing in Resources.FindObjectsOfTypeAll<TrackLaneRing>().Where(x => x.name == "BigTrackLaneRing(Clone)")) {
                bigRings.Add(trackLaneRing.gameObject);
            }
        }

        private static void FindVisualizers() {
            visualizer = new List<GameObject>();
            FindAddGameObject("Environment/Spectrograms", visualizer);
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

            // KDA
            FindAddGameObject("FloorL", towers);
            FindAddGameObject("FloorR", towers);
            if (FindAddGameObject($"GlowLine", towers)) {
                for (int i = 0; i < 100; i++) {
                    FindAddGameObject($"GlowLine ({i})", towers);
                }
            }
        }

        private static void FindHighway() {
            highway = new List<GameObject>();
            FindAddGameObject("Environment/TrackConstruction", highway);
            FindAddGameObject("Environment/FloorConstruction", highway);
            FindAddGameObject("Environment/TrackMirror", highway);
            FindAddGameObject("Environment/Floor", highway);
            FindAddGameObject("Environment/Mirror", highway);
            FindAddGameObject("Environment/RocketCar", highway);
            FindAddGameObject("Environment/RocketCar (1)", highway);

            // KDA
            FindAddGameObject("Environment/Construction", highway);

            //Multiplayer
            //MultiplayerFinder("Construction", highway);
        }

        private static void FindBackColumns() {
            backColumns = new List<GameObject>();
            FindAddGameObject("Environment/BackColumns", backColumns);
        }

        private static void FindRotatingLasers() {
            rotatingLasers = new List<GameObject>();
            // Default, BigMirror, Triangle
            FindAddGameObject("Environment/RotatingLasersPair (3)", rotatingLasers);
            FindAddGameObject("Environment/RotatingLasersPair (4)", rotatingLasers);
            FindAddGameObject("Environment/RotatingLasersPair (5)", rotatingLasers);
            FindAddGameObject("Environment/RotatingLasersPair (6)", rotatingLasers);

            // Nice Env
            FindAddGameObject("Environment/RotatingLasersLeft0", rotatingLasers);
            FindAddGameObject("Environment/RotatingLasersLeft1", rotatingLasers);
            FindAddGameObject("Environment/RotatingLasersLeft2", rotatingLasers);
            FindAddGameObject("Environment/RotatingLasersLeft3", rotatingLasers);
            FindAddGameObject("Environment/RotatingLasersRight0", rotatingLasers);
            FindAddGameObject("Environment/RotatingLasersRight1", rotatingLasers);
            FindAddGameObject("Environment/RotatingLasersRight2", rotatingLasers);
            FindAddGameObject("Environment/RotatingLasersRight3", rotatingLasers);
        }

        private static void FindDoubleColorLasers() {
            doubleColorLasers = new List<GameObject>();

            // Default, BigMirror, Nice, 
            FindAddGameObject("Environment/DoubleColorLaser", doubleColorLasers);
            FindAddGameObject("Environment/DoubleColorLaser (1)", doubleColorLasers);
            FindAddGameObject("Environment/DoubleColorLaser (2)", doubleColorLasers);
            FindAddGameObject("Environment/DoubleColorLaser (3)", doubleColorLasers);
            FindAddGameObject("Environment/DoubleColorLaser (4)", doubleColorLasers);
            FindAddGameObject("Environment/DoubleColorLaser (5)", doubleColorLasers);
            FindAddGameObject("Environment/DoubleColorLaser (6)", doubleColorLasers);
            FindAddGameObject("Environment/DoubleColorLaser (7)", doubleColorLasers);
            FindAddGameObject("Environment/DoubleColorLaser (8)", doubleColorLasers);
            FindAddGameObject("Environment/DoubleColorLaser (9)", doubleColorLasers);
        }

        private static void FindBackLasers() {
            backLasers = new List<GameObject>();
            FindAddGameObject("Environment/FrontLights", backLasers);
            if (Settings.UseInMultiplayer) {
                //MultiplayerFinder("Lasers/LaserBackL", backLasers);
                //MultiplayerFinder("Lasers/LaserBackR", backLasers);
            }
        }

        private static void FindTrackLights() {
            trackLights = new List<GameObject>();
            FindAddGameObject("Environment/GlowLineR", trackLights);
            FindAddGameObject("Environment/GlowLineL", trackLights);
            FindAddGameObject("Environment/GlowLineFarL", trackLights);
            FindAddGameObject("Environment/GlowLineFarR", trackLights);

            // KDA
            FindAddGameObject("GlowLineLVisible", trackLights);
            FindAddGameObject("GlowLineRVisible", trackLights);

            // KDA, Monstercat
            FindAddGameObject("Laser", trackLights);
            for (int i = 0; i < 15; i++) {
                FindAddGameObject($"Laser ({i})", trackLights);
            }
            FindAddGameObject("GlowTopLine", trackLights);
            for (int i = 0; i < 10; i++) {
                FindAddGameObject($"GlowTopLine ({i})", trackLights);
            }

            // Monstercat
            FindAddGameObject("GlowLineLHidden", trackLights);
            FindAddGameObject("GlowLineRHidden", trackLights);

            //Multiplayer
            //MultiplayerFinder("Lasers/LaserFrontL", trackLights);
            //MultiplayerFinder("Lasers/LaserFrontR", trackLights);
            //MultiplayerFinder("Lasers/LaserL", trackLights);
            //MultiplayerFinder("Lasers/LaserR", trackLights);
            //MultiplayerFinder("Lasers/LaserFarL", trackLights);
            //MultiplayerFinder("Lasers/LaserFarR", trackLights);
            //MultiplayerFinder("DirectionalLights", trackLights);
        }
    }
}