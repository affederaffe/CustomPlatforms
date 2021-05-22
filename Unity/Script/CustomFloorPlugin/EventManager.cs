using System;

using UnityEngine;
using UnityEngine.Events;


namespace CustomFloorPlugin
{
    public class EventManager : MonoBehaviour
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
    }
}