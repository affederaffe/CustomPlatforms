using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using CustomFloorPlugin.Configuration;
using CustomFloorPlugin.Extensions;

using IPA.Utilities;

using SiraUtil.Tools;

using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.SceneManagement;

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
        private PlatformLoader _platformLoader;

        [Inject]
        public void Construct(SiraLog siraLog, PluginConfig config, PlatformLoader platformLoader)
        {
            _siraLog = siraLog;
            _config = config;
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
        /// The cover for the default platform
        /// </summary>
        internal Sprite defaultPlatformCover;

        /// <summary>
        /// The cover used for all platforms normally missing one
        /// </summary>
        internal Sprite fallbackCover;

        /// <summary>
        /// Sprite used to indicate that a mod requirement is fulfilled
        /// </summary>
        internal Sprite greenCheck;

        /// <summary>
        /// Sprite used to indicate that a mod suggestion is fulfilled
        /// </summary>
        internal Sprite yellowCheck;

        /// <summary>
        /// Sprite used to indicate that a mod requirement is not fulfilled
        /// </summary>
        internal Sprite redX;

        /// <summary>
        /// Sprite used to indicate that a mod suggestion is not fulfilled
        /// </summary>
        internal Sprite yellowX;

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
        /// Acts as a prefab for custom light sources that require meshes...<br/>
        /// Not 100% bug free tbh<br/>
        /// <br/>
        /// Also:<br/>
        /// We love Beat Saber
        /// </summary>
        internal GameObject heart;

        /// <summary>
        /// Used as a platform in platform preview if <see cref="CustomPlatform.hideDefaultPlatform"/> is false
        /// </summary>
        internal GameObject playersPlace;

        /// <summary>
        /// The Light Source used for non-mesh lights
        /// </summary>
        internal GameObject lightSource;

        /// <summary>
        /// Used as a prefab for light effects in multiplayer
        /// </summary>
        internal GameObject lightEffects;

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
            LoadSprites();
            LoadAssets();
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
            defaultPlatform.icon = defaultPlatformCover;
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
                newPlatform.icon = fallbackCover;
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

        private void LoadSprites()
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            defaultPlatformCover = executingAssembly.GetManifestResourceStream("CustomFloorPlugin.Assets.LvlInsaneCover.png").ReadSprite();
            fallbackCover = executingAssembly.GetManifestResourceStream("CustomFloorPlugin.Assets.FeetIcon.png").ReadSprite();
            greenCheck = executingAssembly.GetManifestResourceStream("CustomFloorPlugin.Assets.GreenCheck.png").ReadSprite();
            yellowCheck = executingAssembly.GetManifestResourceStream("CustomFloorPlugin.Assets.YellowCheck.png").ReadSprite();
            redX = executingAssembly.GetManifestResourceStream("CustomFloorPlugin.Assets.RedX.png").ReadSprite();
            yellowX = executingAssembly.GetManifestResourceStream("CustomFloorPlugin.Assets.YellowX.png").ReadSprite();
        }

        /// <summary>
        /// Steals the heart from the GreenDayScene<br/>
        /// Then De-Serializes the data from the embedded resource heart.mesh onto the GreenDayHeart to make it more visually pleasing<br/>
        /// Also adjusts it position and color.<br/>
        /// Gets the Non-Mesh lightSource and the playersPlace used in the Platform Preview too.<br/>
        /// Now also steals the lightEffects for multiplayer, this scene is really useful
        /// </summary>
        private void LoadAssets()
        {
            StartCoroutine(fuckUnity());
            IEnumerator<WaitUntil> fuckUnity()
            {//did you know loaded scenes are loaded asynchronously, regarless if you use async or not?
                yield return null;
                Scene greenDay = SceneManager.LoadScene("GreenDayGrenadeEnvironment", new LoadSceneParameters(LoadSceneMode.Additive));
                yield return new WaitUntil(() => { return greenDay.isLoaded; });
                GameObject root = greenDay.GetRootGameObjects()[0];

                heart = root.transform.Find("GreenDayCity/ArmHeartLighting").gameObject;
                heart.SetActive(false);
                heart.transform.SetParent(transform);
                heart.name = "<3";

                playersPlace = root.transform.Find("PlayersPlace").gameObject;
                playersPlace.SetActive(false);
                playersPlace.transform.SetParent(transform);

                lightSource = root.transform.Find("GlowLineL (2)").gameObject;
                lightSource.SetActive(false);
                lightSource.transform.SetParent(transform);
                lightSource.name = "LightSource";

                lightEffects = root.transform.Find("LightEffects").gameObject;
                lightEffects.SetActive(false);
                lightEffects.transform.SetParent(transform);

                SceneManager.UnloadSceneAsync("GreenDayGrenadeEnvironment");

                using Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CustomFloorPlugin.Assets.heart.mesh");
                using StreamReader streamReader = new(manifestResourceStream);

                string meshfile = streamReader.ReadToEnd();
                string[] dimension1 = meshfile.Split('|');
                string[][] dimension2 = new string[][] { dimension1[0].Split('/'), dimension1[1].Split('/') };
                string[][] string_vector3s = new string[dimension2[0].Length][];

                int i = 0;
                foreach (string string_vector3 in dimension2[0])
                {
                    string_vector3s[i++] = string_vector3.Split(',');
                }

                List<Vector3> vertices = new();
                List<int> triangles = new();
                foreach (string[] string_vector3 in string_vector3s)
                {
                    vertices.Add(new Vector3(float.Parse(string_vector3[0], NumberFormatInfo.InvariantInfo), float.Parse(string_vector3[1], NumberFormatInfo.InvariantInfo), float.Parse(string_vector3[2], NumberFormatInfo.InvariantInfo)));
                }
                foreach (string s_int in dimension2[1])
                {
                    triangles.Add(int.Parse(s_int, NumberFormatInfo.InvariantInfo));
                }

                Mesh mesh = new()
                {
                    vertices = vertices.ToArray(),
                    triangles = triangles.ToArray()
                };

                DestroyImmediate(heart.GetComponent<ProBuilderMesh>());

                heart.GetComponent<MeshFilter>().mesh = mesh;
                heart.transform.position = new Vector3(-8f, 25f, 26f);
                heart.transform.rotation = Quaternion.Euler(-100f, 90f, 90f);
                heart.transform.localScale = new Vector3(25f, 25f, 25f);

                heart.SetActive(_config.ShowHeart);
                heart.GetComponent<InstancedMaterialLightWithId>().ColorWasSet(Color.magenta);
            }
        }
    }
}
