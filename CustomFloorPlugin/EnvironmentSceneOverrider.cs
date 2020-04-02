using CustomFloorPlugin.UI;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;

using static CustomFloorPlugin.GlobalCollection;
using static CustomFloorPlugin.Utilities.Logging;


namespace CustomFloorPlugin {


    /// <summary>
    /// Used to override what Environment is loaded underneath the selected <see cref="CustomPlatform"/><br/>
    /// Ironically this behaviour is only available to players, via ModSettings, but not to platform creators... even though it changes the platform drastically<br/>
    /// Consider this to be legacy behaviour, documentation partialy omited
    /// </summary>
    internal static partial class EnvironmentSceneOverrider {

        private static readonly List<SceneInfoWithBackup> allSceneInfos = GetAllEnvs();
        private static readonly Dictionary<EnvOverrideMode, SceneInfoWithBackup> supportedSceneInfos = new Dictionary<EnvOverrideMode, SceneInfoWithBackup>() {
            {EnvOverrideMode.Default, EnvWithName("Default")},
            {EnvOverrideMode.Nice, EnvWithName("Nice")},
            {EnvOverrideMode.BigMirror, EnvWithName("BigMirror")},
            {EnvOverrideMode.Triangle, EnvWithName("Triangle")},
            {EnvOverrideMode.KDA, EnvWithName("KDA")},
            {EnvOverrideMode.Monstercat, EnvWithName("Monstercat")}
        };


        /// <summary>
        /// Initializes the <see cref="EnvironmentSceneOverrider"/>
        /// </summary>
        internal static void Init() {
            Settings.EnvOrChanged -= OverrideEnvironment;
            Settings.EnvOrChanged += OverrideEnvironment;
            SetEnabled(!Settings.PlayerData.overrideEnvironmentSettings.overrideEnvironments);
        }


        /// <summary>
        /// Enables or disables the override.
        /// </summary>
        /// <param name="enable">Whether to enable or disable the override</param>
        internal static void SetEnabled(bool enable) {
            if(enable && PlatformManager.CurrentPlatformIndex != 0) {
                OverrideEnvironment(Settings.EnvOr);
            } else {
                Revert();
            }
        }


        /// <summary>
        /// Overrides the environment <see cref="Scene"/>, which the game attempts to load when transitioning into the game scene
        /// </summary>
        /// <param name="mode">The environment to load when transitioning into play mode</param>
        internal static void OverrideEnvironment(EnvOverrideMode mode) {
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


        /// <summary>
        /// Reverts all overridden <see cref="SceneInfo"/>s to their original state
        /// </summary>
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


        /// <summary>
        /// Gathers all <see cref="SceneInfo"/>s in Beat Saber, then creates backups for them
        /// </summary>
        private static List<SceneInfoWithBackup> GetAllEnvs() {
            List<SceneInfo> rawList =
                Resources.FindObjectsOfTypeAll<SceneInfo>().Where(x =>
                   x.name.EndsWith("EnvironmentSceneInfo", STR_INV)
                   &&
                   !(
                       x.name.StartsWith("Menu", STR_INV)
                       ||
                       x.name.StartsWith("Tutorial", STR_INV)
                   )
                ).ToList()
            ;
            List<SceneInfoWithBackup> list = new List<SceneInfoWithBackup>();
            foreach(SceneInfo sceneInfo in rawList) {
                list.Add(new SceneInfoWithBackup(sceneInfo));
            }
            return list;
        }


        /// <summary>
        /// Identifies a <see cref="SceneInfoWithBackup"/> in <see cref="allSceneInfos"/>
        /// </summary>
        /// <param name="name">The original SceneName of the wrapped <see cref="SceneInfo"/></param>
        private static SceneInfoWithBackup EnvWithName(string name) {
            return allSceneInfos.First(x => x.BackupName.StartsWith(name, STR_INV));
        }
    }
}