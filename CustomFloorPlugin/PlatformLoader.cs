using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using static CustomFloorPlugin.Utilities.Logging;

namespace CustomFloorPlugin {
    /// <summary>
    /// Loads AssetBundles containing CustomPlatforms and handles cycling between them
    /// </summary>
    internal static class PlatformLoader {


        /// <summary>
        /// Loads AssetBundles and populates the platforms array with CustomPlatform objects
        /// </summary>
        public static List<CustomPlatform> CreateAllPlatforms(Transform parent) {

            string customPlatformsFolderPath = Path.Combine(Environment.CurrentDirectory, Constants.customFolder);

            // Create the CustomPlatforms folder if it doesn't already exist
            if(!Directory.Exists(customPlatformsFolderPath)) {
                Directory.CreateDirectory(customPlatformsFolderPath);
            }

            // Find AssetBundles in our CustomPlatforms directory
            string[] allBundlePaths = Directory.GetFiles(customPlatformsFolderPath, "*.plat");

            List<CustomPlatform>  platforms = new List<CustomPlatform>();

            // Create a dummy CustomPlatform for the original platform
            CustomPlatform defaultPlatform = new GameObject("Default Platform").AddComponent<CustomPlatform>();
            defaultPlatform.transform.parent = parent;
            defaultPlatform.platName = "Default Environment";
            defaultPlatform.platAuthor = "Beat Saber";
            Texture2D texture = Resources.FindObjectsOfTypeAll<Texture2D>().First(x => x.name == "LvlInsaneCover");
            defaultPlatform.icon = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            platforms.Add(defaultPlatform);
            // Populate the platforms array
            Log("[START OF PLATFORM LOADING SPAM]-------------------------------------");
            int j = 0;
            for(int i = 0; i < allBundlePaths.Length; i++) {
                j++;
                CustomPlatform newPlatform = LoadPlatformBundle(allBundlePaths[i], parent);
                if(newPlatform != null) {
                    platforms.Add(newPlatform);
                }
            }
            Log("[END OF PLATFORM LOADING SPAM]---------------------------------------");
            // Replace materials for all renderers
            MaterialSwapper.ReplaceMaterials(PlatformManager.PlatformManagerScene);

            return platforms;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms", Justification = "MD5 Hash not used in security relevant context")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Too late now... that damage was done a year ago -.-")]
        public static CustomPlatform LoadPlatformBundle(string bundlePath, Transform parent) {

            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);

            if(bundle == null) return null;

            CustomPlatform newPlatform = LoadPlatform(bundle, parent);

            using var md5 = MD5.Create();
            using var stream = File.OpenRead(bundlePath);

            byte[] hash = md5.ComputeHash(stream);
            newPlatform.platHash = BitConverter
                .ToString(hash)
                .Replace("-", string.Empty)
                .ToLowerInvariant();


            return newPlatform;
        }

        /// <summary>
        /// Instantiates a platform from an assetbundle.
        /// </summary>
        /// <param name="bundle">An AssetBundle containing a CustomPlatform</param>
        /// <returns></returns>
        private static CustomPlatform LoadPlatform(AssetBundle bundle, Transform parent) {

            GameObject platformPrefab = bundle.LoadAsset<GameObject>("_CustomPlatform");

            if(platformPrefab == null) {
                return null;
            }

            GameObject newPlatform = UnityEngine.Object.Instantiate(platformPrefab.gameObject);

            newPlatform.transform.parent = parent;

            bundle.Unload(false);

            // Collect author and name
            CustomPlatform customPlatform = newPlatform.GetComponent<CustomPlatform>();

            if(customPlatform == null) {
                // Check for old platform 
                global::CustomPlatform legacyPlatform = newPlatform.GetComponent<global::CustomPlatform>();
                if(legacyPlatform != null) {
                    // Replace legacyplatform component with up to date one
                    customPlatform = newPlatform.AddComponent<CustomPlatform>();
                    customPlatform.platName = legacyPlatform.platName;
                    customPlatform.platAuthor = legacyPlatform.platAuthor;
                    customPlatform.hideDefaultPlatform = true;
                    // Remove old platform data
                    GameObject.Destroy(legacyPlatform);
                } else {
                    // no customplatform component, abort
                    GameObject.Destroy(newPlatform);
                    return null;
                }
            }

            newPlatform.name = customPlatform.platName + " by " + customPlatform.platAuthor;

            if(customPlatform.icon == null) customPlatform.icon = Resources.FindObjectsOfTypeAll<Sprite>().Where(x => x.name == "FeetIcon").FirstOrDefault();

            newPlatform.SetActive(false);

            return customPlatform;
        }
        internal static void AddManagers(CustomPlatform customPlatform) {
            GameObject go = customPlatform.gameObject;
            bool active = go.activeSelf;
            if(active) {
                go.SetActive(false);
            }
            AddManagers(go, go);
            if(active) {
                go.SetActive(true);
            }
        }
        private static void AddManagers(GameObject go, GameObject root) {

            // Rotation effect manager
            if(go.GetComponentInChildren<RotationEventEffect>(true) != null) {
                RotationEventEffectManager rotManager = root.GetComponent<RotationEventEffectManager>();
                if(rotManager == null) {
                    rotManager = root.AddComponent<RotationEventEffectManager>();
                    PlatformManager.SpawnedComponents.Add(rotManager);
                    rotManager.CreateEffects(go);
                    rotManager.RegisterForEvents();
                }
            }

            // Add a trackRing controller if there are track ring descriptors
            if(go.GetComponentInChildren<TrackRings>(true) != null) {
                foreach(TrackRings trackRings in go.GetComponentsInChildren<TrackRings>(true)) {
                    GameObject ringPrefab = trackRings.trackLaneRingPrefab;

                    // Add managers to prefabs (nesting)
                    AddManagers(ringPrefab, root);
                }

                TrackRingsManagerSpawner trms = root.GetComponent<TrackRingsManagerSpawner>();
                if(trms == null) {
                    trms = root.AddComponent<TrackRingsManagerSpawner>();
                    PlatformManager.SpawnedComponents.Add(trms);
                }
                trms.CreateTrackRings(go);
            }

            // Add spectrogram manager
            if(go.GetComponentInChildren<Spectrogram>(true) != null) {
                foreach(Spectrogram spec in go.GetComponentsInChildren<Spectrogram>(true)) {
                    GameObject colPrefab = spec.columnPrefab;
                    AddManagers(colPrefab, root);
                }

                SpectrogramColumnManager specManager = go.GetComponent<SpectrogramColumnManager>();
                if(specManager == null) {
                    specManager = go.AddComponent<SpectrogramColumnManager>();
                    PlatformManager.SpawnedComponents.Add(specManager);
                }
                specManager.CreateColumns(go);
            }

            if(go.GetComponentInChildren<SpectrogramMaterial>(true) != null) {
                // Add spectrogram materials manager
                SpectrogramMaterialManager specMatManager = go.GetComponent<SpectrogramMaterialManager>();
                if(specMatManager == null) {
                    specMatManager = go.AddComponent<SpectrogramMaterialManager>();
                    PlatformManager.SpawnedComponents.Add(specMatManager);
                }
                specMatManager.UpdateMaterials(go);
            }

            if(go.GetComponentInChildren<SpectrogramAnimationState>(true) != null) {
                // Add spectrogram animation state manager
                SpectrogramAnimationStateManager specAnimManager = go.GetComponent<SpectrogramAnimationStateManager>();
                if(specAnimManager == null) {
                    specAnimManager = go.AddComponent<SpectrogramAnimationStateManager>();
                    PlatformManager.SpawnedComponents.Add(specAnimManager);
                }
                specAnimManager.UpdateAnimationStates();
            }

            // Add Song event manager
            if(go.GetComponentInChildren<SongEventHandler>(true) != null) {
                foreach(SongEventHandler handler in go.GetComponentsInChildren<SongEventHandler>()) {
                    SongEventManager manager = handler.gameObject.AddComponent<SongEventManager>();
                    PlatformManager.SpawnedComponents.Add(manager);
                    manager._songEventHandler = handler;
                }
            }

            // Add EventManager 
            if(go.GetComponentInChildren<EventManager>(true) != null) {
                foreach(EventManager em in go.GetComponentsInChildren<EventManager>()) {
                    PlatformEventManager pem = em.gameObject.AddComponent<PlatformEventManager>();
                    PlatformManager.SpawnedComponents.Add(pem);
                    pem._EventManager = em;
                }
            }
        }
    }
}
