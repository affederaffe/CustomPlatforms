using System;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;


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
    }
}