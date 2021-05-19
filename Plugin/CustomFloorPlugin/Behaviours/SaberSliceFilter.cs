using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

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
        [FormerlySerializedAs("SaberSlice")] public UnityEvent? saberSlice;

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
                _events.NoteWasCutEvent += OnSlice;
        }

        public void PlatformDisabled()
        {
            if (_events != null)
                _events.NoteWasCutEvent -= OnSlice;
        }

        private void OnSlice(int saber)
        {
            if ((SaberType)saber == saberType)
                saberSlice!.Invoke();
        }
    }
}