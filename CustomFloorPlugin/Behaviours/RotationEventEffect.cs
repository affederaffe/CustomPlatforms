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

        private PlatformManager _platformManager;
        private IBeatmapObjectCallbackController _beatmapObjectCallbackController;

        [Inject]
        public void Construct(PlatformManager platformManager, [InjectOptional] IBeatmapObjectCallbackController beatmapObjectCallbackController)
        {
            _platformManager = platformManager;
            _beatmapObjectCallbackController = beatmapObjectCallbackController;
        }

        public void PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            if (_beatmapObjectCallbackController == null)
                return;
            LightRotationEventEffect rotEffect = gameObject.AddComponent<LightRotationEventEffect>();
            _platformManager.spawnedComponents.Add(rotEffect);
            rotEffect.SetField("_beatmapObjectCallbackController", _beatmapObjectCallbackController);
            rotEffect.SetField("_event", (BeatmapEventType)eventType);
            rotEffect.SetField("_rotationVector", rotationVector);
            rotEffect.SetField("_transform", transform);
        }
    }
}