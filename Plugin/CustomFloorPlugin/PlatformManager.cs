using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CustomFloorPlugin.Configuration;
using CustomFloorPlugin.Helpers;

using IPA.Utilities;

using SiraUtil.Logging;
using SiraUtil.Zenject;

using UnityEngine;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Handles platform loading and saving
    /// </summary>
    public sealed class PlatformManager : IAsyncInitializable, System.IDisposable
    {
        private readonly SiraLog _siraLog;
        private readonly PluginConfig _config;
        private readonly AssetLoader _assetLoader;
        private readonly PlatformLoader _platformLoader;

        private readonly Transform _anchor;
        private readonly string _directoryPath;
        private readonly string _cacheFilePath;

        /// <summary>
        /// Getter for the CustomPlatforms directory path
        /// </summary>
        internal string DirectoryPath => Directory.CreateDirectory(_directoryPath).FullName;

        /// <summary>
        /// An <see cref="ObservableCollection{T}"/> of all currently loaded <see cref="CustomPlatform"/>s<br/>
        /// When a platform is loaded, the <see cref="ObservableCollection{T}"/> will be notified
        /// </summary>
        internal ObservableCollection<CustomPlatform> AllPlatforms { get; }

        public CustomPlatform DefaultPlatform { get; }
        public CustomPlatform ActivePlatform { get; internal set; }
        public CustomPlatform SingleplayerPlatform { get; internal set; }
        public CustomPlatform MultiplayerPlatform { get; internal set; }
        public CustomPlatform A360Platform { get; internal set; }
        public CustomPlatform MenuPlatform { get; internal set; }
        public CustomPlatform? APIRequestedPlatform { get; internal set; }

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
            _directoryPath = Path.Combine(UnityGame.InstallPath, "CustomPlatforms");
            _cacheFilePath = Path.Combine(DirectoryPath, "cache.dat");
            DefaultPlatform = CreateDefaultPlatform();
            AllPlatforms = new ObservableCollection<CustomPlatform> { DefaultPlatform };
            SingleplayerPlatform = DefaultPlatform;
            MultiplayerPlatform = DefaultPlatform;
            A360Platform = DefaultPlatform;
            MenuPlatform = DefaultPlatform;
            ActivePlatform = DefaultPlatform;
        }

        public async Task InitializeAsync(CancellationToken token)
        {
            await LoadPlatformsAsync(token);
        }

        /// <summary>
        /// Automatically save the descriptors on exit
        /// </summary>
        public void Dispose()
        {
            SavePlatformInfosToFile();
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
            defaultPlatform.icon = _assetLoader.DefaultPlatformCover;
            defaultPlatform.isDescriptor = false;
            return defaultPlatform;
        }

        /// <summary>
        /// Restores the last platform selection
        /// </summary>
        private void LastSelectedPlatform(CustomPlatform platform)
        {
            if (platform.fullPath == _config.SingleplayerPlatformPath)
                SingleplayerPlatform = platform;
            if (platform.fullPath == _config.MultiplayerPlatformPath)
                MultiplayerPlatform = platform;
            if (platform.fullPath == _config.A360PlatformPath)
                A360Platform = platform;
            if (platform.fullPath == _config.MenuPlatformPath)
                MenuPlatform = platform;
        }

        /// <summary>
        /// Loads all platforms or their descriptors if a cache file exists
        /// </summary>
        private async Task LoadPlatformsAsync(CancellationToken cancellationToken)
        {
            Stopwatch sw = Stopwatch.StartNew();

            if (File.Exists(_cacheFilePath))
            {
                foreach (CustomPlatform platform in EnumeratePlatformDescriptorsFromFile())
                    AllPlatforms.AddSorted(1, AllPlatforms.Count - 1, platform);
            }

            // Load all remaining platforms, or all if no cache file is found
            foreach (string path in Directory.EnumerateFiles(DirectoryPath, "*.plat").Where(x => AllPlatforms.All(y => y.fullPath != x)))
            {
                if (cancellationToken.IsCancellationRequested) return;
                CustomPlatform? platform = await CreatePlatformAsync(path);
                if (platform is null) continue;
                AllPlatforms.AddSorted(1, AllPlatforms.Count - 1, platform);
            }

            sw.Stop();
            _siraLog.Info($"Loaded {AllPlatforms.Count.ToString(NumberFormatInfo.InvariantInfo)} platforms in {sw.ElapsedMilliseconds.ToString(NumberFormatInfo.InvariantInfo)}ms");
        }

        /// <summary>
        /// Asynchronously creates a <see cref="CustomPlatform"/> by loading an <see cref="AssetBundle"/> from the given <paramref name="fullPath"/>
        /// </summary>
        public async Task<CustomPlatform?> CreatePlatformAsync(string fullPath)
        {
            CustomPlatform? platform = await _platformLoader.LoadPlatformFromFileAsync(fullPath);
            if (platform is null) return null;
            CustomPlatform newPlatform = Object.Instantiate(platform, _anchor);
            Object.Destroy(platform.gameObject);
            newPlatform.name = platform.name;
            newPlatform.isDescriptor = false;
            LastSelectedPlatform(newPlatform);
            return newPlatform;
        }

        /// <summary>
        /// Reads all saved platform descriptors from the cache file
        /// </summary>
        private IEnumerable<CustomPlatform> EnumeratePlatformDescriptorsFromFile()
        {
            using FileStream stream = new(_cacheFilePath, FileMode.Open, FileAccess.Read);
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
                platform.icon = reader.ReadNullableSprite();
                if (!File.Exists(platform.fullPath))
                {
                    _siraLog.Debug($"File {platform.fullPath} no longer exists; skipped");
                    Object.Destroy(platform.gameObject);
                    continue;
                }

                platform.name = $"{platform.platName} by {platform.platAuthor}";
                platform.transform.SetParent(_anchor);
                LastSelectedPlatform(platform);
                yield return platform;
            }
        }

        /// <summary>
        /// Saves descriptors of all loaded <see cref="CustomPlatform"/>s into a cache file
        /// </summary>
        private void SavePlatformInfosToFile()
        {
            using FileStream stream = new(_cacheFilePath, FileMode.OpenOrCreate, FileAccess.Write);
            using BinaryWriter writer = new(stream, Encoding.UTF8);

            writer.Write(kCacheFileVersion);
            writer.Write(AllPlatforms.Count - 1);
            for (int i = 1; i < AllPlatforms.Count; i++)
            {
                writer.Write(AllPlatforms[i].platName);
                writer.Write(AllPlatforms[i].platAuthor);
                writer.Write(AllPlatforms[i].platHash);
                writer.Write(AllPlatforms[i].fullPath);
                writer.WriteNullableSprite(AllPlatforms[i].icon);
            }

            stream.SetLength(stream.Position);
            File.SetAttributes(_cacheFilePath, FileAttributes.Hidden);
        }
    }
}