using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;


namespace CustomFloorPlugin
{
    public class ComboReachedEvent : MonoBehaviour
    {
        [FormerlySerializedAs("ComboTarget")] public int comboTarget = 50;
        [FormerlySerializedAs("NthComboReached")] public UnityEvent? nthComboReached;
    }
}