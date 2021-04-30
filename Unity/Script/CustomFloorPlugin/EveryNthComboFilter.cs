using UnityEngine.Events;
using UnityEngine.Serialization;


namespace CustomFloorPlugin 
{
    public class EveryNthComboFilter : EventFilterBehaviour 
    {
        [FormerlySerializedAs("ComboStep")] public int comboStep = 50;
        [FormerlySerializedAs("NthComboReached")] public UnityEvent? nthComboReached;
    }
}