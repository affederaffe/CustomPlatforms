using System;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    public class BSEvents : IInitializable, IDisposable
    {
        [Inject]
        private readonly BeatmapObjectCallbackController _beatmapObjectCallbackController;

        [Inject]
        private readonly BeatmapObjectManager _beatmapObjectManager;

        [Inject]
        private readonly GameEnergyCounter _gameEnergyCounter;

        [Inject]
        private readonly GameScenesManager _gameScenesManager;

        [Inject]
        private readonly ObstacleSaberSparkleEffectManager _obstacleSaberSparkleEffectManager;

        [Inject]
        private readonly ScoreController _scoreController;

        [Inject]
        private readonly PlayerDataModel _playerDataModel;

        [Inject]
        private readonly PrepareLevelCompletionResults _prepareLevelCompletionResults;

        [Inject]
        private readonly IDifficultyBeatmap _difficultyBeatmap;

        [Inject(Id = "LastNoteTime")]
        private float _lastNoteTime;

        public event Action<BeatmapEventData> BeatmapEventDidTriggerEvent = delegate { };
        public event Action GameSceneLoadedEvent = delegate { };
        public event Action LevelFinishedEvent = delegate { };
        public event Action LevelFailedEvent = delegate { };
        public event Action<int> NewHighscore = delegate { };
        public event Action<SaberType> NoteWasCutEvent = delegate { };
        public event Action NoteWasMissedEvent = delegate { };
        public event Action ComboDidBreakEvent = delegate { };
        public event Action<int> GoodCutCountDidChangeEvent = delegate { };
        public event Action<int> BadCutCountDidChangeEvent = delegate { };
        public event Action<int> MissCountDidChangeEvent = delegate { };
        public event Action<int, int> AllNotesCountDidChangeEvent = delegate { };
        public event Action MultiplierDidIncreaseEvent = delegate { };
        public event Action<int> ComboDidChangeEvent = delegate { };
        public event Action SabersStartCollideEvent = delegate { };
        public event Action SabersEndCollideEvent = delegate { };
        public event Action<int, int> ScoreDidChangeEvent = delegate { };

        private int allNotesCount = 0;
        private int goodCutCount = 0;
        private int badCutCount = 0;
        private int missCount = 0;
        private int cuttableNotes = 0;

        public void Initialize()
        {
            cuttableNotes = _difficultyBeatmap.beatmapData.cuttableNotesType;
            _beatmapObjectCallbackController.beatmapEventDidTriggerEvent += BeatmapEventDidTrigger;
            _beatmapObjectManager.noteWasCutEvent += NoteWasCut;
            _beatmapObjectManager.noteWasMissedEvent += NoteWasMissed;
            _gameEnergyCounter.gameEnergyDidReach0Event += LevelFailed;
            _gameScenesManager.transitionDidFinishEvent += GameSceneLoaded;
            _obstacleSaberSparkleEffectManager.sparkleEffectDidStartEvent += SabersStartCollide;
            _obstacleSaberSparkleEffectManager.sparkleEffectDidEndEvent += SabersEndCollide;
            _scoreController.comboDidChangeEvent += ComboDidChange;
            _scoreController.comboBreakingEventHappenedEvent += ComboDidBreak;
            _scoreController.multiplierDidChangeEvent += MultiplierDidChange;
            _scoreController.scoreDidChangeEvent += ScoreDidChange;
        }

        public void Dispose()
        {
            _beatmapObjectCallbackController.beatmapEventDidTriggerEvent -= BeatmapEventDidTrigger;
            _beatmapObjectManager.noteWasCutEvent -= NoteWasCut;
            _beatmapObjectManager.noteWasMissedEvent -= NoteWasMissed;
            _gameEnergyCounter.gameEnergyDidReach0Event -= LevelFailed;
            _gameScenesManager.transitionDidFinishEvent -= GameSceneLoaded;
            _obstacleSaberSparkleEffectManager.sparkleEffectDidStartEvent -= SabersStartCollide;
            _obstacleSaberSparkleEffectManager.sparkleEffectDidEndEvent -= SabersEndCollide;
            _scoreController.comboDidChangeEvent -= ComboDidChange;
            _scoreController.comboBreakingEventHappenedEvent -= ComboDidBreak;
            _scoreController.multiplierDidChangeEvent -= MultiplierDidChange;
            _scoreController.scoreDidChangeEvent -= ScoreDidChange;
        }

        private void BeatmapEventDidTrigger(BeatmapEventData eventData)
        {
            BeatmapEventDidTriggerEvent(eventData);
        }

        private void NoteWasCut(NoteController noteController, NoteCutInfo noteCutInfo)
        {
            AllNotesCountDidChangeEvent(++allNotesCount, cuttableNotes);
            if (noteCutInfo.allIsOK)
            {
                NoteWasCutEvent(noteCutInfo.saberType);
                GoodCutCountDidChangeEvent(++goodCutCount);
            }
            else
            {
                BadCutCountDidChangeEvent(++badCutCount);
            }
            if (Mathf.Approximately(noteController.noteData.time, _lastNoteTime))
            {
                _lastNoteTime = 0f;
                LevelFinishedEvent();
                PlayerLevelStatsData playerLevelStatsData = _playerDataModel.playerData.GetPlayerLevelStatsData(_difficultyBeatmap);
                LevelCompletionResults results = _prepareLevelCompletionResults.FillLevelCompletionResults(LevelCompletionResults.LevelEndStateType.Cleared, LevelCompletionResults.LevelEndAction.None);
                if (results.modifiedScore > playerLevelStatsData.highScore)
                    NewHighscore(results.modifiedScore);
            }
        }

        private void NoteWasMissed(NoteController noteController)
        {
            NoteWasMissedEvent();
            AllNotesCountDidChangeEvent(++allNotesCount, cuttableNotes);
            MissCountDidChangeEvent(++missCount);
            if (Mathf.Approximately(noteController.noteData.time, _lastNoteTime))
            {
                _lastNoteTime = 0f;
                LevelFinishedEvent();
                PlayerLevelStatsData playerLevelStatsData = _playerDataModel.playerData.GetPlayerLevelStatsData(_difficultyBeatmap);
                LevelCompletionResults results = _prepareLevelCompletionResults.FillLevelCompletionResults(LevelCompletionResults.LevelEndStateType.Cleared, LevelCompletionResults.LevelEndAction.None);
                if (results.modifiedScore > playerLevelStatsData.highScore)
                    NewHighscore(results.modifiedScore);
            }
        }

        private void LevelFailed()
        {
            LevelFailedEvent();
        }

        private void GameSceneLoaded(ScenesTransitionSetupDataSO setupData, DiContainer container)
        {
            GameSceneLoadedEvent();
        }

        private void SabersStartCollide(SaberType saberType)
        {
            SabersStartCollideEvent();
        }

        private void SabersEndCollide(SaberType saberType)
        {
            SabersEndCollideEvent();
        }

        private void ComboDidChange(int combo)
        {
            ComboDidChangeEvent(combo);
        }

        private void ComboDidBreak()
        {
            ComboDidBreakEvent();
        }

        private void MultiplierDidChange(int multiplier, float progress)
        {
            if (multiplier > 1 && progress < 0.1f)
            {
                MultiplierDidIncreaseEvent();
            }
        }

        private void ScoreDidChange(int rawScore, int modifiedScore)
        {
            ScoreDidChangeEvent(rawScore, modifiedScore);
        }
    }
}
