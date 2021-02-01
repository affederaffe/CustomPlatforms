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
        internal EventManager _eventManager;


        /// <summary>
        /// Registers to lighting events<br/>
        /// [Unity calls this when the <see cref="MonoBehaviour"/> becomes active in the hierachy]
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnEnable() {
            SubscribeToEvents();
            _eventManager.OnLevelStart.Invoke();
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
                _events.GameSceneLoadedEvent += delegate { _eventManager.OnLevelStart.Invoke(); };
                _events.NoteWasCutEvent += delegate { _eventManager.OnSlice.Invoke(); };
                _events.NoteWasMissedEvent += delegate { _eventManager.OnMiss.Invoke(); };
                _events.ComboDidBreakEvent += delegate { _eventManager.OnComboBreak.Invoke(); };
                _events.MultiplierDidIncreaseEvent += delegate { _eventManager.MultiplierUp.Invoke(); };
                _events.ComboDidChangeEvent += delegate (int combo) { _eventManager.OnComboChanged.Invoke(combo); };
                _events.SabersStartCollideEvent += delegate { _eventManager.SaberStartColliding.Invoke(); };
                _events.SabersEndCollideEvent += delegate { _eventManager.SaberStopColliding.Invoke(); };
                _events.LevelFinishedEvent += delegate { _eventManager.OnLevelFinish.Invoke(); };
                _events.LevelFailedEvent += delegate { _eventManager.OnLevelFail.Invoke(); };
                _events.BeatmapEventDidTriggerEvent += LightEventCallBack;
            }
        }


        /// <summary>
        /// Unsubscribes platform specific Actions from game Events
        /// </summary>
        private void UnsubscribeFromEvents() {
            if (_events != null) {
                _events.GameSceneLoadedEvent -= delegate { _eventManager.OnLevelStart.Invoke(); };
                _events.NoteWasCutEvent -= delegate { _eventManager.OnSlice.Invoke(); };
                _events.NoteWasMissedEvent -= delegate { _eventManager.OnMiss.Invoke(); };
                _events.ComboDidBreakEvent -= delegate { _eventManager.OnComboBreak.Invoke(); };
                _events.MultiplierDidIncreaseEvent -= delegate { _eventManager.MultiplierUp.Invoke(); };
                _events.ComboDidChangeEvent -= delegate (int combo) { _eventManager.OnComboChanged.Invoke(combo); };
                _events.SabersStartCollideEvent -= delegate { _eventManager.SaberStartColliding.Invoke(); };
                _events.SabersEndCollideEvent -= delegate { _eventManager.SaberStopColliding.Invoke(); };
                _events.LevelFinishedEvent -= delegate { _eventManager.OnLevelFinish.Invoke(); };
                _events.LevelFailedEvent -= delegate { _eventManager.OnLevelFail.Invoke(); };
                _events.BeatmapEventDidTriggerEvent -= LightEventCallBack;
            }
        }


        /// <summary>
        /// Triggers subscribed functions if lights are turned on.
        /// </summary>
        private void LightEventCallBack(BeatmapEventData songEvent) {
            if ((int)songEvent.type < 5) {
                if (songEvent.value > 0 && songEvent.value < 4) {
                    _eventManager.OnBlueLightOn.Invoke();
                }
                if (songEvent.value > 4 && songEvent.value < 8) {
                    _eventManager.OnRedLightOn.Invoke();
                }
            }
        }
    }
}