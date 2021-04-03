using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CustomFloorPlugin.Configuration;
using CustomFloorPlugin.Extensions;

using IPA.Utilities;
using IPA.Utilities.Async;

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
        /// The Task responsible for platform loading
        /// </summary>
        internal Task<List<CustomPlatform>> allPlatformsTask;

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
        internal readonly List<Object> spawnedObjects = new();

        /// <summary>
        /// Keeps track of which <see cref="AssetBundle"/> has loaded a <see cref="CustomPlatform"/>
        /// </summary>
        internal readonly Dictionary<string, CustomPlatform> platformFilePaths = new();

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
            allPlatformsTask = LoadPlatforms();
        }

        /// <summary>
        /// Automaticly save the descriptors on exit
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private async void OnDestroy()
        {
            await SavePlatformInfosToFile();
        }

        /// <summary>
        /// Loads all platforms or their descritpors if a cache file exists
        /// </summary>
        private async Task<List<CustomPlatform>> LoadPlatforms()
        {
            Stopwatch sw = new();
            sw.Start();

            if (!Directory.Exists(_config.CustomPlatformsDirectory))
                Directory.CreateDirectory(_config.CustomPlatformsDirectory);

            IEnumerable<string> bundlePaths = Directory.EnumerateFiles(_config.CustomPlatformsDirectory, "*.plat");
            List<CustomPlatform> platforms = new();

            CustomPlatform defaultPlatform = new GameObject("Default Platform").AddComponent<CustomPlatform>();
            defaultPlatform.transform.parent = transform;
            defaultPlatform.platName = "Default Environment";
            defaultPlatform.platAuthor = "Beat Saber";
            defaultPlatform.icon = _assetLoader.defaultPlatformCover;
            defaultPlatform.isDescriptor = false;
            platforms.Add(defaultPlatform);

            if (File.Exists(customPlatformsInfoCacheFilePath))
            {
                try
                {
                    foreach (CustomPlatform platform in EnumeratePlatformDescriptorsFromFile())
                    {
                        platforms.Add(platform);
                    }
                }
                catch (System.Exception e)
                {
                    _siraLog.Error("Failed to read cache file");
                    _siraLog.Error(e);
                }

                bundlePaths = bundlePaths.Except(platforms.Select(x => x.fullPath));
            }

            // Load all remaining platforms, or all if no cache file is found
            foreach (string path in bundlePaths)
            {
                CustomPlatform platform = await CreatePlatformAsync(path);
                platforms.Add(platform);
            }

            sw.Stop();
            _siraLog.Info($"Loaded Platforms in {sw.Elapsed.Seconds}.{sw.Elapsed.Milliseconds}");

            return platforms;
        }

        /// <summary>
        /// Returns the index for a <see cref="PlatformType"/> in <see cref="allPlatforms"/>
        /// </summary>
        internal async Task<int> GetIndexForTypeAsync(PlatformType platformType)
        {
            await allPlatformsTask;
            int index = platformType switch
            {
                PlatformType.Singleplayer => allPlatformsTask.Result.IndexOf(currentSingleplayerPlatform),
                PlatformType.Multiplayer => allPlatformsTask.Result.IndexOf(currentMultiplayerPlatform),
                PlatformType.A360 => allPlatformsTask.Result.IndexOf(currentA360Platform),
                PlatformType.API => allPlatformsTask.Result.IndexOf(apiRequestedPlatform),
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
        /// Asynchroniously creates a <see cref="CustomPlatform"/> by loading an <see cref="AssetBundle"/> at the given <paramref name="path"/>
        /// </summary>
        internal async Task<CustomPlatform> CreatePlatformAsync(string path)
        {
            return await await UnityMainThreadTaskScheduler.Factory.StartNew(async () =>
            {
                CustomPlatform platform = await _platformLoader.LoadFromFileAsync(path);
                CustomPlatform newPlatform = Instantiate(platform, transform);
                Destroy(platform.gameObject);
                newPlatform.name = platform.name;
                newPlatform.isDescriptor = false;
                CheckLastSelectedPlatform(newPlatform);

                if (platformFilePaths.ContainsKey(newPlatform.fullPath))
                {
                    await allPlatformsTask;
                    CustomPlatform descriptor = platformFilePaths[newPlatform.fullPath];
                    int index = allPlatformsTask.Result.IndexOf(descriptor);
                    if (activePlatform == descriptor)
                        activePlatform = newPlatform;
                    platformFilePaths[descriptor.fullPath] = newPlatform;
                    allPlatformsTask.Result[index] = newPlatform;
                    Destroy(descriptor.gameObject);
                }
                else
                {
                    platformFilePaths.Add(path, newPlatform);
                }

                return newPlatform;
            });
        }

        /// <summary>
        /// Reads all saved platform descriptors from the cache file
        /// </summary>
        private IEnumerable<CustomPlatform> EnumeratePlatformDescriptorsFromFile()
        {
            using FileStream stream = new(customPlatformsInfoCacheFilePath, FileMode.Open, FileAccess.Read);
            using BinaryReader reader = new(stream, Encoding.UTF8);

            if (reader.ReadByte() != kCacheFileVersion)
                yield break;

            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                CustomPlatform platform = new GameObject().AddComponent<CustomPlatform>();
                platform.platName = reader.ReadString();
                platform.platAuthor = reader.ReadString();
                platform.platHash = reader.ReadString();
                platform.fullPath = reader.ReadString();
                Texture2D tex = reader.ReadTexture2D();
                if (!File.Exists(platform.fullPath))
                {
                    _siraLog.Info($"File {platform.fullPath} no longer exists; skipped");
                    Destroy(platform.gameObject);
                    continue;
                }
                platform.icon = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), Vector2.zero);
                platform.name = platform.platName + " by " + platform.platAuthor;
                platform.transform.SetParent(transform);
                CheckLastSelectedPlatform(platform);
                platformFilePaths.Add(platform.fullPath, platform);
                yield return platform;
            }
        }

        /// <summary>
        /// Saves descritpors of all loaded <see cref="CustomPlatform"/>s into a cache file
        /// </summary>
        private async Task SavePlatformInfosToFile()
        {
            await allPlatformsTask;

            using FileStream stream = new(customPlatformsInfoCacheFilePath, FileMode.OpenOrCreate, FileAccess.Write);
            using BinaryWriter writer = new(stream, Encoding.UTF8, true);

            writer.Write(kCacheFileVersion);
            writer.Write(allPlatformsTask.Result.Count - 1);
            foreach (CustomPlatform platform in allPlatformsTask.Result.Skip(1))
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
    }
}