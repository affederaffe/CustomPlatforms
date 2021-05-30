using System;
using System.Collections.Generic;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Should be pretty self-explanatory, this is a giant wrapper for many events Beat Saber uses
    /// </summary>
    public sealed class BSEvents : IInitializable, IDisposable
    {
        private readonly BeatmapObjectManager _beatmapObjectManager;
        private readonly GameEnergyCounter _gameEnergyCounter;
        private readonly GameplayCoreSceneSetupData _gameplayCoreSceneSetupData;
        private readonly ObstacleSaberSparkleEffectManager _obstacleSaberSparkleEffectManager;
        private readonly ScoreController _scoreController;
        private readonly PlayerDataModel _playerDataModel;
        private readonly PrepareLevelCompletionResults _prepareLevelCompletionResults;
        private readonly IBeatmapObjectCallbackController _beatmapObjectCallbackController;
        private readonly IDifficultyBeatmap _difficultyBeatmap;
        private float _lastNoteTime;
        private int _anyCutCount;
        private int _goodCutCount;
        private int _badCutCount;
        private int _missCount;
        private int _cuttableNotesCount;
        private int _highScore;

        public BSEvents(BeatmapObjectManager beatmapObjectManager,
                        GameEnergyCounter gameEnergyCounter,
                        GameplayCoreSceneSetupData gameplayCoreSceneSetupData,
                        ObstacleSaberSparkleEffectManager obstacleSaberSparkleEffectManager,
                        ScoreController scoreController,
                        PlayerDataModel playerDataModel,
                        PrepareLevelCompletionResults prepareLevelCompletionResults,
                        IBeatmapObjectCallbackController beatmapObjectCallbackController,
                        IDifficultyBeatmap difficultyBeatmap)
        {
            _beatmapObjectManager = beatmapObjectManager;
            _gameEnergyCounter = gameEnergyCounter;
            _gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
            _obstacleSaberSparkleEffectManager = obstacleSaberSparkleEffectManager;
            _scoreController = scoreController;
            _playerDataModel = playerDataModel;
            _prepareLevelCompletionResults = prepareLevelCompletionResults;
            _beatmapObjectCallbackController = beatmapObjectCallbackController;
            _difficultyBeatmap = difficultyBeatmap;
        }

        public event Action<BeatmapEventData>? BeatmapEventDidTriggerEvent;
        public event Action? GameSceneLoadedEvent;
        public event Action? LevelFinishedEvent;
        public event Action? LevelFailedEvent;
        public event Action? NewHighscore;
        public event Action<int>? NoteWasCutEvent;
        public event Action? NoteWasMissedEvent;
        public event Action? ComboDidBreakEvent;
        public event Action<int>? GoodCutCountDidChangeEvent;
        public event Action<int>? BadCutCountDidChangeEvent;
        public event Action<int>? MissCountDidChangeEvent;
        public event Action<int, int>? AllNotesCountDidChangeEvent;
        public event Action? MultiplierDidIncreaseEvent;
        public event Action<int>? ComboDidChangeEvent;
        public event Action? SabersStartCollideEvent;
        public event Action? SabersEndCollideEvent;
        public event Action<int, int>? ScoreDidChangeEvent;

        public void Initialize()
        {
            _cuttableNotesCount = _difficultyBeatmap.beatmapData.cuttableNotesType - 1;
            _highScore = _playerDataModel.playerData.GetPlayerLevelStatsData(_difficultyBeatmap).highScore;
            _lastNoteTime = GetLastNoteTime();
            _beatmapObjectCallbackController.beatmapEventDidTriggerEvent += BeatmapEventDidTrigger;
            _beatmapObjectManager.noteWasCutEvent += NoteWasCut;
            _beatmapObjectManager.noteWasMissedEvent += NoteWasMissed;
            _gameEnergyCounter.gameEnergyDidReach0Event += LevelFailed;
            _obstacleSaberSparkleEffectManager.sparkleEffectDidStartEvent += SabersStartCollide;
            _obstacleSaberSparkleEffectManager.sparkleEffectDidEndEvent += SabersEndCollide;
            _scoreController.comboDidChangeEvent += ComboDidChange;
            _scoreController.comboBreakingEventHappenedEvent += ComboDidBreak;
            _scoreController.multiplierDidChangeEvent += MultiplierDidChange;
            _scoreController.scoreDidChangeEvent += ScoreDidChange;
            GameSceneLoadedEvent?.Invoke();
        }

        public void Dispose()
        {
            _beatmapObjectCallbackController.beatmapEventDidTriggerEvent -= BeatmapEventDidTrigger;
            _beatmapObjectManager.noteWasCutEvent -= NoteWasCut;
            _beatmapObjectManager.noteWasMissedEvent -= NoteWasMissed;
            _gameEnergyCounter.gameEnergyDidReach0Event -= LevelFailed;
            _obstacleSaberSparkleEffectManager.sparkleEffectDidStartEvent -= SabersStartCollide;
            _obstacleSaberSparkleEffectManager.sparkleEffectDidEndEvent -= SabersEndCollide;
            _scoreController.comboDidChangeEvent -= ComboDidChange;
            _scoreController.comboBreakingEventHappenedEvent -= ComboDidBreak;
            _scoreController.multiplierDidChangeEvent -= MultiplierDidChange;
            _scoreController.scoreDidChangeEvent -= ScoreDidChange;
        }

        private float GetLastNoteTime()
        {
            float lastNoteTime = 0f;
            foreach (IReadonlyBeatmapLineData beatmapLineData in _gameplayCoreSceneSetupData.difficultyBeatmap.beatmapData.beatmapLinesData)
            {
                IReadOnlyList<BeatmapObjectData> beatmapObjectsData = beatmapLineData.beatmapObjectsData;
                for (int i = beatmapObjectsData.Count - 1; i >= 0; i--)
                {
                    BeatmapObjectData beatmapObjectData = beatmapObjectsData[i];
                    if (beatmapObjectData.beatmapObjectType == BeatmapObjectType.Note && ((NoteData)beatmapObjectData).colorType != ColorType.None && beatmapObjectData.time > lastNoteTime)
                        lastNoteTime = beatmapObjectData.time;
                }
            }

            return lastNoteTime;
        }

        private void BeatmapEventDidTrigger(BeatmapEventData eventData)
        {
            BeatmapEventDidTriggerEvent?.Invoke(eventData);
        }

        private void NoteWasCut(NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            if (noteController.noteData.colorType == ColorType.None || noteController.noteData.beatmapObjectType != BeatmapObjectType.Note) return;
            AllNotesCountDidChangeEvent?.Invoke(_anyCutCount++, _cuttableNotesCount);
            if (noteCutInfo.allIsOK)
            {
                NoteWasCutEvent?.Invoke((int)noteCutInfo.saberType);
                GoodCutCountDidChangeEvent?.Invoke(_goodCutCount++);
            }
            else
            {
                BadCutCountDidChangeEvent?.Invoke(_badCutCount++);
            }

            if (Mathf.Approximately(noteController.noteData.time, _lastNoteTime))
            {
                _lastNoteTime = 0f;
                LevelFinishedEvent?.Invoke();
                LevelCompletionResults results = _prepareLevelCompletionResults.FillLevelCompletionResults(LevelCompletionResults.LevelEndStateType.Cleared, LevelCompletionResults.LevelEndAction.None);
                if (results.modifiedScore > _highScore)
                    NewHighscore?.Invoke();
            }
        }

        private void NoteWasMissed(NoteController noteController)
        {
            if (noteController.noteData.colorType == ColorType.None || noteController.noteData.beatmapObjectType != BeatmapObjectType.Note) return;
            NoteWasMissedEvent?.Invoke();
            AllNotesCountDidChangeEvent?.Invoke(_anyCutCount++, _cuttableNotesCount);
            MissCountDidChangeEvent?.Invoke(_missCount++);
            if (Mathf.Approximately(noteController.noteData.time, _lastNoteTime))
            {
                _lastNoteTime = 0f;
                LevelFinishedEvent?.Invoke();
                LevelCompletionResults results = _prepareLevelCompletionResults.FillLevelCompletionResults(LevelCompletionResults.LevelEndStateType.Cleared, LevelCompletionResults.LevelEndAction.None);
                if (results.modifiedScore > _highScore)
                    NewHighscore?.Invoke();
            }
        }

        private void MultiplierDidChange(int multiplier, float progress)
        {
            if (multiplier > 1 && progress < 0.1f)
                MultiplierDidIncreaseEvent?.Invoke();
        }

        private void LevelFailed() => LevelFailedEvent?.Invoke();

        private void SabersStartCollide(SaberType saberType) => SabersStartCollideEvent?.Invoke();

        private void SabersEndCollide(SaberType saberType) => SabersEndCollideEvent?.Invoke();

        private void ComboDidChange(int combo) => ComboDidChangeEvent?.Invoke(combo);

        private void ComboDidBreak() => ComboDidBreakEvent?.Invoke();

        private void ScoreDidChange(int rawScore, int modifiedScore) => ScoreDidChangeEvent?.Invoke(rawScore, modifiedScore);
    }
}