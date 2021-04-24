using UnityEngine.Events;

using Zenject;


namespace CustomFloorPlugin
{
    public class SaberSliceFilter : EventFilterBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
    {
        public enum SaberType
        {
            LeftSaber,
            RightSaber
        }

        public SaberType saberType;
        public UnityEvent? SaberSlice;

        public void PlatformEnabled(DiContainer container)
        {
            EventManager.OnSpecificSlice.AddListener(OnSlice);
        }

        public void PlatformDisabled()
        {
            EventManager.OnComboChanged.RemoveListener(OnSlice);
        }

        private void OnSlice(int saber)
        {
            if ((SaberType)saber == saberType)
                SaberSlice!.Invoke();
        }
    }
}