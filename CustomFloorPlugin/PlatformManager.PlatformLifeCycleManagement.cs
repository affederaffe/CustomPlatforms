using IPA.Utilities;

using UnityEngine;

using static CustomFloorPlugin.GlobalCollection;
using static CustomFloorPlugin.Utilities.BeatSaberSearching;
using static CustomFloorPlugin.Utilities.Logging;


namespace CustomFloorPlugin {


    public static partial class PlatformManager {


        /// <summary>
        /// The part of the <see cref="PlatformManager"/> that handles spawning, notifying, etc.
        /// </summary>
        private static class PlatformLifeCycleManagement {


            /// <summary>
            /// Changes to the user selected, or API requested, <see cref="CustomPlatform"/>.
            /// </summary>
            internal static void InternalChangeToPlatform() {
                if (kyleBuffer.HasValue) {
                    InternalChangeToPlatform(kyleBuffer.Value);
                    kyleBuffer = null;
                }
                else {
                    InternalChangeToPlatform(CurrentPlatformIndex);
                }
            }


            /// <summary>
            /// Changes to a specific <see cref="CustomPlatform"/>
            /// </summary>
            /// <param name="index">The index of the new <see cref="CustomPlatform"/> in the list <see cref="AllPlatforms"/></param>
            internal static void InternalChangeToPlatform(int index) {
                Log("Switching to " + AllPlatforms[index].name);
                if (!GetCurrentEnvironment().name.StartsWith("Menu", STR_INV)) {
                    platformSpawned = true;
                }
                activePlatform?.gameObject.SetActive(false);
                NotifyPlatform(activePlatform, NotifyType.Disable);
                DestroyCustomObjects();

                if (index != 0) {
                    activePlatform = AllPlatforms[index % AllPlatforms.Count];
                    activePlatform.gameObject.SetActive(true);
                    AddManagers(activePlatform);
                    NotifyPlatform(activePlatform, NotifyType.Enable);
                    SpawnCustomObjects();
                }
                else {
                    activePlatform = null;
                }
                EnvironmentHider.HideObjectsForPlatform(AllPlatforms[index]);
            }


            /// <summary>
            /// Notifies a given <see cref="CustomPlatform"/> when it gets activated or deactivated
            /// </summary>
            /// <param name="customPlatform">What <see cref="CustomPlatform"/> to notify</param>
            /// <param name="type">What happened to the platform</param>
            private static void NotifyPlatform(CustomPlatform customPlatform, NotifyType type) {
                INotifyOnEnableOrDisable[] things = customPlatform?.gameObject?.GetComponentsInChildren<INotifyOnEnableOrDisable>(true);
                if (things != null) {
                    foreach (INotifyOnEnableOrDisable thing in things) {
                        if (type == NotifyType.Disable) {
                            thing.PlatformDisabled();
                        }
                        else {
                            thing.PlatformEnabled();
                        }
                    }
                }
            }


            /// <summary>
            /// Spawns all registered custom objects, as required by the selected <see cref="CustomPlatform"/>
            /// </summary>
            private static void SpawnCustomObjects() {
                Log("Members in SpawnQueue: " + SpawnQueue.GetInvocationList().Length);
                SpawnQueue(FindLightWithIdManager(GetCurrentEnvironment()));
            }



            /// <summary>
            /// Despawns all registered custom objects, as required by the selected <see cref="CustomPlatform"/>
            /// </summary>
            private static void DestroyCustomObjects() {
                while (SpawnedObjects.Count != 0) {
                    GameObject gameObject = SpawnedObjects[0];
                    SpawnedObjects.Remove(gameObject);
                    Object.Destroy(gameObject);
                    foreach (TubeBloomPrePassLight tubeBloomPrePassLight in gameObject.GetComponentsInChildren<TubeBloomPrePassLight>(true)) {
                        //Unity requires this to be present, otherwise Unregister won't be called. Memory leaks may occour if this is removed.
                        tubeBloomPrePassLight.InvokeMethod<object, BloomPrePassLight>("UnregisterLight");
                    }
                }

                while (SpawnedComponents.Count != 0) {
                    Component component = SpawnedComponents[0];
                    SpawnedComponents.Remove(component);
                    Object.Destroy(component);
                }
            }


            /// <summary>
            /// Adds managers to a <see cref="CustomPlatform"/>
            /// </summary>
            /// <param name="customPlatform">The <see cref="CustomPlatform"/> for which to spawn managers</param>
            internal static void AddManagers(CustomPlatform customPlatform) {
                GameObject go = customPlatform.gameObject;
                bool active = go.activeSelf;
                if (active) {
                    go.SetActive(false);
                }
                AddManagers(go, go);
                if (active) {
                    go.SetActive(true);
                }
            }


            /// <summary>
            /// Recursively attaches managers to a <see cref="CustomPlatform"/>
            /// </summary>
            /// <param name="go">The current <see cref="GameObject"/>, this parameter acts as a pointer</param>
            /// <param name="root">The root <see cref="GameObject"/> of the <see cref="CustomPlatform"/></param>
            private static void AddManagers(GameObject go, GameObject root) {

                // Rotation effect manager
                if (go.GetComponentInChildren<RotationEventEffect>(true) != null || go.GetComponentInChildren<MultiRotationEventEffect>(true) != null) {
                    RotationEventEffectManager rotManager = root.GetComponent<RotationEventEffectManager>();
                    if (rotManager == null) {
                        rotManager = root.AddComponent<RotationEventEffectManager>();
                        SpawnedComponents.Add(rotManager);
                        rotManager.CreateEffects(go);
                        rotManager.RegisterForEvents();
                    }
                }

                // Add a trackRing controller if there are track ring descriptors
                if (go.GetComponentInChildren<TrackRings>(true) != null) {
                    foreach (TrackRings trackRings in go.GetComponentsInChildren<TrackRings>(true)) {
                        GameObject ringPrefab = trackRings.trackLaneRingPrefab;

                        // Add managers to prefabs (nesting)
                        AddManagers(ringPrefab, root);
                    }

                    TrackRingsManagerSpawner trms = root.GetComponent<TrackRingsManagerSpawner>();
                    if (trms == null) {
                        trms = root.AddComponent<TrackRingsManagerSpawner>();
                        SpawnedComponents.Add(trms);
                    }
                    trms.CreateTrackRings(go);
                }

                // Add spectrogram manager
                if (go.GetComponentInChildren<Spectrogram>(true) != null) {
                    foreach (Spectrogram spec in go.GetComponentsInChildren<Spectrogram>(true)) {
                        GameObject colPrefab = spec.columnPrefab;
                        AddManagers(colPrefab, root);
                    }

                    SpectrogramColumnManager specManager = go.GetComponent<SpectrogramColumnManager>();
                    if (specManager == null) {
                        specManager = go.AddComponent<SpectrogramColumnManager>();
                        SpawnedComponents.Add(specManager);
                    }
                    specManager.CreateColumns(go);
                }

                if (go.GetComponentInChildren<SpectrogramMaterial>(true) != null) {
                    // Add spectrogram materials manager
                    SpectrogramMaterialManager specMatManager = go.GetComponent<SpectrogramMaterialManager>();
                    if (specMatManager == null) {
                        specMatManager = go.AddComponent<SpectrogramMaterialManager>();
                        SpawnedComponents.Add(specMatManager);
                    }
                    specMatManager.UpdateMaterials(go);
                }

                if (go.GetComponentInChildren<SpectrogramAnimationState>(true) != null) {
                    // Add spectrogram animation state manager
                    SpectrogramAnimationStateManager specAnimManager = go.GetComponent<SpectrogramAnimationStateManager>();
                    if (specAnimManager == null) {
                        specAnimManager = go.AddComponent<SpectrogramAnimationStateManager>();
                        SpawnedComponents.Add(specAnimManager);
                    }
                    specAnimManager.UpdateAnimationStates();
                }

                // Add Song event manager
                if (go.GetComponentInChildren<SongEventHandler>(true) != null) {
                    foreach (SongEventHandler handler in go.GetComponentsInChildren<SongEventHandler>()) {
                        SongEventManager manager = handler.gameObject.AddComponent<SongEventManager>();
                        SpawnedComponents.Add(manager);
                        manager._songEventHandler = handler;
                    }
                }

                // Add EventManager 
                if (go.GetComponentInChildren<EventManager>(true) != null) {
                    foreach (EventManager em in go.GetComponentsInChildren<EventManager>()) {
                        PlatformEventManager pem = em.gameObject.AddComponent<PlatformEventManager>();  
                        SpawnedComponents.Add(pem);
                        pem._EventManager = em;
                    }
                }
            }


            /// <summary>
            /// Used to destinguish between between platform enables and disables
            /// </summary>
            private enum NotifyType {
                Enable = 0,
                Disable = 1
            }
        }
    }
}
