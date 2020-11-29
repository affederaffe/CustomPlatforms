using System.Collections.Generic;
using System.Reflection;

using BS_Utils.Utilities;

using UnityEngine;


namespace CustomFloorPlugin {


    /// <summary>
    /// Instantiable wrapper class for a single <see cref="SongEventHandler"/> that handles registering and de-registering
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1812:Avoid unistantiated internal classes", Justification = "Instantiated by Unity")]
    internal class SongEventManager : MonoBehaviour {


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
        internal void HandleSongEvent(BeatmapEventData songEventData) {
            if (songEventData.type == (BeatmapEventType)_songEventHandler.eventType) {
                if (songEventData.value == _songEventHandler.value || _songEventHandler.anyValue) {
                    _songEventHandler.OnTrigger.Invoke();
                }
            }
        }


        /// <summary>
        /// Registers to lighting events<br/>
        /// [Unity calls this when the <see cref="MonoBehaviour"/> becomes active in the hierachy]
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnEnable() {
            BSEvents.beatmapEvent += HandleSongEvent;
        }


        /// <summary>
        /// De-Registers from lighting events<br/>
        /// [Unity calls this when the <see cref="MonoBehaviour"/> becomes inactive in the hierachy]
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnDisable() {
            BSEvents.beatmapEvent -= HandleSongEvent;
        }
    }
}