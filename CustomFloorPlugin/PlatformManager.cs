using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using CustomFloorPlugin.Configuration;
using CustomFloorPlugin.Extensions;

using IPA.Utilities;

using SiraUtil.Tools;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Handles Platforms, and hearts, everything about them
    /// </summary>
    public class PlatformManager : MonoBehaviour
    {
        private SiraLog _siraLog;
        private PluginConfig _config;
        private AssetLoader _assetLoader;
        private PlatformLoader _platformLoader;

        [Inject]
        public void Construct(SiraLog siraLog,
                              PluginConfig config,
                              AssetLoader assetLoader,
                              PlatformLoader platformLoader)
        {
            _siraLog = siraLog;
            _config = config;
            _assetLoader = assetLoader;
            _platformLoader = platformLoader;
        }

        /// <summary>
        /// List of all loaded Platforms
        /// </summary>
        internal List<CustomPlatform> allPlatforms = new();

        /// <summary>
        /// Keeps track of the currently selected <see cref="PlatformType"/>
        /// </summary>
        internal PlatformType currentPlatformType = PlatformType.Singleplayer;

        /// <summary>
        /// Keeps track of the currently selected <see cref="CustomPlatform"/>
        /// </summary>
        internal CustomPlatform currentSingleplayerPlatform;

        /// <summary>
        /// Keeps track of the currently selected <see cref="CustomPlatform"/>
        /// </summary>
        internal CustomPlatform currentMultiplayerPlatform;

        /// <summary>
        /// Keeps track of the currently selected <see cref="CustomPlatform"/>
        /// </summary>
        internal CustomPlatform currentA360Platform;

        /// <summary>
        /// Keeps track of the currently active <see cref="CustomPlatform"/>
        /// </summary>
        internal CustomPlatform activePlatform;

        /// <summary>
        /// Stores the index of an API requested <see cref="CustomPlatform"/>
        /// </summary>
        internal CustomPlatform apiRequestedPlatform;

        /// <summary>
        /// Stores the BeatmapLevel the platform was requested for
        /// </summary>
        internal string apiRequestedLevelId;

        /// <summary>
        /// The path used to cache platform descriptors for faster loading
        /// </summary>
        internal readonly string customPlatformsInfoCacheFilePath = Path.Combine(UnityGame.UserDataPath, "Custom PlatformsInfoCache.dat");

        /// <summary>
        /// Keeps track of all spawned custom objects, whichs lifetime ends on any scene transition
        /// </summary>
        internal readonly List<UnityEngine.Object> spawnedObjects = new();

        /// <summary>
        /// The cache file version to prevent loading older ones if something changes
        /// </summary>
        private const byte kCacheFileVersion = 1;

        /// <summary>
        /// Initializes the <see cref="PlatformManager"/>
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void Start()
        {
            LoadPlatforms();
        }

        /// <summary>
        /// Automaticly save the descriptors on exit
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnDestroy()
        {
            SavePlatformInfosToFile();
        }

        /// <summary>
        /// Loads all platforms or their descritpors if a cache file exists
        /// </summary>
        private async void LoadPlatforms()
        {
            if (!Directory.Exists(_config.CustomPlatformsDirectory))
                Directory.CreateDirectory(_config.CustomPlatformsDirectory);

            IEnumerable<string> bundlePaths = Directory.EnumerateFiles(_config.CustomPlatformsDirectory, "*.plat");

            CustomPlatform defaultPlatform = new GameObject("Default Platform").AddComponent<CustomPlatform>();
            defaultPlatform.transform.parent = transform;
            defaultPlatform.platName = "Default Environment";
            defaultPlatform.platAuthor = "Beat Saber";
            defaultPlatform.icon = _assetLoader.defaultPlatformCover;
            defaultPlatform.isDescriptor = false;
            allPlatforms.Add(defaultPlatform);

            if (File.Exists(customPlatformsInfoCacheFilePath))
            {
                LoadPlatformInfosFromFile();
                bundlePaths = bundlePaths.Except(allPlatforms.Select(x => x.fullPath));
            }

            // Load all remaining platforms, or all if no cache file is found
            foreach (string path in bundlePaths)
            {
                await _platformLoader.LoadFromFileAsync(path, HandlePlatformLoaded);
            }
        }

        /// <summary>
        /// Returns the index for a <see cref="PlatformType"/> in <see cref="allPlatforms"/>
        /// </summary>
        internal int GetIndexForType(PlatformType platformType)
        {
            int index = platformType switch
            {
                PlatformType.Singleplayer => allPlatforms.IndexOf(currentSingleplayerPlatform),
                PlatformType.Multiplayer => allPlatforms.IndexOf(currentMultiplayerPlatform),
                PlatformType.A360 => allPlatforms.IndexOf(currentA360Platform),
                PlatformType.API => allPlatforms.IndexOf(apiRequestedPlatform),
                _ => 0
            };
            if (index == -1) index = 0;
            return index;
        }

        /// <summary>
        /// Sets the platforms that were last selected as the current ones
        /// </summary>
        private void CheckLastSelectedPlatform(CustomPlatform platform)
        {
            if (_config.SingleplayerPlatformPath == platform.platName + platform.platAuthor)
                currentSingleplayerPlatform = platform;
            if (_config.MultiplayerPlatformPath == platform.platName + platform.platAuthor)
                currentMultiplayerPlatform = platform;
            if (_config.A360PlatformPath == platform.platName + platform.platAuthor)
                currentA360Platform = platform;
        }

        /// <summary>
        /// The callback executed when a platform is successfully loaded
        /// </summary>
        internal void HandlePlatformLoaded(CustomPlatform platform)
        {
            CustomPlatform newPlatform = Instantiate(platform, transform);
            newPlatform.name = platform.name;
            newPlatform.isDescriptor = false;
            CheckLastSelectedPlatform(newPlatform);
            if (_platformLoader.platformFilePaths.ContainsKey(newPlatform.fullPath))
            {
                CustomPlatform descriptor = _platformLoader.platformFilePaths[newPlatform.fullPath];
                int index = allPlatforms.IndexOf(descriptor);
                if (activePlatform == descriptor)
                    activePlatform = newPlatform;
                _platformLoader.platformFilePaths[descriptor.fullPath] = newPlatform;
                allPlatforms[index] = newPlatform;
                Destroy(descriptor.gameObject);
            }
            else
            {
                _platformLoader.platformFilePaths.Add(newPlatform.fullPath, newPlatform);
                allPlatforms.Add(newPlatform);
            }
        }

        /// <summary>
        /// Reads all saved platform descriptors out of the cache file
        /// </summary>
        private void LoadPlatformInfosFromFile()
        {
            try
            {
                using FileStream stream = new(customPlatformsInfoCacheFilePath, FileMode.Open, FileAccess.Read);
                using BinaryReader reader = new(stream, Encoding.UTF8);

                if (reader.ReadByte() != kCacheFileVersion)
                    return;

                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    CustomPlatform platform = new GameObject().AddComponent<CustomPlatform>();
                    platform.platName = reader.ReadString();
                    platform.platAuthor = reader.ReadString();
                    platform.platHash = reader.ReadString();
                    platform.fullPath = reader.ReadString();
                    Texture2D tex = reader.ReadTexture2D();
                    platform.icon = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), Vector2.zero);
                    platform.name = platform.platName + " by " + platform.platAuthor;
                    platform.transform.parent = transform;
                    if (!File.Exists(platform.fullPath))
                    {
                        _siraLog.Info($"File {platform.fullPath} no longer exists; skipped");
                        Destroy(platform.gameObject);
                        continue;
                    }

                    CheckLastSelectedPlatform(platform);

                    _platformLoader.platformFilePaths.Add(platform.fullPath, platform);
                    allPlatforms.Add(platform);
                }
            }
            catch (Exception e)
            {
                _siraLog.Error("Failed to load cached platform info:\n" + e);
            }
        }

        /// <summary>
        /// Saves descritpors of all loaded <see cref="CustomPlatform"/>s into a cache file
        /// </summary>
        private void SavePlatformInfosToFile()
        {
            try
            {
                using FileStream stream = new(customPlatformsInfoCacheFilePath, FileMode.OpenOrCreate, FileAccess.Write);
                using BinaryWriter writer = new(stream, Encoding.UTF8, true);

                writer.Write(kCacheFileVersion);
                writer.Write(allPlatforms.Count - 1);
                foreach (CustomPlatform platform in allPlatforms.Skip(1))
                {
                    writer.Write(platform.platName);
                    writer.Write(platform.platAuthor);
                    writer.Write(platform.platHash);
                    writer.Write(platform.fullPath);
                    writer.WriteTexture2D(platform.icon.texture, true);
                }

                stream.SetLength(stream.Position);
                File.SetAttributes(customPlatformsInfoCacheFilePath, FileAttributes.Hidden);
            }
            catch (Exception e)
            {
                _siraLog.Error("Failed to save info cache");
                _siraLog.Error(e);
            }
        }
    }
}