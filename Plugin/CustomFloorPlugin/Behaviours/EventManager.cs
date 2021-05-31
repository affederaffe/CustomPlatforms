using System;

using CustomFloorPlugin.Interfaces;

using UnityEngine;
using UnityEngine.Events;

using Zenject;


namespace CustomFloorPlugin
{
    public class EventManager : MonoBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
    {
        [Serializable] public class ComboChangedEvent : UnityEvent<int> { }
        [Serializable] public class OnSpecificSliceEvent : UnityEvent<int> { }
        [Serializable] public class OnScoreChangedEvent : UnityEvent<int, int> { }
        [Serializable] public class OnGoodCutCountChangedEvent : UnityEvent<int> { }
        [Serializable] public class OnBadCutCountChangedEvent : UnityEvent<int> { }
        [Serializable] public class OnMissCountChangedEvent : UnityEvent<int> { }
        [Serializable] public class OnAllNotesCountChangedEvent : UnityEvent<int, int> { }

        // ReSharper disable InconsistentNaming
        public UnityEvent? OnSlice;
        public UnityEvent? OnMiss;
        public UnityEvent? OnComboBreak;
        public UnityEvent? MultiplierUp;
        public UnityEvent? SaberStartColliding;
        public UnityEvent? SaberStopColliding;
        public UnityEvent? OnLevelStart;
        public UnityEvent? OnLevelFail;
        public UnityEvent? OnLevelFinish;
        public UnityEvent? OnBlueLightOn;
        public UnityEvent? OnRedLightOn;
        public UnityEvent? OnNewHighscore;
        public ComboChangedEvent OnComboChanged = new();
        public OnSpecificSliceEvent OnSpecificSlice = new();
        public OnScoreChangedEvent OnScoreChanged = new();
        public OnGoodCutCountChangedEvent OnGoodCutCountChanged = new();
        public OnBadCutCountChangedEvent OnBadCutCountChanged = new();
        public OnMissCountChangedEvent OnMissCountChanged = new();
        public OnAllNotesCountChangedEvent OnAllNotesCountChanged = new();
        // ReSharper restore InconsistentNaming

        private BSEvents? _events;

        [Inject]
        public void Construct([InjectOptional] BSEvents events)
        {
            _events = events;
        }

        public void PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            if (_events is null) return;
            _events.BeatmapEventDidTriggerEvent += LightEventCallBack;
            _events.GameSceneLoadedEvent += OnLevelStart!.Invoke;
            _events.NoteWasCutEvent += OnSimpleSlice;
            _events.NoteWasCutEvent += OnSpecificSlice.Invoke;
            _events.NoteWasMissedEvent += OnMiss!.Invoke;
            _events.ComboDidBreakEvent += OnComboBreak!.Invoke;
            _events.MultiplierDidIncreaseEvent += MultiplierUp!.Invoke;
            _events.ComboDidChangeEvent += OnComboChanged.Invoke;
            _events.SabersStartCollideEvent += SaberStartColliding!.Invoke;
            _events.SabersEndCollideEvent += SaberStopColliding!.Invoke;
            _events.LevelFinishedEvent += OnLevelFinish!.Invoke;
            _events.LevelFailedEvent += OnLevelFail!.Invoke;
            _events.NewHighscore += OnNewHighscore!.Invoke;
            _events.ScoreDidChangeEvent += OnScoreChanged.Invoke;
            _events.GoodCutCountDidChangeEvent += OnGoodCutCountChanged.Invoke;
            _events.BadCutCountDidChangeEvent += OnBadCutCountChanged.Invoke;
            _events.MissCountDidChangeEvent += OnMissCountChanged.Invoke;
            _events.AllNotesCountDidChangeEvent += OnAllNotesCountChanged.Invoke;
        }

        public void PlatformDisabled()
        {
            if (_events is null) return;
            _events.BeatmapEventDidTriggerEvent -= LightEventCallBack;
            _events.GameSceneLoadedEvent -= OnLevelStart!.Invoke;
            _events.NoteWasCutEvent -= OnSimpleSlice;
            _events.NoteWasCutEvent -= OnSpecificSlice.Invoke;
            _events.NoteWasMissedEvent -= OnMiss!.Invoke;
            _events.ComboDidBreakEvent -= OnComboBreak!.Invoke;
            _events.MultiplierDidIncreaseEvent -= MultiplierUp!.Invoke;
            _events.ComboDidChangeEvent -= OnComboChanged.Invoke;
            _events.SabersStartCollideEvent -= SaberStartColliding!.Invoke;
            _events.SabersEndCollideEvent -= SaberStopColliding!.Invoke;
            _events.LevelFinishedEvent -= OnLevelFinish!.Invoke;
            _events.LevelFailedEvent -= OnLevelFail!.Invoke;
            _events.NewHighscore -= OnNewHighscore!.Invoke;
            _events.ScoreDidChangeEvent -= OnScoreChanged.Invoke;
            _events.GoodCutCountDidChangeEvent -= OnGoodCutCountChanged.Invoke;
            _events.BadCutCountDidChangeEvent -= OnBadCutCountChanged.Invoke;
            _events.MissCountDidChangeEvent -= OnMissCountChanged.Invoke;
            _events.AllNotesCountDidChangeEvent -= OnAllNotesCountChanged.Invoke;
        }

        /// <summary>
        /// Triggers subscribed functions when any block was cut
        /// </summary>
        private void OnSimpleSlice(int _)
        {
            OnSlice!.Invoke();
        }

        /// <summary>
        /// Triggers subscribed functions if lights are turned on
        /// </summary>
        private void LightEventCallBack(BeatmapEventData songEvent)
        {
            if ((int)songEvent.type >= 5) return;
            switch (songEvent.value)
            {
                case > 0 and < 4:
                    OnBlueLightOn!.Invoke();
                    break;
                case > 4 and < 8:
                    OnRedLightOn!.Invoke();
                    break;
            }
        }
    }
}