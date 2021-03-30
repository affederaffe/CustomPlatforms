using UnityEngine.Events;

using Zenject;


namespace CustomFloorPlugin
{
    public class SaberSliceFilter : EventFilterBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
    {
        public enum SaberType
        {
            RightSaber,
            LeftSaber
        }

        public SaberType saberType;
        public UnityEvent SaberSlice;

        void INotifyPlatformEnabled.PlatformEnabled(DiContainer container)
        {
            EventManager.OnSpecificSlice.AddListener(OnSlice);
        }

        void INotifyPlatformDisabled.PlatformDisabled()
        {
            EventManager.OnComboChanged.RemoveListener(OnSlice);
        }

        private void OnSlice(int saber)
        {
            if ((SaberType)saber == saberType)
                SaberSlice.Invoke();
        }
    }
}