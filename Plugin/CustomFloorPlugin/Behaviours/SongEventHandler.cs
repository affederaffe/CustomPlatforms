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

        public void Awake() => _subtypeIdentifier = BasicBeatmapEventData.SubtypeIdentifier((BasicBeatmapEventType)eventType);

        [Inject]
        public void Construct([InjectOptional] BSEvents events) => _events = events;

        public void PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            if (_events is not null)
                _events.BeatmapEventDidTriggerEvent += OnSongEvent;
        }

        public void PlatformDisabled()
        {
            if (_events is not null)
                _events.BeatmapEventDidTriggerEvent -= OnSongEvent;
        }

        /// <summary>
        /// Gatekeeper function for <see cref="OnTrigger"/><br/>
        /// (I refuse calling that a good implementation)<br/>
        /// (Who the fuck did this???)<br/>
        /// (Use a <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> instead)
        /// </summary>
        /// <param name="songEventData">Event to evaluate</param>
        private void OnSongEvent(BeatmapDataItem songEventData)
        {
            switch (songEventData)
            {
                case BasicBeatmapEventData basicBeatmapEventData when (basicBeatmapEventData.subtypeIdentifier == _subtypeIdentifier && basicBeatmapEventData.value == value) || anyValue:
                    OnTrigger!.Invoke();
                    break;
                case ColorBoostBeatmapEventData colorBoostBeatmapEventData when colorBoostBeatmapEventData.boostColorsAreOn == (value > 0) || anyValue:
                    OnTrigger!.Invoke();
                    break;
            }
        }
    }
}
