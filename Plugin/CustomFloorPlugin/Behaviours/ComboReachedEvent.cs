using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

using Zenject;


namespace CustomFloorPlugin
{
    public class ComboReachedEvent : MonoBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
    {
        [FormerlySerializedAs("ComboTarget")] public int comboTarget = 50;
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
            if (_events != null)
                _events.ComboDidChangeEvent += OnComboReached;
        }

        public void PlatformDisabled()
        {
            if (_events != null)
                _events.ComboDidChangeEvent -= OnComboReached;
        }

        private void OnComboReached(int combo)
        {
            if (combo == comboTarget)
                nthComboReached!.Invoke();
        }
    }
}