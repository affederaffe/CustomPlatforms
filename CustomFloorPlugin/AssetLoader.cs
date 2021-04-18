using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using CustomFloorPlugin.Extensions;

using IPA.Utilities;
using IPA.Utilities.Async;

using SiraUtil.Tools;

using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.SceneManagement;

using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Loads all images into sprites as well as stealing some important GameObjects 
    /// from the GreenDayGrenade environment.
    /// </summary>
    public class AssetLoader : MonoBehaviour
    {
        private SiraLog? _siraLog;

        /// <summary>
        /// The Task responsible for asset loading
        /// </summary>
        internal Task? loadAssetsTask;

        /// <summary>
        /// Acts as a prefab for custom light sources that require meshes...<br/>
        /// Not 100% bug free tbh<br/>
        /// <br/>
        /// Also:<br/>
        /// We love Beat Saber
        /// </summary>
        internal GameObject? heart;

        /// <summary>
        /// The Light Source used for non-mesh lights
        /// </summary>
        internal GameObject? lightSource;

        /// <summary>
        /// Used as a prefab for light effects in multiplayer
        /// </summary>
        internal GameObject? lightEffects;

        /// <summary>
        /// Used as a platform in platform preview if <see cref="CustomPlatform.hideDefaultPlatform"/> is false
        /// </summary>
        internal GameObject? playersPlace;

        /// <summary>
        /// The cover for the default platform
        /// </summary>
        internal Sprite? defaultPlatformCover;

        /// <summary>
        /// The cover used for all platforms normally missing one
        /// </summary>
        internal Sprite? fallbackCover;

        /// <summary>
        /// Transparent glow material for the <see cref="MaterialSwapper"/>
        /// </summary>
        internal Material? transparentGlowMaterial;

        /// <summary>
        /// Opaque glow material for the <see cref="MaterialSwapper"/>
        /// </summary>
        internal Material? opaqueGlowMaterial;

        /// <summary>
        /// Simple dark environment material for the <see cref="MaterialSwapper"/>
        /// </summary>
        internal Material? darkEnvMaterial;

        [Inject]
        public void Construct(SiraLog siraLog)
        {
            _siraLog = siraLog;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void Start()
        {
            DontDestroyOnLoad(this);
            LoadSprites();
            loadAssetsTask = LoadAssetsAsync();
        }

        private void LoadSprites()
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            defaultPlatformCover = executingAssembly.GetManifestResourceStream("CustomFloorPlugin.Assets.LvlInsaneCover.png").ReadPNGToSprite();
            fallbackCover = executingAssembly.GetManifestResourceStream("CustomFloorPlugin.Assets.FeetIcon.png").ReadPNGToSprite();
        }

        /// <summary>
        /// Steals the heart from the GreenDayScene<br/>
        /// Then De-Serializes the data from the embedded resource heart.mesh onto the GreenDayHeart to make it more visually pleasing<br/>
        /// Also adjusts it position and color<br/>
        /// Gets the Non-Mesh lightSource and the playersPlace used in the Platform Preview too<br/>
        /// Now also steals the LightEffects for multiplayer, this scene is really useful
        /// </summary>
        private async Task LoadAssetsAsync()
        {
            Stopwatch sw = new();
            sw.Start();

            await Coroutines.AsTask(WaitForEndOfFrameCoroutine());
            static IEnumerator<WaitForEndOfFrame> WaitForEndOfFrameCoroutine() { yield return new WaitForEndOfFrame(); };
            Scene greenDay = await LoadSceneAsync("GreenDayGrenadeEnvironment");
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

            SceneManager.UnloadSceneAsync(greenDay);

            DestroyImmediate(heart.GetComponent<ProBuilderMesh>());
            DestroyImmediate(playersPlace.GetComponentInChildren<SaberBurnMarkArea>().gameObject);
            DestroyImmediate(playersPlace.GetComponentInChildren<SaberBurnMarkSparkles>().gameObject);

            ValueTuple<Vector3[], int[]> meshData = await Task.Run(CreateMeshData);
            Mesh mesh = new() // TIL the Mesh constructor is not thread-safe
            {
                vertices = meshData.Item1,
                triangles = meshData.Item2
            };

            heart.GetComponent<MeshFilter>().mesh = mesh;
            heart.transform.position = new Vector3(-8f, 25f, 26f);
            heart.transform.rotation = Quaternion.Euler(-100f, 90f, 90f);
            heart.transform.localScale = new Vector3(25f, 25f, 25f);
            heart.layer = 13;

            Renderer buildingsRenderer = root.transform.Find("GreenDayCity/BackgroundBuildings").GetComponent<Renderer>(); // "DarkEnvironmentSimple" Material
            darkEnvMaterial = buildingsRenderer.material;
            Renderer rotLightRenderer = root.transform.Find("RotatingLasersPair/BaseL/Laser/BoxLight").GetComponent<Renderer>(); // "EnvLight" Material
            transparentGlowMaterial = rotLightRenderer.material;
            Renderer lightSourceRenderer = lightSource.GetComponentInChildren<ParametricBoxController>().GetComponent<Renderer>(); // "EnvLightOpaque" Material
            opaqueGlowMaterial = new(lightSourceRenderer.material);
            opaqueGlowMaterial.DisableKeyword("ENABLE_HEIGHT_FOG");
            opaqueGlowMaterial.name = "PlatformsCustomGlow";
            opaqueGlowMaterial.color = Color.cyan;

            lightSourceRenderer.material = opaqueGlowMaterial;
            heart.GetComponent<Renderer>().material = opaqueGlowMaterial;
            heart.GetComponent<MaterialPropertyBlockColorSetter>().SetField("_property", "_Color");

            InstancedMaterialLightWithId materialLightWithId = heart.GetComponent<InstancedMaterialLightWithId>();
            materialLightWithId.SetField("_minAlpha", 0f);
            materialLightWithId.SetField("_intensity", 1.675f);
            materialLightWithId.ColorWasSet(Color.magenta);

            TubeBloomPrePassLight tubeBloomLight = lightSource.GetComponent<TubeBloomPrePassLight>();
            tubeBloomLight.SetField("_maxAlpha", 0.1f);
            tubeBloomLight.bloomFogIntensityMultiplier = 0.125f;

            foreach (LightSwitchEventEffect lightSwitchEventEffect in lightEffects.GetComponents<LightSwitchEventEffect>())
            {
                lightSwitchEventEffect.SetField("_lightIsOn", false);
                lightSwitchEventEffect.SetField("_initialized", false);
            }

            sw.Stop();
            _siraLog!.Info($"Loaded Assets in {sw.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// Creates the heart's <see cref="Mesh"/>data
        /// </summary>
        private static ValueTuple<Vector3[], int[]> CreateMeshData()
        {
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

            return new ValueTuple<Vector3[], int[]>(vertices.ToArray(), triangles.ToArray());
        }

        /// <summary>
        /// Asynchroniously loads and then returns a <see cref="Scene"/>
        /// </summary>
        private static async Task<Scene> LoadSceneAsync(string name)
        {
            TaskCompletionSource<Scene> taskSource = new();
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(name, new LoadSceneParameters(LoadSceneMode.Additive));
            asyncOperation.completed += delegate
            {
                Scene scene = SceneManager.GetSceneByName(name);
                taskSource.TrySetResult(scene);
            };
            return await taskSource.Task;
        }
    }
}