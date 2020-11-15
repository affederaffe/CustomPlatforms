using BS_Utils.Utilities;

using UnityEngine;


namespace CustomFloorPlugin {


    /// <summary>
    /// Instantiable wrapper class for a single <see cref="EventManager"/> that handles registering and de-registering, as well as Light Event CallsBacks
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1812:Avoid unistantiated internal classes", Justification = "Instantiated by Unity")]
    internal class PlatformEventManager : MonoBehaviour {


        /// <summary>
        /// Instance reference to a specific <see cref="CustomPlatform"/>s <see cref="EventManager"/>
        /// </summary>
        internal EventManager _EventManager;



        /// <summary>
        /// Registers to lighting events<br/>
        /// [Unity calls this when the <see cref="MonoBehaviour"/> becomes active in the hierachy]
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnEnable() {
            SubscribeToEvents();
            _EventManager.OnLevelStart.Invoke();
        }


        /// <summary>
        /// De-Registers from lighting events<br/>
        /// [Unity calls this when the <see cref="MonoBehaviour"/> becomes inactive in the hierachy]
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnDisable() {
            UnsubscribeFromEvents();
        }


        /// <summary>
        /// Subscribes platform specific Actions to game Events
        /// </summary>
        private void SubscribeToEvents() {
            BSEvents.gameSceneLoaded += delegate () { _EventManager.OnLevelStart.Invoke(); };
            BSEvents.noteWasCut += delegate (NoteData data, NoteCutInfo info, int multiplier) { if (info.allIsOK) _EventManager.OnSlice.Invoke(); };
            BSEvents.comboDidBreak += delegate () { _EventManager.OnComboBreak.Invoke(); };
            BSEvents.multiplierDidIncrease += delegate (int multiplier) { _EventManager.MultiplierUp.Invoke(); };
            BSEvents.comboDidChange += delegate (int combo) { _EventManager.OnComboChanged.Invoke(combo); };
            BSEvents.sabersStartCollide += delegate (SaberType saber) { _EventManager.SaberStartColliding.Invoke(); };
            BSEvents.sabersEndCollide += delegate (SaberType saber) { _EventManager.SaberStopColliding.Invoke(); };
            BSEvents.levelFailed += delegate (StandardLevelScenesTransitionSetupDataSO transition, LevelCompletionResults results) { _EventManager.OnLevelFail.Invoke(); };
            BSEvents.beatmapEvent += LightEventCallBack;
        }


        /// <summary>
        /// Unsubscribes platform specific Actions from game Events
        /// </summary>
        private void UnsubscribeFromEvents() {
            BSEvents.gameSceneLoaded -= delegate () { _EventManager.OnLevelStart.Invoke(); };
            BSEvents.noteWasCut -= delegate (NoteData data, NoteCutInfo info, int multiplier) { if (info.allIsOK) _EventManager.OnSlice.Invoke(); };
            BSEvents.comboDidBreak -= delegate () { _EventManager.OnComboBreak.Invoke(); };
            BSEvents.multiplierDidIncrease -= delegate (int multiplier) { _EventManager.MultiplierUp.Invoke(); };
            BSEvents.comboDidChange -= delegate (int combo) { _EventManager.OnComboChanged.Invoke(combo); };
            BSEvents.sabersStartCollide -= delegate (SaberType saber) { _EventManager.SaberStartColliding.Invoke(); };
            BSEvents.sabersEndCollide -= delegate (SaberType saber) { _EventManager.SaberStopColliding.Invoke(); };
            BSEvents.levelFailed -= delegate (StandardLevelScenesTransitionSetupDataSO transition, LevelCompletionResults results) { _EventManager.OnLevelFail.Invoke(); };
            BSEvents.beatmapEvent -= LightEventCallBack;
        }


        /// <summary>
        /// Triggers subscribed functions if lights are turned on.
        /// </summary>
        private void LightEventCallBack(BeatmapEventData songEvent) {
            if ((int)songEvent.type < 5) {
                if (songEvent.value > 0 && songEvent.value < 4) {
                    _EventManager.OnBlueLightOn.Invoke();
                }
                if (songEvent.value > 4 && songEvent.value < 8) {
                    _EventManager.OnRedLightOn.Invoke();
                }
            }
        }
    }
}