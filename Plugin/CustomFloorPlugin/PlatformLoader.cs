using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

using SiraUtil.Tools;

using UnityEngine;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Loads AssetBundles containing CustomPlatforms
    /// </summary>
    public class PlatformLoader
    {
        private readonly SiraLog _siraLog;
        private readonly MaterialSwapper _materialSwapper;

        public PlatformLoader(SiraLog siraLog, MaterialSwapper materialSwapper)
        {
            _siraLog = siraLog;
            _materialSwapper = materialSwapper;
        }

        /// <summary>
        /// Asynchronously loads a <see cref="CustomPlatform"/> from a specified file path
        /// </summary>
        internal async Task<CustomPlatform?> LoadFromFileAsync(string fullPath)
        {
            if (!File.Exists(fullPath))
            {
                _siraLog.Error($"File could not be found:\n{fullPath}");
                return null;
            }

            using FileStream fileStream = File.OpenRead(fullPath);

            AssetBundle assetBundle = await LoadAssetBundleFromStreamAsync(fileStream);

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

            CustomPlatform customPlatform = platformPrefab.GetComponent<CustomPlatform>();

            if (customPlatform == null)
            {
                // Check for old platform 
                global::CustomPlatform legacyPlatform = platformPrefab.GetComponent<global::CustomPlatform>();
                if (legacyPlatform != null)
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

            using MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(fileStream);
            customPlatform.platHash = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
            customPlatform.fullPath = fullPath;
            customPlatform.name = $"{customPlatform.platName} by {customPlatform.platAuthor}";

            await _materialSwapper.ReplaceMaterials(customPlatform.gameObject);

            return customPlatform;
        }

        /// <summary>
        /// Asynchronously loads and <see cref="AssetBundle"/> from a <see cref="FileStream"/>
        /// </summary>
        private static async Task<AssetBundle> LoadAssetBundleFromStreamAsync(FileStream fileStream)
        {
            TaskCompletionSource<AssetBundle> taskSource = new();
            AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromStreamAsync(fileStream);
            assetBundleCreateRequest.completed += delegate
            {
                AssetBundle assetBundle = assetBundleCreateRequest.assetBundle;
                taskSource.SetResult(assetBundle);
            };

            return await taskSource.Task;
        }

        /// <summary>
        /// Asynchronously loads an Asset <typeparamref name="T"/> from an <see cref="AssetBundle"/>
        /// </summary>
        private static async Task<T> LoadAssetFromAssetBundleAsync<T>(AssetBundle assetBundle, string assetName) where T : UnityEngine.Object
        {
            TaskCompletionSource<T> taskSource = new();
            AssetBundleRequest assetBundleRequest = assetBundle.LoadAssetAsync<T>(assetName);
            assetBundleRequest.completed += delegate
            {
                T asset = (T)assetBundleRequest.asset;
                taskSource.SetResult(asset);
            };

            return await taskSource.Task;
        }
    }
}