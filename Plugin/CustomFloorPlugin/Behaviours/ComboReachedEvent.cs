using UnityEngine;
using UnityEngine.Events;

using Zenject;


namespace CustomFloorPlugin
{
    public class ComboReachedEvent : MonoBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
    {
        // ReSharper disable InconsistentNaming
        public int ComboTarget = 50;
        public UnityEvent? NthComboReached;
        // ReSharper restore InconsistentNaming

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
            _events.ComboDidChangeEvent += OnComboReached;
        }

        public void PlatformDisabled()
        {
            if (_events == null) return;
            _events.ComboDidChangeEvent -= OnComboReached;
        }

        private void OnComboReached(int combo)
        {
            if (combo == ComboTarget)
                NthComboReached!.Invoke();
        }
    }
}