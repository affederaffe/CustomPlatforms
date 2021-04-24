using IPA.Utilities;

using UnityEngine;

using Zenject;


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
        private IBeatmapObjectCallbackController? _beatmapObjectCallbackController;

        private TrackLaneRingsManager? _trackLaneRingsManager;
        private TrackLaneRingsRotationEffectSpawner? _trackLaneRingsRotationEffectSpawner;
        private TrackLaneRingsPositionStepEffectSpawner? _trackLaneRingsPositionStepEffectSpawner;

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.blue;
            for (int i = 0; i < ringCount; i++)
            {
                Vector3 zOffset = i * ringPositionStep * Vector3.forward;
                if (trackLaneRingPrefab != null)
                {
                    foreach (Renderer r in trackLaneRingPrefab.GetComponentsInChildren<Renderer>())
                    {
                        Bounds bounds = r.bounds;
                        Gizmos.DrawCube(zOffset + bounds.center, bounds.size);
                    }
                }
                else
                {
                    Gizmos.DrawCube(zOffset, Vector3.one * 0.5f);
                }
            }
        }

        [Inject]
        public void Construct(MaterialSwapper materialSwapper, [InjectOptional] IBeatmapObjectCallbackController beatmapObjectCallbackController)
        {
            _materialSwapper = materialSwapper;
            _beatmapObjectCallbackController = beatmapObjectCallbackController;
        }

        public void PlatformEnabled(DiContainer container)
        {
            if (trackLaneRingPrefab == null) return;
            container.Inject(this);

            if (_trackLaneRingsManager == null)
            { 
                _materialSwapper!.ReplaceMaterials(trackLaneRingPrefab);
                gameObject.SetActive(false);
                TrackLaneRing trackLaneRing = trackLaneRingPrefab.AddComponent<TrackLaneRing>();
                _trackLaneRingsManager = gameObject.AddComponent<TrackLaneRingsManager>();
                _trackLaneRingsManager.SetField("_trackLaneRingPrefab", trackLaneRing);
                _trackLaneRingsManager.SetField("_ringCount", ringCount);
                _trackLaneRingsManager.SetField("_ringPositionStep", ringPositionStep);
                _trackLaneRingsManager.SetField("_spawnAsChildren", true);
            }

            if (useRotationEffect && _trackLaneRingsRotationEffectSpawner != null)
            {
                _trackLaneRingsRotationEffectSpawner.SetField("_beatmapObjectCallbackController", _beatmapObjectCallbackController);
                try { _trackLaneRingsRotationEffectSpawner.Start(); } catch { }
            }
            else if (useRotationEffect)
            {
                TrackLaneRingsRotationEffect trackLaneRingsRotationEffect = gameObject.AddComponent<TrackLaneRingsRotationEffect>();
                trackLaneRingsRotationEffect.SetField("_trackLaneRingsManager", _trackLaneRingsManager);
                trackLaneRingsRotationEffect.SetField("_startupRotationAngle", startupRotationAngle);
                trackLaneRingsRotationEffect.SetField("_startupRotationStep", startupRotationStep);
                int timePerRing = startupRotationPropagationSpeed / ringCount;
                float ringsPerFrame = Time.fixedDeltaTime / timePerRing;
                trackLaneRingsRotationEffect.SetField("_startupRotationPropagationSpeed", Mathf.Max((int)ringsPerFrame, 1));
                trackLaneRingsRotationEffect.SetField("_startupRotationFlexySpeed", startupRotationFlexySpeed);

                _trackLaneRingsRotationEffectSpawner = gameObject.AddComponent<TrackLaneRingsRotationEffectSpawner>();
                _trackLaneRingsRotationEffectSpawner.SetField("_beatmapObjectCallbackController", _beatmapObjectCallbackController);
                _trackLaneRingsRotationEffectSpawner.SetField("_beatmapEventType", (BeatmapEventType)rotationSongEventType);
                _trackLaneRingsRotationEffectSpawner.SetField("_rotationStep", rotationStep);
                int timePerRing2 = rotationPropagationSpeed / ringCount;
                float ringsPerFrame2 = Time.fixedDeltaTime / timePerRing2;
                _trackLaneRingsRotationEffectSpawner.SetField("_rotationPropagationSpeed", Mathf.Max((int)ringsPerFrame2, 1));
                _trackLaneRingsRotationEffectSpawner.SetField("_rotationFlexySpeed", rotationFlexySpeed);
                _trackLaneRingsRotationEffectSpawner.SetField("_trackLaneRingsRotationEffect", trackLaneRingsRotationEffect);
            }

            if (useStepEffect && _trackLaneRingsPositionStepEffectSpawner != null)
            {
                _trackLaneRingsPositionStepEffectSpawner.SetField("_beatmapObjectCallbackController", _beatmapObjectCallbackController);
                try { _trackLaneRingsPositionStepEffectSpawner.Start(); } catch { }
            }
            else if (useStepEffect)
            {
                _trackLaneRingsPositionStepEffectSpawner = gameObject.AddComponent<TrackLaneRingsPositionStepEffectSpawner>();
                _trackLaneRingsPositionStepEffectSpawner.SetField("_beatmapObjectCallbackController", _beatmapObjectCallbackController);
                _trackLaneRingsPositionStepEffectSpawner.SetField("_trackLaneRingsManager", _trackLaneRingsManager);
                _trackLaneRingsPositionStepEffectSpawner.SetField("_beatmapEventType", (BeatmapEventType)stepSongEventType);
                _trackLaneRingsPositionStepEffectSpawner.SetField("_minPositionStep", minPositionStep);
                _trackLaneRingsPositionStepEffectSpawner.SetField("_maxPositionStep", maxPositionStep);
                _trackLaneRingsPositionStepEffectSpawner.SetField("_moveSpeed", moveSpeed);
            }

            gameObject.SetActive(true);

            foreach (INotifyPlatformEnabled notifyEnable in GetComponentsInChildren<INotifyPlatformEnabled>(true))
            {
                if ((Object)notifyEnable != this)
                    notifyEnable.PlatformEnabled(container);
            }
        }

        public void PlatformDisabled()
        {
            if (_trackLaneRingsRotationEffectSpawner != null) _trackLaneRingsRotationEffectSpawner.OnDestroy();
            if (_trackLaneRingsPositionStepEffectSpawner != null) _trackLaneRingsPositionStepEffectSpawner.OnDestroy();

            foreach (INotifyPlatformDisabled notifyDisable in GetComponentsInChildren<INotifyPlatformDisabled>(true))
            {
                if ((Object)notifyDisable != this)
                    notifyDisable.PlatformDisabled();
            }
        }
    }
}