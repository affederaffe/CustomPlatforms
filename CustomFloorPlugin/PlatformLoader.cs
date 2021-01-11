using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

using UnityEngine;

using CustomFloorPlugin.Exceptions;

using static CustomFloorPlugin.GlobalCollection;
using static CustomFloorPlugin.Utilities.Logging;
using static CustomFloorPlugin.Utilities.UnityObjectSearching;


namespace CustomFloorPlugin {


    /// <summary>
    /// Loads AssetBundles containing CustomFloorPlugin
    /// </summary>
    internal static class PlatformLoader {


        /// <summary>
        /// <see cref="List{string}<"/> holding all Hashes of CustomScripts
        /// </summary>
        internal static List<string> scriptHashList;


        internal static bool newScriptsFound = false;


        internal static readonly string CustomFloorPluginFolderPath = Path.Combine(Environment.CurrentDirectory, FOLDER);
        internal static readonly string CustomFloorPluginScriptFolderPath = Path.Combine(CustomFloorPluginFolderPath, SCRIPT_FOLDER);
        internal static readonly string ScriptHashesPath = Path.Combine(Environment.CurrentDirectory, "UserData", SCRIPT_HASHES_FILENAME);

        private static Sprite FeetIcon {
            get {
                if (_feetIcon == null) {
                    _feetIcon = Resources.FindObjectsOfTypeAll<Sprite>().Where(x => x.name == "FeetIcon").FirstOrDefault();
                }
                return _feetIcon;
            }
        }
        private static Sprite _feetIcon;


        /// <summary>
        /// Loads AssetBundles and populates the platforms array with CustomPlatform objects
        /// </summary>
        internal static List<CustomPlatform> CreateAllPlatforms(Transform parent) {


            // Create the CustomFloorPlugin folder if it doesn't already exist
            if (!Directory.Exists(CustomFloorPluginFolderPath)) {
                Directory.CreateDirectory(CustomFloorPluginFolderPath);
            }

            // Find AssetBundles in our CustomFloorPlugin directory
            string[] allBundlePaths = Directory.GetFiles(CustomFloorPluginFolderPath, "*.plat");

            List<CustomPlatform> platforms = new List<CustomPlatform>();

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
            for (int i = 0; i < allBundlePaths.Length; i++) {
                j++;
                CustomPlatform newPlatform = LoadPlatformBundle(allBundlePaths[i], parent);
                if (newPlatform != null) {
                    platforms.Add(newPlatform);
                    Log(newPlatform.platName + " by " + newPlatform.platAuthor);
                }
            }
            Log("[END OF PLATFORM LOADING SPAM]---------------------------------------");
            // Replace materials for all renderers
            MaterialSwapper.ReplaceMaterials(SCENE);

            return platforms;
        }


        /// <summary>
        /// Loads a <see cref="CustomPlatform"/> from disk into memory and instantiates it.<br/>
        /// Part of this logic has been moved to a different function, for no apparent reason.
        /// </summary>
        /// <param name="bundlePath">The location of the <see cref="CustomPlatform"/>s <see cref="AssetBundle"/> file on disk</param>
        /// <param name="parent">The parent <see cref="Transform"/> for the new <see cref="CustomPlatform"/></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms", Justification = "MD5 Hash not used in security relevant context")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Too late now... that damage was done a year ago -.-")]
        internal static CustomPlatform LoadPlatformBundle(string bundlePath, Transform parent) {

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
        private static CustomPlatform LoadPlatform(AssetBundle bundle, Transform parent) {

            GameObject platformPrefab = bundle.LoadAsset<GameObject>("_CustomPlatform");

            if (platformPrefab == null) {
                return null;
            }

            GameObject newPlatform = UnityEngine.Object.Instantiate(platformPrefab.gameObject);

            try {
                foreach (AudioListener al in FindAll<AudioListener>(newPlatform)) {
                    UnityEngine.Object.DestroyImmediate(al);
                }
            }
            catch (ComponentNotFoundException) {

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

            if (customPlatform.icon == null) {
                customPlatform.icon = FeetIcon;
            }

            newPlatform.SetActive(false);

            return customPlatform;
        }


        /// <summary>
        ///  Tries to load all CustomScripts, but aborts when <see cref="UI.Settings.LoadCustomScripts"/> is false or a new Script is found
        /// </summary>
        /// <returns>
        /// <see cref="bool"/> newScriptsFound
        /// </returns>
        internal static void LoadScripts() {

            // Create the CustomFloorPlugin script folder if it doesn't already exist
            if (!Directory.Exists(CustomFloorPluginScriptFolderPath)) {
                Directory.CreateDirectory(CustomFloorPluginScriptFolderPath);
            }

            // Preventing Issue when a script exists but the option is first enabled in menu
            if (!File.Exists(ScriptHashesPath)) {
                File.Create(ScriptHashesPath).Close();
            }

            // Find Dlls in our CustomFloorPluginScript directory
            string[] allScriptPaths = Directory.GetFiles(CustomFloorPluginScriptFolderPath, "*.dll");

            // Checks if a new CustomScript is found, if not loads all Scripts
            if (UI.Settings.LoadCustomScripts) {
                scriptHashList = new List<string>();
                foreach (string path in allScriptPaths) {
                    string hash = ComputeHashFromPath(path);
                    scriptHashList.Add(hash);
                }

                string[] oldHashes = File.ReadAllLines(ScriptHashesPath);
                int i = 0;
                if (scriptHashList.Count == oldHashes.Length) {
                    foreach (string hash1 in scriptHashList) {
                        foreach (string hash2 in oldHashes) {
                            if (hash1 == hash2) {
                                i++;
                            }
                        }
                    }
                    if (scriptHashList.Count != i) {
                        newScriptsFound = true;
                    }
                    else {
                        newScriptsFound = false;
                    }
                }
                else {
                    newScriptsFound = true;
                }

                if (!newScriptsFound) {
                    Log("No new CustomScripts found, loading all in directory " + CustomFloorPluginScriptFolderPath);
                    foreach (string path in allScriptPaths) {
                        Assembly.LoadFrom(path);
                    }
                }
                else {
                    Log("New CustomScripts found, loading aborted!");
                }
            }
        }



        /// <summary>
        /// Computes a MD5 Hash and converts it to Hex
        /// </summary>
        /// <param name="path">Path to the File the Hash should be created for</param>
        /// <returns>The Hex Hash of the File</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms", Justification = "MD5 Hash not used in security relevant context")]
        private static string ComputeHashFromPath(string path) {
            using MD5 md5 = MD5.Create();
            using FileStream stream = File.OpenRead(path);
            byte[] byteHash = md5.ComputeHash(stream);
            string hash = BitConverter.ToString(byteHash).Replace("-", string.Empty).ToUpperInvariant();
            return hash;
        }
    }
}