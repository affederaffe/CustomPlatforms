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

        private IBeatmapObjectCallbackController? _beatmapObjectCallbackController;

        private LightRotationEventEffect? _lightRotationEventEffect;
        private Quaternion _startRot;

        [Inject]
        public void Construct([InjectOptional] IBeatmapObjectCallbackController beatmapObjectCallbackController)
        {
            _beatmapObjectCallbackController = beatmapObjectCallbackController;
        }

        public void PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            if (_beatmapObjectCallbackController is null) return;
            _startRot = transform.rotation;
            if (_lightRotationEventEffect is null)
            {
                _lightRotationEventEffect = gameObject.AddComponent<LightRotationEventEffect>();
                _lightRotationEventEffect.SetField("_event", (BeatmapEventType)eventType);
                _lightRotationEventEffect.SetField("_rotationVector", rotationVector);
                _lightRotationEventEffect.SetField("_transform", transform);
                _lightRotationEventEffect.SetField("_beatmapObjectCallbackController", _beatmapObjectCallbackController);
            }
            else if (_beatmapObjectCallbackController is not null)
            {
                _lightRotationEventEffect.SetField("_beatmapObjectCallbackController", _beatmapObjectCallbackController);
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