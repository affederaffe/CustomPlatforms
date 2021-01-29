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
    internal class PlatformLoader : IInitializable {

        [Inject]
        private readonly PluginConfig _config;

        private string customPlatformsFolderPath;

        private Sprite feetIcon;

        public void Initialize() {
            customPlatformsFolderPath = Path.Combine(Environment.CurrentDirectory, "CustomPlatforms");
        }


        /// <summary>
        /// Loads AssetBundles and populates the platforms array with CustomPlatform objects
        /// </summary>
        internal List<CustomPlatform> CreateAllPlatforms(Transform parent) {


            // Create the CustomFloorPlugin folder if it doesn't already exist
            if (!Directory.Exists(customPlatformsFolderPath)) {
                Directory.CreateDirectory(customPlatformsFolderPath);
            }

            // Find AssetBundles in our CustomFloorPlugin directory
            string[] allBundlePaths = Directory.GetFiles(customPlatformsFolderPath, "*.plat");

            List<CustomPlatform> platforms = new List<CustomPlatform>();

            // Create a dummy CustomPlatform for the original platform
            CustomPlatform defaultPlatform = new GameObject("Default Platform").AddComponent<CustomPlatform>();
            defaultPlatform.transform.parent = parent;
            defaultPlatform.platName = "Default Environment";
            defaultPlatform.platAuthor = "Beat Saber";
            Texture2D texture = Resources.FindObjectsOfTypeAll<Texture2D>().FirstOrDefault(x => x.name == "LvlInsaneCover");
            defaultPlatform.icon = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            platforms.Add(defaultPlatform);
            // Populate the platforms array
            Utilities.Logging.Log("[START OF PLATFORM LOADING SPAM]-------------------------------------");
            for (int i = 0; i < allBundlePaths.Length; i++) {
                CustomPlatform newPlatform = LoadPlatformBundle(allBundlePaths[i], parent);
                if (newPlatform != null) {
                    platforms.Add(newPlatform);
                    MaterialSwapper.ReplaceMaterials(newPlatform.gameObject);
                    Logging.Log(newPlatform.platName + " by " + newPlatform.platAuthor);
                }
            }
            Logging.Log("[END OF PLATFORM LOADING SPAM]---------------------------------------");

            return platforms;
        }


        /// <summary>
        /// Loads a <see cref="CustomPlatform"/> from disk into memory and instantiates it.<br/>
        /// Part of this logic has been moved to a different function, for no apparent reason.
        /// </summary>
        /// <param name="bundlePath">The location of the <see cref="CustomPlatform"/>s <see cref="AssetBundle"/> file on disk</param>
        /// <param name="parent">The parent <see cref="Transform"/> for the new <see cref="CustomPlatform"/></param>
        private CustomPlatform LoadPlatformBundle(string bundlePath, Transform parent) {

            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);

            if (bundle == null) {
                return null;
            }

            CustomPlatform newPlatform = LoadPlatform(bundle, parent);

            using MD5 md5 = MD5.Create();
            using FileStream stream = File.OpenRead(bundlePath);

            byte[] hash = md5.ComputeHash(stream);
            newPlatform.platHash = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();

            return newPlatform;
        }


        /// <summary>
        /// Instantiates a platform from an assetbundle.
        /// </summary>
        /// <param name="bundle">An AssetBundle containing a CustomPlatform</param>
        /// <param name="parent">The <see cref="Transform"/> under which this <paramref name="bundle"/> will be instantiated</param>
        /// <returns></returns>
        private CustomPlatform LoadPlatform(AssetBundle bundle, Transform parent) {

            // CustomScripts are now integrated into the AssetBundle (finally)
            TextAsset scriptsAsset = bundle.LoadAsset<TextAsset>("_Scripts.dll");
            if (scriptsAsset != null && _config.LoadCustomScripts) {
                Assembly.Load(scriptsAsset.bytes);
            }

            GameObject platformPrefab = bundle.LoadAsset<GameObject>("_CustomPlatform");
            if (platformPrefab == null) {
                return null;
            }

            GameObject newPlatform = UnityEngine.Object.Instantiate(platformPrefab.gameObject);

            foreach (AudioListener al in newPlatform.transform.GetComponentsInChildren<AudioListener>(true)) {
                GameObject.DestroyImmediate(al);
            }


            newPlatform.transform.parent = parent;

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

            newPlatform.name = customPlatform.platName + " by " + customPlatform.platAuthor;

            if (feetIcon == null) {
                feetIcon = Resources.FindObjectsOfTypeAll<Sprite>().Where(x => x.name == "FeetIcon").FirstOrDefault();
            }

            if (customPlatform.icon == null) {
                customPlatform.icon = feetIcon;
            }

            newPlatform.SetActive(false);

            return customPlatform;
        }
    }
}