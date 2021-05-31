using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CustomFloorPlugin.Configuration;
using CustomFloorPlugin.Helpers;

using SiraUtil.Tools;

using UnityEngine;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Handles Platforms, and hearts, everything about them
    /// </summary>
    public sealed class PlatformManager : System.IDisposable
    {
        private readonly SiraLog _siraLog;
        private readonly PluginConfig _config;
        private readonly AssetLoader _assetLoader;
        private readonly PlatformLoader _platformLoader;
        private readonly Transform _anchor;
        private readonly string _cacheFilePath;

        /// <summary>
        /// Keeps track of which <see cref="AssetBundle"/> has loaded a <see cref="CustomPlatform"/>
        /// </summary>
        internal Dictionary<string, CustomPlatform> PlatformFilePaths { get; }

        /// <summary>
        /// The Task responsible for platform loading
        /// </summary>
        internal Task<List<CustomPlatform>> PlatformsLoadingTask { get; }

        internal CustomPlatform DefaultPlatform { get; }
        internal CustomPlatform ActivePlatform { get; set; }
        internal CustomPlatform CurrentSingleplayerPlatform { get; set; }
        internal CustomPlatform CurrentMultiplayerPlatform { get; set; }
        internal CustomPlatform CurrentA360Platform { get; set; }
        internal CustomPlatform? APIRequestedPlatform { get; set; }

        /// <summary>
        /// Stores the BeatmapLevelId the platform was requested for
        /// </summary>
        internal string? APIRequestedLevelId { get; set; }

        /// <summary>
        /// The cache file version, change this to prevent loading older ones if something changes
        /// </summary>
        private const byte kCacheFileVersion = 3;

        public PlatformManager(SiraLog siraLog,
                               PluginConfig config,
                               AssetLoader assetLoader,
                               PlatformLoader platformLoader)
        {
            _siraLog = siraLog;
            _config = config;
            _assetLoader = assetLoader;
            _platformLoader = platformLoader;
            _anchor = new GameObject("CustomPlatforms").transform;
            _cacheFilePath = Path.Combine(_config.CustomPlatformsDirectory, "cache.dat");
            PlatformFilePaths = new Dictionary<string, CustomPlatform>();
            DefaultPlatform = CreateDefaultPlatform();
            CurrentSingleplayerPlatform = DefaultPlatform;
            CurrentMultiplayerPlatform = DefaultPlatform;
            CurrentA360Platform = DefaultPlatform;
            ActivePlatform = DefaultPlatform;
            PlatformsLoadingTask = LoadPlatformsAsync();
        }

        /// <summary>
        /// Automatically save the descriptors on exit
        /// </summary>
        public void Dispose()
        {
            SavePlatformInfosToFileAsync();
        }

        /// <summary>
        /// Loads all platforms or their descriptors if a cache file exists
        /// </summary>
        private async Task<List<CustomPlatform>> LoadPlatformsAsync()
        {
            Stopwatch sw = Stopwatch.StartNew();

            if (!Directory.Exists(_config!.CustomPlatformsDirectory))
                Directory.CreateDirectory(_config.CustomPlatformsDirectory);

            List<string> bundlePaths = Directory.EnumerateFiles(_config.CustomPlatformsDirectory, "*.plat").ToList();
            List<CustomPlatform> platforms = new(bundlePaths.Count + 1)
            {
                DefaultPlatform
            };

            if (File.Exists(_cacheFilePath))
                foreach (CustomPlatform platform in EnumeratePlatformDescriptorsFromFile())
                {
                    platforms.Add(platform);
                    bundlePaths.Remove(platform.fullPath);
                }

            // Load all remaining platforms, or all if no cache file is found
            foreach (string path in bundlePaths)
            {
                CustomPlatform? platform = await CreatePlatformAsync(path);
                if (platform != null)
                    platforms.Add(platform);
            }

            sw.Stop();
            _siraLog!.Info($"Loaded Platforms in {sw.ElapsedMilliseconds.ToString(NumberFormatInfo.InvariantInfo)}ms");

            return platforms;
        }

        /// <summary>
        /// Creates a fake <see cref="CustomPlatform"/> used to indicate that no platform should be used
        /// </summary>
        private CustomPlatform CreateDefaultPlatform()
        {
            CustomPlatform defaultPlatform = new GameObject("Default Platform").AddComponent<CustomPlatform>();
            defaultPlatform.transform.SetParent(_anchor);
            defaultPlatform.platName = "Default Environment";
            defaultPlatform.platAuthor = "Beat Saber";
            defaultPlatform.icon = _assetLoader!.DefaultPlatformCover;
            defaultPlatform.isDescriptor = false;
            return defaultPlatform;
        }

        /// <summary>
        /// Returns the index of the <see cref="CustomPlatform"/> for a <see cref="PlatformType"/> in <see cref="PlatformsLoadingTask"/>
        /// </summary>
        internal async Task<int> GetIndexForTypeAsync(PlatformType platformType)
        {
            List<CustomPlatform> allPlatforms = await PlatformsLoadingTask;
            return platformType switch
            {
                PlatformType.Singleplayer => allPlatforms.IndexOf(CurrentSingleplayerPlatform!),
                PlatformType.Multiplayer => allPlatforms.IndexOf(CurrentMultiplayerPlatform!),
                PlatformType.A360 => allPlatforms.IndexOf(CurrentA360Platform!),
                PlatformType.API => allPlatforms.IndexOf(APIRequestedPlatform!),
                PlatformType.Active => allPlatforms.IndexOf(ActivePlatform!),
                _ => 0
            };
        }

        /// <summary>
        /// Sets the platforms that were last selected as the current ones
        /// </summary>
        private void CheckLastSelectedPlatform(CustomPlatform platform)
        {
            if (_config!.SingleplayerPlatformPath == platform.fullPath)
                CurrentSingleplayerPlatform = platform;
            if (_config!.MultiplayerPlatformPath == platform.fullPath)
                CurrentMultiplayerPlatform = platform;
            if (_config!.A360PlatformPath == platform.fullPath)
                CurrentA360Platform = platform;
        }

        /// <summary>
        /// Asynchronously creates a <see cref="CustomPlatform"/> by loading an <see cref="AssetBundle"/> from the given <paramref name="path"/>
        /// </summary>
        internal async Task<CustomPlatform?> CreatePlatformAsync(string path)
        {
            CustomPlatform? platform = await _platformLoader!.LoadFromFileAsync(path);
            if (platform == null) return null;
            CustomPlatform newPlatform = Object.Instantiate(platform, _anchor);
            Object.Destroy(platform.gameObject);
            newPlatform.name = platform.name;
            newPlatform.isDescriptor = false;
            CheckLastSelectedPlatform(newPlatform);

            if (PlatformFilePaths.ContainsKey(newPlatform.fullPath))
            {
                List<CustomPlatform> allPlatforms = await PlatformsLoadingTask;
                CustomPlatform descriptor = PlatformFilePaths[newPlatform.fullPath];
                int index = allPlatforms.IndexOf(descriptor);
                PlatformFilePaths[newPlatform.fullPath] = newPlatform;
                allPlatforms[index] = newPlatform;
                Object.Destroy(descriptor.gameObject);
            }
            else
            {
                PlatformFilePaths.Add(path, newPlatform);
            }

            return newPlatform;
        }

        /// <summary>
        /// Reads all saved platform descriptors from the cache file
        /// </summary>
        private IEnumerable<CustomPlatform> EnumeratePlatformDescriptorsFromFile()
        {
            using FileStream stream = new(_cacheFilePath!, FileMode.Open, FileAccess.Read);
            using BinaryReader reader = new(stream, Encoding.UTF8);

            if (reader.ReadByte() != kCacheFileVersion) yield break;

            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                CustomPlatform platform = new GameObject().AddComponent<CustomPlatform>();
                platform.platName = reader.ReadString();
                platform.platAuthor = reader.ReadString();
                platform.platHash = reader.ReadString();
                platform.fullPath = reader.ReadString();
                if (reader.ReadBoolean())
                    platform.icon = reader.ReadTexture2D().ToSprite();
                if (!File.Exists(platform.fullPath))
                {
                    _siraLog!.Info($"File {platform.fullPath} no longer exists; skipped");
                    Object.Destroy(platform.gameObject);
                    continue;
                }

                platform.name = $"{platform.platName} by {platform.platAuthor}";
                platform.transform.SetParent(_anchor);
                CheckLastSelectedPlatform(platform);
                PlatformFilePaths.Add(platform.fullPath, platform);
                yield return platform;
            }
        }

        /// <summary>
        /// Saves descriptors of all loaded <see cref="CustomPlatform"/>s into a cache file
        /// </summary>
        private void SavePlatformInfosToFileAsync()
        {
            if (!PlatformsLoadingTask.IsCompleted) return;

            using FileStream stream = new(_cacheFilePath!, FileMode.OpenOrCreate, FileAccess.Write);
            using BinaryWriter writer = new(stream, Encoding.UTF8);

            List<CustomPlatform> allPlatforms = PlatformsLoadingTask.Result;
            writer.Write(kCacheFileVersion);
            writer.Write(allPlatforms.Count - 1);
            foreach (CustomPlatform platform in allPlatforms.Skip(1))
            {
                writer.Write(platform.platName);
                writer.Write(platform.platAuthor);
                writer.Write(platform.platHash);
                writer.Write(platform.fullPath);
                writer.WriteSprite(platform.icon);
            }

            stream.SetLength(stream.Position);
            File.SetAttributes(_cacheFilePath!, FileAttributes.Hidden);
        }
    }
}