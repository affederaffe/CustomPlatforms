using CustomFloorPlugin.Interfaces;
using UnityEngine;
using UnityEngine.Events;

using Zenject;


// ReSharper disable once CheckNamespace
namespace CustomFloorPlugin
{
    public class SongEventHandler : MonoBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
    {
        public SongEventType eventType;
        public int value;
        public bool anyValue;
        // ReSharper disable once InconsistentNaming
        public UnityEvent? OnTrigger;

        private BSEvents? _events;
        private int _subtypeIdentifier;

        public void Awake()
        {
            _subtypeIdentifier = BasicBeatmapEventData.SubtypeIdentifier((BasicBeatmapEventType)eventType);
        }

        [Inject]
        public void Construct([InjectOptional] BSEvents events)
        {
            _events = events;
        }

        public void PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            if (_events is null) return;
            _events.BeatmapEventDidTriggerEvent += OnSongEvent;
        }

        public void PlatformDisabled()
        {
            if (_events is null) return;
            _events.BeatmapEventDidTriggerEvent -= OnSongEvent;
        }

        /// <summary>
        /// Gatekeeper function for <see cref="OnTrigger"/><br/>
        /// (I refuse calling that a good implementation)<br/>
        /// (Who the fuck did this???)<br/>
        /// (Use a <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> instead)
        /// </summary>
        /// <param name="songEventData">Event to evaluate</param>
        private void OnSongEvent(BasicBeatmapEventData songEventData)
        {
            if (songEventData.subtypeIdentifier == _subtypeIdentifier && (songEventData.value == value || anyValue))
                OnTrigger!.Invoke();
        }
    }
}