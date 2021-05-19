using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using CustomFloorPlugin.Helpers;

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
        /// The cover for the default platform
        /// </summary>
        internal Sprite DefaultPlatformCover { get; }

        /// <summary>
        /// The cover used for all platforms normally missing one
        /// </summary>
        internal Sprite FallbackCover { get; }

        /// <summary>
        /// The old heart, to remind everyone that this plugin is some legacy garbage
        /// </summary>
        private readonly Task<GameObject> _heartLoadingTask;

        /// <summary>
        /// Used as a players place replacement in platform preview
        /// </summary>
        private readonly Task<GameObject> _playersPlaceLoadingTask;

        public AssetLoader(DiContainer container, MaterialSwapper materialSwapper)
        {
            _container = container;
            _materialSwapper = materialSwapper;
            _heartLoadingTask = CreateHeart();
            _playersPlaceLoadingTask = CreatePlayersPlace();
            using Stream defaultCoverStream = GetEmbeddedResource("CustomFloorPlugin.Assets.LvlInsaneCover.png");
            DefaultPlatformCover = defaultCoverStream.ReadTexture2D().ToSprite();
            using Stream fallbackCoverStream = GetEmbeddedResource("CustomFloorPlugin.Assets.FeetIcon.png");
            FallbackCover = fallbackCoverStream.ReadTexture2D().ToSprite();
        }

        /// <summary>
        /// (De-)Activates the heart
        /// </summary>
        /// <param name="value">The desired state</param>
        internal async void ToggleHeart(bool value)
        {
            GameObject heart = await _heartLoadingTask!;
            if (value)
            {
                heart.SetActive(true);
                heart.GetComponent<InstancedMaterialLightWithId>()?.ColorWasSet(Color.magenta);
            }
            else
            {
                heart.SetActive(false);
            }
        }

        /// <summary>
        /// (De-)Activates the players place replacement
        /// </summary>
        /// <param name="value">The desired state</param>
        internal async void TogglePlayersPlace(bool value)
        {
            GameObject playersPlace = await _playersPlaceLoadingTask!;
            if (value)
            {
                playersPlace.SetActive(true);
                playersPlace.GetComponentInChildren<InstancedMaterialLightWithId>()?.ColorWasSet(Color.blue);
            }
            else
            {
                playersPlace.SetActive(false);
            }
        }

        private async Task<GameObject> CreateHeart()
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

        private async Task<GameObject> CreatePlayersPlace()
        {
            (Vector3[] vertices, int[] triangles) = await Task.Run(() => ParseMesh("CustomFloorPlugin.Assets.Playersplace.mesh"));
            Mesh mesh = new()
            {
                vertices = vertices,
                triangles = triangles
            };

            GameObject playersPlaceCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            playersPlaceCube.SetActive(false);
            MeshRenderer cubeRenderer = playersPlaceCube.GetComponent<MeshRenderer>();
            (Material darkEnvSimpleMaterial, _, _) = await _materialSwapper.MaterialsLoadingTask;
            cubeRenderer.material = darkEnvSimpleMaterial;
            playersPlaceCube.transform.localPosition = new Vector3(0f, -12.5f, 0f);
            playersPlaceCube.transform.localScale = new Vector3(3f, 25f, 2f);
            playersPlaceCube.name = "PlayersPlaceReplacement";

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

        private static Stream GetEmbeddedResource(string name) =>
            Assembly.GetManifestResourceStream(name) ??
            throw new InvalidOperationException($"No embedded resource found: {name}");

        private static Assembly Assembly => _assembly ??= Assembly.GetExecutingAssembly();
        private static Assembly? _assembly;

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
            {
                vertices[i++] = new Vector3(float.Parse(strVector3[0], NumberFormatInfo.InvariantInfo),
                    float.Parse(strVector3[1], NumberFormatInfo.InvariantInfo),
                    float.Parse(strVector3[2], NumberFormatInfo.InvariantInfo));
            }

            i = 0;
            int[] triangles = new int[dimension2[1].Length];
            foreach (string strInt in dimension2[1])
                triangles[i++] = int.Parse(strInt, NumberFormatInfo.InvariantInfo);

            return (vertices, triangles);
        }
    }
}