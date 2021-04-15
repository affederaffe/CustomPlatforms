using UnityEngine.Events;

using Zenject;


namespace CustomFloorPlugin
{
    public class ComboReachedEvent : EventFilterBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
    {
        public int ComboTarget = 50;
        public UnityEvent? NthComboReached;

        void INotifyPlatformEnabled.PlatformEnabled(DiContainer container)
        {
            EventManager.OnComboChanged.AddListener(OnComboReached);
        }

        void INotifyPlatformDisabled.PlatformDisabled()
        {
            EventManager.OnComboChanged.RemoveListener(OnComboReached);
        }

        private void OnComboReached(int combo)
        {
            if (combo == ComboTarget)
            {
                NthComboReached!.Invoke();
            }
        }
    }
}