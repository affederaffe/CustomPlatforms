using System.Collections;
using System.Linq;

using CustomFloorPlugin.Configuration;

using IPA.Utilities;

using SiraUtil.Tools;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    internal abstract class PlatformSpawner
    {
        [Inject]
        protected readonly SiraLog _siraLog;

        [Inject]
        protected readonly PluginConfig _config;

        [Inject]
        protected readonly EnvironmentHider _hider;

        [Inject]
        protected readonly PlatformLoader _platformLoader;

        [Inject]
        protected readonly PlatformManager _platformManager;

        [Inject]
        protected readonly LightWithIdManager _lightManager;

        protected DiContainer _container;

        /// <summary>
        /// Changes to a specific <see cref="CustomPlatform"/> and saves the choice
        /// </summary>
        /// <param name="index">The index of the new <see cref="CustomPlatform"/> in the list <see cref="AllPlatforms"/></param>
        internal void SetPlatformAndShow(int index, PlatformType platformType)
        {
            switch (platformType)
            {
                case PlatformType.Singleplayer:
                    _platformManager.currentSingleplayerPlatform = _platformManager.allPlatforms[index];
                    _config.SingleplayerPlatformPath = _platformManager.currentSingleplayerPlatform.platName + _platformManager.currentSingleplayerPlatform.platAuthor;
                    break;
                case PlatformType.Multiplayer:
                    _platformManager.currentMultiplayerPlatform = _platformManager.allPlatforms[index];
                    _config.MultiplayerPlatformPath = _platformManager.currentMultiplayerPlatform.platName + _platformManager.currentMultiplayerPlatform.platAuthor;
                    break;
                case PlatformType.A360:
                    _platformManager.currentA360Platform = _platformManager.allPlatforms[index];
                    _config.A360PlatformPath = _platformManager.currentA360Platform.platName + _platformManager.currentA360Platform.platAuthor;
                    break;
            }
            ChangeToPlatform(index);
        }

        /// <summary>
        /// Changes to the currently selected <see cref="CustomPlatform"/>
        /// </summary>
        internal void ChangeToPlatform(PlatformType platformType)
        {
            int index = _platformManager.GetIndexForType(platformType);
            ChangeToPlatform(index);
        }

        /// <summary>
        /// Changes to a specific <see cref="CustomPlatform"/>
        /// </summary>
        /// <param name="index">The index of the new <see cref="CustomPlatform"/> in the list <see cref="AllPlatforms"/></param>
        internal void ChangeToPlatform(int index)
        {
            if (!_platformManager.allPlatforms[index].requirements.All(x => _platformManager.allPluginNames.Contains(x)))
            {
                _siraLog.Warning("Missing requirement for platform " + _platformManager.allPlatforms[index].name);
                ChangeToPlatform(0);
                return;
            }

            _siraLog.Info("Switching to " + _platformManager.allPlatforms[index].name);
            _platformManager.activePlatform?.gameObject.SetActive(false);
            DestroyCustomObjects();
            _platformManager.activePlatform = _platformManager.allPlatforms[index];

            SharedCoroutineStarter.instance.StartCoroutine(WaitAndSpawn());
            IEnumerator WaitAndSpawn()
            {
                if (_platformManager.activePlatform.transform.childCount == 0 && index != 0)
                {
                    yield return SharedCoroutineStarter.instance.StartCoroutine(_platformLoader.LoadFromFileAsync(_platformManager.activePlatform.fullPath, _platformManager.HandlePlatformLoaded));
                }

                yield return new WaitForEndOfFrame();
                if (index != 0)
                {
                    _platformManager.activePlatform.gameObject.SetActive(true);
                    AddManagers(_platformManager.activePlatform);
                    SpawnCustomObjects();
                }
                else
                    _platformManager.activePlatform = null;

                _hider.HideObjectsForPlatform(_platformManager.allPlatforms[index]);
            }
        }

        /// <summary>
        /// Spawns all registered custom objects, as required by the selected <see cref="CustomPlatform"/>
        /// </summary>
        private void SpawnCustomObjects()
        {
            PlatformManager.SpawnQueue(_lightManager);
        }

        /// <summary>
        /// Despawns all registered custom objects, as required by the selected <see cref="CustomPlatform"/>
        /// </summary>
        private void DestroyCustomObjects()
        {
            while (PlatformManager.SpawnedObjects.Count != 0)
            {
                GameObject gameObject = PlatformManager.SpawnedObjects[0];
                PlatformManager.SpawnedObjects.Remove(gameObject);
                GameObject.Destroy(gameObject);
                foreach (TubeBloomPrePassLight tubeBloomPrePassLight in gameObject.GetComponentsInChildren<TubeBloomPrePassLight>(true))
                {
                    //Unity requires this to be present, otherwise Unregister won't be called. Memory leaks may occour if this is removed.
                    tubeBloomPrePassLight.InvokeMethod<object, BloomPrePassLight>("UnregisterLight");
                }
            }

            while (PlatformManager.SpawnedComponents.Count != 0)
            {
                Component component = PlatformManager.SpawnedComponents[0];
                PlatformManager.SpawnedComponents.Remove(component);
                GameObject.Destroy(component);
            }
        }

        private enum NotifyType
        {
            Enable = 0,
            Disable = 1
        }

        /// <summary>
        /// Adds managers to a <see cref="CustomPlatform"/>
        /// </summary>
        /// <param name="customPlatform">The <see cref="CustomPlatform"/> for which to spawn managers</param>
        private void AddManagers(CustomPlatform customPlatform)
        {
            GameObject go = customPlatform.gameObject;
            bool active = go.activeSelf;
            if (active)
                go.SetActive(false);
            AddManagers(go, go);
            if (active)
                go.SetActive(true);
        }

        /// <summary>
        /// Recursively attaches managers to a <see cref="CustomPlatform"/>
        /// </summary>
        /// <param name="go">The current <see cref="GameObject"/>, this parameter acts as a pointer</param>
        /// <param name="root">The root <see cref="GameObject"/> of the <see cref="CustomPlatform"/></param>
        private void AddManagers(GameObject go, GameObject root)
        {
            // Rotation effect manager
            if (go.GetComponentInChildren<RotationEventEffect>(true) != null || go.GetComponentInChildren<PairRotationEventEffect>(true) != null)
            {
                RotationEventEffectManager rotManager = go.GetComponent<RotationEventEffectManager>();
                if (rotManager == null)
                {
                    rotManager = _container.InstantiateComponent<RotationEventEffectManager>(go);
                    PlatformManager.SpawnedComponents.Add(rotManager);
                    rotManager.CreateEffects(go);
                }
            }

            // Add a trackRing controller if there are track ring descriptors
            if (go.GetComponentInChildren<TrackRings>(true) != null)
            {
                foreach (TrackRings trackRings in go.GetComponentsInChildren<TrackRings>(true))
                {
                    GameObject ringPrefab = trackRings.trackLaneRingPrefab;

                    // Add managers to prefabs (nesting)
                    AddManagers(ringPrefab, root);
                }

                TrackRingsManagerSpawner trms = go.GetComponent<TrackRingsManagerSpawner>();
                if (trms == null)
                {
                    trms = _container.InstantiateComponent<TrackRingsManagerSpawner>(go);
                    PlatformManager.SpawnedComponents.Add(trms);
                }
                trms.CreateTrackRings(go);
            }

            // Add spectrogram manager
            if (go.GetComponentInChildren<Spectrogram>(true) != null)
            {
                foreach (Spectrogram spec in go.GetComponentsInChildren<Spectrogram>(true))
                {
                    GameObject colPrefab = spec.columnPrefab;
                    AddManagers(colPrefab, root);
                }

                SpectrogramColumnManager specManager = go.GetComponent<SpectrogramColumnManager>();
                if (specManager == null)
                {
                    specManager = _container.InstantiateComponent<SpectrogramColumnManager>(go);
                    PlatformManager.SpawnedComponents.Add(specManager);
                }
                specManager.CreateColumns(go);
            }

            if (go.GetComponentInChildren<SpectrogramMaterial>(true) != null)
            {
                // Add spectrogram materials manager
                SpectrogramMaterialManager specMatManager = go.GetComponent<SpectrogramMaterialManager>();
                if (specMatManager == null)
                {
                    specMatManager = _container.InstantiateComponent<SpectrogramMaterialManager>(go);
                    PlatformManager.SpawnedComponents.Add(specMatManager);
                }
                specMatManager.UpdateMaterials(go);
            }

            if (go.GetComponentInChildren<SpectrogramAnimationState>(true) != null)
            {
                // Add spectrogram animation state manager
                SpectrogramAnimationStateManager specAnimManager = go.GetComponent<SpectrogramAnimationStateManager>();
                if (specAnimManager == null)
                {
                    specAnimManager = _container.InstantiateComponent<SpectrogramAnimationStateManager>(go);
                    PlatformManager.SpawnedComponents.Add(specAnimManager);
                }
                specAnimManager.UpdateAnimationStates();
            }

            if (go.GetComponentInChildren<ColorMaterial>(true) != null)
            {
                // Add color materials manager
                ColorMaterialManager colMatManager = go.GetComponent<ColorMaterialManager>();
                if (colMatManager == null)
                {
                    colMatManager = _container.InstantiateComponent<ColorMaterialManager>(go);
                    PlatformManager.SpawnedComponents.Add(colMatManager);
                }
                colMatManager.UpdateMaterials(go);
            }

            // Add Song event manager
            if (go.GetComponentInChildren<SongEventHandler>(true) != null)
            {
                foreach (SongEventHandler handler in go.GetComponentsInChildren<SongEventHandler>())
                {
                    SongEventManager manager = _container.InstantiateComponent<SongEventManager>(handler.gameObject);
                    PlatformManager.SpawnedComponents.Add(manager);
                    manager._songEventHandler = handler;
                }
            }

            // Add EventManager 
            if (go.GetComponentInChildren<EventManager>(true) != null)
            {
                foreach (EventManager em in go.GetComponentsInChildren<EventManager>())
                {
                    PlatformEventManager pem = _container.InstantiateComponent<PlatformEventManager>(em.gameObject);
                    PlatformManager.SpawnedComponents.Add(pem);
                    pem._eventManager = em;
                }
            }
        }
    }
}
