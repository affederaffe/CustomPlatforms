using CustomFloorPlugin.Interfaces;
using UnityEngine;
using UnityEngine.Events;

using Zenject;


namespace CustomFloorPlugin
{
    public class SaberSliceFilter : MonoBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
    {
        public enum SaberType
        {
            LeftSaber,
            RightSaber
        }

        public SaberType saberType;
        // ReSharper disable once InconsistentNaming
        public UnityEvent? SaberSlice;

        private BSEvents? _events;

        [Inject]
        public void Construct([InjectOptional] BSEvents events)
        {
            _events = events;
        }

        public void PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            if (_events is null) return;
            _events.NoteWasCutEvent += OnSlice;
        }

        public void PlatformDisabled()
        {
            if (_events is null) return;
            _events.NoteWasCutEvent -= OnSlice;
        }

        private void OnSlice(int saber)
        {
            if ((SaberType)saber == saberType)
                SaberSlice!.Invoke();
        }
    }
}