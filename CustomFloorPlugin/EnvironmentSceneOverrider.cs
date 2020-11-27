using CustomFloorPlugin.UI;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using static CustomFloorPlugin.GlobalCollection;


namespace CustomFloorPlugin {


    /// <summary>
    /// Used to override what Environment is loaded underneath the selected <see cref="CustomPlatform"/><br/>
    /// Ironically this behaviour is only available to players, via ModSettings, but not to platform creators... even though it changes the platform drastically<br/>
    /// Consider this to be legacy behaviour, documentation partialy omited
    /// </summary>
    internal static partial class EnvironmentSceneOverrider {

        private static readonly EnvironmentInfoSO[] allSceneInfos = GetAllEnvs();
        private static readonly EnvironmentTypeSO environmentType = Resources.FindObjectsOfTypeAll<EnvironmentTypeSO>()[0]; // NormalEnvironemntTypeSO
        private static readonly Dictionary<EnvOverrideMode, EnvironmentInfoSO> supportedEnvironmentInfos = new Dictionary<EnvOverrideMode, EnvironmentInfoSO>() {
            {EnvOverrideMode.Origins, EnvWithName("Origins")},
            {EnvOverrideMode.Nice, EnvWithName("Nice")},
            {EnvOverrideMode.BigMirror, EnvWithName("BigMirror")},
            {EnvOverrideMode.Triangle, EnvWithName("Triangle")},
            {EnvOverrideMode.KDA, EnvWithName("KDA")},
            {EnvOverrideMode.Monstercat, EnvWithName("Monstercat")}
        };
        private static EnvironmentInfoSO oldEnvironmentInfoSO;

        internal static bool didOverrideEnvironment;


        /// <summary>
        /// Enables the <see cref="OverrideEnvironmentSettings"/> and changes it to the selected <see cref="EnvOverrideMode"/>
        /// </summary>
        /// <param name="mode">The environment to load when transitioning into play mode</param>
        internal static void OverrideEnvironment(EnvOverrideMode mode) {
            Utilities.Logging.Log("Overridden Environment: " + supportedEnvironmentInfos[mode].environmentName);
            Settings.PlayerData.overrideEnvironmentSettings.overrideEnvironments = true;
            oldEnvironmentInfoSO = Settings.PlayerData.overrideEnvironmentSettings.GetOverrideEnvironmentInfoForType(environmentType);
            Settings.PlayerData.overrideEnvironmentSettings.SetEnvironmentInfoForType(environmentType, supportedEnvironmentInfos[mode]);
            didOverrideEnvironment = true;
        }


        /// <summary>
        /// Reverts the changes to the <see cref="OverrideEnvironmentSettings"/>
        /// </summary>
        internal static void Revert() {
            if (didOverrideEnvironment && oldEnvironmentInfoSO != null) {
                Utilities.Logging.Log("Resetted Environment Override Settings");
                Settings.PlayerData.overrideEnvironmentSettings.SetEnvironmentInfoForType(environmentType, oldEnvironmentInfoSO);
                Settings.PlayerData.overrideEnvironmentSettings.overrideEnvironments = false;
                didOverrideEnvironment = false;
            }
        }


        /// <summary>
        /// Gathers all <see cref="EnvironmentInfoSO"/>s in Beat Saber
        /// </summary>
        private static EnvironmentInfoSO[] GetAllEnvs() {
            EnvironmentInfoSO[] environmentInfos = Resources.FindObjectsOfTypeAll<EnvironmentInfoSO>().Where(x =>
                   !(
                       x.name.Contains("Menu")
                       ||
                       x.name.Contains("Tutorial")
                       ||
                       x.name.Contains("GlassDesert")
                       ||
                       x.name.Contains("Multiplayer")
                   )
                ).ToArray();
            return environmentInfos;
        }


        /// <summary>
        /// Identifies a <see cref="EnvironmentInfoSO"/> in <see cref="allSceneInfos"/>
        /// </summary>
        /// <param name="name">The original SceneName of the wrapped <see cref="SceneInfo"/></param>
        private static EnvironmentInfoSO EnvWithName(string name) {
            return allSceneInfos.First(x => x.name.StartsWith(name, STR_INV));
        }
    }
}