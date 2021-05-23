using IPA.Utilities;

using UnityEngine;

using Zenject;


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

        [Inject]
        public void Construct([InjectOptional] IBeatmapObjectCallbackController beatmapObjectCallbackController)
        {
            _beatmapObjectCallbackController = beatmapObjectCallbackController;
        }

        public void PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            if (_beatmapObjectCallbackController == null) return;
            if (_lightRotationEventEffect == null)
            {
                _lightRotationEventEffect = gameObject.AddComponent<LightRotationEventEffect>();
                _lightRotationEventEffect.SetField("_event", (BeatmapEventType)eventType);
                _lightRotationEventEffect.SetField("_rotationVector", rotationVector);
                _lightRotationEventEffect.SetField("_transform", transform);
            }

            _lightRotationEventEffect.SetField("_beatmapObjectCallbackController", _beatmapObjectCallbackController);
            _lightRotationEventEffect.enabled = true;
        }

        public void PlatformDisabled()
        {
            if (_lightRotationEventEffect == null) return;
            _lightRotationEventEffect.enabled = false;
        }
    }
}