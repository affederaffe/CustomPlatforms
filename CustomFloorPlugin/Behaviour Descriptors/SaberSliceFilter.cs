using UnityEngine.Events;


namespace CustomFloorPlugin
{
    public class SaberSliceFilter : EventFilterBehaviour
    {
        public SaberType saberType;
        public UnityEvent SaberSlice;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnEnable()
        {
            EventManager.OnSpecificSlice.AddListener(OnSlice);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnDisable()
        {
            EventManager.OnComboChanged.RemoveListener(OnSlice);
        }

        private void OnSlice(int saber)
        {
            if ((SaberType)saber == saberType)
                SaberSlice.Invoke();
        }

        public enum SaberType
        {
            RightSaber,
            LeftSaber
        }
    }
}