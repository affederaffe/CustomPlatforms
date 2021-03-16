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
        public void Construct(SiraLog siraLog, PluginConfig config, AssetLoader assetLoader, PlatformLoader platformLoader)
        {
            _siraLog = siraLog;
            _config = config;
            _assetLoader = assetLoader;
            _platformLoader = platformLoader;
        }

        /// <summary>
        /// List of all loaded Platforms
        /// </summary>
        internal List<CustomPlatform> allPlatforms;

        /// <summary>
        /// Stores the index of an API requested <see cref="CustomPlatform"/>
        /// </summary>
        internal int apiRequestIndex = -1;

        /// <summary>
        /// Stores the BeatmapLevel the platform was requested for
        /// </summary>
        internal string apiRequestedLevelId;

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
        /// List of all loaded plugins
        /// </summary>
        internal readonly IReadOnlyList<string> allPluginNames = IPA.Loader.PluginManager.EnabledPlugins.Select(x => x.Name).ToList();

        /// <summary>
        /// The folder all CustomPlatform files are located
        /// </summary>
        internal readonly string customPlatformsFolderPath = Path.Combine(UnityGame.InstallPath, "CustomPlatforms");

        /// <summary>
        /// The path used to cache platform descriptors for faster loading
        /// </summary>
        internal readonly string customPlatformsInfoCacheFilePath = Path.Combine(UnityGame.UserDataPath, "Custom PlatformsInfoCache.dat");

        /// <summary>
        /// Keeps track of all spawned custom <see cref="GameObject"/>s, whichs lifetime ends on any scene transition
        /// </summary>
        internal readonly List<GameObject> spawnedObjects = new();

        /// <summary>
        /// Keeps track of all spawned custom <see cref="Component"/>s, whichs lifetime ends on any scene transition
        /// </summary>
        internal readonly List<Component> spawnedComponents = new();

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
            _assetLoader.LoadSprites();
            _assetLoader.LoadAssets(transform);
            LoadPlatforms();
        }

        /// <summary>
        /// Automaticly save platform descriptors on exit
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnDestroy()
        {
            SavePlatformInfosToFile();
        }

        /// <summary>
        /// Loads all platforms or their descritpors if a cache file exists
        /// </summary>
        private void LoadPlatforms()
        {
            if (!Directory.Exists(customPlatformsFolderPath))
                Directory.CreateDirectory(customPlatformsFolderPath);

            allPlatforms = new List<CustomPlatform>();

            string[] bundlePaths = Directory.GetFiles(customPlatformsFolderPath, "*.plat");

            CustomPlatform defaultPlatform = new GameObject("Default Platform").AddComponent<CustomPlatform>();
            defaultPlatform.transform.parent = transform;
            defaultPlatform.platName = "Default Environment";
            defaultPlatform.platAuthor = "Beat Saber";
            defaultPlatform.icon = _assetLoader.defaultPlatformCover;
            allPlatforms.Add(defaultPlatform);

            if (File.Exists(customPlatformsInfoCacheFilePath))
            {
                LoadPlatformInfosFromFile();
                foreach (string path in bundlePaths.Select(x => Path.GetFullPath(x)).Except(allPlatforms.Select(x => x.fullPath)))
                {
                    StartCoroutine(_platformLoader.LoadFromFileAsync(path, HandlePlatformLoaded));
                }
            }
            else
            {
                foreach (string path in bundlePaths)
                {
                    string fullPath = Path.GetFullPath(path);
                    StartCoroutine(_platformLoader.LoadFromFileAsync(fullPath, HandlePlatformLoaded));
                }
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
                _ => 0
            };
            return index != -1 ? index : 0;
        }

        /// <summary>
        /// Sets the platforms that were last selected as the current ones
        /// </summary>
        internal void CheckLastSelectedPlatform(in CustomPlatform platform)
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
        internal void HandlePlatformLoaded(CustomPlatform platform, string fullPath)
        {
            CustomPlatform newPlatform = Instantiate(platform);
            newPlatform.name = platform.name;
            newPlatform.fullPath = platform.fullPath;
            newPlatform.platHash = platform.platHash;
            newPlatform.transform.parent = transform;
            if (newPlatform.icon == null)
                newPlatform.icon = _assetLoader.fallbackCover;
            CheckLastSelectedPlatform(in newPlatform);

            if (_platformLoader.platformFilePaths.ContainsKey(fullPath))
            {
                int index = allPlatforms.IndexOf(_platformLoader.platformFilePaths[fullPath]);
                if (activePlatform == _platformLoader.platformFilePaths[fullPath])
                    activePlatform = newPlatform;
                Destroy(_platformLoader.platformFilePaths[fullPath].gameObject);
                _platformLoader.platformFilePaths[fullPath] = newPlatform;
                allPlatforms[index] = newPlatform;
            }
            else
            {
                _platformLoader.platformFilePaths.Add(fullPath, newPlatform);
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
                    int reqCount = reader.ReadInt32();
                    for (int j = 0; j < reqCount; j++)
                        platform.requirements.Add(reader.ReadString());
                    int sugCount = reader.ReadInt32();
                    for (int j = 0; j < sugCount; j++)
                        platform.suggestions.Add(reader.ReadString());
                    platform.icon = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), Vector2.zero);
                    platform.name = platform.platName + " by " + platform.platAuthor;
                    platform.transform.parent = transform;
                    if (!File.Exists(platform.fullPath))
                    {
                        _siraLog.Info($"File {platform.fullPath} no longer exists; skipped");
                        continue;
                    }

                    CheckLastSelectedPlatform(in platform);

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
                using (FileStream stream = new(customPlatformsInfoCacheFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (BinaryWriter writer = new(stream, Encoding.UTF8, true))
                    {
                        writer.Write(kCacheFileVersion);
                        writer.Write(allPlatforms.Count - 1);
                        foreach (CustomPlatform platform in allPlatforms.Skip(1))
                        {
                            writer.Write(platform.platName);
                            writer.Write(platform.platAuthor);
                            writer.Write(platform.platHash);
                            writer.Write(platform.fullPath);
                            writer.Write(platform.icon.texture, true);
                            writer.Write(platform.requirements.Count);
                            for (int i = 0; i < platform.requirements.Count; i++)
                                writer.Write(platform.requirements[i]);
                            writer.Write(platform.suggestions.Count);
                            for (int i = 0; i < platform.suggestions.Count; i++)
                                writer.Write(platform.suggestions[i]);
                        }
                    }
                    stream.SetLength(stream.Position);
                }
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
