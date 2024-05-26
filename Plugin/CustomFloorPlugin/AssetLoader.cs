using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

using CustomFloorPlugin.Helpers;
using CustomFloorPlugin.Interfaces;

using IPA.Utilities;

using JetBrains.Annotations;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Loads all images and creates the heart and a replacement for the default players place
    /// </summary>
    [UsedImplicitly]
    public class AssetLoader(DiContainer container, Assembly assembly, [Inject(Id = "CustomPlatforms")] Transform anchor, MaterialSwapper materialSwapper)
    {
        private readonly Transform _anchor = anchor;
        private readonly DiContainer _container = container;
        private readonly Assembly _assembly = assembly;
        private readonly MaterialSwapper _materialSwapper = materialSwapper;

        /// <summary>
        /// Used as a players place replacement in platform preview
        /// </summary>
        internal GameObject PlayersPlace => _playersPlace ??= CreatePlayersPlace();
        private GameObject? _playersPlace;

        /// <summary>
        /// The cover for the default platform
        /// </summary>
        internal Sprite DefaultPlatformCover => _defaultPlatformCover ??= CreateSpriteFromEmbeddedResource("CustomFloorPlugin.Assets.LvlInsaneCover.png");
        private Sprite? _defaultPlatformCover;

        internal Sprite RandomPlatformCover => _randomPlatformCover ??= CreateSpriteFromEmbeddedResource("CustomFloorPlugin.Assets.QuestionMarkIcon.png");
        private Sprite? _randomPlatformCover;

        /// <summary>
        /// The cover used for all platforms normally missing one
        /// </summary>
        internal Sprite FallbackCover => _fallbackCover ??= CreateSpriteFromEmbeddedResource("CustomFloorPlugin.Assets.FeetIcon.png");
        private Sprite? _fallbackCover;

        /// <summary>
        /// Multiplayer light effects
        /// </summary>
        internal LightEffects MultiplayerLightEffects => _lightEffects ??= CreateLightEffects();
        private LightEffects? _lightEffects;

        private Sprite CreateSpriteFromEmbeddedResource(string resourcePath)
        {
            using Stream stream = GetEmbeddedResource(resourcePath);
            return stream.ReadTexture2D().ToSprite();
        }

        private LightEffects CreateLightEffects()
        {
            GameObject lightEffects = new("LightEffects");
            lightEffects.transform.SetParent(_anchor);
            return lightEffects.AddComponent<LightEffects>();
        }

        private GameObject CreatePlayersPlace()
        {
            GameObject playersPlaceCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            playersPlaceCube.SetActive(false);
            Transform playersPlaceCubeTransform = playersPlaceCube.transform;
            playersPlaceCubeTransform.SetParent(_anchor);
            MeshRenderer cubeRenderer = playersPlaceCube.GetComponent<MeshRenderer>();
            cubeRenderer.material = _materialSwapper.DarkEnvSimpleMaterial;
            playersPlaceCubeTransform.localPosition = new Vector3(0f, -50.0075f, 0f);
            playersPlaceCubeTransform.localScale = new Vector3(3f, 100f, 2f);
            playersPlaceCube.name = "PlayersPlace";

            GameObject playersPlaceMirror = GameObject.CreatePrimitive(PrimitiveType.Plane);
            Transform playersPlaceMirrorTransform = playersPlaceMirror.transform;
            playersPlaceMirror.name = "Mirror";
            playersPlaceMirrorTransform.SetParent(playersPlaceCubeTransform);
            playersPlaceMirrorTransform.localScale = new Vector3(0.1f, 0f, 0.1f);
            playersPlaceMirrorTransform.localPosition = new Vector3(0f, 0.5001f, 0f);
            TrackMirror trackMirror = playersPlaceMirror.AddComponent<TrackMirror>();
            trackMirror.bumpIntensity = 0.02f;
            using Stream floorStream = GetEmbeddedResource("CustomFloorPlugin.Assets.Floor.png");
            trackMirror.normalTexture = floorStream.ReadTexture2D();
            trackMirror.PlatformEnabled(_container);

            GameObject playersPlaceFrame = new("Frame");
            playersPlaceFrame.transform.SetParent(playersPlaceCubeTransform);
            playersPlaceFrame.AddComponent<MeshRenderer>();
            MeshFilter meshFilter = playersPlaceFrame.AddComponent<MeshFilter>();
            meshFilter.mesh = ParseMesh("CustomFloorPlugin.Assets.PlayersPlace.mesh");
            TubeLight tubeLight = playersPlaceFrame.AddComponent<TubeLight>();
            tubeLight.color = Color.blue;
            tubeLight.PlatformEnabled(_container);

            return playersPlaceCube;
        }

        private Stream GetEmbeddedResource(string name) => _assembly.GetManifestResourceStream(name)!;

        private Mesh ParseMesh(string resourcePath)
        {
            using Stream manifestResourceStream = GetEmbeddedResource(resourcePath);
            using StreamReader streamReader = new(manifestResourceStream);

            string meshFile = streamReader.ReadToEnd();
            string[] meshSplit = meshFile.Split('|');
            string[] verticesRaw = meshSplit[0].Split('/');
            string[] trianglesRaw = meshSplit[1].Split('/');

            int i = 0;
            string[][] strVector3S = new string[verticesRaw.Length][];
            foreach (string vector3 in verticesRaw)
                strVector3S[i++] = vector3.Split(',');

            i = 0;
            Vector3[] vertices = new Vector3[strVector3S.Length];
            foreach (string[] strVector3 in strVector3S)
                vertices[i++] = new Vector3(float.Parse(strVector3[0], NumberFormatInfo.InvariantInfo), float.Parse(strVector3[1], NumberFormatInfo.InvariantInfo), float.Parse(strVector3[2], NumberFormatInfo.InvariantInfo));

            i = 0;
            int[] triangles = new int[trianglesRaw.Length];
            foreach (string strInt in trianglesRaw)
                triangles[i++] = int.Parse(strInt, NumberFormatInfo.InvariantInfo);

            return new Mesh { vertices = vertices, triangles = triangles };
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

            private BeatmapCallbacksController _beatmapCallbacksController = null!;
            private LightWithIdManager _lightWithIdManager = null!;
            private ColorScheme _colorScheme = null!;

            private LightSwitchEventEffect[]? _lightSwitchEventEffects;

            [Inject]
            public void Construct(BeatmapCallbacksController beatmapCallbacksController, LightWithIdManager lightWithIdManager, ColorScheme colorScheme)
            {
                _beatmapCallbacksController = beatmapCallbacksController;
                _lightWithIdManager = lightWithIdManager;
                _colorScheme = colorScheme;
            }

            public void PlatformEnabled(DiContainer container)
            {
                container.Inject(this);
                gameObject.SetActive(false);
                List<ILightWithId>?[] lights = _lightWithIdManager._lights;
                if (_lightSwitchEventEffects is null)
                {
                    Color normalColor = new(1f, 1f, 1f, 0.7490196f);
                    Color boostColor = new(1f, 1f, 1f, 0.8f);
                    Color highlightColor = new(1f, 1f, 1f, 1f);
                    _lightSwitchEventEffects = new LightSwitchEventEffect[5];
                    for (int i = 0; i < _lightSwitchEventEffects.Length; i++)
                    {
                        lights[i] ??= [];
                        _lightSwitchEventEffects[i] = container.InstantiateComponent<LightSwitchEventEffect>(gameObject);
                        _lightSwitchEventEffects[i].SetField("_lightsID", i + 1);
                        _lightSwitchEventEffects[i]._event = (BasicBeatmapEventType)i;
                        _lightSwitchEventEffects[i]._lightColor0 = CreateMultipliedColorSO(_simpleLightColor0, normalColor);
                        _lightSwitchEventEffects[i]._lightColor1 = CreateMultipliedColorSO(_simpleLightColor1, normalColor);
                        _lightSwitchEventEffects[i]._highlightColor0 = CreateMultipliedColorSO(_simpleHighlightColor0, highlightColor);
                        _lightSwitchEventEffects[i]._highlightColor1 = CreateMultipliedColorSO(_simpleHighlightColor1, highlightColor);
                        _lightSwitchEventEffects[i]._lightColor0Boost = CreateMultipliedColorSO(_simpleLightColor0Boost, boostColor);
                        _lightSwitchEventEffects[i]._lightColor1Boost = CreateMultipliedColorSO(_simpleLightColor1Boost, boostColor);
                        _lightSwitchEventEffects[i]._highlightColor0Boost = CreateMultipliedColorSO(_simpleHighlightColor0Boost, highlightColor);
                        _lightSwitchEventEffects[i]._highlightColor1Boost = CreateMultipliedColorSO(_simpleHighlightColor1Boost, highlightColor);
                    }
                }
                else
                {
                    for (int i = 0; i < _lightSwitchEventEffects.Length; i++)
                    {
                        lights[_lightSwitchEventEffects[i].lightsId] ??= [];
                        _lightSwitchEventEffects[i].SetField("_beatmapCallbacksController", _beatmapCallbacksController);
                        _lightSwitchEventEffects[i].SetField("_lightManager", _lightWithIdManager);
                        _lightSwitchEventEffects[i].SetColor(Color.clear);
                        _lightSwitchEventEffects[i].Start();
                    }
                }

                _simpleLightColor0.SetColor(_colorScheme.environmentColor0);
                _simpleLightColor1.SetColor(_colorScheme.environmentColor1);
                _simpleHighlightColor0.SetColor(_colorScheme.environmentColor0);
                _simpleHighlightColor1.SetColor(_colorScheme.environmentColor1);
                _simpleLightColor0Boost.SetColor(_colorScheme.environmentColor0Boost);
                _simpleLightColor1Boost.SetColor(_colorScheme.environmentColor1Boost);
                _simpleHighlightColor0Boost.SetColor(_colorScheme.environmentColor0Boost);
                _simpleHighlightColor1Boost.SetColor(_colorScheme.environmentColor1Boost);
                gameObject.SetActive(true);
            }

            public void PlatformDisabled()
            {
                if (_lightSwitchEventEffects is null) return;
                foreach (LightSwitchEventEffect lse in _lightSwitchEventEffects)
                    lse.OnDestroy();
                gameObject.SetActive(false);
            }

            private static MultipliedColorSO CreateMultipliedColorSO(SimpleColorSO simpleColor, Color color)
            {
                MultipliedColorSO multipliedColor = ScriptableObject.CreateInstance<MultipliedColorSO>();
                multipliedColor._baseColor = simpleColor;
                multipliedColor._multiplierColor = color;
                return multipliedColor;
            }
        }
    }
}
