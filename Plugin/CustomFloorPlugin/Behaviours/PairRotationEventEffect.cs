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
        public void Construct([InjectOptional] BeatmapCallbacksController beatmapCallbacksController) => _beatmapCallbacksController = beatmapCallbacksController;

        public void PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            if (_beatmapCallbacksController is null || transformL is null || transformR is null)
                return;
            _startRotL = transformL.rotation;
            _startRotR = transformR.rotation;
            if (_lightPairRotationEventEffect is null)
            {
                _lightPairRotationEventEffect = gameObject.AddComponent<LightPairRotationEventEffect>();
                _lightPairRotationEventEffect._eventL = (BasicBeatmapEventType)eventL;
                _lightPairRotationEventEffect._eventR = (BasicBeatmapEventType)eventR;
                _lightPairRotationEventEffect._switchOverrideRandomValuesEvent = (BasicBeatmapEventType)switchOverrideRandomValuesEvent;
                _lightPairRotationEventEffect._rotationVector = rotationVector;
                _lightPairRotationEventEffect._transformL = transformL;
                _lightPairRotationEventEffect._transformR = transformR;
                _lightPairRotationEventEffect._useZPositionForAngleOffset = useZPositionForAngleOffset;
                _lightPairRotationEventEffect._zPositionAngleOffsetScale = zPositionAngleOffsetScale;
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
            if (_lightPairRotationEventEffect is null || transformL is null || transformR is null)
                return;
            transformL.rotation = _startRotL;
            transformR.rotation = _startRotR;
            _lightPairRotationEventEffect.enabled = false;
            _lightPairRotationEventEffect.OnDestroy();
        }
    }
}
