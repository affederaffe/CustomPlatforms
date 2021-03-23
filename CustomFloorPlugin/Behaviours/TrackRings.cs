using IPA.Utilities;

using SiraUtil.Tools;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    public class TrackRings : MonoBehaviour, INotifyPlatformEnabled
    {
        [Space]
        [Header("Rings")]
        public GameObject trackLaneRingPrefab;
        public int ringCount = 10;
        public float ringPositionStep = 2f;
        [Space]
        [Header("Rotation Effect")]
        public bool useRotationEffect = false;
        public SongEventType rotationSongEventType = SongEventType.RingsRotationEffect;
        [Space]
        public float rotationStep = 5f;
        public int rotationPropagationSpeed = 1;
        public float rotationFlexySpeed = 1f;
        [Space]
        public float startupRotationAngle = 0f;
        public float startupRotationStep = 10f;
        public int startupRotationPropagationSpeed = 10;
        public float startupRotationFlexySpeed = 0.5f;
        [Space]
        [Header("Step Effect")]
        public bool useStepEffect = false;
        public SongEventType stepSongEventType = SongEventType.RingsStepEffect;
        [Space]
        public float minPositionStep = 1f;
        public float maxPositionStep = 2f;
        public float moveSpeed = 1f;

        private SiraLog _siraLog;
        private MaterialSwapper _materialSwapper;
        private PlatformManager _platformManager;
        private IBeatmapObjectCallbackController _beatmapObjectCallbackController;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.blue;
            Vector3 zOffset;
            for (int i = 0; i < ringCount; i++)
            {
                zOffset = i * ringPositionStep * Vector3.forward;
                if (trackLaneRingPrefab != null)
                {
                    foreach (Renderer r in trackLaneRingPrefab.GetComponentsInChildren<Renderer>())
                    {
                        Gizmos.DrawCube(zOffset + r.bounds.center, r.bounds.size);
                    }
                }
                else
                {
                    Gizmos.DrawCube(zOffset, Vector3.one * 0.5f);
                }
            }
        }

        [Inject]
        public void Construct(SiraLog siraLog, MaterialSwapper materialSwapper, PlatformManager platformManager, [InjectOptional] IBeatmapObjectCallbackController beatmapObjectCallbackController)
        {
            _siraLog = siraLog;
            _materialSwapper = materialSwapper;
            _platformManager = platformManager;
            _beatmapObjectCallbackController = beatmapObjectCallbackController;
        }

        void INotifyPlatformEnabled.PlatformEnabled(DiContainer container)
        {
            bool active = gameObject.activeSelf;
            gameObject.SetActive(false);
            container.Inject(this);
            _materialSwapper.ReplaceMaterials(trackLaneRingPrefab);
            TrackLaneRingsManager ringsManager = gameObject.AddComponent<TrackLaneRingsManager>();
            _platformManager.spawnedObjects.Add(ringsManager);

            TrackLaneRing trackRing = trackLaneRingPrefab.AddComponent<TrackLaneRing>();
            _platformManager.spawnedObjects.Add(trackRing);
            ringsManager.SetField("_trackLaneRingPrefab", trackRing);
            ringsManager.SetField("_ringCount", ringCount);
            ringsManager.SetField("_ringPositionStep", ringPositionStep);
            ringsManager.SetField("_spawnAsChildren", true);

            // Check if a TrackRing is using an in-scene GameObject rather than a prefab, which would result in spawning TubeLights twice.
            // (looking at you, JumpGate)
            // Sometimes I just want to scrap everything :)
            if (trackLaneRingPrefab.scene.name != null)
            {
                foreach (var tubeLight in trackLaneRingPrefab.GetComponentsInChildren<TubeLight>())
                {
                    while (tubeLight.transform.childCount != 0)
                    {
                        DestroyImmediate(tubeLight.transform.GetChild(0).gameObject);
                    }
                    _siraLog.Warning("Using GameObjects as rings is deprecated. Please use Prefabs instead and re-export your Platform!");
                }
            }

            gameObject.SetActive(active);

            if (useRotationEffect)
            {
                TrackLaneRingsRotationEffect rotationEffect = gameObject.AddComponent<TrackLaneRingsRotationEffect>();
                _platformManager.spawnedObjects.Add(rotationEffect);
                rotationEffect.SetField("_trackLaneRingsManager", ringsManager);
                rotationEffect.SetField("_startupRotationAngle", startupRotationAngle);
                rotationEffect.SetField("_startupRotationStep", startupRotationStep);
                int timePerRing = startupRotationPropagationSpeed / ringCount;
                float ringsPerFrame = Time.fixedDeltaTime / timePerRing;
                rotationEffect.SetField("_startupRotationPropagationSpeed", Mathf.Max((int)ringsPerFrame, 1));
                rotationEffect.SetField("_startupRotationFlexySpeed", startupRotationFlexySpeed);

                TrackLaneRingsRotationEffectSpawner rotationEffectSpawner = gameObject.AddComponent<TrackLaneRingsRotationEffectSpawner>();
                _platformManager.spawnedObjects.Add(rotationEffectSpawner);
                rotationEffectSpawner.SetField("_beatmapObjectCallbackController", _beatmapObjectCallbackController);

                rotationEffectSpawner.SetField("_beatmapEventType", (BeatmapEventType)rotationSongEventType);
                rotationEffectSpawner.SetField("_rotationStep", rotationStep);
                int timePerRing2 = rotationPropagationSpeed / ringCount;
                float ringsPerFrame2 = Time.fixedDeltaTime / timePerRing2;
                rotationEffectSpawner.SetField("_rotationPropagationSpeed", Mathf.Max((int)ringsPerFrame2, 1));
                rotationEffectSpawner.SetField("_rotationFlexySpeed", rotationFlexySpeed);
                rotationEffectSpawner.SetField("_trackLaneRingsRotationEffect", rotationEffect);
            }
            if (useStepEffect)
            {
                TrackLaneRingsPositionStepEffectSpawner stepEffectSpawner = gameObject.AddComponent<TrackLaneRingsPositionStepEffectSpawner>();
                _platformManager.spawnedObjects.Add(stepEffectSpawner);
                stepEffectSpawner.SetField("_beatmapObjectCallbackController", _beatmapObjectCallbackController);
                stepEffectSpawner.SetField("_trackLaneRingsManager", ringsManager);
                stepEffectSpawner.SetField("_beatmapEventType", (BeatmapEventType)stepSongEventType);
                stepEffectSpawner.SetField("_minPositionStep", minPositionStep);
                stepEffectSpawner.SetField("_maxPositionStep", maxPositionStep);
                stepEffectSpawner.SetField("_moveSpeed", moveSpeed);
            }

            // If the GameObject was inactive at start, the rings won't be created, thus Rings could be null
            if (ringsManager.Rings != null)
            {
                foreach (TrackLaneRing ring in ringsManager.Rings)
                {
                    _platformManager.spawnedObjects.Add(ring.gameObject);
                    foreach (INotifyPlatformEnabled notifyEnable in ring.GetComponentsInChildren<INotifyPlatformEnabled>(true))
                    {
                        notifyEnable.PlatformEnabled(container);
                    }
                }
            }
        }
    }
}