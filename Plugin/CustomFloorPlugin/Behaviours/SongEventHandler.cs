using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

using Zenject;


namespace CustomFloorPlugin
{
    public class SongEventHandler : MonoBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
    {
        public SongEventType eventType;
        public int value;
        public bool anyValue;
        [FormerlySerializedAs("OnTrigger")] public UnityEvent? onTrigger;

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
            _events.BeatmapEventDidTriggerEvent += OnSongEvent;
        }

        public void PlatformDisabled()
        {
            if (_events == null) return;
            _events.BeatmapEventDidTriggerEvent -= OnSongEvent;
        }

        /// <summary>
        /// Gatekeeper function for <see cref="onTrigger"/><br/>
        /// (I refuse calling that a good implementation)<br/>
        /// (Who the fuck did this???)<br/>
        /// (Use a <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> instead)
        /// </summary>
        /// <param name="songEventData">Event to evaluate</param>
        private void OnSongEvent(BeatmapEventData songEventData)
        {
            if (songEventData.type == (BeatmapEventType)eventType)
            {
                if (songEventData.value == value || anyValue)
                {
                    onTrigger!.Invoke();
                }
            }
        }
    }
}