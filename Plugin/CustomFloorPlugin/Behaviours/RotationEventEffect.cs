using CustomFloorPlugin.Interfaces;
using IPA.Utilities;

using UnityEngine;

using Zenject;


// ReSharper disable once CheckNamespace
namespace CustomFloorPlugin
{
    /// <summary>
    /// Instantiable wrapper class for <see cref="LightRotationEventEffect"/>, to be used by mappers.
    /// </summary>
    public class RotationEventEffect : MonoBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
    {
        public SongEventType eventType;
        public Vector3 rotationVector;

        private IAudioTimeSource? _audioTimeSource;
        private BeatmapCallbacksController? _beatmapCallbacksController;

        private LightRotationEventEffect? _lightRotationEventEffect;
        private Quaternion _startRot;

        private static readonly FieldAccessor<LightRotationEventEffect, BasicBeatmapEventType>.Accessor _eventAccessor = FieldAccessor<LightRotationEventEffect, BasicBeatmapEventType>.GetAccessor("_event");
        private static readonly FieldAccessor<LightRotationEventEffect, Vector3>.Accessor _rotationVectorAccessor = FieldAccessor<LightRotationEventEffect, Vector3>.GetAccessor("_rotationVector");
        private static readonly FieldAccessor<LightRotationEventEffect, Transform>.Accessor _transformAccessor = FieldAccessor<LightRotationEventEffect, Transform>.GetAccessor("_transform");
        private static readonly FieldAccessor<LightRotationEventEffect, IAudioTimeSource?>.Accessor _audioTimeSourceAccessor = FieldAccessor<LightRotationEventEffect, IAudioTimeSource?>.GetAccessor("_audioTimeSource");
        private static readonly FieldAccessor<LightRotationEventEffect, BeatmapCallbacksController?>.Accessor _beatmapCallbacksControllerAccessor = FieldAccessor<LightRotationEventEffect, BeatmapCallbacksController?>.GetAccessor("_beatmapCallbacksController");

        [Inject]
        public void Construct([InjectOptional] IAudioTimeSource audioTimeSource, [InjectOptional] BeatmapCallbacksController beatmapCallbacksController)
        {
            _audioTimeSource = audioTimeSource;
            _beatmapCallbacksController = beatmapCallbacksController;
        }

        public void PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            if (_beatmapCallbacksController is null) return;
            _startRot = transform.rotation;
            if (_lightRotationEventEffect is null)
            {
                _lightRotationEventEffect = gameObject.AddComponent<LightRotationEventEffect>();
                _eventAccessor(ref _lightRotationEventEffect) = (BasicBeatmapEventType)eventType;
                _rotationVectorAccessor(ref _lightRotationEventEffect) = rotationVector;
                _transformAccessor(ref _lightRotationEventEffect) = transform;
                _audioTimeSourceAccessor(ref _lightRotationEventEffect) = _audioTimeSource;
                _beatmapCallbacksControllerAccessor(ref _lightRotationEventEffect) = _beatmapCallbacksController;
            }
            else if (_beatmapCallbacksController is not null)
            {
                _beatmapCallbacksControllerAccessor(ref _lightRotationEventEffect) = _beatmapCallbacksController;
                _lightRotationEventEffect.Start();
            }
        }

        public void PlatformDisabled()
        {
            if (_lightRotationEventEffect is null) return;
            transform.rotation = _startRot;
            _lightRotationEventEffect.enabled = false;
            _lightRotationEventEffect.OnDestroy();
        }
    }
}
