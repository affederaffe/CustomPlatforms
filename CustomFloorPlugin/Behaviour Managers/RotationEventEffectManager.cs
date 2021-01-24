using System.Collections.Generic;

using IPA.Utilities;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin {


    /// <summary>
    /// Instantiable wrapper class for <see cref="RotationEventEffect"/>s, that handles registering and de-registering
    /// </summary>
    internal class RotationEventEffectManager : MonoBehaviour {

        [InjectOptional]
        private readonly BeatmapObjectCallbackController _beatmapObjectCallbackController;

        /// <summary>
        /// To be filled with spawned <see cref="LightRotationEventEffect"/>s
        /// </summary>
        private List<LightRotationEventEffect> lightRotationEffects;


        /// <summary>
        /// To be filled with spawned <see cref="MultiRotationEventEffect.Actor"/>s
        /// </summary>
        private List<MultiRotationEventEffect.Actor> multiEffects;


        /// <summary>
        /// Registers all currently known <see cref="LightRotationEventEffect"/>s for Events.
        /// </summary>
        internal void RegisterForEvents() {
            if (_beatmapObjectCallbackController != null) {
                foreach (LightRotationEventEffect rotEffect in lightRotationEffects) {
                    _beatmapObjectCallbackController.beatmapEventDidTriggerEvent += rotEffect.HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger;
                }
                foreach (MultiRotationEventEffect.Actor effect in multiEffects) {
                    _beatmapObjectCallbackController.beatmapEventDidTriggerEvent += effect.EventCallback;
                }
            }
        }




        /// <summary>
        /// De-Registers from lighting events<br/>
        /// [Unity calls this when the <see cref="MonoBehaviour"/> becomes inactive in the hierachy]
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnDisable() {
            if (_beatmapObjectCallbackController != null) {
                foreach (LightRotationEventEffect rotEffect in lightRotationEffects) {
                    _beatmapObjectCallbackController.beatmapEventDidTriggerEvent -= rotEffect.HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger;
                }
                foreach (MultiRotationEventEffect.Actor effect in multiEffects) {
                    _beatmapObjectCallbackController.beatmapEventDidTriggerEvent -= effect.EventCallback;
                }
            }
        }


        /// <summary>
        /// Creates all <see cref="LightRotationEventEffect"/>s for the <paramref name="currentPlatform"/><br/>
        /// (From <see cref="RotationEventEffect"/>s present on the <paramref name="currentPlatform"/>)
        /// </summary>
        /// <param name="currentPlatform">Current <see cref="CustomPlatform"/>s <see cref="GameObject"/></param>
        internal void CreateEffects(GameObject currentPlatform) {
            lightRotationEffects = new List<LightRotationEventEffect>();
            multiEffects = new List<MultiRotationEventEffect.Actor>();

            RotationEventEffect[] effectDescriptors = currentPlatform.GetComponentsInChildren<RotationEventEffect>(true);

            foreach (RotationEventEffect effectDescriptor in effectDescriptors) {
                LightRotationEventEffect rotEvent = effectDescriptor.gameObject.AddComponent<LightRotationEventEffect>();
                PlatformManager.SpawnedComponents.Add(rotEvent);
                rotEvent.SetField("_event", (BeatmapEventType)effectDescriptor.eventType);
                rotEvent.SetField("_rotationVector", effectDescriptor.rotationVector);
                rotEvent.SetField("_transform", rotEvent.transform);
                rotEvent.SetField("_startRotation", rotEvent.transform.rotation);
                rotEvent.SetField("_beatmapObjectCallbackController", _beatmapObjectCallbackController);
                lightRotationEffects.Add(rotEvent);
            }
            MultiRotationEventEffect[] effectDescriptors2 = currentPlatform.GetComponentsInChildren<MultiRotationEventEffect>(true);
            foreach (MultiRotationEventEffect effectDescriptor in effectDescriptors2) {
                MultiRotationEventEffect.Actor rotEvent = effectDescriptor.Create();
                PlatformManager.SpawnedComponents.Add(rotEvent);
                multiEffects.Add(rotEvent);
            }
        }
    }
}