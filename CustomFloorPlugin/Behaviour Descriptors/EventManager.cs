using System;

using UnityEngine;
using UnityEngine.Events;


namespace CustomFloorPlugin
{
    public class EventManager : MonoBehaviour
    {
        public UnityEvent OnSlice;
        public UnityEvent<SaberType> OnSpecificSlice;
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
        public UnityEvent<int> OnNewHighScore;
        public UnityEvent<int, int> OnScoreChanged;
        public UnityEvent<int> OnGoodCutCountChanged;
        public UnityEvent<int> OnBadCutCountChanged;
        public UnityEvent<int> OnMissCountChanged;
        public UnityEvent<int, int> OnAllNotesCountChanged;

        [Serializable]
        public class ComboChangedEvent : UnityEvent<int>
        {
        }
        public ComboChangedEvent OnComboChanged = new ComboChangedEvent();
    }
}