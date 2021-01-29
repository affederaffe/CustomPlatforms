using UnityEngine;

using Zenject;


namespace CustomFloorPlugin {


    /// <summary>
    /// Instantiable wrapper class for a single <see cref="EventManager"/> that handles registering and de-registering, as well as Light Event CallsBacks
    /// </summary>
    internal class PlatformEventManager : MonoBehaviour {

        [InjectOptional]
        private readonly BSEvents _events;


        /// <summary>
        /// Instance reference to a specific <see cref="CustomPlatform"/>s <see cref="EventManager"/>
        /// </summary>
        internal EventManager _EventManager;


        /// <summary>
        /// Registers to lighting events<br/>
        /// [Unity calls this when the <see cref="MonoBehaviour"/> becomes active in the hierachy]
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnEnable() {
            SubscribeToEvents();
            _EventManager.OnLevelStart.Invoke();
        }



        /// <summary>
        /// De-Registers from lighting events<br/>
        /// [Unity calls this when the <see cref="MonoBehaviour"/> becomes inactive in the hierachy]
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnDisable() {
            UnsubscribeFromEvents();
        }


        /// <summary>
        /// Subscribes platform specific Actions to game Events
        /// </summary>
        private void SubscribeToEvents() {
            if (_events != null) {
                _events.GameSceneLoaded += delegate (ScenesTransitionSetupDataSO setupData, DiContainer container) { _EventManager.OnLevelStart.Invoke(); };
                _events.NoteWasCutEvent += delegate (NoteController controller, NoteCutInfo info) { if (info.allIsOK) { _EventManager.OnSlice.Invoke(); } };
                _events.ComboDidBreak += delegate () { _EventManager.OnComboBreak.Invoke(); };
                _events.MultiplierDidIncrease += delegate (int multiplier) { _EventManager.MultiplierUp.Invoke(); };
                _events.ComboDidChange += delegate (int combo) { _EventManager.OnComboChanged.Invoke(combo); };
                _events.SabersStartCollide += delegate (SaberType saber) { _EventManager.SaberStartColliding.Invoke(); };
                _events.SabersEndCollide += delegate (SaberType saber) { _EventManager.SaberStopColliding.Invoke(); };
                _events.LevelFailed += delegate { _EventManager.OnLevelFail.Invoke(); };
                _events.BeatmapEvent += LightEventCallBack;
            }
        }


        /// <summary>
        /// Unsubscribes platform specific Actions from game Events
        /// </summary>
        private void UnsubscribeFromEvents() {
            if (_events != null) {
                _events.GameSceneLoaded -= delegate (ScenesTransitionSetupDataSO setupData, DiContainer container) { _EventManager.OnLevelStart.Invoke(); };
                _events.NoteWasCutEvent -= delegate (NoteController controller, NoteCutInfo info) { if (info.allIsOK) { _EventManager.OnSlice.Invoke(); } };
                _events.ComboDidBreak -= delegate () { _EventManager.OnComboBreak.Invoke(); };
                _events.MultiplierDidIncrease -= delegate (int multiplier) { _EventManager.MultiplierUp.Invoke(); };
                _events.ComboDidChange -= delegate (int combo) { _EventManager.OnComboChanged.Invoke(combo); };
                _events.SabersStartCollide -= delegate (SaberType saber) { _EventManager.SaberStartColliding.Invoke(); };
                _events.SabersEndCollide -= delegate (SaberType saber) { _EventManager.SaberStopColliding.Invoke(); };
                _events.LevelFailed -= delegate { _EventManager.OnLevelFail.Invoke(); };
                _events.BeatmapEvent -= LightEventCallBack;
            }
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