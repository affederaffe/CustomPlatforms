using BS_Utils.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace CustomFloorPlugin {


    /// <summary>
    /// Instantiable wrapper and manager class for <see cref="TrackRings"/>.<br/>
    /// Handles spawning of multiple Components, relevant to track rings<br/>
    /// Handles reparenting of <see cref="TrackLaneRing"/>s after the game has spawned them<br/>
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1812:Avoid unistantiated internal classes", Justification = "Instantiated by Unity")]
    internal class TrackRingsManagerSpawner:MonoBehaviour {


        /// <summary>
        /// <see cref="List{T}"/> of all known <see cref="TrackRings"/> under a specific <see cref="GameObject"/>
        /// </summary>
        private List<TrackRings> trackRingsDescriptors;


        /// <summary>
        /// <see cref="List{T}"/> of all known <see cref="TrackLaneRingsManager"/>s under a specific <see cref="GameObject"/>
        /// </summary>
        internal List<TrackLaneRingsManager> trackLaneRingsManagers;


        /// <summary>
        /// <see cref="List{T}"/> of all known <see cref="TrackLaneRingsRotationEffectSpawner"/>s under a specific <see cref="GameObject"/>
        /// </summary>
        private List<TrackLaneRingsRotationEffectSpawner> rotationSpawners;


        /// <summary>
        /// <see cref="List{T}"/> of all known <see cref="TrackLaneRingsPositionStepEffectSpawner"/>s under a specific <see cref="GameObject"/>
        /// </summary>
        private List<TrackLaneRingsPositionStepEffectSpawner> stepSpawners;


        /// <summary>
        /// Re-Parenting <see cref="TrackLaneRing"/>s, created by the game, to this <see cref="CustomPlatform"/><br/>
        /// [Unity calls this before any of this <see cref="MonoBehaviour"/>s Update functions are called for the first time]
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void Start() {
            foreach(TrackLaneRingsManager trackLaneRingsManager in trackLaneRingsManagers) {
                TrackLaneRing[] rings = ReflectionUtil.GetPrivateField<TrackLaneRing[]>(trackLaneRingsManager, "_rings");
                foreach(TrackLaneRing ring in rings) {
                    ring.transform.parent = transform;
                    PlatformManager.SpawnedObjects.Add(ring.gameObject);
                    MaterialSwapper.ReplaceMaterials(ring.gameObject);
                }
            }
        }


        /// <summary>
        /// Creates and stores references to multiple objects (of different types) per <see cref="TrackRings"/> on the <paramref name="gameObject"/>
        /// </summary>
        /// <param name="gameObject">What <see cref="GameObject"/> to create TrackRings for</param>
        internal void CreateTrackRings(GameObject gameObject) {
            rotationSpawners = new List<TrackLaneRingsRotationEffectSpawner>();
            stepSpawners = new List<TrackLaneRingsPositionStepEffectSpawner>();
            trackLaneRingsManagers = new List<TrackLaneRingsManager>();
            trackRingsDescriptors = new List<TrackRings>();

            TrackRings[] ringsDescriptors = gameObject.GetComponentsInChildren<TrackRings>();
            foreach(TrackRings trackRingDesc in ringsDescriptors) {
                trackRingsDescriptors.Add(trackRingDesc);

                TrackLaneRingsManager ringsManager = trackRingDesc.gameObject.AddComponent<TrackLaneRingsManager>();
                trackLaneRingsManagers.Add(ringsManager);
                PlatformManager.SpawnedComponents.Add(ringsManager);

                TrackLaneRing ring = trackRingDesc.trackLaneRingPrefab.AddComponent<TrackLaneRing>();
                PlatformManager.SpawnedComponents.Add(ring);

                ReflectionUtil.SetPrivateField(ringsManager, "_trackLaneRingPrefab", ring);
                ReflectionUtil.SetPrivateField(ringsManager, "_ringCount", trackRingDesc.ringCount);
                ReflectionUtil.SetPrivateField(ringsManager, "_ringPositionStep", trackRingDesc.ringPositionStep);

                if(trackRingDesc.useRotationEffect) {
                    TrackLaneRingsRotationEffect rotationEffect = trackRingDesc.gameObject.AddComponent<TrackLaneRingsRotationEffect>();
                    PlatformManager.SpawnedComponents.Add(rotationEffect);
                    ReflectionUtil.SetPrivateField(rotationEffect, "_trackLaneRingsManager", ringsManager);
                    ReflectionUtil.SetPrivateField(rotationEffect, "_startupRotationAngle", trackRingDesc.startupRotationAngle);
                    ReflectionUtil.SetPrivateField(rotationEffect, "_startupRotationStep", trackRingDesc.startupRotationStep);
                    ReflectionUtil.SetPrivateField(rotationEffect, "_startupRotationPropagationSpeed", trackRingDesc.startupRotationPropagationSpeed);
                    ReflectionUtil.SetPrivateField(rotationEffect, "_startupRotationFlexySpeed", trackRingDesc.startupRotationFlexySpeed);

                    TrackLaneRingsRotationEffectSpawner rotationEffectSpawner = trackRingDesc.gameObject.AddComponent<TrackLaneRingsRotationEffectSpawner>();
                    rotationSpawners.Add(rotationEffectSpawner);
                    PlatformManager.SpawnedComponents.Add(rotationEffectSpawner);
                    ReflectionUtil.SetPrivateField(rotationEffectSpawner, "_beatmapObjectCallbackController", Plugin.Bocc);
                    ReflectionUtil.SetPrivateField(rotationEffectSpawner, "_beatmapEventType", (BeatmapEventType)trackRingDesc.rotationSongEventType);
                    ReflectionUtil.SetPrivateField(rotationEffectSpawner, "_rotationStep", trackRingDesc.rotationStep);
                    ReflectionUtil.SetPrivateField(rotationEffectSpawner, "_rotationPropagationSpeed", trackRingDesc.rotationPropagationSpeed);
                    ReflectionUtil.SetPrivateField(rotationEffectSpawner, "_rotationFlexySpeed", trackRingDesc.rotationFlexySpeed);
                    ReflectionUtil.SetPrivateField(rotationEffectSpawner, "_trackLaneRingsRotationEffect", rotationEffect);
                }
                if(trackRingDesc.useStepEffect) {
                    TrackLaneRingsPositionStepEffectSpawner stepEffectSpawner = trackRingDesc.gameObject.AddComponent<TrackLaneRingsPositionStepEffectSpawner>();
                    stepSpawners.Add(stepEffectSpawner);
                    PlatformManager.SpawnedComponents.Add(stepEffectSpawner);
                    ReflectionUtil.SetPrivateField(stepEffectSpawner, "_beatmapObjectCallbackController", Plugin.Bocc);
                    ReflectionUtil.SetPrivateField(stepEffectSpawner, "_trackLaneRingsManager", ringsManager);
                    ReflectionUtil.SetPrivateField(stepEffectSpawner, "_beatmapEventType", (BeatmapEventType)trackRingDesc.stepSongEventType);
                    ReflectionUtil.SetPrivateField(stepEffectSpawner, "_minPositionStep", trackRingDesc.minPositionStep);
                    ReflectionUtil.SetPrivateField(stepEffectSpawner, "_maxPositionStep", trackRingDesc.maxPositionStep);
                    ReflectionUtil.SetPrivateField(stepEffectSpawner, "_moveSpeed", trackRingDesc.moveSpeed);
                }
            }
        }
    }
}
