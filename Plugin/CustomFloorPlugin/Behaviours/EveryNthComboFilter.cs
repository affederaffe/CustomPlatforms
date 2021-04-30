using UnityEngine.Events;
using UnityEngine.Serialization;

using Zenject;


namespace CustomFloorPlugin
{
    public class EveryNthComboFilter : EventFilterBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
    {
        [FormerlySerializedAs("ComboStep")] public int comboStep = 50;
        [FormerlySerializedAs("NthComboReached")] public UnityEvent? nthComboReached;

        public void PlatformEnabled(DiContainer container)
        {
            EventManager.onComboChanged.AddListener(OnComboStep);
        }

        public void PlatformDisabled()
        {
            EventManager.onComboChanged.RemoveListener(OnComboStep);
        }

        private void OnComboStep(int combo)
        {
            if (combo % comboStep == 0 && combo != 0)
            {
                nthComboReached!.Invoke();
            }
        }
    }
}