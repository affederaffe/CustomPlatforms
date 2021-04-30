using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using CustomFloorPlugin.Helpers;

using IPA.Utilities;

using UnityEngine;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Loads all images and creates the heart and a replacement for the default players place
    /// </summary>
    public class AssetLoader
    {
        private readonly MaterialSwapper _materialSwapper;

        /// <summary>
        /// The cover for the default platform
        /// </summary>
        internal readonly Sprite DefaultPlatformCover;

        /// <summary>
        /// The cover used for all platforms normally missing one
        /// </summary>
        internal readonly Sprite FallbackCover;

        /// <summary>
        /// The old heart, to remind everyone that this plugin is some legacy garbage
        /// </summary>
        private readonly Task<GameObject> _heartLoadingTask;

        /// <summary>
        /// Used as a players place replacement in platform preview
        /// </summary>
        private readonly Task<GameObject> _playersPlaceLoadingTask;

        public AssetLoader(MaterialSwapper materialSwapper)
        {
            _materialSwapper = materialSwapper;
            _heartLoadingTask = CreateHeart();
            _playersPlaceLoadingTask = CreatePlayersPlace();
            DefaultPlatformCover = GetEmbeddedResource("CustomFloorPlugin.Assets.LvlInsaneCover.png").ReadSprite();
            FallbackCover = GetEmbeddedResource("CustomFloorPlugin.Assets.FeetIcon.png").ReadSprite();
        }

        /// <summary>
        /// (De-)Activates the heart
        /// </summary>
        /// <param name="value">The desired state</param>
        internal async void ToggleHeart(bool value)
        {
            await _heartLoadingTask!;
            if (value)
            {
                _heartLoadingTask.Result.SetActive(true);
                _heartLoadingTask.Result.GetComponent<InstancedMaterialLightWithId>().ColorWasSet(Color.magenta);
            }
            else
            {
                _heartLoadingTask.Result.SetActive(false);
            }
        }
        
        /// <summary>
        /// (De-)Activates the players place replacement
        /// </summary>
        /// <param name="value">The desired state</param>
        internal async void TogglePlayersPlace(bool value)
        {
            await _playersPlaceLoadingTask!;
            if (value)
            {
                _playersPlaceLoadingTask.Result.SetActive(true);
                _playersPlaceLoadingTask.Result.GetComponentInChildren<InstancedMaterialLightWithId>().ColorWasSet(Color.cyan);
            }
            else
            {
                _playersPlaceLoadingTask.Result.SetActive(false);
            }
        }

        private async Task<GameObject> CreateHeart()
        {
            (Vector3[] vertices, int[] triangles) = await Task.Run(() => ParseMesh("CustomFloorPlugin.Assets.heart.mesh"));
            Mesh mesh = new()
            {
                vertices = vertices,
                triangles = triangles
            };

            GameObject heart = new("<3");
            heart.SetActive(false);
            MeshRenderer meshRenderer = heart.AddComponent<MeshRenderer>();
            await _materialSwapper.MaterialsLoadingTask;
            meshRenderer.material = _materialSwapper.MaterialsLoadingTask.Result.OpaqueGlowMaterial;
            MeshFilter meshFilter = heart.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            heart.transform.position = new Vector3(-8f, 25f, 26f);
            heart.transform.rotation = Quaternion.Euler(-100f, 90f, 90f);
            heart.transform.localScale = new Vector3(25f, 25f, 25f);
            AddLight(meshRenderer).ColorWasSet(Color.magenta);

            return heart;
        }

        private async Task<GameObject> CreatePlayersPlace()
        {
            (Vector3[] vertices, int[] triangles) = await Task.Run(() => ParseMesh("CustomFloorPlugin.Assets.playersplace.mesh"));
            Mesh mesh = new()
            {
                vertices = vertices,
                triangles = triangles
            };

            GameObject playersPlaceCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            playersPlaceCube.SetActive(false);
            MeshRenderer cubeRenderer = playersPlaceCube.GetComponent<MeshRenderer>();
            await _materialSwapper.MaterialsLoadingTask;
            cubeRenderer.material = _materialSwapper.MaterialsLoadingTask.Result.DarkEnvSimpleMaterial;
            cubeRenderer.material.color = Color.black;
            playersPlaceCube.transform.position = new Vector3(0f, -12.5f, 0f);
            playersPlaceCube.transform.localScale = new Vector3(3f, 25f, 2f);
            playersPlaceCube.name = "PlayersPlace";
            
            GameObject playersPlaceFrame = new("Frame");
            playersPlaceFrame.transform.SetParent(playersPlaceCube.transform);
            MeshRenderer frameRenderer = playersPlaceFrame.AddComponent<MeshRenderer>();
            frameRenderer.material = _materialSwapper.MaterialsLoadingTask.Result.OpaqueGlowMaterial;
            MeshFilter meshFilter = playersPlaceFrame.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            AddLight(frameRenderer).ColorWasSet(Color.blue);

            return playersPlaceCube;
        }

        private static InstancedMaterialLightWithId AddLight(Renderer renderer)
        {
            MaterialPropertyBlockController materialPropertyBlockController = renderer.gameObject.AddComponent<MaterialPropertyBlockController>();
            materialPropertyBlockController.SetField("_renderers", new[] { renderer });
            MaterialPropertyBlockColorSetter materialPropertyBlockColorSetter = renderer.gameObject.AddComponent<MaterialPropertyBlockColorSetter>();
            materialPropertyBlockColorSetter.materialPropertyBlockController = materialPropertyBlockController;
            materialPropertyBlockColorSetter.SetField("_property", "_Color");
            InstancedMaterialLightWithId instancedMaterialLightWithId = renderer.gameObject.AddComponent<InstancedMaterialLightWithId>();
            instancedMaterialLightWithId.SetField("_materialPropertyBlockColorSetter", materialPropertyBlockColorSetter);
            instancedMaterialLightWithId.SetField("_intensity", 1.4f);
            return instancedMaterialLightWithId;
        }

        private static (Vector3[] vertices, int[] triangles) ParseMesh(string resourcePath)
        {
            using Stream manifestResourceStream = GetEmbeddedResource(resourcePath);
            using StreamReader streamReader = new(manifestResourceStream);

            string meshFile = streamReader.ReadToEnd();
            string[] dimension1 = meshFile.Split('|');
            string[][] dimension2 = { dimension1[0].Split('/'), dimension1[1].Split('/') };
            string[][] strVector3S = new string[dimension2[0].Length][];

            int i = 0;
            foreach (string vector3 in dimension2[0])
            {
                strVector3S[i++] = vector3.Split(',');
            }

            List<Vector3> vertices = new();
            List<int> triangles = new();
            foreach (string[] strVector3 in strVector3S)
            {
                vertices.Add(new Vector3(float.Parse(strVector3[0], NumberFormatInfo.InvariantInfo),
                    float.Parse(strVector3[1], NumberFormatInfo.InvariantInfo),
                    float.Parse(strVector3[2], NumberFormatInfo.InvariantInfo)));
            }
            
            foreach (string strInt in dimension2[1])
            {
                triangles.Add(int.Parse(strInt, NumberFormatInfo.InvariantInfo));
            }

            return (vertices.ToArray(), triangles.ToArray());
        }

        private static Stream GetEmbeddedResource(string name) =>
            Assembly.GetManifestResourceStream(name) ??
            throw new InvalidOperationException($"No embedded resource found: {name}");
        
        private static Assembly Assembly => _assembly ??= Assembly.GetExecutingAssembly();
        private static Assembly? _assembly;
    }
}