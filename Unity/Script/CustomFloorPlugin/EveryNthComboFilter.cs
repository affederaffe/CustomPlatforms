using UnityEngine;
using UnityEngine.Events;


namespace CustomFloorPlugin
{
    public class EveryNthComboFilter : MonoBehaviour
    {
        // ReSharper disable InconsistentNaming
        public int ComboStep = 50;
        public UnityEvent? NthComboReached;
        // ReSharper restore InconsistentNaming
    }
}