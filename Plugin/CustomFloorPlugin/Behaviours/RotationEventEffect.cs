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

        [Inject]
        public void Construct([InjectOptional] IAudioTimeSource audioTimeSource, [InjectOptional] BeatmapCallbacksController beatmapCallbacksController)
        {
            _audioTimeSource = audioTimeSource;
            _beatmapCallbacksController = beatmapCallbacksController;
        }

        public void PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            if (_beatmapCallbacksController is null)
                return;
            _startRot = transform.rotation;
            if (_lightRotationEventEffect is null)
            {
                _lightRotationEventEffect = gameObject.AddComponent<LightRotationEventEffect>();
                _lightRotationEventEffect._event = (BasicBeatmapEventType)eventType;
                _lightRotationEventEffect._rotationVector = rotationVector;
                _lightRotationEventEffect._transform = transform;
                _lightRotationEventEffect.SetField("_audioTimeSource", _audioTimeSource);
                _lightRotationEventEffect.SetField("_beatmapCallbacksController", _beatmapCallbacksController);
            }
            else if (_beatmapCallbacksController is not null)
            {
                _lightRotationEventEffect.SetField("_beatmapCallbacksController", _beatmapCallbacksController);
                _lightRotationEventEffect.Start();
            }
        }

        public void PlatformDisabled()
        {
            if (_lightRotationEventEffect is null)
                return;
            transform.rotation = _startRot;
            _lightRotationEventEffect.enabled = false;
            _lightRotationEventEffect.OnDestroy();
        }
    }
}
