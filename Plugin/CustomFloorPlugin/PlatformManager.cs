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

using SiraUtil.Tools;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Handles platform loading and saving
    /// </summary>
    public sealed class PlatformManager : IInitializable, System.IDisposable
    {
        private readonly SiraLog _siraLog;
        private readonly PluginConfig _config;
        private readonly AssetLoader _assetLoader;
        private readonly PlatformLoader _platformLoader;

        private readonly Transform _anchor;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly string _cacheFilePath;

        /// <summary>
        /// An <see cref="ObservableCollection{T}"/> of all currently loaded <see cref="CustomPlatform"/>s<br/>
        /// When a platform is loaded, the <see cref="ObservableCollection{T}"/> will be notified
        /// </summary>
        internal ObservableCollection<CustomPlatform> AllPlatforms { get; }

        internal CustomPlatform DefaultPlatform { get; }
        internal CustomPlatform ActivePlatform { get; set; }
        internal CustomPlatform SingleplayerPlatform { get; set; }
        internal CustomPlatform MultiplayerPlatform { get; set; }
        internal CustomPlatform A360Platform { get; set; }
        internal CustomPlatform? APIRequestedPlatform { get; set; }

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
            _cancellationTokenSource = new CancellationTokenSource();
            _cacheFilePath = Path.Combine(_config.CustomPlatformsDirectory, "cache.dat");
            DefaultPlatform = CreateDefaultPlatform();
            AllPlatforms = new ObservableCollection<CustomPlatform> { DefaultPlatform };
            SingleplayerPlatform = DefaultPlatform;
            MultiplayerPlatform = DefaultPlatform;
            A360Platform = DefaultPlatform;
            ActivePlatform = DefaultPlatform;
        }

        public async void Initialize()
        {
            try
            {
                await LoadPlatformsAsync(_cancellationTokenSource.Token);
            }
            catch (System.OperationCanceledException) { }
        }

        /// <summary>
        /// Automatically save the descriptors on exit
        /// </summary>
        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
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
            defaultPlatform.icon = _assetLoader!.DefaultPlatformCover;
            defaultPlatform.isDescriptor = false;
            return defaultPlatform;
        }

        /// <summary>
        /// Restores the last platform selection
        /// </summary>
        private void LastSelectedPlatform(CustomPlatform platform)
        {
            if (_config.SingleplayerPlatformPath == platform.fullPath)
                SingleplayerPlatform = platform;
            if (_config.MultiplayerPlatformPath == platform.fullPath)
                MultiplayerPlatform = platform;
            if (_config.A360PlatformPath == platform.fullPath)
                A360Platform = platform;
        }

        /// <summary>
        /// Loads all platforms or their descriptors if a cache file exists
        /// </summary>
        private async Task LoadPlatformsAsync(CancellationToken cancellationToken)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Directory.CreateDirectory(_config.CustomPlatformsDirectory);

            if (File.Exists(_cacheFilePath))
            {
                foreach (CustomPlatform platform in EnumeratePlatformDescriptorsFromFile())
                    AllPlatforms.AddSorted(1, AllPlatforms.Count - 1, platform, null);
            }

            // Load all remaining platforms, or all if no cache file is found
            foreach (string path in Directory.EnumerateFiles(_config.CustomPlatformsDirectory, "*.plat").Except(AllPlatforms.Select(x => x.fullPath)))
            {
                cancellationToken.ThrowIfCancellationRequested();
                CustomPlatform? platform = await CreatePlatformAsync(path);
                if (platform is null) continue;
                AllPlatforms.AddSorted(1, AllPlatforms.Count - 1, platform, null);
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
            for (int i = 0; i < AllPlatforms.Count; i++)
            {
                CustomPlatform oldPlatform = AllPlatforms[i];
                if (oldPlatform.fullPath != fullPath) continue;
                Object.Destroy(oldPlatform.gameObject);
                AllPlatforms[i] = newPlatform;
            }

            return newPlatform;
        }

        /// <summary>
        /// Reads all saved platform descriptors from the cache file
        /// </summary>
        private IEnumerable<CustomPlatform> EnumeratePlatformDescriptorsFromFile()
        {
            using FileStream stream = new(_cacheFilePath, FileMode.Open, FileAccess.Read);
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