using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace CustomFloorPlugin {
    /// <summary> 
    /// Activates and deactivates world geometry in the active scene as required by CustomPlatforms
    /// </summary>
    public class EnvironmentHider {
        private List<GameObject> feet;
        private List<GameObject> originalPlatform;
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

        public static bool showFeetOverride = false;

        /// <summary>
        /// Hide and unhide world objects as required by a platform
        /// </summary>
        /// <param name="platform">A platform that defines which objects are to be hidden</param>
        public void HideObjectsForPlatform(CustomPlatform platform) {
            Plugin.Log("Finding Environment");
            FindEnvironment();
            Plugin.Log("Hiding Environment");
            if(feet != null) SetCollectionHidden(feet, (platform.hideDefaultPlatform && !showFeetOverride));
            if(originalPlatform != null) SetCollectionHidden(originalPlatform, platform.hideDefaultPlatform);
            if(smallRings != null) SetCollectionHidden(smallRings, platform.hideSmallRings);
            if(bigRings != null) SetCollectionHidden(bigRings, platform.hideBigRings);
            if(visualizer != null) SetCollectionHidden(visualizer, platform.hideEQVisualizer);
            if(towers != null) SetCollectionHidden(towers, platform.hideTowers);
            if(highway != null) SetCollectionHidden(highway, platform.hideHighway);
            if(backColumns != null) SetCollectionHidden(backColumns, platform.hideBackColumns);
            if(backLasers != null) SetCollectionHidden(backLasers, platform.hideBackLasers);
            if(doubleColorLasers != null) SetCollectionHidden(doubleColorLasers, platform.hideDoubleColorLasers);
            if(rotatingLasers != null) SetCollectionHidden(rotatingLasers, platform.hideRotatingLasers);
            if(trackLights != null) SetCollectionHidden(trackLights, platform.hideTrackLights);
            Plugin.Log("Done Hiding Environment");
        }

        /// <summary>
        /// Finds all GameObjects that make up the default environment
        /// and groups them into array lists
        /// </summary>
        int i;
        void FindEnvironment() {
            Plugin.Log("Starting Operation Hide and Seek");
            i = 0;
            FindFeetIcon();
            FindOriginalPlatform();
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
            Plugin.Log("Hunt concluded. " + i + " Victims found!");
        }

        /// <summary>
        /// Set the active state of a Collection of GameObjects
        /// </summary>
        /// <param name="arlist">An List<GameObject> of GameObjects</param>
        /// <param name="hidden">A boolean describing the desired hidden state</param>
        private void SetCollectionHidden(List<GameObject> list, bool hidden) {
            if(list == null) return;
            foreach(GameObject go in list) {
                if(go != null) go.SetActive(!hidden);
            }
        }

        /// <summary>
        /// Finds a GameObject by name and adds it to the provided List<GameObject>
        /// </summary>
        /// <param name="name">The name of the desired GameObject</param>
        /// <param name="alist">The List<GameObject> to be added to</param>
        private bool FindAddGameObject(string name, List<GameObject> list) {
            GameObject[] roots = PlatformManager.GetCurrentEnvironment().GetRootGameObjects();
            GameObject go = null;
            foreach(GameObject root in roots) {
                go = root.transform.Find(name)?.gameObject;
                if(go != null) {
                    list.Add(go);
                    i++;
                    return true;
                } else if(root.name == name) {
                    list.Add(root);
                }
            }
            return false;
        }

        private void FindFeetIcon() {
            feet = new List<GameObject>();
            FindAddGameObject("MenuPlayersPlace/Feet", feet);
            FindAddGameObject("PlayersPlace/Feet", feet);
            FindAddGameObject("Feet", feet);
            feet[0].transform.parent = null; // remove from original platform 
        }

        private void FindOriginalPlatform() {
            originalPlatform = new List<GameObject>();
            FindAddGameObject("PlayersPlace", originalPlatform);
            FindAddGameObject("MenuPlayersPlace", originalPlatform);
        }

        private void FindSmallRings() {
            smallRings = new List<GameObject>();
            FindAddGameObject("SmallTrackLaneRings", smallRings);
            foreach(TrackLaneRing trackLaneRing in Resources.FindObjectsOfTypeAll<TrackLaneRing>().Where(x => x.name == "TrackLaneRing(Clone)")) {
                smallRings.Add(trackLaneRing.gameObject);
            }
            FindAddGameObject("TriangleTrackLaneRings", smallRings); // Triangle Rings from TriangleEnvironment
            foreach(TrackLaneRing trackLaneRing in Resources.FindObjectsOfTypeAll<TrackLaneRing>().Where(x => x.name == "TriangleTrackLaneRing(Clone)")) {
                smallRings.Add(trackLaneRing.gameObject);
            }
            // KDA
            FindAddGameObject("TentacleLeft", smallRings);
            FindAddGameObject("TentacleRight", smallRings);
        }

        private void FindBigRings() {
            bigRings = new List<GameObject>();
            FindAddGameObject("BigTrackLaneRings", bigRings);
            foreach(var trackLaneRing in Resources.FindObjectsOfTypeAll<TrackLaneRing>().Where(x => x.name == "TrackLaneRingBig(Clone)")) {
                bigRings.Add(trackLaneRing.gameObject);
            }
        }

        private void FindVisualizers() {
            visualizer = new List<GameObject>();
            FindAddGameObject("Spectrograms", visualizer);
        }

        private void FindTowers() {
            towers = new List<GameObject>();
            // Song Environments
            FindAddGameObject("Buildings", towers);
            FindAddGameObject("NearBuildingRight (1)", towers);
            FindAddGameObject("NearBuildingLeft (1)", towers);
            FindAddGameObject("NearBuildingLeft", towers);
            FindAddGameObject("NearBuildingRight", towers);

            // Menu
            FindAddGameObject("NearBuildingRight (1)", towers);
            FindAddGameObject("NearBuildingLeft (1)", towers);
            FindAddGameObject("NearBuildingLeft", towers);
            FindAddGameObject("NearBuildingRight", towers);

            // Monstercat
            FindAddGameObject("MonstercatLogoL", towers);
            FindAddGameObject("MonstercatLogoR", towers);

            // KDA
            FindAddGameObject("FloorL", towers);
            FindAddGameObject("FloorR", towers);
            if(FindAddGameObject($"GlowLine", towers)) {
                for(int i = 0; i < 100; i++) {
                    FindAddGameObject($"GlowLine ({i})", towers);
                }
            }
        }

        private void FindHighway() {
            highway = new List<GameObject>();
            FindAddGameObject("Floor", highway);
            FindAddGameObject("FloorConstruction", highway);
            FindAddGameObject("Construction", highway);
            FindAddGameObject("TrackConstruction", highway);
            FindAddGameObject("TrackMirror", highway);

            FindAddGameObject($"Cube", highway);
            for(int i = 1; i <= 10; i++) {
                FindAddGameObject($"Cube ({i})", highway);
            }

            //Menu
            FindAddGameObject("LeftSmallBuilding", highway);
            FindAddGameObject("RightSmallBuilding", highway);
        }

        private void FindBackColumns() {
            backColumns = new List<GameObject>();
            FindAddGameObject("BackColumns", backColumns);
            FindAddGameObject("BackColumns (1)", backColumns);

            FindAddGameObject("BackColumns", backColumns);
            FindAddGameObject("CeilingLamp", backColumns);
        }

        private void FindRotatingLasers() {
            rotatingLasers = new List<GameObject>();
            // Default, BigMirror, Triangle
            FindAddGameObject("RotatingLasersPair (6)", rotatingLasers);
            FindAddGameObject("RotatingLasersPair (5)", rotatingLasers);
            FindAddGameObject("RotatingLasersPair (4)", rotatingLasers);
            FindAddGameObject("RotatingLasersPair (3)", rotatingLasers);
            FindAddGameObject("RotatingLasersPair (2)", rotatingLasers);
            FindAddGameObject("RotatingLasersPair (1)", rotatingLasers);
            FindAddGameObject("RotatingLasersPair", rotatingLasers);

            // Nice Env
            FindAddGameObject("RotatingLasersLeft0", rotatingLasers);
            FindAddGameObject("RotatingLasersLeft1", rotatingLasers);
            FindAddGameObject("RotatingLasersLeft2", rotatingLasers);
            FindAddGameObject("RotatingLasersLeft3", rotatingLasers);
            FindAddGameObject("RotatingLasersRight0", rotatingLasers);
            FindAddGameObject("RotatingLasersRight1", rotatingLasers);
            FindAddGameObject("RotatingLasersRight2", rotatingLasers);
            FindAddGameObject("RotatingLasersRight3", rotatingLasers);
        }

        private void FindDoubleColorLasers() {
            doubleColorLasers = new List<GameObject>();

            // Default, BigMirror, Nice, 
            FindAddGameObject("DoubleColorLaser", doubleColorLasers);
            FindAddGameObject("DoubleColorLaser (1)", doubleColorLasers);
            FindAddGameObject("DoubleColorLaser (2)", doubleColorLasers);
            FindAddGameObject("DoubleColorLaser (3)", doubleColorLasers);
            FindAddGameObject("DoubleColorLaser (4)", doubleColorLasers);
            FindAddGameObject("DoubleColorLaser (5)", doubleColorLasers);
            FindAddGameObject("DoubleColorLaser (6)", doubleColorLasers);
            FindAddGameObject("DoubleColorLaser (7)", doubleColorLasers);
            FindAddGameObject("DoubleColorLaser (8)", doubleColorLasers);
            FindAddGameObject("DoubleColorLaser (9)", doubleColorLasers);
        }

        private void FindBackLasers() {
            backLasers = new List<GameObject>();
            FindAddGameObject("FrontLights", backLasers);

        }

        private void FindTrackLights() {
            trackLights = new List<GameObject>();
            FindAddGameObject("GlowLineR", trackLights);
            FindAddGameObject("GlowLineL", trackLights);
            FindAddGameObject("GlowLineR2", trackLights);
            FindAddGameObject("GlowLineL2", trackLights);
            FindAddGameObject("GlowLineFarL", trackLights);
            FindAddGameObject("GlowLineFarR", trackLights);

            //KDA
            FindAddGameObject("GlowLineLVisible", trackLights);
            FindAddGameObject("GlowLineRVisible", trackLights);

            //KDA, Monstercat
            FindAddGameObject("Laser", trackLights);
            for(int i = 0; i < 15; i++) {
                FindAddGameObject($"Laser ({i})", trackLights);
            }
            FindAddGameObject("GlowTopLine", trackLights);
            for(int i = 0; i < 10; i++) {
                FindAddGameObject($"GlowTopLine ({i})", trackLights);
            }

            // Monstercat
            FindAddGameObject("GlowLineLHidden", trackLights);
            FindAddGameObject("GlowLineRHidden", trackLights);
        }
    }
}
