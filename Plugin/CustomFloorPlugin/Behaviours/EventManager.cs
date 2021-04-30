using System;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

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

        [FormerlySerializedAs("OnSlice")] public UnityEvent? onSlice;
        [FormerlySerializedAs("OnMiss")] public UnityEvent? onMiss;
        [FormerlySerializedAs("OnComboBreak")] public UnityEvent? onComboBreak;
        [FormerlySerializedAs("MultiplierUp")] public UnityEvent? multiplierUp;
        [FormerlySerializedAs("SaberStartColliding")] public UnityEvent? saberStartColliding;
        [FormerlySerializedAs("SaberStopColliding")] public UnityEvent? saberStopColliding;
        [FormerlySerializedAs("OnLevelStart")] public UnityEvent? onLevelStart;
        [FormerlySerializedAs("OnLevelFail")] public UnityEvent? onLevelFail;
        [FormerlySerializedAs("OnLevelFinish")] public UnityEvent? onLevelFinish;
        [FormerlySerializedAs("OnBlueLightOn")] public UnityEvent? onBlueLightOn;
        [FormerlySerializedAs("OnRedLightOn")] public UnityEvent? onRedLightOn;
        [FormerlySerializedAs("OnNewHighscore")] public UnityEvent? onNewHighscore;
        [FormerlySerializedAs("OnComboChanged")] public ComboChangedEvent onComboChanged = new();
        [FormerlySerializedAs("OnSpecificSlice")] public OnSpecificSliceEvent onSpecificSlice = new();
        [FormerlySerializedAs("OnScoreChanged")] public OnScoreChangedEvent onScoreChanged = new();
        [FormerlySerializedAs("OnGoodCutCountChanged")] public OnGoodCutCountChangedEvent onGoodCutCountChanged = new();
        [FormerlySerializedAs("OnBadCutCountChanged")] public OnBadCutCountChangedEvent onBadCutCountChangedEvent = new();
        [FormerlySerializedAs("OnMissCountChanged")] public OnMissCountChangedEvent onMissCountChanged = new();
        [FormerlySerializedAs("OnAllNotesCountChanged")] public OnAllNotesCountChangedEvent onAllNotesCountChanged = new();

        private BSEvents? _events;

        [Inject]
        public void Construct([InjectOptional] BSEvents events)
        {
            _events = events;
        }

        public void PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            if (_events == null) return;
            _events.BeatmapEventDidTriggerEvent += LightEventCallBack;
            _events.GameSceneLoadedEvent += onLevelStart!.Invoke;
            _events.NoteWasCutEvent += OnSimpleSlice;
            _events.NoteWasCutEvent += onSpecificSlice.Invoke;
            _events.NoteWasMissedEvent += onMiss!.Invoke;
            _events.ComboDidBreakEvent += onComboBreak!.Invoke;
            _events.MultiplierDidIncreaseEvent += multiplierUp!.Invoke;
            _events.ComboDidChangeEvent += onComboChanged.Invoke;
            _events.SabersStartCollideEvent += saberStartColliding!.Invoke;
            _events.SabersEndCollideEvent += saberStopColliding!.Invoke;
            _events.LevelFinishedEvent += onLevelFinish!.Invoke;
            _events.LevelFailedEvent += onLevelFail!.Invoke;
            _events.NewHighscore += onNewHighscore!.Invoke;
            _events.ScoreDidChangeEvent += onScoreChanged.Invoke;
            _events.GoodCutCountDidChangeEvent += onGoodCutCountChanged.Invoke;
            _events.BadCutCountDidChangeEvent += onBadCutCountChangedEvent.Invoke;
            _events.MissCountDidChangeEvent += onMissCountChanged.Invoke;
            _events.AllNotesCountDidChangeEvent += onAllNotesCountChanged.Invoke;
        }

        public void PlatformDisabled()
        {
            if (_events == null) return;
            _events.BeatmapEventDidTriggerEvent -= LightEventCallBack;
            _events.GameSceneLoadedEvent -= onLevelStart!.Invoke;
            _events.NoteWasCutEvent -= OnSimpleSlice;
            _events.NoteWasCutEvent -= onSpecificSlice.Invoke;
            _events.NoteWasMissedEvent -= onMiss!.Invoke;
            _events.ComboDidBreakEvent -= onComboBreak!.Invoke;
            _events.MultiplierDidIncreaseEvent -= multiplierUp!.Invoke;
            _events.ComboDidChangeEvent -= onComboChanged.Invoke;
            _events.SabersStartCollideEvent -= saberStartColliding!.Invoke;
            _events.SabersEndCollideEvent -= saberStopColliding!.Invoke;
            _events.LevelFinishedEvent -= onLevelFinish!.Invoke;
            _events.LevelFailedEvent -= onLevelFail!.Invoke;
            _events.NewHighscore -= onNewHighscore!.Invoke;
            _events.ScoreDidChangeEvent -= onScoreChanged.Invoke;
            _events.GoodCutCountDidChangeEvent -= onGoodCutCountChanged.Invoke;
            _events.BadCutCountDidChangeEvent -= onBadCutCountChangedEvent.Invoke;
            _events.MissCountDidChangeEvent -= onMissCountChanged.Invoke;
            _events.AllNotesCountDidChangeEvent -= onAllNotesCountChanged.Invoke;
        }

        /// <summary>
        /// Triggers subscribed functions when any block was cut
        /// </summary>
        private void OnSimpleSlice(int _)
        {
            onSlice!.Invoke();
        }

        /// <summary>
        /// Triggers subscribed functions if lights are turned on
        /// </summary>
        private void LightEventCallBack(BeatmapEventData songEvent)
        {
            if ((int)songEvent.type < 5)
            {
                if (songEvent.value is > 0 and < 4)
                {
                    onBlueLightOn!.Invoke();
                }
                if (songEvent.value is > 4 and < 8)
                {
                    onRedLightOn!.Invoke();
                }
            }
        }
    }
}