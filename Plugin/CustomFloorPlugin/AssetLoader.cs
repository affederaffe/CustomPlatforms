using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using CustomFloorPlugin.Helpers;
using CustomFloorPlugin.Interfaces;
using IPA.Utilities;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Loads all images and creates the heart and a replacement for the default players place
    /// </summary>
    public class AssetLoader
    {
        private readonly DiContainer _container;
        private readonly MaterialSwapper _materialSwapper;

        /// <summary>
        /// The old heart, to remind everyone that this plugin is some legacy garbage
        /// </summary>
        private readonly Task<GameObject> _heartTask;

        /// <summary>
        /// Used as a players place replacement in platform preview
        /// </summary>
        private readonly Task<GameObject> _playersPlaceTask;

        /// <summary>
        /// The cover for the default platform
        /// </summary>
        internal Sprite DefaultPlatformCover { get; }

        /// <summary>
        /// The cover used for all platforms normally missing one
        /// </summary>
        internal Sprite FallbackCover { get; }

        /// <summary>
        /// Multiplayer light effects
        /// </summary>
        internal LightEffects MultiplayerLightEffects { get; }

        public AssetLoader(DiContainer container, MaterialSwapper materialSwapper)
        {
            _container = container;
            _materialSwapper = materialSwapper;
            _heartTask = CreateHeartAsync();
            _playersPlaceTask = CreatePlayersPlaceAsync();
            using Stream defaultCoverStream = GetEmbeddedResource("CustomFloorPlugin.Assets.LvlInsaneCover.png");
            DefaultPlatformCover = defaultCoverStream.ReadTexture2D().ToSprite();
            using Stream fallbackCoverStream = GetEmbeddedResource("CustomFloorPlugin.Assets.FeetIcon.png");
            FallbackCover = fallbackCoverStream.ReadTexture2D().ToSprite();
            MultiplayerLightEffects = new GameObject("LightEffects").AddComponent<LightEffects>();
        }

        internal async void ToggleHeart(bool active)
        {
            GameObject heart = await _heartTask;
            heart.SetActive(active);
        }

        internal async void TogglePlayersPlace(bool active)
        {
            GameObject playersPlace = await _playersPlaceTask;
            playersPlace.SetActive(active);
        }

        private async Task<GameObject> CreateHeartAsync()
        {
            (Vector3[] vertices, int[] triangles) = await Task.Run(() => ParseMesh("CustomFloorPlugin.Assets.Heart.mesh"));
            Mesh mesh = new()
            {
                vertices = vertices,
                triangles = triangles
            };

            GameObject heart = new("<3");
            heart.SetActive(false);
            heart.AddComponent<MeshRenderer>();
            MeshFilter meshFilter = heart.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            heart.transform.localPosition = new Vector3(-8f, 25f, 26f);
            heart.transform.localRotation = Quaternion.Euler(-100f, 90f, 90f);
            heart.transform.localScale = new Vector3(25f, 25f, 25f);
            TubeLight tubeLight = heart.AddComponent<TubeLight>();
            tubeLight.color = Color.magenta;
            tubeLight.PlatformEnabled(_container);
            return heart;
        }

        private async Task<GameObject> CreatePlayersPlaceAsync()
        {
            (Vector3[] vertices, int[] triangles) = await Task.Run(() => ParseMesh("CustomFloorPlugin.Assets.PlayersPlace.mesh"));
            Mesh mesh = new()
            {
                vertices = vertices,
                triangles = triangles
            };

            GameObject playersPlaceCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            playersPlaceCube.SetActive(false);
            MeshRenderer cubeRenderer = playersPlaceCube.GetComponent<MeshRenderer>();
            (Material darkEnvSimpleMaterial, _, _) = await _materialSwapper.MaterialsTask;
            cubeRenderer.material = darkEnvSimpleMaterial;
            playersPlaceCube.transform.localPosition = new Vector3(0f, -50.0075f, 0f);
            playersPlaceCube.transform.localScale = new Vector3(3f, 100f, 2f);
            playersPlaceCube.name = "PlayersPlace";

            GameObject playersPlaceMirror = GameObject.CreatePrimitive(PrimitiveType.Plane);
            playersPlaceMirror.name = "Mirror";
            playersPlaceMirror.transform.SetParent(playersPlaceCube.transform);
            playersPlaceMirror.transform.localScale = new Vector3(0.1f, 0f, 0.1f);
            playersPlaceMirror.transform.localPosition = new Vector3(0f, 0.5001f, 0f);
            TrackMirror trackMirror = playersPlaceMirror.AddComponent<TrackMirror>();
            trackMirror.bumpIntensity = 0.02f;
            using Stream floorStream = GetEmbeddedResource("CustomFloorPlugin.Assets.Floor.png");
            trackMirror.normalTexture = floorStream.ReadTexture2D();
            trackMirror.PlatformEnabled(_container);

            GameObject playersPlaceFrame = new("Frame");
            playersPlaceFrame.transform.SetParent(playersPlaceCube.transform);
            playersPlaceFrame.AddComponent<MeshRenderer>();
            MeshFilter meshFilter = playersPlaceFrame.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            TubeLight tubeLight = playersPlaceFrame.AddComponent<TubeLight>();
            tubeLight.color = Color.blue;
            tubeLight.PlatformEnabled(_container);

            return playersPlaceCube;
        }

        private static Assembly Assembly => _assembly ??= Assembly.GetExecutingAssembly();
        private static Assembly? _assembly;

        private static Stream GetEmbeddedResource(string name) =>
            Assembly.GetManifestResourceStream(name) ??
            throw new InvalidOperationException($"No embedded resource found: {name}");

        private static (Vector3[] vertices, int[] triangles) ParseMesh(string resourcePath)
        {
            using Stream manifestResourceStream = GetEmbeddedResource(resourcePath);
            using StreamReader streamReader = new(manifestResourceStream);

            string meshFile = streamReader.ReadToEnd();
            string[] dimension1 = meshFile.Split('|');
            string[][] dimension2 = { dimension1[0].Split('/'), dimension1[1].Split('/') };

            int i = 0;
            string[][] strVector3S = new string[dimension2[0].Length][];
            foreach (string vector3 in dimension2[0])
                strVector3S[i++] = vector3.Split(',');

            i = 0;
            Vector3[] vertices = new Vector3[strVector3S.Length];
            foreach (string[] strVector3 in strVector3S)
                vertices[i++] = new Vector3(float.Parse(strVector3[0], NumberFormatInfo.InvariantInfo), float.Parse(strVector3[1], NumberFormatInfo.InvariantInfo), float.Parse(strVector3[2], NumberFormatInfo.InvariantInfo));

            i = 0;
            int[] triangles = new int[dimension2[1].Length];
            foreach (string strInt in dimension2[1])
                triangles[i++] = int.Parse(strInt, NumberFormatInfo.InvariantInfo);

            return (vertices, triangles);
        }

        /// <summary>
        /// Replacement for the missing light effects in multiplayer games
        /// </summary>
        internal class LightEffects : MonoBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
        {
            private readonly SimpleColorSO _simpleLightColor0 = ScriptableObject.CreateInstance<SimpleColorSO>();
            private readonly SimpleColorSO _simpleLightColor1 = ScriptableObject.CreateInstance<SimpleColorSO>();
            private readonly SimpleColorSO _simpleHighlightColor0 = ScriptableObject.CreateInstance<SimpleColorSO>();
            private readonly SimpleColorSO _simpleHighlightColor1 = ScriptableObject.CreateInstance<SimpleColorSO>();
            private readonly SimpleColorSO _simpleLightColor0Boost = ScriptableObject.CreateInstance<SimpleColorSO>();
            private readonly SimpleColorSO _simpleLightColor1Boost = ScriptableObject.CreateInstance<SimpleColorSO>();
            private readonly SimpleColorSO _simpleHighlightColor0Boost = ScriptableObject.CreateInstance<SimpleColorSO>();
            private readonly SimpleColorSO _simpleHighlightColor1Boost = ScriptableObject.CreateInstance<SimpleColorSO>();

            private ColorScheme? _colorScheme;

            private LightSwitchEventEffect[]? _lightSwitchEventEffects;

            [Inject]
            public void Construct(ColorScheme colorScheme)
            {
                _colorScheme = colorScheme;
            }

            public void PlatformEnabled(DiContainer container)
            {
                container.InjectGameObject(gameObject);

                if (_lightSwitchEventEffects is null)
                {
                    Color normalColor = new(1f, 1f, 1f, 0.7490196f);
                    Color boostColor = new(1f, 1f, 1f, 0.8f);
                    Color highlightColor = new(1f, 1f, 1f, 1f);

                    _lightSwitchEventEffects = new LightSwitchEventEffect[5];
                    for (int i = 0; i < _lightSwitchEventEffects.Length; i++)
                    {
                        _lightSwitchEventEffects[i] = container.InstantiateComponent<LightSwitchEventEffect>(gameObject);
                        _lightSwitchEventEffects[i].SetField("_lightsID", i + 1);
                        _lightSwitchEventEffects[i].SetField("_event", (BeatmapEventType)i);
                        _lightSwitchEventEffects[i].SetField("_colorBoostEvent", BeatmapEventType.Event5);
                        _lightSwitchEventEffects[i].SetField("_lightColor0", (ColorSO)CreateMultipliedColorSO(_simpleLightColor0, normalColor));
                        _lightSwitchEventEffects[i].SetField("_lightColor1", (ColorSO)CreateMultipliedColorSO(_simpleLightColor1, normalColor));
                        _lightSwitchEventEffects[i].SetField("_highlightColor0", (ColorSO)CreateMultipliedColorSO(_simpleHighlightColor0, highlightColor));
                        _lightSwitchEventEffects[i].SetField("_highlightColor1", (ColorSO)CreateMultipliedColorSO(_simpleHighlightColor1, highlightColor));
                        _lightSwitchEventEffects[i].SetField("_lightColor0Boost", (ColorSO)CreateMultipliedColorSO(_simpleLightColor0Boost, boostColor));
                        _lightSwitchEventEffects[i].SetField("_lightColor1Boost", (ColorSO)CreateMultipliedColorSO(_simpleLightColor1Boost, boostColor));
                        _lightSwitchEventEffects[i].SetField("_highlightColor0Boost", (ColorSO)CreateMultipliedColorSO(_simpleHighlightColor0Boost, highlightColor));
                        _lightSwitchEventEffects[i].SetField("_highlightColor1Boost", (ColorSO)CreateMultipliedColorSO(_simpleHighlightColor1Boost, highlightColor));
                    }
                }
                else
                {
                    foreach (LightSwitchEventEffect lse in _lightSwitchEventEffects)
                    {
                        lse.SetField("_initialized", false);
                        lse.SetField("_lightIsOn", false);
                        lse.Start();
                    }
                }

                _simpleLightColor0.SetColor(_colorScheme!.environmentColor0);
                _simpleLightColor1.SetColor(_colorScheme.environmentColor1);
                _simpleHighlightColor0.SetColor(_colorScheme.environmentColor0);
                _simpleHighlightColor1.SetColor(_colorScheme.environmentColor1);
                _simpleLightColor0Boost.SetColor(_colorScheme.environmentColor0Boost);
                _simpleLightColor1Boost.SetColor(_colorScheme.environmentColor1Boost);
                _simpleHighlightColor0Boost.SetColor(_colorScheme.environmentColor0Boost);
                _simpleHighlightColor1Boost.SetColor(_colorScheme.environmentColor1Boost);

                foreach (LightSwitchEventEffect lse in _lightSwitchEventEffects)
                    lse.enabled = true;
            }

            public void PlatformDisabled()
            {
                if (_lightSwitchEventEffects is null) return;
                foreach (LightSwitchEventEffect lse in _lightSwitchEventEffects)
                {
                    lse.OnDestroy();
                    lse.enabled = false;
                }
            }

            private static MultipliedColorSO CreateMultipliedColorSO(SimpleColorSO simpleColor, Color color)
            {
                MultipliedColorSO multipliedColor = ScriptableObject.CreateInstance<MultipliedColorSO>();
                multipliedColor.SetField("_baseColor", simpleColor);
                multipliedColor.SetField("_multiplierColor", color);
                return multipliedColor;
            }
        }
    }
}