using UnityEngine.Events;

using TMPro;


namespace CustomFloorPlugin
{
    public class TextEventFilter : EventFilterBehaviour
    {
        public EventType eventType = EventType.AllNotes;
        public TextMeshPro tmPro;
        private string startText;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnEnable()
        {
            if (!tmPro)
                return;
            startText = tmPro.text;
            switch (eventType)
            {
                case EventType.AllNotes:
                    EventManager.OnAllNotesCountChanged.AddListener(OnAllNotesCountChanged);
                    break;
                case EventType.GoodCuts:
                    EventManager.OnGoodCutCountChanged.AddListener(OnGoodCutCountChanged);
                    break;
                case EventType.BadCuts:
                    EventManager.OnBadCutCountChanged.AddListener(OnBadCutCountChanged);
                    break;
                case EventType.Misses:
                    EventManager.OnMissCountChanged.AddListener(OnMissCountChanged);
                    break;
                case EventType.Score:
                    EventManager.OnScoreChanged.AddListener(OnScoreChanged);
                    break;
                case EventType.Combo:
                    EventManager.OnComboChanged.AddListener(OnComboChanged);
                    break;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnDisable()
        {
            if (!tmPro)
                return;
            tmPro.text = startText;
            switch (eventType)
            {
                case EventType.AllNotes:
                    EventManager.OnAllNotesCountChanged.RemoveListener(OnAllNotesCountChanged);
                    break;
                case EventType.GoodCuts:
                    EventManager.OnGoodCutCountChanged.RemoveListener(OnGoodCutCountChanged);
                    break;
                case EventType.BadCuts:
                    EventManager.OnBadCutCountChanged.RemoveListener(OnBadCutCountChanged);
                    break;
                case EventType.Misses:
                    EventManager.OnMissCountChanged.RemoveListener(OnMissCountChanged);
                    break;
                case EventType.Score:
                    EventManager.OnScoreChanged.RemoveListener(OnScoreChanged);
                    break;
                case EventType.Combo:
                    EventManager.OnComboChanged.RemoveListener(OnComboChanged);
                    break;
            }
        }

        private void OnAllNotesCountChanged(int allNotes, int cuttableNotes)
        {
            tmPro.text = $"{allNotes} | {cuttableNotes}";
        }

        private void OnGoodCutCountChanged(int goodCuts)
        {
            tmPro.text = goodCuts.ToString();
        }

        private void OnBadCutCountChanged(int badCuts)
        {
            tmPro.text = badCuts.ToString();
        }

        private void OnMissCountChanged(int misses)
        {
            tmPro.text = misses.ToString();
        }

        private void OnScoreChanged(int rawScore, int modifiedScore)
        {
            tmPro.text = $"{rawScore} | {modifiedScore}";
        }

        private void OnComboChanged(int combo)
        {
            tmPro.text = combo.ToString();
        }

        public enum EventType
        {
            AllNotes,
            GoodCuts,
            BadCuts,
            Misses,
            Score,
            Combo
        }
    }
}