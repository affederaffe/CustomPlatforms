using System.Linq;

using CustomFloorPlugin.Interfaces;

using IPA.Utilities;

using UnityEngine;

using Zenject;


// ReSharper disable once CheckNamespace
namespace CustomFloorPlugin
{
    public class TrackRings : MonoBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
    {
        [Space]
        [Header("Rings")]
        public GameObject? trackLaneRingPrefab;
        public int ringCount = 10;
        public float ringPositionStep = 2f;
        [Space]
        [Header("Rotation Effect")]
        public bool useRotationEffect;
        public SongEventType rotationSongEventType = SongEventType.RingsRotationEffect;
        [Space]
        public float rotationStep = 5f;
        public int rotationPropagationSpeed = 1;
        public float rotationFlexySpeed = 1f;
        [Space]
        public float startupRotationAngle;
        public float startupRotationStep = 10f;
        public int startupRotationPropagationSpeed = 10;
        public float startupRotationFlexySpeed = 0.5f;
        [Space]
        [Header("Step Effect")]
        public bool useStepEffect;
        public SongEventType stepSongEventType = SongEventType.RingsStepEffect;
        [Space]
        public float minPositionStep = 1f;
        public float maxPositionStep = 2f;
        public float moveSpeed = 1f;

        private MaterialSwapper? _materialSwapper;
        private BeatmapCallbacksController? _beatmapCallbacksController;

        private TrackLaneRingsManager? _trackLaneRingsManager;
        private TrackLaneRingsRotationEffectSpawner? _trackLaneRingsRotationEffectSpawner;
        private TrackLaneRingsPositionStepEffectSpawner? _trackLaneRingsPositionStepEffectSpawner;

        [Inject]
        public void Construct(MaterialSwapper materialSwapper, [InjectOptional] BeatmapCallbacksController beatmapCallbacksController)
        {
            _materialSwapper = materialSwapper;
            _beatmapCallbacksController = beatmapCallbacksController;
        }

        // ReSharper disable once CognitiveComplexity
        public void PlatformEnabled(DiContainer container)
        {
            if (trackLaneRingPrefab is null)
                return;
            container.Inject(this);

            if (_trackLaneRingsManager is null)
            {
                _materialSwapper!.ReplaceMaterials(trackLaneRingPrefab);
                gameObject.SetActive(false);
                TrackLaneRing trackLaneRing = trackLaneRingPrefab.AddComponent<TrackLaneRing>();
                _trackLaneRingsManager = gameObject.AddComponent<TrackLaneRingsManager>();
                _trackLaneRingsManager._trackLaneRingPrefab = trackLaneRing;
                _trackLaneRingsManager.SetField("_container", container);
                _trackLaneRingsManager._ringCount = ringCount;
                _trackLaneRingsManager._ringPositionStep = ringPositionStep;
                _trackLaneRingsManager._spawnAsChildren = true;
                gameObject.SetActive(true);
                foreach (INotifyPlatformEnabled? notifyEnable in GetComponentsInChildren<INotifyPlatformEnabled>(true).Where(x => !ReferenceEquals(x, this)))
                    notifyEnable?.PlatformEnabled(container);
            }

            if (useRotationEffect)
            {
                if (_trackLaneRingsRotationEffectSpawner is null)
                {
                    TrackLaneRingsRotationEffect trackLaneRingsRotationEffect = gameObject.AddComponent<TrackLaneRingsRotationEffect>();
                    trackLaneRingsRotationEffect._trackLaneRingsManager = _trackLaneRingsManager;
                    trackLaneRingsRotationEffect._startupRotationAngle = startupRotationAngle;
                    trackLaneRingsRotationEffect._startupRotationStep = startupRotationStep;
                    int timePerRing = startupRotationPropagationSpeed / ringCount;
                    float ringsPerFrame = Time.fixedDeltaTime / timePerRing;
                    trackLaneRingsRotationEffect._startupRotationPropagationSpeed = Mathf.Max((int)ringsPerFrame, 1);
                    trackLaneRingsRotationEffect._startupRotationFlexySpeed = startupRotationFlexySpeed;

                    _trackLaneRingsRotationEffectSpawner = gameObject.AddComponent<TrackLaneRingsRotationEffectSpawner>();
                    _trackLaneRingsRotationEffectSpawner.enabled = _beatmapCallbacksController is not null;
                    _trackLaneRingsRotationEffectSpawner.SetField("_beatmapCallbacksController", _beatmapCallbacksController);
                    _trackLaneRingsRotationEffectSpawner._beatmapEventType = (BasicBeatmapEventType)rotationSongEventType;
                    _trackLaneRingsRotationEffectSpawner._rotationStep = rotationStep;
                    int timePerRing2 = rotationPropagationSpeed / ringCount;
                    float ringsPerFrame2 = Time.fixedDeltaTime / timePerRing2;
                    _trackLaneRingsRotationEffectSpawner._rotationPropagationSpeed = Mathf.Max((int)ringsPerFrame2, 1);
                    _trackLaneRingsRotationEffectSpawner._rotationFlexySpeed = rotationFlexySpeed;
                    _trackLaneRingsRotationEffectSpawner._trackLaneRingsRotationEffect = trackLaneRingsRotationEffect;
                }
                else if (_beatmapCallbacksController is not null)
                {
                    _trackLaneRingsRotationEffectSpawner.enabled = true;
                    _trackLaneRingsRotationEffectSpawner.SetField("_beatmapCallbacksController", _beatmapCallbacksController);
                    _trackLaneRingsRotationEffectSpawner.Start();
                }
            }

            if (useStepEffect)
            {
                if (_trackLaneRingsPositionStepEffectSpawner is null)
                {
                    _trackLaneRingsPositionStepEffectSpawner = gameObject.AddComponent<TrackLaneRingsPositionStepEffectSpawner>();
                    _trackLaneRingsPositionStepEffectSpawner.enabled = _beatmapCallbacksController is not null;
                    _trackLaneRingsPositionStepEffectSpawner._trackLaneRingsManager = _trackLaneRingsManager;
                    _trackLaneRingsPositionStepEffectSpawner.SetField("_beatmapCallbacksController", _beatmapCallbacksController);
                    _trackLaneRingsPositionStepEffectSpawner._beatmapEventType = (BasicBeatmapEventType)stepSongEventType;
                    _trackLaneRingsPositionStepEffectSpawner._minPositionStep = minPositionStep;
                    _trackLaneRingsPositionStepEffectSpawner._maxPositionStep = maxPositionStep;
                    _trackLaneRingsPositionStepEffectSpawner._moveSpeed = moveSpeed;
                }
                else if (_beatmapCallbacksController is not null)
                {
                    _trackLaneRingsPositionStepEffectSpawner.enabled = true;
                    _trackLaneRingsPositionStepEffectSpawner.SetField("_beatmapCallbacksController", _beatmapCallbacksController);
                    _trackLaneRingsPositionStepEffectSpawner.Start();
                }
            }
        }

        public void PlatformDisabled()
        {
            _trackLaneRingsRotationEffectSpawner?.OnDestroy();
            _trackLaneRingsPositionStepEffectSpawner?.OnDestroy();
        }
    }
}
