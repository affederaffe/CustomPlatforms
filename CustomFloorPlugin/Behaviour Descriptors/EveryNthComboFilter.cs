using UnityEngine.Events;


namespace CustomFloorPlugin {


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Too old to change")]
    public class EveryNthComboFilter:EventFilterBehaviour {
        public int ComboStep = 50;
        public UnityEvent NthComboReached;


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnEnable() {
            EventManager.OnComboChanged.AddListener(OnComboStep);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnDisable() {
            EventManager.OnComboChanged.RemoveListener(OnComboStep);
        }

        private void OnComboStep(int combo) {
            if(combo % ComboStep == 0 && combo != 0) {
                NthComboReached.Invoke();
            }
        }
    }
}