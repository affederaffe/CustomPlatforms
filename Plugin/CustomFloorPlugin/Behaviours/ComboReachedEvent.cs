using CustomFloorPlugin.Interfaces;
using UnityEngine;
using UnityEngine.Events;

using Zenject;


// ReSharper disable once CheckNamespace
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
        public void Construct([InjectOptional] BSEvents events) => _events = events;

        public void PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            if (_events is null) return;
            _events.ComboDidChangeEvent += OnComboReached;
        }

        public void PlatformDisabled()
        {
            if (_events is null) return;
            _events.ComboDidChangeEvent -= OnComboReached;
        }

        private void OnComboReached(int combo)
        {
            if (combo == ComboTarget)
                NthComboReached!.Invoke();
        }
    }
}
