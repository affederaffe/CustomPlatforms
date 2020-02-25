using CustomFloorPlugin.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CustomFloorPlugin.Utilities.Logging;

namespace CustomFloorPlugin {
    internal static partial class EnvironmentSceneOverrider {

        private static readonly List<SceneInfoWithBackup> allSceneInfos = SceneInfoWithBackup.GetAllEnvs();
        private static readonly Dictionary<EnvOverrideMode, SceneInfoWithBackup> supportedSceneInfos = new Dictionary<EnvOverrideMode, SceneInfoWithBackup>() {
            {EnvOverrideMode.Default, EnvWithName("Default")},
            {EnvOverrideMode.Nice, EnvWithName("Nice")},
            {EnvOverrideMode.BigMirror, EnvWithName("BigMirror")},
            {EnvOverrideMode.Triangle, EnvWithName("Triangle")},
            {EnvOverrideMode.KDA, EnvWithName("KDA")},
            {EnvOverrideMode.Monstercat, EnvWithName("Monstercat")}
        };
        internal static void Load() {
            Settings.EnvOrChanged -= OverrideEnvironmentScene;
            Settings.EnvOrChanged += OverrideEnvironmentScene;
            OverrideEnvironmentScene();
        }
        
        internal static void OverrideEnvironmentScene() {
            OverrideEnvironmentScene(Settings.EnvOr);
        }
        internal static void OverrideEnvironmentScene(EnvOverrideMode mode) {
            Settings.EnvOr = mode;
            if(Settings.PlayerData.overrideEnvironmentSettings.overrideEnvironments) {
                Revert();
            } else {
                OverrideScenes(mode);
            }
        }
        private static void Revert() {
            for(int i = 0; i < allSceneInfos.Count; i++) {
                allSceneInfos[i].SceneName = allSceneInfos[i].BackupName;
            }
            Log("Logging names after revert");
            foreach(SceneInfoWithBackup info in allSceneInfos) {
                Log(info.SceneName);
                Log(info.BackupName);
            }
        }
        private static void OverrideScenes(EnvOverrideMode mode) {
            string sceneName = supportedSceneInfos[mode].SceneName;
            for(int i = 0; i < allSceneInfos.Count; i++) {
                allSceneInfos[i].SceneName = sceneName;
            }
            Log("Logging names after override");
            foreach(SceneInfoWithBackup info in allSceneInfos) {
                Log(info.SceneName);
                Log(info.BackupName);
            }
        }
    }
}
