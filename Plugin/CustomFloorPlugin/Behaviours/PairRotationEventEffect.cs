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

        private static readonly FieldAccessor<LightPairRotationEventEffect, BasicBeatmapEventType>.Accessor _eventLAccessor = FieldAccessor<LightPairRotationEventEffect, BasicBeatmapEventType>.GetAccessor("_eventL");
        private static readonly FieldAccessor<LightPairRotationEventEffect, BasicBeatmapEventType>.Accessor _eventRAccessor = FieldAccessor<LightPairRotationEventEffect, BasicBeatmapEventType>.GetAccessor("_eventR");
        private static readonly FieldAccessor<LightPairRotationEventEffect, BasicBeatmapEventType>.Accessor _switchOverrideRandomValuesEventAccessor = FieldAccessor<LightPairRotationEventEffect, BasicBeatmapEventType>.GetAccessor("_switchOverrideRandomValuesEvent");
        private static readonly FieldAccessor<LightPairRotationEventEffect, Vector3>.Accessor _rotationVectorAccessor = FieldAccessor<LightPairRotationEventEffect, Vector3>.GetAccessor("_rotationVector");
        private static readonly FieldAccessor<LightPairRotationEventEffect, Transform>.Accessor _transformLAccessor = FieldAccessor<LightPairRotationEventEffect, Transform>.GetAccessor("_transformL");
        private static readonly FieldAccessor<LightPairRotationEventEffect, Transform>.Accessor _transformRAccessor = FieldAccessor<LightPairRotationEventEffect, Transform>.GetAccessor("_transformR");
        private static readonly FieldAccessor<LightPairRotationEventEffect, bool>.Accessor _useZPositionForAngleOffsetAccessor = FieldAccessor<LightPairRotationEventEffect, bool>.GetAccessor("_useZPositionForAngleOffset");
        private static readonly FieldAccessor<LightPairRotationEventEffect, float>.Accessor _zPositionAngleOffsetScaleAccessor = FieldAccessor<LightPairRotationEventEffect, float>.GetAccessor("_zPositionAngleOffsetScale");
        private static readonly FieldAccessor<LightPairRotationEventEffect, BeatmapCallbacksController>.Accessor _beatmapCallbacksControllerAccessor = FieldAccessor<LightPairRotationEventEffect, BeatmapCallbacksController>.GetAccessor("_beatmapCallbacksController");

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
                _eventLAccessor(ref _lightPairRotationEventEffect) = (BasicBeatmapEventType)eventL;
                _eventRAccessor(ref _lightPairRotationEventEffect) = (BasicBeatmapEventType)eventR;
                _switchOverrideRandomValuesEventAccessor(ref _lightPairRotationEventEffect) = (BasicBeatmapEventType)switchOverrideRandomValuesEvent;
                _rotationVectorAccessor(ref _lightPairRotationEventEffect) = rotationVector;
                _transformLAccessor(ref _lightPairRotationEventEffect) = transformL;
                _transformRAccessor(ref _lightPairRotationEventEffect) = transformR;
                _useZPositionForAngleOffsetAccessor(ref _lightPairRotationEventEffect) = useZPositionForAngleOffset;
                _zPositionAngleOffsetScaleAccessor(ref _lightPairRotationEventEffect) = zPositionAngleOffsetScale;
                _beatmapCallbacksControllerAccessor(ref _lightPairRotationEventEffect) = _beatmapCallbacksController;
            }
            else if (_beatmapCallbacksController is not null)
            {
                _beatmapCallbacksControllerAccessor(ref _lightPairRotationEventEffect) = _beatmapCallbacksController;
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