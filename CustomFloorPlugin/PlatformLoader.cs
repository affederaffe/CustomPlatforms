using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

using CustomFloorPlugin.Configuration;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Loads AssetBundles containing CustomFloorPlugin
    /// </summary>
    internal class PlatformLoader
    {
        [Inject]
        private readonly MaterialSwapper _materialSwapper;

        [Inject]
        private readonly PluginConfig _config;

        internal readonly string customPlatformsFolderPath = Path.Combine(Environment.CurrentDirectory, "CustomPlatforms");
        internal readonly Dictionary<CustomPlatform, string> customPlatformPaths = new Dictionary<CustomPlatform, string>();

        private Sprite lvlInsaneCover;
        private Sprite fallbackCover;

        internal List<CustomPlatform> CreateAllDescriptors(Transform parent)
        {
            // Create the CustomFloorPlugin folder if it doesn't already exist
            if (!Directory.Exists(customPlatformsFolderPath))
            {
                Directory.CreateDirectory(customPlatformsFolderPath);
            }

            // Find AssetBundles in our CustomFloorPlugin directory
            string[] allBundlePaths = Directory.GetFiles(customPlatformsFolderPath, "*.plat");

            List<CustomPlatform> platforms = new List<CustomPlatform>();

            Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
            lvlInsaneCover = allSprites.First(x => x.name == "LvlInsaneCover");
            fallbackCover = allSprites.First(x => x.name == "FeetIcon");

            // Create a dummy CustomPlatform for the original platform
            CustomPlatform defaultPlatform = new GameObject("Default Platform").AddComponent<CustomPlatform>();
            defaultPlatform.transform.parent = parent;
            defaultPlatform.platName = "Default Environment";
            defaultPlatform.platAuthor = "Beat Saber";
            defaultPlatform.icon = lvlInsaneCover;
            platforms.Add(defaultPlatform);

            foreach (string path in allBundlePaths)
            {
                CustomPlatform newPlatform = GetPlatformInfo(path);
                if (newPlatform != null)
                {
                    platforms.Add(newPlatform);
                    customPlatformPaths.Add(newPlatform, path);
                }
            }
            return platforms;
        }

        internal CustomPlatform GetPlatformInfo(string bundlePath)
        {
            AssetBundle assetBundle = AssetBundle.LoadFromFile(bundlePath);
            if (!assetBundle)
                return null;

            GameObject platformPrefab = assetBundle.LoadAsset<GameObject>("_CustomPlatform");
            if (!platformPrefab)
                return null;

            platformPrefab.transform.DetachChildren();
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
                    return null;
                }
            }

            // When the bundle is unloaded, the texture gets destroyed, when it isn't it drasticly increases the loading time.
            // Since the texture isn't instantiable, this clusterfuck is needed. Just Unity things ._.
            customPlatform = GameObject.Instantiate(customPlatform);
            if (customPlatform.icon != null)
            {
                int width = Mathf.Max((int)customPlatform.icon.rect.width, 128);
                int height = Mathf.Max((int)customPlatform.icon.rect.height, 128);
                RenderTexture renderTex = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
                Graphics.Blit(customPlatform.icon.texture, renderTex);
                Texture2D clonedTex = new Texture2D(width, height);
                clonedTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                clonedTex.Apply();
                RenderTexture.ReleaseTemporary(renderTex);
                customPlatform.icon = Sprite.Create(clonedTex, new Rect(Vector2.zero, new Vector2(width, height)), Vector2.zero);
            }
            else
            {
                customPlatform.icon = fallbackCover;
            }

            assetBundle.Unload(true);
            customPlatform.name = customPlatform.platName + " by " + customPlatform.platAuthor;

            return customPlatform;
        }

        internal CustomPlatform LoadPlatformBundle(string bundlePath, Transform parent)
        {

            AssetBundle assetBundle = AssetBundle.LoadFromFile(bundlePath);
            if (!assetBundle)
                return null;

            TextAsset scriptAsset = assetBundle.LoadAsset<TextAsset>("_Scripts.dll");
            if (_config.LoadCustomScripts && scriptAsset)
            {
                Assembly.Load(scriptAsset.bytes);
            }

            GameObject platformPrefab = assetBundle.LoadAsset<GameObject>("_CustomPlatform");
            if (!platformPrefab)
                return null;

            GameObject newPlatform = GameObject.Instantiate(platformPrefab, parent);
            assetBundle.Unload(false);

            // Collect author and name
            CustomPlatform customPlatform = newPlatform.GetComponent<CustomPlatform>();

            if (customPlatform == null)
            {
                // Check for old platform 
                global::CustomPlatform legacyPlatform = newPlatform.GetComponent<global::CustomPlatform>();
                if (legacyPlatform != null)
                {
                    // Replace legacyplatform component with up to date one
                    customPlatform = newPlatform.AddComponent<CustomPlatform>();
                    customPlatform.platName = legacyPlatform.platName;
                    customPlatform.platAuthor = legacyPlatform.platAuthor;
                    customPlatform.hideDefaultPlatform = true;
                    // Remove old platform data
                    GameObject.Destroy(legacyPlatform);
                }
                else
                {
                    // no customplatform component, abort
                    GameObject.Destroy(newPlatform);
                    return null;
                }
            }

            customPlatform.name = customPlatform.platName + " by " + customPlatform.platAuthor;

            using MD5 md5 = MD5.Create();
            using FileStream fileStream = File.OpenRead(bundlePath);
            byte[] hash = md5.ComputeHash(fileStream);
            customPlatform.platHash = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();

            _materialSwapper.ReplaceMaterials(newPlatform);

            return customPlatform;
        }
    }
}