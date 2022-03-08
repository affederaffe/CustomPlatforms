using System;

using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Should be pretty self-explanatory, this is a giant wrapper for many events Beat Saber uses
    /// </summary>
    public sealed class BSEvents : IInitializable, IDisposable
    {
        private readonly ILevelEndActions? _levelEndActions;
        private readonly IReadonlyBeatmapData _beatmapData;
        private readonly ObstacleSaberSparkleEffectManager _obstacleSaberSparkleEffectManager;
        private readonly ScoreController _scoreController;
        private readonly BeatmapObjectManager _beatmapObjectManager;
        private readonly ComboController _comboController;
        private readonly BeatmapCallbacksController _beatmapCallbacksController;
        private readonly int _bombSubtypeIdentifier;

        private BeatmapDataCallbackWrapper? _beatmapDataCallbackWrapper;
        private int _anyCutCount;
        private int _goodCutCount;
        private int _badCutCount;
        private int _missCount;
        private int _cuttableNotesCount;

        public BSEvents([InjectOptional] ILevelEndActions levelEndActions,
                        IReadonlyBeatmapData beatmapData,
                        ObstacleSaberSparkleEffectManager obstacleSaberSparkleEffectManager,
                        ScoreController scoreController,
                        BeatmapObjectManager beatmapObjectManager,
                        BeatmapCallbacksController beatmapCallbacksController,
                        ComboController comboController)
        {
            _levelEndActions = levelEndActions;
            _beatmapData = beatmapData;
            _obstacleSaberSparkleEffectManager = obstacleSaberSparkleEffectManager;
            _scoreController = scoreController;
            _beatmapObjectManager = beatmapObjectManager;
            _beatmapCallbacksController = beatmapCallbacksController;
            _comboController = comboController;
            _bombSubtypeIdentifier = NoteData.SubtypeIdentifier(ColorType.None);
        }

        public event Action<BasicBeatmapEventData>? BeatmapEventDidTriggerEvent;
        public event Action? LevelFinishedEvent;
        public event Action? LevelFailedEvent;
        public event Action<SaberType>? NoteWasCutEvent;
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
            _cuttableNotesCount = _beatmapData.cuttableNotesCount - 1;
            _beatmapDataCallbackWrapper = _beatmapCallbacksController.AddBeatmapCallback<BasicBeatmapEventData>(BeatmapEventDidTrigger);
            _obstacleSaberSparkleEffectManager.sparkleEffectDidStartEvent += SabersStartCollide;
            _obstacleSaberSparkleEffectManager.sparkleEffectDidEndEvent += SabersEndCollide;
            _beatmapObjectManager.noteWasCutEvent += NoteWasCut;
            _beatmapObjectManager.noteWasMissedEvent += NoteWasMissed;
            _comboController.comboDidChangeEvent += ComboDidChange;
            _comboController.comboBreakingEventHappenedEvent += ComboDidBreak;
            _scoreController.multiplierDidChangeEvent += MultiplierDidChange;
            _scoreController.scoreDidChangeEvent += ScoreDidChange;
            if (_levelEndActions is null) return;
            _levelEndActions.levelFinishedEvent += LevelFinished;
            _levelEndActions.levelFailedEvent += LevelFailed;
        }

        public void Dispose()
        {
            _beatmapCallbacksController.RemoveBeatmapCallback(_beatmapDataCallbackWrapper);
            _obstacleSaberSparkleEffectManager.sparkleEffectDidStartEvent -= SabersStartCollide;
            _obstacleSaberSparkleEffectManager.sparkleEffectDidEndEvent -= SabersEndCollide;
            _beatmapObjectManager.noteWasCutEvent -= NoteWasCut;
            _beatmapObjectManager.noteWasMissedEvent -= NoteWasMissed;
            _comboController.comboDidChangeEvent -= ComboDidChange;
            _comboController.comboBreakingEventHappenedEvent -= ComboDidBreak;
            _scoreController.multiplierDidChangeEvent -= MultiplierDidChange;
            _scoreController.scoreDidChangeEvent -= ScoreDidChange;
            if (_levelEndActions is null) return;
            _levelEndActions.levelFinishedEvent -= LevelFinished;
            _levelEndActions.levelFailedEvent -= LevelFailed;
        }

        private void BeatmapEventDidTrigger(BasicBeatmapEventData eventData)
        {
            BeatmapEventDidTriggerEvent?.Invoke(eventData);
        }

        private void NoteWasCut(NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            if (noteController.noteData.subtypeIdentifier == _bombSubtypeIdentifier) return;
            AllNotesCountDidChangeEvent?.Invoke(_anyCutCount++, _cuttableNotesCount);
            if (noteCutInfo.allIsOK)
            {
                NoteWasCutEvent?.Invoke(noteCutInfo.saberType);
                GoodCutCountDidChangeEvent?.Invoke(_goodCutCount++);
            }
            else
            {
                BadCutCountDidChangeEvent?.Invoke(_badCutCount++);
            }
        }

        private void NoteWasMissed(NoteController noteController)
        {
            if (noteController.noteData.subtypeIdentifier == _bombSubtypeIdentifier) return;
            NoteWasMissedEvent?.Invoke();
            AllNotesCountDidChangeEvent?.Invoke(_anyCutCount++, _cuttableNotesCount);
            MissCountDidChangeEvent?.Invoke(_missCount++);
        }

        private void MultiplierDidChange(int multiplier, float progress)
        {
            if (multiplier > 1 && progress < 0.1f)
                MultiplierDidIncreaseEvent?.Invoke();
        }

        private void LevelFinished() => LevelFinishedEvent?.Invoke();

        private void LevelFailed() => LevelFailedEvent?.Invoke();

        private void SabersStartCollide(SaberType saberType) => SabersStartCollideEvent?.Invoke();

        private void SabersEndCollide(SaberType saberType) => SabersEndCollideEvent?.Invoke();

        private void ComboDidChange(int combo) => ComboDidChangeEvent?.Invoke(combo);

        private void ComboDidBreak() => ComboDidBreakEvent?.Invoke();

        private void ScoreDidChange(int rawScore, int modifiedScore) => ScoreDidChangeEvent?.Invoke(rawScore, modifiedScore);
    }
}