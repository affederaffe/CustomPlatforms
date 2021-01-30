using System.Collections.Generic;

using CustomFloorPlugin.Configuration;
using CustomFloorPlugin.Utilities;

using IPA.Utilities;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin {


    internal abstract class PlatformSpawner {

        [Inject]
        protected readonly PluginConfig _config;

        [Inject]
        protected readonly EnvironmentHider _hider;

        [Inject]
        protected readonly PlatformManager _platformManager;

        [Inject]
        protected readonly LightWithIdManager _lightManager;

        protected DiContainer _container;

        /// <summary>
        /// Changes to a specific <see cref="CustomPlatform"/> and saves the choice
        /// </summary>
        /// <param name="index">The index of the new <see cref="CustomPlatform"/> in the list <see cref="AllPlatforms"/></param>
        internal void SetPlatformAndShow(int index, PlatformType platformType) {
            if (platformType == PlatformType.Singleplayer) {
                _platformManager.currentSingleplayerPlatform = _platformManager.AllPlatforms[index % _platformManager.AllPlatforms.Count];
                _config.SingleplayerPlatformPath = _platformManager.currentSingleplayerPlatform.platName + _platformManager.currentSingleplayerPlatform.platAuthor;
                ChangeToPlatform(index);
            }
            else {
                _platformManager.currentMultiplayerPlatform = _platformManager.AllPlatforms[index % _platformManager.AllPlatforms.Count];
                _config.MultiplayerPlatformPath = _platformManager.currentMultiplayerPlatform.platName + _platformManager.currentMultiplayerPlatform.platAuthor;
                ChangeToPlatform(index);
            }
        }

        /// <summary>
        /// Changes to the currently selected <see cref="CustomPlatform"/>
        /// </summary>
        internal void ChangeToPlatform(PlatformType platformType) {
            if (platformType == PlatformType.Singleplayer) {
                ChangeToPlatform(_platformManager.CurrentSingleplayerPlatformIndex);
            }
            else {
                ChangeToPlatform(_platformManager.CurrentMultiplayerPlatformIndex);
            }
        }

        /// <summary>
        /// Changes to a specific <see cref="CustomPlatform"/>
        /// </summary>
        /// <param name="index">The index of the new <see cref="CustomPlatform"/> in the list <see cref="AllPlatforms"/></param>
        internal void ChangeToPlatform(int index) {
            Logging.Log("Switching to " + _platformManager.AllPlatforms[index].name);
            _platformManager.activePlatform?.gameObject.SetActive(false);
            NotifyPlatform(_platformManager.activePlatform, NotifyType.Disable);
            DestroyCustomObjects();

            SharedCoroutineStarter.instance.StartCoroutine(WaitAndSpawn());

            IEnumerator<WaitForEndOfFrame> WaitAndSpawn() {
                yield return new WaitForEndOfFrame();
                if (index != 0) {
                    _platformManager.activePlatform = _platformManager.AllPlatforms[index % _platformManager.AllPlatforms.Count];
                    _platformManager.activePlatform.gameObject.SetActive(true);
                    AddManagers(_platformManager.activePlatform);
                    NotifyPlatform(_platformManager.activePlatform, NotifyType.Enable);
                    SpawnCustomObjects();
                }
                else {
                    _platformManager.activePlatform = null;
                }
                _hider.HideObjectsForPlatform(_platformManager.AllPlatforms[index]);
            }
        }

        /// <summary>
        /// Notifies a given <see cref="CustomPlatform"/> when it gets activated or deactivated
        /// </summary>
        /// <param name="customPlatform">What <see cref="CustomPlatform"/> to notify</param>
        /// <param name="type">What happened to the platform</param>
        private void NotifyPlatform(CustomPlatform customPlatform, NotifyType type) {
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
        private void SpawnCustomObjects() {
            Logging.Log("Members in SpawnQueue: " + (PlatformManager.SpawnQueue.GetInvocationList().Length - 1));
            PlatformManager.SpawnQueue(_lightManager);
        }

        /// <summary>
        /// Despawns all registered custom objects, as required by the selected <see cref="CustomPlatform"/>
        /// </summary>
        private void DestroyCustomObjects() {
            while (PlatformManager.SpawnedObjects.Count != 0) {
                GameObject gameObject = PlatformManager.SpawnedObjects[0];
                PlatformManager.SpawnedObjects.Remove(gameObject);
                GameObject.Destroy(gameObject);
                foreach (TubeBloomPrePassLight tubeBloomPrePassLight in gameObject.GetComponentsInChildren<TubeBloomPrePassLight>(true)) {
                    //Unity requires this to be present, otherwise Unregister won't be called. Memory leaks may occour if this is removed.
                    tubeBloomPrePassLight.InvokeMethod<object, BloomPrePassLight>("UnregisterLight");
                }
            }

            while (PlatformManager.SpawnedComponents.Count != 0) {
                Component component = PlatformManager.SpawnedComponents[0];
                PlatformManager.SpawnedComponents.Remove(component);
                GameObject.Destroy(component);
            }
        }

        private enum NotifyType {
            Enable = 0,
            Disable = 1
        }

        /// <summary>
        /// Adds managers to a <see cref="CustomPlatform"/>
        /// </summary>
        /// <param name="customPlatform">The <see cref="CustomPlatform"/> for which to spawn managers</param>
        internal void AddManagers(CustomPlatform customPlatform) {
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
        private void AddManagers(GameObject go, GameObject root) {

            // Rotation effect manager
            if (go.GetComponentInChildren<RotationEventEffect>(true) != null || go.GetComponentInChildren<MultiRotationEventEffect>(true) != null) {
                RotationEventEffectManager rotManager = root.GetComponent<RotationEventEffectManager>();
                if (rotManager == null) {
                    rotManager = _container.InstantiateComponent<RotationEventEffectManager>(root);
                    PlatformManager.SpawnedComponents.Add(rotManager);
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
                    trms = _container.InstantiateComponent<TrackRingsManagerSpawner>(root);
                    PlatformManager.SpawnedComponents.Add(trms);
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
                    specManager = _container.InstantiateComponent<SpectrogramColumnManager>(go);
                    PlatformManager.SpawnedComponents.Add(specManager);
                }
                specManager.CreateColumns(go);
            }

            if (go.GetComponentInChildren<SpectrogramMaterial>(true) != null) {
                // Add spectrogram materials manager
                SpectrogramMaterialManager specMatManager = go.GetComponent<SpectrogramMaterialManager>();
                if (specMatManager == null) {
                    specMatManager = _container.InstantiateComponent<SpectrogramMaterialManager>(go);
                    PlatformManager.SpawnedComponents.Add(specMatManager);
                }
                specMatManager.UpdateMaterials(go);
            }

            if (go.GetComponentInChildren<SpectrogramAnimationState>(true) != null) {
                // Add spectrogram animation state manager
                SpectrogramAnimationStateManager specAnimManager = go.GetComponent<SpectrogramAnimationStateManager>();
                if (specAnimManager == null) {
                    specAnimManager = _container.InstantiateComponent<SpectrogramAnimationStateManager>(go);
                    PlatformManager.SpawnedComponents.Add(specAnimManager);
                }
                specAnimManager.UpdateAnimationStates();
            }

            // Add Song event manager
            if (go.GetComponentInChildren<SongEventHandler>(true) != null) {
                foreach (SongEventHandler handler in go.GetComponentsInChildren<SongEventHandler>()) {
                    SongEventManager manager = _container.InstantiateComponent<SongEventManager>(handler.gameObject);
                    PlatformManager.SpawnedComponents.Add(manager);
                    manager._songEventHandler = handler;
                }
            }

            // Add EventManager 
            if (go.GetComponentInChildren<EventManager>(true) != null) {
                foreach (EventManager em in go.GetComponentsInChildren<EventManager>()) {
                    PlatformEventManager pem = _container.InstantiateComponent<PlatformEventManager>(em.gameObject);
                    PlatformManager.SpawnedComponents.Add(pem);
                    pem._EventManager = em;
                }
            }
        }
    }
}
