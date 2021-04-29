using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CustomFloorPlugin.Configuration;
using CustomFloorPlugin.Helpers;

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
        private SiraLog? _siraLog;
        private PluginConfig? _config;
        private AssetLoader? _assetLoader;
        private PlatformLoader? _platformLoader;
        private string? _cacheFilePath;

        /// <summary>
        /// The Task responsible for platform loading
        /// </summary>
        internal Task<List<CustomPlatform>>? LoadPlatformsTask;

        /// <summary>
        /// Keeps track of the currently selected <see cref="PlatformType"/>
        /// </summary>
        internal PlatformType CurrentPlatformType;

        /// <summary>
        /// Keeps track of the currently selected <see cref="CustomPlatform"/>
        /// </summary>
        internal CustomPlatform? CurrentSingleplayerPlatform;

        /// <summary>
        /// Keeps track of the currently selected <see cref="CustomPlatform"/>
        /// </summary>
        internal CustomPlatform? CurrentMultiplayerPlatform;

        /// <summary>
        /// Keeps track of the currently selected <see cref="CustomPlatform"/>
        /// </summary>
        internal CustomPlatform? CurrentA360Platform;

        /// <summary>
        /// Keeps track of the currently active <see cref="CustomPlatform"/>
        /// </summary>
        internal CustomPlatform? ActivePlatform;

        /// <summary>
        /// Stores the index of an API requested <see cref="CustomPlatform"/>
        /// </summary>
        internal CustomPlatform? APIRequestedPlatform;

        /// <summary>
        /// Stores the BeatmapLevelId the platform was requested for
        /// </summary>
        internal string? APIRequestedLevelId;

        /// <summary>
        /// Keeps track of which <see cref="AssetBundle"/> has loaded a <see cref="CustomPlatform"/>
        /// </summary>
        internal readonly Dictionary<string, CustomPlatform> PlatformFilePaths = new();

        /// <summary>
        /// The cache file version to prevent loading older ones if something changes
        /// </summary>
        private const byte kCacheFileVersion = 1;

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
            _cacheFilePath = Path.Combine(_config.CustomPlatformsDirectory, "cache.dat");
        }

        /// <summary>
        /// Initializes the <see cref="PlatformManager"/>
        /// </summary>
        private void Start()
        {
            LoadPlatformsTask = LoadPlatformsAsync();
        }

        /// <summary>
        /// Automatically save the descriptors on exit
        /// </summary>
        private async void OnDestroy()
        {
            await SavePlatformInfosToFileAsync();
        }

        /// <summary>
        /// Loads all platforms or their descriptors if a cache file exists
        /// </summary>
        private async Task<List<CustomPlatform>> LoadPlatformsAsync()
        {
            Stopwatch sw = new();
            sw.Start();

            if (!Directory.Exists(_config!.CustomPlatformsDirectory))
                Directory.CreateDirectory(_config.CustomPlatformsDirectory);

            IEnumerable<string> bundlePaths = Directory.EnumerateFiles(_config.CustomPlatformsDirectory, "*.plat");
            List<CustomPlatform> platforms = new();

            CustomPlatform defaultPlatform = CreateDefaultPlatform();
            platforms.Add(defaultPlatform);

            if (File.Exists(_cacheFilePath))
            {
                platforms.AddRange(EnumeratePlatformDescriptorsFromFile());
                bundlePaths = bundlePaths.Except(platforms.Select(x => x.fullPath));
            }

            // Load all remaining platforms, or all if no cache file is found
            foreach (string path in bundlePaths)
            {
                CustomPlatform? platform = await CreatePlatformAsync(path);
                if (platform != null) 
                    platforms.Add(platform);
            }

            sw.Stop();
            _siraLog!.Info($"Loaded Platforms in {sw.ElapsedMilliseconds.ToString()}ms");

            return platforms;
        }

        private CustomPlatform CreateDefaultPlatform()
        {
            CustomPlatform defaultPlatform = new GameObject("Default Platform").AddComponent<CustomPlatform>();
            defaultPlatform.transform.parent = transform;
            defaultPlatform.platName = "Default Environment";
            defaultPlatform.platAuthor = "Beat Saber";
            defaultPlatform.icon = _assetLoader!.DefaultPlatformCover!;
            defaultPlatform.isDescriptor = false;
            CurrentSingleplayerPlatform = defaultPlatform;
            CurrentMultiplayerPlatform = defaultPlatform;
            CurrentA360Platform = defaultPlatform;
            ActivePlatform = defaultPlatform;
            return defaultPlatform;
        }

        /// <summary>
        /// Returns the index of the <see cref="CustomPlatform"/> for a <see cref="PlatformType"/> in <see cref="LoadPlatformsTask"/>
        /// </summary>
        internal int GetIndexForType(PlatformType platformType)
        {
            return platformType switch
            {
                PlatformType.Singleplayer => LoadPlatformsTask!.Result.IndexOf(CurrentSingleplayerPlatform!),
                PlatformType.Multiplayer => LoadPlatformsTask!.Result.IndexOf(CurrentMultiplayerPlatform!),
                PlatformType.A360 => LoadPlatformsTask!.Result.IndexOf(CurrentA360Platform!),
                PlatformType.API => LoadPlatformsTask!.Result.IndexOf(APIRequestedPlatform!),
                PlatformType.Active => LoadPlatformsTask!.Result.IndexOf(ActivePlatform!),
                _ => 0
            };
        }

        /// <summary>
        /// Sets the platforms that were last selected as the current ones
        /// </summary>
        private void CheckLastSelectedPlatform(CustomPlatform platform)
        {
            if (_config!.SingleplayerPlatformPath == platform.platName + platform.platAuthor)
                CurrentSingleplayerPlatform = platform;
            if (_config!.MultiplayerPlatformPath == platform.platName + platform.platAuthor)
                CurrentMultiplayerPlatform = platform;
            if (_config!.A360PlatformPath == platform.platName + platform.platAuthor)
                CurrentA360Platform = platform;
        }

        /// <summary>
        /// Asynchronously creates a <see cref="CustomPlatform"/> by loading an <see cref="AssetBundle"/> from the given <paramref name="path"/>
        /// </summary>
        internal async Task<CustomPlatform?> CreatePlatformAsync(string path)
        {

            CustomPlatform? platform = await _platformLoader!.LoadFromFileAsync(path);
            if (platform == null) return null;
            CustomPlatform newPlatform = Instantiate(platform, transform);
            Destroy(platform.gameObject);
            newPlatform.name = platform.name;
            newPlatform.isDescriptor = false;
            CheckLastSelectedPlatform(newPlatform);

            if (PlatformFilePaths.ContainsKey(newPlatform.fullPath))
            {
                await LoadPlatformsTask!;
                CustomPlatform descriptor = PlatformFilePaths[newPlatform.fullPath];
                int index = LoadPlatformsTask.Result.IndexOf(descriptor);
                PlatformFilePaths[descriptor.fullPath] = newPlatform;
                LoadPlatformsTask.Result[index] = newPlatform;
                Destroy(descriptor.gameObject);
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
                Texture2D tex = reader.ReadTexture2D();
                if (!File.Exists(platform.fullPath))
                {
                    _siraLog!.Info($"File {platform.fullPath} no longer exists; skipped");
                    Destroy(platform.gameObject);
                    continue;
                }
                platform.icon = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), Vector2.zero);
                platform.name = $"{platform.platName} by {platform.platAuthor}";
                platform.transform.SetParent(transform);
                CheckLastSelectedPlatform(platform);
                PlatformFilePaths.Add(platform.fullPath, platform);
                yield return platform;
            }
        }

        /// <summary>
        /// Saves descriptors of all loaded <see cref="CustomPlatform"/>s into a cache file
        /// </summary>
        private async Task SavePlatformInfosToFileAsync()
        {
            await LoadPlatformsTask!;

            using FileStream stream = new(_cacheFilePath!, FileMode.OpenOrCreate, FileAccess.Write);
            using BinaryWriter writer = new(stream, Encoding.UTF8);

            writer.Write(kCacheFileVersion);
            writer.Write(LoadPlatformsTask.Result.Count - 1);
            foreach (CustomPlatform platform in LoadPlatformsTask.Result.Skip(1))
            {
                writer.Write(platform.platName);
                writer.Write(platform.platAuthor);
                writer.Write(platform.platHash);
                writer.Write(platform.fullPath);
                writer.WriteTexture2D(platform.icon!.texture);
            }
            stream.SetLength(stream.Position);
            File.SetAttributes(_cacheFilePath!, FileAttributes.Hidden);
        }
    }
}