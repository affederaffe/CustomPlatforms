using UnityEngine;
using UnityEngine.Events;


namespace CustomFloorPlugin
{
    public class SongEventHandler : MonoBehaviour
    {
        public SongEventType eventType;
        public int value;
        public bool anyValue;
        // ReSharper disable once InconsistentNaming
        public UnityEvent? OnTrigger;
    }
}