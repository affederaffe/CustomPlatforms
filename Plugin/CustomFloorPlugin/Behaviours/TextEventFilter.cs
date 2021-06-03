using System.Globalization;

using CustomFloorPlugin.Interfaces;

using TMPro;

using UnityEngine;

using Zenject;


// ReSharper disable once CheckNamespace
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
            if (_events is null || textMeshPro is null) return;
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
            if (_events is null || textMeshPro is null) return;
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

        private void OnAllNotesCountDidChange(int anyCuts, int cuttableNotes) => textMeshPro!.text = $"{anyCuts.ToString(NumberFormatInfo.InvariantInfo)} | {cuttableNotes.ToString(NumberFormatInfo.InvariantInfo)}";
        private void OnGoodCutCountDidChange(int goodCuts) => textMeshPro!.text = $"{goodCuts.ToString(NumberFormatInfo.InvariantInfo)}";
        private void OnBadCutCountDidChange(int badCuts) => textMeshPro!.text = $"{badCuts.ToString(NumberFormatInfo.InvariantInfo)}";
        private void OnMissCountDidChange(int misses) => textMeshPro!.text = $"{misses.ToString(NumberFormatInfo.InvariantInfo)}";
        private void OnScoreDidChange(int rawScore, int modifiedScore) => textMeshPro!.text = $"{rawScore.ToString(NumberFormatInfo.InvariantInfo)} | {modifiedScore.ToString(NumberFormatInfo.InvariantInfo)}";
        private void OnComboDidChange(int combo) => textMeshPro!.text = $"{combo.ToString(NumberFormatInfo.InvariantInfo)}";
    }
}