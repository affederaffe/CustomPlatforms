using System.Collections.Generic;
using System.Linq;

using BS_Utils.Utilities;

using static CustomFloorPlugin.GlobalCollection;


namespace CustomFloorPlugin {


    /// <summary>
    /// Used to override what Environment is loaded underneath the selected <see cref="CustomPlatform"/><br/>
    /// Ironically this behaviour is only available to players, via ModSettings, but not to platform creators... even though it changes the platform drastically<br/>
    /// Consider this to be legacy behaviour, documentation partialy omited
    /// </summary>
    internal static partial class EnvironmentSceneOverrider {

        private static readonly EnvironmentInfoSO[] allSceneInfos = GetAllEnvs();
        internal static readonly Dictionary<EnvOverrideMode, EnvironmentInfoSO> supportedEnvironmentInfos = new Dictionary<EnvOverrideMode, EnvironmentInfoSO>() {
            {EnvOverrideMode.Song, null},
            {EnvOverrideMode.Origins, EnvWithName("Origins")},
            {EnvOverrideMode.Nice, EnvWithName("Nice")},
            {EnvOverrideMode.BigMirror, EnvWithName("BigMirror")},
            {EnvOverrideMode.Triangle, EnvWithName("Triangle")},
            {EnvOverrideMode.KDA, EnvWithName("KDA")},
            {EnvOverrideMode.Monstercat, EnvWithName("Monstercat")},
            {EnvOverrideMode.Dragons, EnvWithName("Dragons")},
            {EnvOverrideMode.CrabRave, EnvWithName("CrabRave")},
            {EnvOverrideMode.Panic, EnvWithName("Panic")},
            {EnvOverrideMode.Rocket, EnvWithName("Rocket")},
            {EnvOverrideMode.GreenDayGrenade, EnvWithName("GreenDayGrenade")},
            {EnvOverrideMode.GreenDay, EnvWithName("GreenDay")},
            {EnvOverrideMode.Timbaland, EnvWithName("Timbaland")},
            {EnvOverrideMode.FitBeat, EnvWithName("FitBeat")},
            {EnvOverrideMode.LinkinPark, EnvWithName("LinkinPark")},
            {EnvOverrideMode.BTS, EnvWithName("BTS")},
            {EnvOverrideMode.GlassDesert, EnvWithName("GlassDesert")}
        };

        /// <summary>
        /// Gathers all <see cref="EnvironmentInfoSO"/>s in Beat Saber
        /// </summary>
        private static EnvironmentInfoSO[] GetAllEnvs() {
            EnvironmentInfoSO[] environmentInfos = PDM.GetField< PlayerDataFileManagerSO>("_playerDataFileManager").GetField<EnvironmentsListSO>("_allEnvironmentInfos").environmentInfos.Where(x =>
                   !(
                       x.name.Contains("Menu")
                       ||
                       x.name.Contains("Tutorial")
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