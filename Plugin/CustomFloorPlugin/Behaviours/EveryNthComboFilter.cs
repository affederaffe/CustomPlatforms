using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

using Zenject;


namespace CustomFloorPlugin
{
    public class EveryNthComboFilter : MonoBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
    {
        [FormerlySerializedAs("ComboStep")] public int comboStep = 50;
        [FormerlySerializedAs("NthComboReached")] public UnityEvent? nthComboReached;

        private BSEvents? _events;
        
        [Inject]
        public void Construct([InjectOptional] BSEvents events)
        {
            _events = events;
        }
        
        public void PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            if (_events == null) return;
            _events.ComboDidChangeEvent += OnComboStep;
        }

        public void PlatformDisabled()
        {
            if (_events == null) return;
            _events.ComboDidChangeEvent -= OnComboStep;
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