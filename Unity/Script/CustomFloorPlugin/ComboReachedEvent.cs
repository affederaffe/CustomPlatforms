using UnityEngine.Events;


namespace CustomFloorPlugin 
{
    public class ComboReachedEvent : EventFilterBehaviour 
    {
        public int ComboTarget = 50;
        public UnityEvent NthComboReached;
    }
}