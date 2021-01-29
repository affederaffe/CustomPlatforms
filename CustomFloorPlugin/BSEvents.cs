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

        public event Action<ScenesTransitionSetupDataSO, DiContainer> GameSceneLoaded = delegate { };
        public event Action LevelFinished = delegate { };
        public event Action LevelFailed = delegate { };
        public event Action<NoteController, NoteCutInfo> NoteWasCutEvent = delegate { };
        public event Action<NoteController> NoteWasMissedEvent = delegate { };
        public event Action ComboDidBreak = delegate { };
        public event Action<int> MultiplierDidIncrease = delegate { };
        public event Action<int> ComboDidChange = delegate { };
        public event Action<SaberType> SabersStartCollide = delegate { };
        public event Action<SaberType> SabersEndCollide = delegate { };
        public event Action<BeatmapEventData> BeatmapEvent = delegate { };

        public void Initialize() {
            _beatmapObjectCallbackController.beatmapEventDidTriggerEvent += BeatmapEvent;
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
            _beatmapObjectCallbackController.beatmapEventDidTriggerEvent -= BeatmapEvent;
            _beatmapObjectManager.noteWasCutEvent -= NoteWasCut;
            _beatmapObjectManager.noteWasMissedEvent -= NoteWasMissed;
            _gameEnergyCounter.gameEnergyDidReach0Event -= LevelFailed;
            _obstacleSaberSparkleEffectManager.sparkleEffectDidStartEvent -= SabersStartCollide;
            _obstacleSaberSparkleEffectManager.sparkleEffectDidEndEvent -= SabersEndCollide;
            _scoreController.comboDidChangeEvent -= ComboDidChange;
            _scoreController.comboBreakingEventHappenedEvent -= ComboDidBreak;
            _scoreController.multiplierDidChangeEvent -= MultiplierDidChange;
        }

        private void MultiplierDidChange(int multiplier, float progress) {
            if (multiplier > 1 && progress < 0.1f) {
                MultiplierDidIncrease(multiplier);
            }
        }

        private void NoteWasCut(NoteController noteController, NoteCutInfo noteCutInfo) {
            NoteWasCutEvent(noteController, noteCutInfo);
            if (Mathf.Approximately(noteController.noteData.time, _lastNoteTime)) {
                _lastNoteTime = 0f;
                LevelFinished();
            }
        }

        private void NoteWasMissed(NoteController noteController) {
            NoteWasMissedEvent(noteController);
            if (Mathf.Approximately(noteController.noteData.time, _lastNoteTime)) {
                _lastNoteTime = 0f;
                LevelFinished();
            }
        }
    }
}
