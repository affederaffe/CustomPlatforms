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
        internal readonly Dictionary<string, CustomPlatform> platformFilePaths = new();
        private readonly MaterialSwapper _materialSwapper;

        public PlatformLoader(MaterialSwapper materialSwapper)
        {
            _materialSwapper = materialSwapper;
        }

        /// <summary>
        /// Asynchronously loads a <see cref="CustomPlatform"/> from a specified file path
        /// </summary>
        internal IEnumerator<AsyncOperation> LoadFromFileAsync(string fullPath, Action<CustomPlatform, string> callback)
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

            GameObject platformPrefab = (GameObject)platformAssetBundleRequest.asset;

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

            customPlatform.name = customPlatform.platName + " by " + customPlatform.platAuthor;
            customPlatform.fullPath = fullPath;

            using MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(fileStream);
            customPlatform.platHash = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();

            _materialSwapper.ReplaceMaterials(customPlatform.gameObject);

            callback(customPlatform, fullPath);

            GameObject.Destroy(platformPrefab);

            yield break;
        }
    }
}