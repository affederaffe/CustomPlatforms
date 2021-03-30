using UnityEngine;
using UnityEngine.Events;

using Zenject;


namespace CustomFloorPlugin
{
    public class SongEventHandler : MonoBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
    {
        public SongEventType eventType;
        public int value;
        public bool anyValue;
        public UnityEvent OnTrigger;

        private BSEvents _events;

        [Inject]
        public void Construct([InjectOptional] BSEvents events)
        {
            _events = events;
        }

        void INotifyPlatformEnabled.PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            if (_events != null)
                _events.BeatmapEventDidTriggerEvent += HandleSongEvent;
        }

        void INotifyPlatformDisabled.PlatformDisabled()
        {
            if (_events != null)
                _events.BeatmapEventDidTriggerEvent -= HandleSongEvent;
        }

        /// <summary>
        /// Gatekeeper function for <see cref="SongEventHandler.OnTrigger"/><br/>
        /// (I refuse calling that a good implementation)<br/>
        /// (Who the fuck did this???)<br/>
        /// (Use a <see cref="Dictionary{TKey, TValue}"/> instead)
        /// </summary>
        /// <param name="songEventData">Event to evaluate</param>
        private void HandleSongEvent(BeatmapEventData songEventData)
        {
            if (songEventData.type == (BeatmapEventType)eventType)
            {
                if (songEventData.value == value || anyValue)
                {
                    OnTrigger.Invoke();
                }
            }
        }
    }
}