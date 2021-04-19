using TMPro;


namespace CustomFloorPlugin
{
    public class TextEventFilter : EventFilterBehaviour
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

        public CounterType eventType;
        public TextMeshPro textMeshPro;
    }
}