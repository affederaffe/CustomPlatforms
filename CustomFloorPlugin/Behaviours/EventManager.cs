using System;

using UnityEngine;
using UnityEngine.Events;

using Zenject;


namespace CustomFloorPlugin
{
    public class EventManager : MonoBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
    {
        [Serializable]
        public class ComboChangedEvent : UnityEvent<int> { }
        [Serializable]
        public class OnSpecificSliceEvent : UnityEvent<int> { }
        [Serializable]
        public class OnScoreChangedEvent : UnityEvent<int, int> { }
        [Serializable]
        public class OnGoodCutCountChangedEvent : UnityEvent<int> { }
        [Serializable]
        public class OnBadCutCountChangedEvent : UnityEvent<int> { }
        [Serializable]
        public class OnMissCountChangedEvent : UnityEvent<int> { }
        [Serializable]
        public class OnAllNotesCountChangedEvent : UnityEvent<int, int> { }

        public UnityEvent OnSlice;
        public UnityEvent OnMiss;
        public UnityEvent OnComboBreak;
        public UnityEvent MultiplierUp;
        public UnityEvent SaberStartColliding;
        public UnityEvent SaberStopColliding;
        public UnityEvent OnLevelStart;
        public UnityEvent OnLevelFail;
        public UnityEvent OnLevelFinish;
        public UnityEvent OnBlueLightOn;
        public UnityEvent OnRedLightOn;
        public UnityEvent OnNewHighscore;
        public ComboChangedEvent OnComboChanged = new();
        public OnSpecificSliceEvent OnSpecificSlice = new();
        public OnScoreChangedEvent OnScoreChanged = new();
        public OnGoodCutCountChangedEvent OnGoodCutCountChanged = new();
        public OnBadCutCountChangedEvent OnBadCutCountChanged = new();
        public OnMissCountChangedEvent OnMissCountChanged = new();
        public OnAllNotesCountChangedEvent OnAllNotesCountChanged = new();

        private BSEvents _events;

        [Inject]
        public void Construct([InjectOptional] BSEvents events)
        {
            _events = events;
        }

        void INotifyPlatformEnabled.PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            SubscribeToEvents();
        }

        void INotifyPlatformDisabled.PlatformDisabled()
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// Subscribes platform specific Actions from game Events
        /// </summary>
        private void SubscribeToEvents()
        {
            if (_events != null)
            {
                _events.BeatmapEventDidTriggerEvent += LightEventCallBack;
                _events.GameSceneLoadedEvent += OnLevelStart.Invoke;
                _events.NoteWasCutEvent += (_) => OnSlice.Invoke();
                _events.NoteWasCutEvent += OnSpecificSlice.Invoke;
                _events.NoteWasMissedEvent += OnMiss.Invoke;
                _events.ComboDidBreakEvent += OnComboBreak.Invoke;
                _events.MultiplierDidIncreaseEvent += MultiplierUp.Invoke;
                _events.ComboDidChangeEvent += OnComboChanged.Invoke;
                _events.SabersStartCollideEvent += SaberStartColliding.Invoke;
                _events.SabersEndCollideEvent += SaberStopColliding.Invoke;
                _events.LevelFinishedEvent += OnLevelFinish.Invoke;
                _events.LevelFailedEvent += OnLevelFail.Invoke;
                _events.NewHighscore += OnNewHighscore.Invoke;
                _events.ScoreDidChangeEvent += OnScoreChanged.Invoke;
                _events.GoodCutCountDidChangeEvent += OnGoodCutCountChanged.Invoke;
                _events.BadCutCountDidChangeEvent += OnBadCutCountChanged.Invoke;
                _events.MissCountDidChangeEvent += OnMissCountChanged.Invoke;
                _events.AllNotesCountDidChangeEvent += OnAllNotesCountChanged.Invoke;
            }
        }

        /// <summary>
        /// Unsubscribes platform specific Actions to game Events
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (_events != null)
            {
                _events.BeatmapEventDidTriggerEvent -= LightEventCallBack;
                _events.GameSceneLoadedEvent -= OnLevelStart.Invoke;
                _events.NoteWasCutEvent -= (_) => OnSlice.Invoke();
                _events.NoteWasCutEvent -= OnSpecificSlice.Invoke;
                _events.NoteWasMissedEvent -= OnMiss.Invoke;
                _events.ComboDidBreakEvent -= OnComboBreak.Invoke;
                _events.MultiplierDidIncreaseEvent -= MultiplierUp.Invoke;
                _events.ComboDidChangeEvent -= OnComboChanged.Invoke;
                _events.SabersStartCollideEvent -= SaberStartColliding.Invoke;
                _events.SabersEndCollideEvent -= SaberStopColliding.Invoke;
                _events.LevelFinishedEvent -= OnLevelFinish.Invoke;
                _events.LevelFailedEvent -= OnLevelFail.Invoke;
                _events.NewHighscore -= OnNewHighscore.Invoke;
                _events.ScoreDidChangeEvent -= OnScoreChanged.Invoke;
                _events.GoodCutCountDidChangeEvent -= OnGoodCutCountChanged.Invoke;
                _events.BadCutCountDidChangeEvent -= OnBadCutCountChanged.Invoke;
                _events.MissCountDidChangeEvent -= OnMissCountChanged.Invoke;
                _events.AllNotesCountDidChangeEvent -= OnAllNotesCountChanged.Invoke;
            }
        }

        /// <summary>
        /// Triggers subscribed functions if lights are turned on.
        /// </summary>
        private void LightEventCallBack(BeatmapEventData songEvent)
        {
            if ((int)songEvent.type < 5)
            {
                if (songEvent.value is > 0 and < 4)
                {
                    OnBlueLightOn.Invoke();
                }
                if (songEvent.value is > 4 and < 8)
                {
                    OnRedLightOn.Invoke();
                }
            }
        }
    }
}