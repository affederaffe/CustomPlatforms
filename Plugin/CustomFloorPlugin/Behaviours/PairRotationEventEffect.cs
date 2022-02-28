using CustomFloorPlugin.Interfaces;
using IPA.Utilities;

using UnityEngine;

using Zenject;


// ReSharper disable once CheckNamespace
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

        private BeatmapCallbacksController? _beatmapCallbacksController;

        private LightPairRotationEventEffect? _lightPairRotationEventEffect;
        private Quaternion _startRotL;
        private Quaternion _startRotR;

        [Inject]
        public void Construct([InjectOptional] BeatmapCallbacksController beatmapCallbacksController)
        {
            _beatmapCallbacksController = beatmapCallbacksController;
        }

        public void PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            if (_beatmapCallbacksController is null || transformL is null || transformR is null) return;
            _startRotL = transformL.rotation;
            _startRotR = transformR.rotation;
            if (_lightPairRotationEventEffect is null)
            {
                _lightPairRotationEventEffect = gameObject.AddComponent<LightPairRotationEventEffect>();
                _lightPairRotationEventEffect.SetField("_eventL", (BasicBeatmapEventType)eventL);
                _lightPairRotationEventEffect.SetField("_eventR", (BasicBeatmapEventType)eventR);
                _lightPairRotationEventEffect.SetField("_switchOverrideRandomValuesEvent", (BasicBeatmapEventType)switchOverrideRandomValuesEvent);
                _lightPairRotationEventEffect.SetField("_rotationVector", rotationVector);
                _lightPairRotationEventEffect.SetField("_transformL", transformL);
                _lightPairRotationEventEffect.SetField("_transformR", transformR);
                _lightPairRotationEventEffect.SetField("_useZPositionForAngleOffset", useZPositionForAngleOffset);
                _lightPairRotationEventEffect.SetField("_zPositionAngleOffsetScale", zPositionAngleOffsetScale);
                _lightPairRotationEventEffect.SetField("_beatmapCallbacksController", _beatmapCallbacksController);
            }
            else if (_beatmapCallbacksController is not null)
            {
                _lightPairRotationEventEffect.SetField("_beatmapCallbacksController", _beatmapCallbacksController);
                _lightPairRotationEventEffect.Start();
            }
        }

        public void PlatformDisabled()
        {
            if (_lightPairRotationEventEffect is null || transformL is null || transformR is null) return;
            transformL.rotation = _startRotL;
            transformR.rotation = _startRotR;
            _lightPairRotationEventEffect.enabled = false;
            _lightPairRotationEventEffect.OnDestroy();
        }
    }
}