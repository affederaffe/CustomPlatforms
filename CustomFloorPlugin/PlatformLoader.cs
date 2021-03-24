using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

using UnityEngine;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Loads AssetBundles containing CustomPlatorms
    /// </summary>
    public class PlatformLoader
    {
        private readonly AssetLoader _assetLoader;
        private readonly MaterialSwapper _materialSwapper;

        internal readonly Dictionary<string, CustomPlatform> platformFilePaths = new();

        public PlatformLoader(AssetLoader assetLoader, MaterialSwapper materialSwapper)
        {
            _assetLoader = assetLoader;
            _materialSwapper = materialSwapper;
        }

        /// <summary>
        /// Asynchronously loads a <see cref="CustomPlatform"/> from a specified file path
        /// </summary>
        internal IEnumerator<AsyncOperation> LoadFromFileAsync(string fullPath, Action<CustomPlatform> callback)
        {
            if (!File.Exists(fullPath))
                throw new FileNotFoundException("File could not be found", fullPath);

            using FileStream fileStream = File.OpenRead(fullPath);

            AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromStreamAsync(fileStream);
            yield return assetBundleCreateRequest;
            if (!assetBundleCreateRequest.isDone || !assetBundleCreateRequest.assetBundle)
                throw new FileLoadException("File coulnd not be loaded", fullPath);

            AssetBundleRequest platformAssetBundleRequest = assetBundleCreateRequest.assetBundle.LoadAssetAsync("_CustomPlatform");
            yield return platformAssetBundleRequest;
            if (!platformAssetBundleRequest.isDone || !platformAssetBundleRequest.asset)
            {
                assetBundleCreateRequest.assetBundle.Unload(true);
                throw new FileLoadException("File coulnd not be loaded", fullPath);
            }

            assetBundleCreateRequest.assetBundle.Unload(false);

            GameObject platformPrefab = platformAssetBundleRequest.asset as GameObject;

            foreach (AudioListener al in platformPrefab.GetComponentsInChildren<AudioListener>())
            {
                GameObject.DestroyImmediate(al);
            }

            CustomPlatform customPlatform = platformPrefab.GetComponent<CustomPlatform>();

            if (customPlatform == null)
            {
                // Check for old platform 
                global::CustomPlatform legacyPlatform = platformPrefab.GetComponent<global::CustomPlatform>();
                if (legacyPlatform != null)
                {
                    // Replace legacyplatform component with up to date one
                    customPlatform = platformPrefab.AddComponent<CustomPlatform>();
                    customPlatform.platName = legacyPlatform.platName;
                    customPlatform.platAuthor = legacyPlatform.platAuthor;
                    customPlatform.hideDefaultPlatform = true;
                    // Remove old platform data
                    GameObject.Destroy(legacyPlatform);
                }
                else
                {
                    // no customplatform component, abort
                    GameObject.Destroy(platformPrefab);
                    yield break;
                }
            }

            using MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(fileStream);
            customPlatform.platHash = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
            customPlatform.fullPath = fullPath;
            customPlatform.name = customPlatform.platName + " by " + customPlatform.platAuthor;
            if (customPlatform.icon == null)
                customPlatform.icon = _assetLoader.fallbackCover;

            _materialSwapper.ReplaceMaterials(customPlatform.gameObject);

            callback.Invoke(customPlatform);

            GameObject.Destroy(platformPrefab);
        }
    }
}