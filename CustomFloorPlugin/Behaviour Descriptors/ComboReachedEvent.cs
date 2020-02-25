using UnityEngine.Events;

namespace CustomFloorPlugin {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Too old to change")]
    public class ComboReachedEvent:EventFilterBehaviour {
        public int ComboTarget = 50;
        public UnityEvent NthComboReached;

        private void OnEnable() {
            EventManager.OnComboChanged.AddListener(OnComboReached);
        }

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
