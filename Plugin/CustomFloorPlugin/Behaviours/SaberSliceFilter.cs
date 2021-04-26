using UnityEngine.Events;
using UnityEngine.Serialization;

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
        [FormerlySerializedAs("SaberSlice")] public UnityEvent? saberSlice;

        public void PlatformEnabled(DiContainer container)
        {
            EventManager.onSpecificSlice.AddListener(OnSlice);
        }

        public void PlatformDisabled()
        {
            EventManager.onComboChanged.RemoveListener(OnSlice);
        }

        private void OnSlice(int saber)
        {
            if ((SaberType)saber == saberType)
                saberSlice!.Invoke();
        }
    }
}