using System;

using UnityEngine;
using UnityEngine.Events;

using Zenject;


namespace CustomFloorPlugin
{
    public class EventManager : MonoBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
    {
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
                _events.GameSceneLoadedEvent += delegate { OnLevelStart.Invoke(); };
                _events.NoteWasCutEvent += delegate { OnSlice.Invoke(); };
                _events.NoteWasCutEvent += delegate (int saberType) { OnSpecificSlice.Invoke(saberType); };
                _events.NoteWasMissedEvent += delegate { OnMiss.Invoke(); };
                _events.ComboDidBreakEvent += delegate { OnComboBreak.Invoke(); };
                _events.MultiplierDidIncreaseEvent += delegate { MultiplierUp.Invoke(); };
                _events.ComboDidChangeEvent += delegate (int combo) { OnComboChanged.Invoke(combo); };
                _events.SabersStartCollideEvent += delegate { SaberStartColliding.Invoke(); };
                _events.SabersEndCollideEvent += delegate { SaberStopColliding.Invoke(); };
                _events.LevelFinishedEvent += delegate { OnLevelFinish.Invoke(); };
                _events.LevelFailedEvent += delegate { OnLevelFail.Invoke(); };
                _events.NewHighscore += delegate { OnNewHighscore.Invoke(); };
                _events.ScoreDidChangeEvent += delegate (int rawScore, int modifiedScore) { OnScoreChanged.Invoke(rawScore, modifiedScore); };
                _events.GoodCutCountDidChangeEvent += delegate (int goodCuts) { OnGoodCutCountChanged.Invoke(goodCuts); };
                _events.BadCutCountDidChangeEvent += delegate (int badCuts) { OnBadCutCountChanged.Invoke(badCuts); };
                _events.MissCountDidChangeEvent += delegate (int misses) { OnMissCountChanged.Invoke(misses); };
                _events.AllNotesCountDidChangeEvent += delegate (int spawnedNotes, int allNotesInBeatmap) { OnAllNotesCountChanged.Invoke(spawnedNotes, allNotesInBeatmap); };
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
                _events.GameSceneLoadedEvent -= delegate { OnLevelStart.Invoke(); };
                _events.NoteWasCutEvent -= delegate { OnSlice.Invoke(); };
                _events.NoteWasMissedEvent -= delegate { OnMiss.Invoke(); };
                _events.ComboDidBreakEvent -= delegate { OnComboBreak.Invoke(); };
                _events.MultiplierDidIncreaseEvent -= delegate { MultiplierUp.Invoke(); };
                _events.ComboDidChangeEvent -= delegate (int combo) { OnComboChanged.Invoke(combo); };
                _events.SabersStartCollideEvent -= delegate { SaberStartColliding.Invoke(); };
                _events.SabersEndCollideEvent -= delegate { SaberStopColliding.Invoke(); };
                _events.LevelFinishedEvent -= delegate { OnLevelFinish.Invoke(); };
                _events.LevelFailedEvent -= delegate { OnLevelFail.Invoke(); };
                _events.NewHighscore -= delegate { OnNewHighscore.Invoke(); };
                _events.ScoreDidChangeEvent -= delegate (int rawScore, int modifiedScore) { OnScoreChanged.Invoke(rawScore, modifiedScore); };
                _events.GoodCutCountDidChangeEvent -= delegate (int goodCuts) { OnGoodCutCountChanged.Invoke(goodCuts); };
                _events.BadCutCountDidChangeEvent -= delegate (int badCuts) { OnBadCutCountChanged.Invoke(badCuts); };
                _events.MissCountDidChangeEvent -= delegate (int misses) { OnMissCountChanged.Invoke(misses); };
                _events.AllNotesCountDidChangeEvent -= delegate (int spawnedNotes, int allNotesInBeatmap) { OnAllNotesCountChanged.Invoke(spawnedNotes, allNotesInBeatmap); };
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