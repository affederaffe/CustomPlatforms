using BS_Utils.Utilities;

using CustomFloorPlugin.Exceptions;
using System;
using System.Globalization;
using System.Linq;

using UnityEngine.SceneManagement;

using static CustomFloorPlugin.Utilities.Logging;
using static CustomFloorPlugin.Utilities.UnityObjectSearching;


namespace CustomFloorPlugin {


    /// <summary>
    /// The <see cref="GlobalCollection"/> class holds global items that are logically not assignable to any one class, or do not belong to this assembly
    /// </summary>
    internal static class GlobalCollection {


        /// <summary>
        /// Config file/object for this Plugin
        /// </summary>
        internal static Config CONFIG = new Config("Custom Platforms");



        /// <summary>
        /// Beat Sabers <see cref="GameScenesManager"/>, available after loading.
        /// </summary>
        internal static GameScenesManager GSM {
            get {
                if (_GSM == null) {
                    _GSM = SceneManager.GetSceneByName("PCInit").GetRootGameObjects().First(x => x.name == "AppCoreSceneContext")?.GetComponent<MarkSceneAsPersistent>().GetPrivateField<GameScenesManager>("_gameScenesManager");
                }
                return _GSM;
            }
        }
        private static GameScenesManager _GSM;


        /// <summary>
        /// Beat Sabers <see cref="BeatmapObjectCallbackController"/> for the current level, available after loading into a level.
        /// </summary>
        internal static BeatmapObjectCallbackController BOCC {
            get {
                if (_BOCC == null) {
                    try {
                        _BOCC = FindFirst<BeatmapObjectCallbackController>();
                    }
                    catch (ComponentNotFoundException e) {
                        Log("Tried Referencing BOCC out of context, returning null!");
                        Log(e);
                    }

                }
                return _BOCC;
            }
        }
        private static BeatmapObjectCallbackController _BOCC;


        /// <summary>
        /// The <see cref="Scene"/> used by this Beat Saber Plugin
        /// </summary>
        internal static Scene SCENE {
            get {
                if (_SCENE == null) {
                    _SCENE = SceneManager.CreateScene("CustomPlatforms", new CreateSceneParameters(LocalPhysicsMode.None));
                }
                return _SCENE.Value;
            }
        }
        private static Scene? _SCENE;


        /// <summary>
        /// Provides an invariant format provider, without having to create a variable for it in every file
        /// </summary>
        internal const StringComparison STR_INV = StringComparison.Ordinal;


        /// <summary>
        /// Provides an invariant IFormatProvider, without having to create a variable for it in every file
        /// </summary>
        internal static readonly NumberFormatInfo NUM_INV = NumberFormatInfo.InvariantInfo;


        /// <summary>
        /// The folder path used by CustomPlatforms
        /// </summary>
        internal const string FOLDER = "CustomPlatforms";


        /// <summary>
        /// The Scripts folder path used for custom scripts
        /// </summary>
        internal const string SCRIPT_FOLDER = "Scripts";


        internal const string SCRIPT_HASHES_FILENAME = "CustomScriptHashes.hashes";
    }
}