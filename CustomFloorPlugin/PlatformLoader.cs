using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

using CustomFloorPlugin.Configuration;
using CustomFloorPlugin.Utilities;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin {


    /// <summary>
    /// Loads AssetBundles containing CustomFloorPlugin
    /// </summary>
    internal class PlatformLoader {

        [Inject]
        private readonly PluginConfig _config;

        internal string customPlatformsFolderPath = Path.Combine(Environment.CurrentDirectory, "CustomPlatforms");

        private Sprite lvlInsaneCover;
        private Sprite fallbackCover;

        internal List<CustomPlatform> CreateAllPlatforms(Transform parent) {

            // Create the CustomFloorPlugin folder if it doesn't already exist
            if (!Directory.Exists(customPlatformsFolderPath)) {
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

            foreach (string path in allBundlePaths) {
                CustomPlatform newPlatform = LoadPlatformBundle(path, parent);
                if (newPlatform != null) platforms.Add(newPlatform);
            }

            return platforms;
        }

        internal CustomPlatform LoadPlatformBundle(string bundlePath, Transform parent) {

            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null) return null;

            TextAsset scripts = bundle.LoadAsset<TextAsset>("_Scripts.dll");
            if (scripts != null && _config.LoadCustomScripts) {
                Assembly.Load(scripts.bytes);
            }

            GameObject platformPrefab = bundle.LoadAsset<GameObject>("_CustomPlatform");
            if (platformPrefab == null) return null;

            GameObject newPlatform = GameObject.Instantiate(platformPrefab, parent);

            bundle.Unload(false);

            // Collect author and name
            CustomPlatform customPlatform = newPlatform.GetComponent<CustomPlatform>();

            if (customPlatform == null) {
                // Check for old platform 
                global::CustomPlatform legacyPlatform = newPlatform.GetComponent<global::CustomPlatform>();
                if (legacyPlatform != null) {
                    // Replace legacyplatform component with up to date one
                    customPlatform = newPlatform.AddComponent<CustomPlatform>();
                    customPlatform.platName = legacyPlatform.platName;
                    customPlatform.platAuthor = legacyPlatform.platAuthor;
                    customPlatform.hideDefaultPlatform = true;
                    // Remove old platform data
                    GameObject.Destroy(legacyPlatform);
                }
                else {
                    // no customplatform component, abort
                    GameObject.Destroy(newPlatform);
                    return null;
                }
            }

            customPlatform.name = customPlatform.platName + " by " + customPlatform.platAuthor;

            platformPrefab.SetActive(false);

            using MD5 md5 = MD5.Create();
            using FileStream fileStream = File.OpenRead(bundlePath);
            byte[] hash = md5.ComputeHash(fileStream);
            customPlatform.platHash = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();

            if (customPlatform.icon == null) customPlatform.icon = fallbackCover;

            MaterialSwapper.ReplaceMaterials(newPlatform);

            return customPlatform;
        }
    }
}