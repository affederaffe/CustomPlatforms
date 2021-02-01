using System;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin {


    public class BSEvents : IInitializable, IDisposable {

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

        [Inject(Id = "LastNoteId")]
        private float _lastNoteTime;

        public event Action GameSceneLoadedEvent = delegate { };
        public event Action LevelFinishedEvent = delegate { };
        public event Action LevelFailedEvent = delegate { };
        public event Action NoteWasCutEvent = delegate { };
        public event Action<NoteController> NoteWasMissedEvent = delegate { };
        public event Action ComboDidBreakEvent = delegate { };
        public event Action MultiplierDidIncreaseEvent = delegate { };
        public event Action<int> ComboDidChangeEvent = delegate { };
        public event Action SabersStartCollideEvent = delegate { };
        public event Action SabersEndCollideEvent = delegate { };
        public event Action<BeatmapEventData> BeatmapEventDidTriggerEvent = delegate { };

        public void Initialize() {
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
        }

        public void Dispose() {
            _beatmapObjectCallbackController.beatmapEventDidTriggerEvent -= BeatmapEventDidTrigger;
            _beatmapObjectManager.noteWasCutEvent -= NoteWasCut;
            _beatmapObjectManager.noteWasMissedEvent -= NoteWasMissed;
            _gameEnergyCounter.gameEnergyDidReach0Event -= LevelFailed;
            _obstacleSaberSparkleEffectManager.sparkleEffectDidStartEvent -= SabersStartCollide;
            _obstacleSaberSparkleEffectManager.sparkleEffectDidEndEvent -= SabersEndCollide;
            _scoreController.comboDidChangeEvent -= ComboDidChange;
            _scoreController.comboBreakingEventHappenedEvent -= ComboDidBreak;
            _scoreController.multiplierDidChangeEvent -= MultiplierDidChange;
        }

        private void BeatmapEventDidTrigger(BeatmapEventData eventData) {
            BeatmapEventDidTriggerEvent(eventData);
        }

        private void NoteWasCut(NoteController noteController, NoteCutInfo noteCutInfo) {
            if (noteCutInfo.allIsOK) {
                NoteWasCutEvent();
            }
            if (Mathf.Approximately(noteController.noteData.time, _lastNoteTime)) {
                _lastNoteTime = 0f;
                LevelFinishedEvent();
            }
        }

        private void NoteWasMissed(NoteController noteController) {
            NoteWasMissedEvent(noteController);
            if (Mathf.Approximately(noteController.noteData.time, _lastNoteTime)) {
                _lastNoteTime = 0f;
                LevelFinishedEvent();
            }
        }

        private void LevelFailed() {
            LevelFailedEvent();
        }

        private void GameSceneLoaded(ScenesTransitionSetupDataSO setupData, DiContainer container) {
            GameSceneLoadedEvent();
        }

        private void SabersStartCollide(SaberType saberType) {
            SabersStartCollideEvent();
        }

        private void SabersEndCollide(SaberType saberType) {
            SabersEndCollideEvent();
        }

        private void ComboDidChange(int combo) {
            ComboDidChangeEvent(combo);
        }

        private void ComboDidBreak() {
            ComboDidBreakEvent();
        }

        private void MultiplierDidChange(int multiplier, float progress) {
            if (multiplier > 1 && progress < 0.1f) {
                MultiplierDidIncreaseEvent();
            }
        }
    }
}
