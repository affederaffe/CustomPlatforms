using UnityEngine.Events;


namespace CustomFloorPlugin {


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Too old to change")]
    public class ComboReachedEvent:EventFilterBehaviour {
        public int ComboTarget = 50;
        public UnityEvent NthComboReached;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnEnable() {
            EventManager.OnComboChanged.AddListener(OnComboReached);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnDisable() {
            EventManager.OnComboChanged.RemoveListener(OnComboReached);
        }

        private void OnComboReached(int combo) {
            if(combo == ComboTarget) {
                NthComboReached.Invoke();
            }
        }
    }
}