using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;


namespace CustomFloorPlugin 
{
    public class SongEventHandler : MonoBehaviour 
    {
        public SongEventType eventType;
        public int value;
        public bool anyValue;
        [FormerlySerializedAs("OnTrigger")] public UnityEvent? onTrigger;
    }
}