using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

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
        internal async Task LoadFromFileAsync(string fullPath, Action<CustomPlatform> callback)
        {
            if (!File.Exists(fullPath))
                throw new FileNotFoundException("File could not be found", fullPath);

            using FileStream fileStream = File.OpenRead(fullPath);

            AssetBundle assetBundle = await LoadAssetBundleFromStreamAsync(fileStream);

            if (assetBundle == null)
                throw new FileLoadException("File could not be loaded", fullPath);

            GameObject platformPrefab = await LoadAssetFromAssetBundleAsync<GameObject>(assetBundle, "_CustomPlatform");

            if (platformPrefab == null)
            {
                assetBundle.Unload(true);
                throw new FileLoadException("Platform GameObject could not be loaded", fullPath);
            }

            assetBundle.Unload(false);

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
                    return;
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

        /// <summary>
        /// Asynchronously loads and <see cref="AssetBundle"/> from a <see cref="FileStream"/>
        /// </summary>
        private async Task<AssetBundle> LoadAssetBundleFromStreamAsync(FileStream fileStream)
        {
            TaskCompletionSource<AssetBundle> taskSource = new();
            AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromStreamAsync(fileStream);
            assetBundleCreateRequest.completed += delegate
            {
                AssetBundle assetBundle = assetBundleCreateRequest.assetBundle;
                taskSource.TrySetResult(assetBundle);
            };
            return await taskSource.Task;
        }

        /// <summary>
        /// Asynchronously loads an Asset <typeparamref name="T"/> from an <see cref="AssetBundle"/>
        /// </summary>
        private async Task<T> LoadAssetFromAssetBundleAsync<T>(AssetBundle assetBundle, string assetName) where T : UnityEngine.Object
        {
            TaskCompletionSource<T> taskSource = new();
            AssetBundleRequest assetBundleRequest = assetBundle.LoadAssetAsync<T>(assetName);
            assetBundleRequest.completed += delegate
            {
                T asset = (T)assetBundleRequest.asset;
                taskSource.TrySetResult(asset);
            };
            return await taskSource.Task;
        }
    }
}