using UnityEngine;


namespace CustomFloorPlugin
{
    [RequireComponent(typeof(EventManager))]
    public class EventFilterBehaviour : MonoBehaviour
    {
        protected EventManager EventManager => _eventManager ??= GetComponent<EventManager>();
        private EventManager? _eventManager;
    }
}