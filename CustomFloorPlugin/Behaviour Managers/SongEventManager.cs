using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Instantiable wrapper class for a single <see cref="SongEventHandler"/> that handles registering and de-registering
    /// </summary>
    internal class SongEventManager : MonoBehaviour
    {
        [InjectOptional]
        private readonly BSEvents _events;

        /// <summary>
        /// Holds the <see cref="SongEventHandler"/> reference for this instance<br/>
        /// Has to be public! Will throw a <see cref="TargetInvocationException"/> otherwise!
        /// </summary>
        public SongEventHandler _songEventHandler;

        /// <summary>
        /// Gatekeeper function for <see cref="SongEventHandler.OnTrigger"/><br/>
        /// (I refuse calling that a good implementation)<br/>
        /// (Who the fuck did this???)<br/>
        /// (Use a <see cref="Dictionary{TKey, TValue}"/> instead)
        /// </summary>
        /// <param name="songEventData">Event to evaluate</param>
        internal void HandleSongEvent(BeatmapEventData songEventData)
        {
            if (songEventData.type == (BeatmapEventType)_songEventHandler.eventType)
            {
                if (songEventData.value == _songEventHandler.value || _songEventHandler.anyValue)
                {
                    _songEventHandler.OnTrigger.Invoke();
                }
            }
        }

        /// <summary>
        /// Registers to lighting events<br/>
        /// [Unity calls this when the <see cref="MonoBehaviour"/> becomes active in the hierachy]
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnEnable()
        {
            if (_events != null)
            {
                _events.BeatmapEventDidTriggerEvent += HandleSongEvent;
            }
        }

        /// <summary>
        /// De-Registers from lighting events<br/>
        /// [Unity calls this when the <see cref="MonoBehaviour"/> becomes inactive in the hierachy]
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnDisable()
        {
            if (_events != null)
            {
                _events.BeatmapEventDidTriggerEvent -= HandleSongEvent;
            }
        }
    }
}