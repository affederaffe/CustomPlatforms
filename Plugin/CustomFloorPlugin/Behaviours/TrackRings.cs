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

        private static readonly FieldAccessor<TrackLaneRingsManager, TrackLaneRing>.Accessor _trackLaneRingsManagerPrefabAccessor = FieldAccessor<TrackLaneRingsManager, TrackLaneRing>.GetAccessor("_trackLaneRingPrefab");
        private static readonly FieldAccessor<TrackLaneRingsManager, DiContainer>.Accessor _trackLaneRingsManagerContainerAccessor = FieldAccessor<TrackLaneRingsManager, DiContainer>.GetAccessor("_container");
        private static readonly FieldAccessor<TrackLaneRingsManager, int>.Accessor _ringCountAccessor = FieldAccessor<TrackLaneRingsManager, int>.GetAccessor("_ringCount");
        private static readonly FieldAccessor<TrackLaneRingsManager, float>.Accessor _ringPositionStepAccessor = FieldAccessor<TrackLaneRingsManager, float>.GetAccessor("_ringPositionStep");
        private static readonly FieldAccessor<TrackLaneRingsManager, bool>.Accessor _spawnAsChildrenAccessor = FieldAccessor<TrackLaneRingsManager, bool>.GetAccessor("_spawnAsChildren");
        private static readonly FieldAccessor<TrackLaneRingsRotationEffect, TrackLaneRingsManager>.Accessor _rotationEffectTrackLaneRingsManagerAccessor = FieldAccessor<TrackLaneRingsRotationEffect, TrackLaneRingsManager>.GetAccessor("_trackLaneRingsManager");
        private static readonly FieldAccessor<TrackLaneRingsRotationEffect, float>.Accessor _startupRotationAngleAccessor = FieldAccessor<TrackLaneRingsRotationEffect, float>.GetAccessor("_startupRotationAngle");
        private static readonly FieldAccessor<TrackLaneRingsRotationEffect, float>.Accessor _startupRotationStepAccessor = FieldAccessor<TrackLaneRingsRotationEffect, float>.GetAccessor("_startupRotationStep");
        private static readonly FieldAccessor<TrackLaneRingsRotationEffect, int>.Accessor _startupRotationPropagationSpeedAccessor = FieldAccessor<TrackLaneRingsRotationEffect, int>.GetAccessor("_startupRotationPropagationSpeed");
        private static readonly FieldAccessor<TrackLaneRingsRotationEffect, float>.Accessor _startupRotationFlexySpeedAccessor = FieldAccessor<TrackLaneRingsRotationEffect, float>.GetAccessor("_startupRotationFlexySpeed");
        private static readonly FieldAccessor<TrackLaneRingsRotationEffectSpawner, BeatmapCallbacksController?>.Accessor _rotationEffectSpawnerBeatmapCallbacksControllerAccessor = FieldAccessor<TrackLaneRingsRotationEffectSpawner, BeatmapCallbacksController?>.GetAccessor("_beatmapCallbacksController");
        private static readonly FieldAccessor<TrackLaneRingsRotationEffectSpawner, BasicBeatmapEventType>.Accessor _rotationEffectSpawnerBeatmapEventTypeAccessor = FieldAccessor<TrackLaneRingsRotationEffectSpawner, BasicBeatmapEventType>.GetAccessor("_beatmapEventType");
        private static readonly FieldAccessor<TrackLaneRingsRotationEffectSpawner, float>.Accessor _rotationStepAccessor = FieldAccessor<TrackLaneRingsRotationEffectSpawner, float>.GetAccessor("_rotationStep");
        private static readonly FieldAccessor<TrackLaneRingsRotationEffectSpawner, int>.Accessor _rotationPropagationSpeedAccessor = FieldAccessor<TrackLaneRingsRotationEffectSpawner, int>.GetAccessor("_rotationPropagationSpeed");
        private static readonly FieldAccessor<TrackLaneRingsRotationEffectSpawner, float>.Accessor _rotationFlexySpeedAccessor = FieldAccessor<TrackLaneRingsRotationEffectSpawner, float>.GetAccessor("_rotationFlexySpeed");
        private static readonly FieldAccessor<TrackLaneRingsRotationEffectSpawner, TrackLaneRingsRotationEffect>.Accessor _trackLaneRingsRotationEffectAccessor = FieldAccessor<TrackLaneRingsRotationEffectSpawner, TrackLaneRingsRotationEffect>.GetAccessor("_trackLaneRingsRotationEffect");
        private static readonly FieldAccessor<TrackLaneRingsPositionStepEffectSpawner, TrackLaneRingsManager>.Accessor _positionStepEffectSpawnerTrackLaneRingsManagerAccessor = FieldAccessor<TrackLaneRingsPositionStepEffectSpawner, TrackLaneRingsManager>.GetAccessor("_trackLaneRingsManager");
        private static readonly FieldAccessor<TrackLaneRingsPositionStepEffectSpawner, BeatmapCallbacksController?>.Accessor _positionStepEffectSpawnerBeatmapCallbacksControllerAccessor = FieldAccessor<TrackLaneRingsPositionStepEffectSpawner, BeatmapCallbacksController?>.GetAccessor("_beatmapCallbacksController");
        private static readonly FieldAccessor<TrackLaneRingsPositionStepEffectSpawner, BasicBeatmapEventType>.Accessor _positionStepEffectSpawnerBeatmapEventTypeAccessor = FieldAccessor<TrackLaneRingsPositionStepEffectSpawner, BasicBeatmapEventType>.GetAccessor("_beatmapEventType");
        private static readonly FieldAccessor<TrackLaneRingsPositionStepEffectSpawner, float>.Accessor _minPositionStepAccessor = FieldAccessor<TrackLaneRingsPositionStepEffectSpawner, float>.GetAccessor("_minPositionStep");
        private static readonly FieldAccessor<TrackLaneRingsPositionStepEffectSpawner, float>.Accessor _maxPositionStepAccessor = FieldAccessor<TrackLaneRingsPositionStepEffectSpawner, float>.GetAccessor("_maxPositionStep");
        private static readonly FieldAccessor<TrackLaneRingsPositionStepEffectSpawner, float>.Accessor _moveSpeedAccessor = FieldAccessor<TrackLaneRingsPositionStepEffectSpawner, float>.GetAccessor("_moveSpeed");

        [Inject]
        public void Construct(MaterialSwapper materialSwapper, [InjectOptional] BeatmapCallbacksController beatmapCallbacksController)
        {
            _materialSwapper = materialSwapper;
            _beatmapCallbacksController = beatmapCallbacksController;
        }

        // ReSharper disable once CognitiveComplexity
        public void PlatformEnabled(DiContainer container)
        {
            if (trackLaneRingPrefab is null) return;
            container.Inject(this);

            if (_trackLaneRingsManager is null)
            {
                _materialSwapper!.ReplaceMaterials(trackLaneRingPrefab);
                gameObject.SetActive(false);
                TrackLaneRing trackLaneRing = trackLaneRingPrefab.AddComponent<TrackLaneRing>();
                _trackLaneRingsManager = gameObject.AddComponent<TrackLaneRingsManager>();
                _trackLaneRingsManagerPrefabAccessor(ref _trackLaneRingsManager) = trackLaneRing;
                _trackLaneRingsManagerContainerAccessor(ref _trackLaneRingsManager) = container;
                _ringCountAccessor(ref _trackLaneRingsManager) = ringCount;
                _ringPositionStepAccessor(ref _trackLaneRingsManager) = ringPositionStep;
                _spawnAsChildrenAccessor(ref _trackLaneRingsManager) = true;
                gameObject.SetActive(true);
                foreach (INotifyPlatformEnabled? notifyEnable in GetComponentsInChildren<INotifyPlatformEnabled>(true).Where(x => !ReferenceEquals(x, this)))
                    notifyEnable?.PlatformEnabled(container);
            }

            if (useRotationEffect)
            {
                if (_trackLaneRingsRotationEffectSpawner is null)
                {
                    TrackLaneRingsRotationEffect trackLaneRingsRotationEffect = gameObject.AddComponent<TrackLaneRingsRotationEffect>();
                    _rotationEffectTrackLaneRingsManagerAccessor(ref trackLaneRingsRotationEffect) = _trackLaneRingsManager;
                    _startupRotationAngleAccessor(ref trackLaneRingsRotationEffect) = startupRotationAngle;
                    _startupRotationStepAccessor(ref trackLaneRingsRotationEffect) = startupRotationStep;
                    int timePerRing = startupRotationPropagationSpeed / ringCount;
                    float ringsPerFrame = Time.fixedDeltaTime / timePerRing;
                    _startupRotationPropagationSpeedAccessor(ref trackLaneRingsRotationEffect) = Mathf.Max((int)ringsPerFrame, 1);
                    _startupRotationFlexySpeedAccessor(ref trackLaneRingsRotationEffect) = startupRotationFlexySpeed;

                    _trackLaneRingsRotationEffectSpawner = gameObject.AddComponent<TrackLaneRingsRotationEffectSpawner>();
                    _trackLaneRingsRotationEffectSpawner.enabled = _beatmapCallbacksController is not null;
                    _rotationEffectSpawnerBeatmapCallbacksControllerAccessor(ref _trackLaneRingsRotationEffectSpawner) = _beatmapCallbacksController;
                    _rotationEffectSpawnerBeatmapEventTypeAccessor(ref _trackLaneRingsRotationEffectSpawner) = (BasicBeatmapEventType)rotationSongEventType;
                    _rotationStepAccessor(ref _trackLaneRingsRotationEffectSpawner) = rotationStep;
                    int timePerRing2 = rotationPropagationSpeed / ringCount;
                    float ringsPerFrame2 = Time.fixedDeltaTime / timePerRing2;
                    _rotationPropagationSpeedAccessor(ref _trackLaneRingsRotationEffectSpawner) = Mathf.Max((int)ringsPerFrame2, 1);
                    _rotationFlexySpeedAccessor(ref _trackLaneRingsRotationEffectSpawner) = rotationFlexySpeed;
                    _trackLaneRingsRotationEffectAccessor(ref _trackLaneRingsRotationEffectSpawner) = trackLaneRingsRotationEffect;
                }
                else if (_beatmapCallbacksController is not null)
                {
                    _trackLaneRingsRotationEffectSpawner.enabled = true;
                    _rotationEffectSpawnerBeatmapCallbacksControllerAccessor(ref _trackLaneRingsRotationEffectSpawner) = _beatmapCallbacksController;
                    _trackLaneRingsRotationEffectSpawner.Start();
                }
            }

            if (useStepEffect)
            {
                if (_trackLaneRingsPositionStepEffectSpawner is null)
                {
                    _trackLaneRingsPositionStepEffectSpawner = gameObject.AddComponent<TrackLaneRingsPositionStepEffectSpawner>();
                    _trackLaneRingsPositionStepEffectSpawner.enabled = _beatmapCallbacksController is not null;
                    _positionStepEffectSpawnerTrackLaneRingsManagerAccessor(ref _trackLaneRingsPositionStepEffectSpawner) = _trackLaneRingsManager;
                    _positionStepEffectSpawnerBeatmapCallbacksControllerAccessor(ref _trackLaneRingsPositionStepEffectSpawner) = _beatmapCallbacksController;
                    _positionStepEffectSpawnerBeatmapEventTypeAccessor(ref _trackLaneRingsPositionStepEffectSpawner) = (BasicBeatmapEventType)stepSongEventType;
                    _minPositionStepAccessor(ref _trackLaneRingsPositionStepEffectSpawner) = minPositionStep;
                    _maxPositionStepAccessor(ref _trackLaneRingsPositionStepEffectSpawner) = maxPositionStep;
                    _moveSpeedAccessor(ref _trackLaneRingsPositionStepEffectSpawner) = moveSpeed;
                }
                else if (_beatmapCallbacksController is not null)
                {
                    _trackLaneRingsPositionStepEffectSpawner.enabled = true;
                    _positionStepEffectSpawnerBeatmapCallbacksControllerAccessor(ref _trackLaneRingsPositionStepEffectSpawner) = _beatmapCallbacksController;
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
