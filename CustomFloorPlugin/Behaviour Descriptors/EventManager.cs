using System;

using UnityEngine;
using UnityEngine.Events;


namespace CustomFloorPlugin {


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Too old to change")]
    public class EventManager : MonoBehaviour {
        public UnityEvent OnSlice;
        public UnityEvent OnComboBreak;
        public UnityEvent MultiplierUp;
        public UnityEvent SaberStartColliding;
        public UnityEvent SaberStopColliding;
        public UnityEvent OnLevelStart;
        public UnityEvent OnLevelFail;
        public UnityEvent OnBlueLightOn;
        public UnityEvent OnRedLightOn;

        [Serializable]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Too old to change")]
        public class ComboChangedEvent : UnityEvent<int> {
        }
        public ComboChangedEvent OnComboChanged = new ComboChangedEvent();
    }
}