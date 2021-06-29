using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

using SiraUtil.Tools;

using UnityEngine;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Loads AssetBundles containing <see cref="CustomPlatform"/>s
    /// </summary>
    public class PlatformLoader
    {
        private readonly SiraLog _siraLog;
        private readonly MaterialSwapper _materialSwapper;

        private readonly Dictionary<string, Task<CustomPlatform?>> _pathTaskPairs;

        public PlatformLoader(SiraLog siraLog, MaterialSwapper materialSwapper)
        {
            _siraLog = siraLog;
            _materialSwapper = materialSwapper;
            _pathTaskPairs = new Dictionary<string, Task<CustomPlatform?>>();
        }

        /// <summary>
        /// Loads the platform's AssetBundle located at <param name="fullPath"></param><br/>
        /// If the loading process for this path is already started, the corresponding Task is awaited and the result returned
        /// </summary>
        /// <param name="fullPath">The path to the platform's AssetBundle</param>
        /// <returns>The loaded <see cref="CustomPlatform"/>, or null if an error occurs</returns>
        internal async Task<CustomPlatform?> LoadPlatformFromFileAsync(string fullPath)
        {
            if (!File.Exists(fullPath))
            {
                _siraLog.Error($"File could not be found:\n{fullPath}");
                return null;
            }

            if (_pathTaskPairs.TryGetValue(fullPath, out Task<CustomPlatform?> task)) return await task;
            task = LoadPlatformFromFileAsyncImpl(fullPath);
            _pathTaskPairs.Add(fullPath, task);
            CustomPlatform? platform = await task;
            _pathTaskPairs.Remove(fullPath);
            return platform;
        }

        /// <summary>
        /// Asynchronously loads a <see cref="CustomPlatform"/> from a specified file path
        /// </summary>
        private async Task<CustomPlatform?> LoadPlatformFromFileAsyncImpl(string fullPath)
        {
            byte[] bundleData = await Task.Run(() => File.ReadAllBytes(fullPath));
            AssetBundle assetBundle = await LoadAssetBundleFromBytesAsync(bundleData);

            if (assetBundle == null)
            {
                _siraLog.Error($"File could not be loaded:\n{fullPath}");
                return null;
            }

            GameObject platformPrefab = await LoadAssetFromAssetBundleAsync<GameObject>(assetBundle, "_CustomPlatform");

            if (platformPrefab == null)
            {
                assetBundle.Unload(true);
                _siraLog.Error($"Platform GameObject could not be loaded:\n{fullPath}");
                return null;
            }

            assetBundle.Unload(false);

            CustomPlatform? customPlatform = platformPrefab.GetComponent<CustomPlatform>();

            if (customPlatform is null)
            {
                // Check for old platform 
                global::CustomPlatform? legacyPlatform = platformPrefab.GetComponent<global::CustomPlatform>();
                if (legacyPlatform is not null)
                {
                    // Replace legacy platform component with up to date one
                    customPlatform = platformPrefab.AddComponent<CustomPlatform>();
                    customPlatform.platName = legacyPlatform.platName;
                    customPlatform.platAuthor = legacyPlatform.platAuthor;
                    customPlatform.hideDefaultPlatform = true;
                    // Remove old platform data
                    UnityEngine.Object.Destroy(legacyPlatform);
                }
                else
                {
                    // No CustomPlatform component, abort
                    UnityEngine.Object.Destroy(platformPrefab);
                    _siraLog.Error($"AssetBundle does not contain a CustomPlatform:\n{fullPath}");
                    return null;
                }
            }

            customPlatform.platHash = await Task.Run(() => ComputeHash(bundleData));
            customPlatform.fullPath = fullPath;
            customPlatform.name = $"{customPlatform.platName} by {customPlatform.platAuthor}";

            await _materialSwapper.ReplaceMaterialsAsync(customPlatform.gameObject);

            return customPlatform;
        }

        /// <summary>
        /// Computes the MD5 hash value for the given <see cref="data"/> and returns it's hexadecimal string representation
        /// </summary>
        private static string ComputeHash(byte[] data)
        {
            using MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }

        /// <summary>
        /// Asynchronously loads and <see cref="AssetBundle"/> from a <see cref="byte"/>[]
        /// </summary>
        private static async Task<AssetBundle> LoadAssetBundleFromBytesAsync(byte[] data)
        {
            TaskCompletionSource<AssetBundle> taskCompletionSource = new();
            AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync(data);
            assetBundleCreateRequest.completed += delegate
            {
                AssetBundle assetBundle = assetBundleCreateRequest.assetBundle;
                taskCompletionSource.SetResult(assetBundle);
            };

            return await taskCompletionSource.Task;
        }

        /// <summary>
        /// Asynchronously loads an asset of type <typeparamref name="T"/> from an <see cref="AssetBundle"/>
        /// </summary>
        private static async Task<T> LoadAssetFromAssetBundleAsync<T>(AssetBundle assetBundle, string assetName) where T : UnityEngine.Object
        {
            TaskCompletionSource<T> taskCompletionSource = new();
            AssetBundleRequest assetBundleRequest = assetBundle.LoadAssetAsync<T>(assetName);
            assetBundleRequest.completed += delegate
            {
                T asset = (T)assetBundleRequest.asset;
                taskCompletionSource.SetResult(asset);
            };

            return await taskCompletionSource.Task;
        }
    }
}