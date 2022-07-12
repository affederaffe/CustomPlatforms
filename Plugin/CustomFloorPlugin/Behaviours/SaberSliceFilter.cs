using CustomFloorPlugin.Interfaces;
using UnityEngine;
using UnityEngine.Events;

using Zenject;


// ReSharper disable once CheckNamespace
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
        public void Construct([InjectOptional] BSEvents events) => _events = events;

        public void PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            if (_events is null) return;
            _events.NoteWasCutEvent += OnNoteWasCut;
        }

        public void PlatformDisabled()
        {
            if (_events is null) return;
            _events.NoteWasCutEvent -= OnNoteWasCut;
        }

        private void OnNoteWasCut(global::SaberType saber)
        {
            if ((SaberType)saber == saberType)
                SaberSlice!.Invoke();
        }
    }
}
