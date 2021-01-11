using System;
using System.Collections.Generic;

using IPA.Utilities;

using UnityEngine;

using static CustomFloorPlugin.Utilities.BeatSaberSearching;
using static CustomFloorPlugin.GlobalCollection;


namespace CustomFloorPlugin {


    /// <summary>
    /// Instantiable wrapper and manager class for <see cref="TrackRings"/>.<br/>
    /// Handles spawning of multiple Components, relevant to track rings<br/>
    /// Handles reparenting of <see cref="TrackLaneRing"/>s after the game has spawned them<br/>
    /// </summary>
    internal class TrackRingsManagerSpawner : MonoBehaviour {


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
            foreach (TrackLaneRingsManager trackLaneRingsManager in trackLaneRingsManagers) {
                TrackLaneRing[] rings = trackLaneRingsManager.GetField<TrackLaneRing[], TrackLaneRingsManager>("_rings");
                foreach (TrackLaneRing ring in rings) {
                    ring.transform.parent = transform;
                    PlatformManager.SpawnedObjects.Add(ring.gameObject);
                    MaterialSwapper.ReplaceMaterials(ring.gameObject);
                    foreach (TubeLight tubeLight in ring.GetComponentsInChildren<TubeLight>()) {
                        tubeLight.GameAwake(FindLightWithIdManager(GetCurrentEnvironment()));
                    }
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
            foreach (TrackRings trackRingDesc in ringsDescriptors) {
                trackRingsDescriptors.Add(trackRingDesc);

                TrackLaneRingsManager ringsManager = trackRingDesc.gameObject.AddComponent<TrackLaneRingsManager>();
                trackLaneRingsManagers.Add(ringsManager);
                PlatformManager.SpawnedComponents.Add(ringsManager);

                TrackLaneRing ring = trackRingDesc.trackLaneRingPrefab.AddComponent<TrackLaneRing>();
                PlatformManager.SpawnedComponents.Add(ring);

                ringsManager.SetField("_trackLaneRingPrefab", ring);
                ringsManager.SetField("_ringCount", trackRingDesc.ringCount);
                ringsManager.SetField("_ringPositionStep", trackRingDesc.ringPositionStep);

                if (trackRingDesc.useRotationEffect) {
                    TrackLaneRingsRotationEffect rotationEffect = trackRingDesc.gameObject.AddComponent<TrackLaneRingsRotationEffect>();
                    PlatformManager.SpawnedComponents.Add(rotationEffect);
                    rotationEffect.SetField("_trackLaneRingsManager", ringsManager);
                    rotationEffect.SetField("_startupRotationAngle", trackRingDesc.startupRotationAngle);
                    rotationEffect.SetField("_startupRotationStep", trackRingDesc.startupRotationStep);
                    int timePerRing = trackRingDesc.startupRotationPropagationSpeed / trackRingDesc.ringCount;
                    float ringsPerFrame = Time.fixedDeltaTime / timePerRing;
                    rotationEffect.SetField("_startupRotationPropagationSpeed", Math.Max((int)ringsPerFrame, 1));
                    rotationEffect.SetField("_startupRotationFlexySpeed", trackRingDesc.startupRotationFlexySpeed);

                    TrackLaneRingsRotationEffectSpawner rotationEffectSpawner = trackRingDesc.gameObject.AddComponent<TrackLaneRingsRotationEffectSpawner>();
                    rotationSpawners.Add(rotationEffectSpawner);
                    PlatformManager.SpawnedComponents.Add(rotationEffectSpawner);
                    if (!GetCurrentEnvironment().name.StartsWith("Menu", STR_INV)) {
                        rotationEffectSpawner.SetField("_beatmapObjectCallbackController", BOCC);
                    }

                    rotationEffectSpawner.SetField("_beatmapEventType", (BeatmapEventType)trackRingDesc.rotationSongEventType);
                    rotationEffectSpawner.SetField("_rotationStep", trackRingDesc.rotationStep);
                    int timePerRing2 = trackRingDesc.rotationPropagationSpeed / trackRingDesc.ringCount;
                    float ringsPerFrame2 = Time.fixedDeltaTime / timePerRing2;
                    rotationEffectSpawner.SetField("_rotationPropagationSpeed", Math.Max((int)ringsPerFrame2, 1));
                    rotationEffectSpawner.SetField("_rotationFlexySpeed", trackRingDesc.rotationFlexySpeed);
                    rotationEffectSpawner.SetField("_trackLaneRingsRotationEffect", rotationEffect);
                }
                if (trackRingDesc.useStepEffect) {
                    TrackLaneRingsPositionStepEffectSpawner stepEffectSpawner = trackRingDesc.gameObject.AddComponent<TrackLaneRingsPositionStepEffectSpawner>();
                    stepSpawners.Add(stepEffectSpawner);
                    PlatformManager.SpawnedComponents.Add(stepEffectSpawner);
                    if (!GetCurrentEnvironment().name.StartsWith("Menu", STR_INV)) {
                        stepEffectSpawner.SetField("_beatmapObjectCallbackController", BOCC);
                    }

                    stepEffectSpawner.SetField("_trackLaneRingsManager", ringsManager);
                    stepEffectSpawner.SetField("_beatmapEventType", (BeatmapEventType)trackRingDesc.stepSongEventType);
                    stepEffectSpawner.SetField("_minPositionStep", trackRingDesc.minPositionStep);
                    stepEffectSpawner.SetField("_maxPositionStep", trackRingDesc.maxPositionStep);
                    stepEffectSpawner.SetField("_moveSpeed", trackRingDesc.moveSpeed);
                }
            }
        }
    }
}