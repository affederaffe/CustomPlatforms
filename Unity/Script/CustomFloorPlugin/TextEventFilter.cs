using TMPro;

using UnityEngine;


namespace CustomFloorPlugin
{
    public class TextEventFilter : MonoBehaviour
    {
        public enum CounterType
        {
            AnyCuts,
            GoodCuts,
            BadCuts,
            Misses,
            Score,
            Combo
        }

        public CounterType counterType;
        public TextMeshPro? textMeshPro;
    }
}