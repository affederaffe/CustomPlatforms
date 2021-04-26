using UnityEngine.Events;
using UnityEngine.Serialization;

using Zenject;


namespace CustomFloorPlugin
{
    public class ComboReachedEvent : EventFilterBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
    {
        [FormerlySerializedAs("ComboTarget")] public int comboTarget = 50;
        [FormerlySerializedAs("NthComboReached")] public UnityEvent? nthComboReached;

        public void PlatformEnabled(DiContainer container)
        {
            EventManager.onComboChanged.AddListener(OnComboReached);
        }

        public void PlatformDisabled()
        {
            EventManager.onComboChanged.RemoveListener(OnComboReached);
        }

        private void OnComboReached(int combo)
        {
            if (combo == comboTarget)
            {
                nthComboReached!.Invoke();
            }
        }
    }
}