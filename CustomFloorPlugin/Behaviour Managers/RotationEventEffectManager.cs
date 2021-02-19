using IPA.Utilities;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Instantiable wrapper class for <see cref="RotationEventEffect"/>s, that handles registering and de-registering
    /// </summary>
    internal class RotationEventEffectManager : MonoBehaviour
    {
        [InjectOptional]
        private readonly BeatmapObjectCallbackController _beatmapObjectCallbackController;

        internal void CreateEffects(GameObject currentPlatform)
        {
            if (_beatmapObjectCallbackController != null)
            {
                foreach (PairRotationEventEffect descriptor in currentPlatform.GetComponentsInChildren<PairRotationEventEffect>(true))
                {
                    LightPairRotationEventEffect rotEffect = descriptor.gameObject.AddComponent<LightPairRotationEventEffect>();
                    rotEffect.SetField("_beatmapObjectCallbackController", _beatmapObjectCallbackController);
                    rotEffect.SetField("_eventL", (BeatmapEventType)descriptor.eventL);
                    rotEffect.SetField("_eventR", (BeatmapEventType)descriptor.eventR);
                    rotEffect.SetField("_rotationVector", descriptor.rotationVector);
                    rotEffect.SetField("_transformL", descriptor.tranformL);
                    rotEffect.SetField("_transformR", descriptor.tranformR);
                    rotEffect.SetField("_useZPositionForAngleOffset", descriptor.useZPositionForAngleOffset);
                    rotEffect.SetField("_overrideRandomValues", descriptor.overrideRandomValues);
                    rotEffect.SetField("_randomStartRotation", descriptor.randomStartRotation);
                    rotEffect.SetField("_randomDirection", descriptor.randomDirection);
                    PlatformManager.SpawnedComponents.Add(rotEffect);
                }
                foreach (var descriptor in currentPlatform.GetComponentsInChildren<RotationEventEffect>(true))
                {
                    LightRotationEventEffect rotEffect = descriptor.gameObject.AddComponent<LightRotationEventEffect>();
                    rotEffect.SetField("_beatmapObjectCallbackController", _beatmapObjectCallbackController);
                    rotEffect.SetField("_event", (BeatmapEventType)descriptor.eventType);
                    rotEffect.SetField("_rotationVector", descriptor.rotationVector);
                    rotEffect.SetField("_transform", descriptor.transform);
                }
            }
        }
    }
}