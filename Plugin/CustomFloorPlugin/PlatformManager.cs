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


namespace CustomFloorPlugin
{
    /// <summary>
    /// Handles platform loading and saving
    /// </summary>
    public sealed class PlatformManager : System.IDisposable
    {
        private readonly SiraLog _siraLog;
        private readonly PluginConfig _config;
        private readonly AssetLoader _assetLoader;
        private readonly PlatformLoader _platformLoader;

        private readonly Transform _anchor;
        private readonly CancellationTokenSource _cancellationSource;
        private readonly string _cacheFilePath;

        /// <summary>
        /// Keeps track of which <see cref="AssetBundle"/> has loaded a <see cref="CustomPlatform"/>
        /// </summary>
        internal Dictionary<string, CustomPlatform> PlatformFilePaths { get; }

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
            _cancellationSource = new CancellationTokenSource();
            _cacheFilePath = Path.Combine(_config.CustomPlatformsDirectory, "cache.dat");
            PlatformFilePaths = new Dictionary<string, CustomPlatform>();
            DefaultPlatform = CreateDefaultPlatform();
            AllPlatforms = new ObservableCollection<CustomPlatform> { DefaultPlatform };
            SingleplayerPlatform = DefaultPlatform;
            MultiplayerPlatform = DefaultPlatform;
            A360Platform = DefaultPlatform;
            ActivePlatform = DefaultPlatform;
            LoadPlatformsAsync(_cancellationSource.Token);
        }

        /// <summary>
        /// Automatically save the descriptors on exit
        /// </summary>
        public void Dispose()
        {
            _cancellationSource.Cancel();
            SavePlatformInfosToFileAsync();
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
        /// Returns the index of the <see cref="CustomPlatform"/> for a <see cref="PlatformType"/> in <see cref="AllPlatforms"/>
        /// </summary>
        internal int GetIndexForType(PlatformType platformType)
        {
            return platformType switch
            {
                PlatformType.Singleplayer => AllPlatforms.IndexOf(SingleplayerPlatform),
                PlatformType.Multiplayer => AllPlatforms.IndexOf(MultiplayerPlatform),
                PlatformType.A360 => AllPlatforms.IndexOf(A360Platform),
                PlatformType.API => AllPlatforms.IndexOf(APIRequestedPlatform!),
                PlatformType.Active => AllPlatforms.IndexOf(ActivePlatform),
                _ => 0
            };
        }

        /// <summary>
        /// Restores the last selection of platforms
        /// </summary>
        private void LastSelectedPlatform(CustomPlatform platform)
        {
            if (_config!.SingleplayerPlatformPath == platform.fullPath)
                SingleplayerPlatform = platform;
            if (_config!.MultiplayerPlatformPath == platform.fullPath)
                MultiplayerPlatform = platform;
            if (_config!.A360PlatformPath == platform.fullPath)
                A360Platform = platform;
        }

        /// <summary>
        /// Loads all platforms or their descriptors if a cache file exists
        /// </summary>
        private async void LoadPlatformsAsync(CancellationToken cancellationToken)
        {
            Stopwatch sw = Stopwatch.StartNew();

            if (!Directory.Exists(_config!.CustomPlatformsDirectory))
                Directory.CreateDirectory(_config.CustomPlatformsDirectory);

            if (File.Exists(_cacheFilePath))
            {
                foreach (CustomPlatform platform in EnumeratePlatformDescriptorsFromFile())
                    AllPlatforms.AddSorted(1, AllPlatforms.Count - 1, platform);
            }

            // Load all remaining platforms, or all if no cache file is found
            foreach (string path in Directory.EnumerateFiles(_config.CustomPlatformsDirectory, "*.plat").Except(PlatformFilePaths.Keys))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    sw.Stop();
                    _siraLog!.Info($"Loaded {AllPlatforms.Count.ToString(NumberFormatInfo.InvariantInfo)} platforms in {sw.ElapsedMilliseconds.ToString(NumberFormatInfo.InvariantInfo)}ms");
                    return;
                }

                CustomPlatform? platform = await CreatePlatformAsync(path);
                if (platform == null) continue;
                AllPlatforms.AddSorted(1, AllPlatforms.Count - 1, platform);
            }

            sw.Stop();
            _siraLog!.Info($"Loaded {AllPlatforms.Count.ToString(NumberFormatInfo.InvariantInfo)} platforms in {sw.ElapsedMilliseconds.ToString(NumberFormatInfo.InvariantInfo)}ms");
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
            LastSelectedPlatform(newPlatform);

            if (PlatformFilePaths.ContainsKey(newPlatform.fullPath))
            {
                CustomPlatform descriptor = PlatformFilePaths[newPlatform.fullPath];
                int index = AllPlatforms.IndexOf(descriptor);
                PlatformFilePaths[newPlatform.fullPath] = newPlatform;
                AllPlatforms[index] = newPlatform;
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
                if (reader.ReadBoolean()) platform.icon = reader.ReadTexture2D().ToSprite();
                if (!File.Exists(platform.fullPath))
                {
                    _siraLog!.Info($"File {platform.fullPath} no longer exists; skipped");
                    Object.Destroy(platform.gameObject);
                    continue;
                }

                platform.name = $"{platform.platName} by {platform.platAuthor}";
                platform.transform.SetParent(_anchor);
                PlatformFilePaths.Add(platform.fullPath, platform);
                LastSelectedPlatform(platform);

                yield return platform;
            }
        }

        /// <summary>
        /// Saves descriptors of all loaded <see cref="CustomPlatform"/>s into a cache file
        /// </summary>
        private void SavePlatformInfosToFileAsync()
        {
            using FileStream stream = new(_cacheFilePath!, FileMode.OpenOrCreate, FileAccess.Write);
            using BinaryWriter writer = new(stream, Encoding.UTF8);

            writer.Write(kCacheFileVersion);
            writer.Write(AllPlatforms.Count - 1);
            for (int i = 1; i < AllPlatforms.Count; i++)
            {
                writer.Write(AllPlatforms[i].platName);
                writer.Write(AllPlatforms[i].platAuthor);
                writer.Write(AllPlatforms[i].platHash);
                writer.Write(AllPlatforms[i].fullPath);
                writer.WriteSprite(AllPlatforms[i].icon);
            }

            stream.SetLength(stream.Position);
            File.SetAttributes(_cacheFilePath!, FileAttributes.Hidden);
        }
    }
}