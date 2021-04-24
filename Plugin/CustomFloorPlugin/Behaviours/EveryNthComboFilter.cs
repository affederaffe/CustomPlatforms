using UnityEngine.Events;

using Zenject;


namespace CustomFloorPlugin
{
    public class EveryNthComboFilter : EventFilterBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
    {
        public int ComboStep = 50;
        public UnityEvent? NthComboReached;

        public void PlatformEnabled(DiContainer container)
        {
            EventManager.OnComboChanged.AddListener(OnComboStep);
        }

        public void PlatformDisabled()
        {
            EventManager.OnComboChanged.RemoveListener(OnComboStep);
        }

        private void OnComboStep(int combo)
        {
            if (combo % ComboStep == 0 && combo != 0)
            {
                NthComboReached!.Invoke();
            }
        }
    }
}