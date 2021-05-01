using TMPro;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    public class TextEventFilter : MonoBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
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

        private BSEvents? _events;
        private string? _startText;

        [Inject]
        public void Construct([InjectOptional] BSEvents events)
        {
            _events = events;
        }

        public void PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            if (_events == null || textMeshPro == null) return;
            _startText = textMeshPro!.text;
            switch (counterType)
            {
                case CounterType.AnyCuts:
                    _events.AllNotesCountDidChangeEvent += OnAllNotesCountDidChange;
                    break;
                case CounterType.GoodCuts:
                    _events.GoodCutCountDidChangeEvent += OnGoodCutCountDidChange;
                    break;
                case CounterType.BadCuts:
                    _events.BadCutCountDidChangeEvent += OnBadCutCountDidChange;
                    break;
                case CounterType.Misses:
                    _events.MissCountDidChangeEvent += OnMissCountDidChange;
                    break;
                case CounterType.Score:
                    _events.ScoreDidChangeEvent += OnScoreDidChange;
                    break;
                case CounterType.Combo:
                    _events.ComboDidChangeEvent += OnComboDidChange;
                    break;
            }
        }

        public void PlatformDisabled()
        {
            if (_events == null || textMeshPro == null) return;
            textMeshPro!.text = _startText!;
            switch (counterType)
            {
                case CounterType.AnyCuts:
                    _events.AllNotesCountDidChangeEvent -= OnAllNotesCountDidChange;
                    break;
                case CounterType.GoodCuts:
                    _events.GoodCutCountDidChangeEvent -= OnGoodCutCountDidChange;
                    break;
                case CounterType.BadCuts:
                    _events.BadCutCountDidChangeEvent -= OnBadCutCountDidChange;
                    break;
                case CounterType.Misses:
                    _events.MissCountDidChangeEvent -= OnMissCountDidChange;
                    break;
                case CounterType.Score:
                    _events.ScoreDidChangeEvent -= OnScoreDidChange;
                    break;
                case CounterType.Combo:
                    _events.ComboDidChangeEvent -= OnComboDidChange;
                    break;
            }
        }

        private void OnAllNotesCountDidChange(int anyCuts, int cuttableNotes) => textMeshPro!.text = $"{anyCuts.ToString()} | {cuttableNotes.ToString()}";
        private void OnGoodCutCountDidChange(int goodCuts) => textMeshPro!.text = $"{goodCuts.ToString()}";
        private void OnBadCutCountDidChange(int badCuts) => textMeshPro!.text = $"{badCuts.ToString()}";
        private void OnMissCountDidChange(int misses) => textMeshPro!.text = $"{misses.ToString()}";
        private void OnScoreDidChange(int rawScore, int modifiedScore) => textMeshPro!.text = $"{rawScore.ToString()} | {modifiedScore.ToString()}";
        private void OnComboDidChange(int combo) => textMeshPro!.text = $"{combo.ToString()}";
    }
}