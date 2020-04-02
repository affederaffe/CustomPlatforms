using UnityEngine;
using UnityEngine.Events;


namespace CustomFloorPlugin {


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Too old to change")]
    public class SongEventHandler:MonoBehaviour {
        public SongEventType eventType;
        public int value; // enum? 
        public bool anyValue;
        public UnityEvent OnTrigger;
    }
}