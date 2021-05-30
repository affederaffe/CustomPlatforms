using IPA.Utilities;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    public class PairRotationEventEffect : MonoBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
    {
        [Header("Rotation Effect")]
        public SongEventType eventL;
        public SongEventType eventR;
        public SongEventType switchOverrideRandomValuesEvent;
        public Transform? transformL;
        public Transform? transformR;
        public Vector3 rotationVector;
        [Space]
        public bool useZPositionForAngleOffset;
        public float zPositionAngleOffsetScale = 1f;

        private IBeatmapObjectCallbackController? _beatmapObjectCallbackController;

        private LightPairRotationEventEffect? _lightPairRotationEventEffect;

        [Inject]
        public void Construct([InjectOptional] IBeatmapObjectCallbackController beatmapObjectCallbackController)
        {
            _beatmapObjectCallbackController = beatmapObjectCallbackController;
        }

        public void PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            if (_beatmapObjectCallbackController is null || transformL is null || transformR is null) return;
            if (_lightPairRotationEventEffect is null)
            {
                _lightPairRotationEventEffect = gameObject.AddComponent<LightPairRotationEventEffect>();
                _lightPairRotationEventEffect.SetField("_eventL", (BeatmapEventType)eventL);
                _lightPairRotationEventEffect.SetField("_eventR", (BeatmapEventType)eventR);
                _lightPairRotationEventEffect.SetField("_switchOverrideRandomValuesEvent", (BeatmapEventType)switchOverrideRandomValuesEvent);
                _lightPairRotationEventEffect.SetField("_rotationVector", rotationVector);
                _lightPairRotationEventEffect.SetField("_transformL", transformL);
                _lightPairRotationEventEffect.SetField("_transformR", transformR);
                _lightPairRotationEventEffect.SetField("_useZPositionForAngleOffset", useZPositionForAngleOffset);
                _lightPairRotationEventEffect.SetField("_zPositionAngleOffsetScale", zPositionAngleOffsetScale);
            }

            _lightPairRotationEventEffect.SetField("_beatmapObjectCallbackController", _beatmapObjectCallbackController);
            _lightPairRotationEventEffect.enabled = true;
        }

        public void PlatformDisabled()
        {
            if (_lightPairRotationEventEffect is null) return;
            _lightPairRotationEventEffect.enabled = false;
        }
    }
}