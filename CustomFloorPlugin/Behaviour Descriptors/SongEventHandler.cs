using UnityEngine;
using UnityEngine.Events;

namespace CustomFloorPlugin {
    public class SongEventHandler:MonoBehaviour {
        public SongEventType eventType;
        public int value; // enum?
        public bool anyValue;
        public UnityEvent OnTrigger;
    }
}
