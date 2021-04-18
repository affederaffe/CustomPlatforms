using IPA.Utilities;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Instatiable wrapper class for <see cref="LightRotationEventEffect"/>, to be used by mappers.
    /// </summary>
    public class RotationEventEffect : MonoBehaviour, INotifyPlatformEnabled
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

        void INotifyPlatformEnabled.PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            if (_beatmapObjectCallbackController == null)
                return;

            if (_lightRotationEventEffect == null)
            {
                _lightRotationEventEffect = gameObject.AddComponent<LightRotationEventEffect>();
                _lightRotationEventEffect.SetField("_event", (BeatmapEventType)eventType);
                _lightRotationEventEffect.SetField("_rotationVector", rotationVector);
                _lightRotationEventEffect.SetField("_transform", transform);
            }

            _lightRotationEventEffect.SetField("_beatmapObjectCallbackController", _beatmapObjectCallbackController);
        }
    }
}