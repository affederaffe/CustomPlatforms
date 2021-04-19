using UnityEngine.Events;


namespace CustomFloorPlugin 
{
    public class EveryNthComboFilter : EventFilterBehaviour 
    {
        public int ComboStep = 50;
        public UnityEvent NthComboReached;
    }
}