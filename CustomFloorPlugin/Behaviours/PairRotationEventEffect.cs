using IPA.Utilities;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    public class PairRotationEventEffect : MonoBehaviour, INotifyPlatformEnabled
    {
        [Header("Rotation Effect")]
        public SongEventType eventL;
        public SongEventType eventR;
        public Transform tranformL;
        public Transform tranformR;
        public Vector3 rotationVector;
        [Space]
        public bool useZPositionForAngleOffset;
        public float zPositionAngleOffsetScale = 1f;
        [Space]
        public bool overrideRandomValues;
        public float randomStartRotation;
        public float randomDirection;

        private PlatformManager _platformManager;
        private IBeatmapObjectCallbackController _beatmapObjectCallbackController;

        [Inject]
        public void Construct(PlatformManager platformManager, [InjectOptional] IBeatmapObjectCallbackController beatmapObjectCallbackController)
        {
            _platformManager = platformManager;
            _beatmapObjectCallbackController = beatmapObjectCallbackController;
        }

        void INotifyPlatformEnabled.PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            if (_beatmapObjectCallbackController == null)
                return;
            LightPairRotationEventEffect rotEffect = gameObject.AddComponent<LightPairRotationEventEffect>();
            _platformManager.spawnedComponents.Add(rotEffect);
            rotEffect.SetField("_beatmapObjectCallbackController", _beatmapObjectCallbackController);
            rotEffect.SetField("_eventL", (BeatmapEventType)eventL);
            rotEffect.SetField("_eventR", (BeatmapEventType)eventR);
            rotEffect.SetField("_rotationVector", rotationVector);
            rotEffect.SetField("_transformL", tranformL);
            rotEffect.SetField("_transformR", tranformR);
            rotEffect.SetField("_useZPositionForAngleOffset", useZPositionForAngleOffset);
            rotEffect.SetField("_overrideRandomValues", overrideRandomValues);
            rotEffect.SetField("_randomStartRotation", randomStartRotation);
            rotEffect.SetField("_randomDirection", randomDirection);
        }
    }
}