using System;

using UnityEngine;
using UnityEngine.Events;


namespace CustomFloorPlugin
{
    public class EventManager : MonoBehaviour
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

        public ComboChangedEvent OnComboChanged = new ComboChangedEvent();
        public OnSpecificSliceEvent OnSpecificSlice = new OnSpecificSliceEvent();
        public OnScoreChangedEvent OnScoreChanged = new OnScoreChangedEvent();
        public OnGoodCutCountChangedEvent OnGoodCutCountChanged = new OnGoodCutCountChangedEvent();
        public OnBadCutCountChangedEvent OnBadCutCountChanged = new OnBadCutCountChangedEvent();
        public OnMissCountChangedEvent OnMissCountChanged = new OnMissCountChangedEvent();
        public OnAllNotesCountChangedEvent OnAllNotesCountChanged = new OnAllNotesCountChangedEvent();
    }
}