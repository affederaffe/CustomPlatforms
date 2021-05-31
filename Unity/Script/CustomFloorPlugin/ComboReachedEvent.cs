using UnityEngine;
using UnityEngine.Events;


namespace CustomFloorPlugin
{
    public class ComboReachedEvent : MonoBehaviour
    {
        // ReSharper disable InconsistentNaming
        public int ComboTarget = 50;
        public UnityEvent? NthComboReached;
        // ReSharper restore InconsistentNaming
    }
}