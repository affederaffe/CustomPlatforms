using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

using CustomFloorPlugin.Extensions;

using IPA.Utilities;

using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.SceneManagement;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Loads all images into sprites as well as stealing some important GameObjects 
    /// from the GreenDayGrenade environment
    /// </summary>
    public class AssetLoader : MonoBehaviour
    {
        /// <summary>
        /// Acts as a prefab for custom light sources that require meshes...<br/>
        /// Not 100% bug free tbh<br/>
        /// <br/>
        /// Also:<br/>
        /// We love Beat Saber
        /// </summary>
        internal GameObject heart;

        /// <summary>
        /// The Light Source used for non-mesh lights
        /// </summary>
        internal GameObject lightSource;

        /// <summary>
        /// Used as a prefab for light effects in multiplayer
        /// </summary>
        internal GameObject lightEffects;

        /// <summary>
        /// Used as a platform in platform preview if <see cref="CustomPlatform.hideDefaultPlatform"/> is false
        /// </summary>
        internal GameObject playersPlace;

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
        /// Reduces Resources calls
        /// </summary>
        internal Material[] AllMaterials => _AllMaterials ??= Resources.FindObjectsOfTypeAll<Material>();
        private Material[] _AllMaterials;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void Start()
        {
            DontDestroyOnLoad(this);
            LoadSprites();
            LoadAssets();
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
        /// Now also steals the LightEffects for multiplayer, this scene is really useful
        /// </summary>
        private void LoadAssets()
        {
            StartCoroutine(FuckUnity());
            IEnumerator<WaitUntil> FuckUnity()
            {
                yield return null;
                // Did you know loaded scenes are loaded asynchronously, regarless if you use async or not?
                Scene greenDay = SceneManager.LoadScene("GreenDayGrenadeEnvironment", new LoadSceneParameters(LoadSceneMode.Additive));
                yield return new WaitUntil(() => greenDay.isLoaded);
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

                Material envGlowMat = AllMaterials.First(x => x.name == "EnvLightOpaque");
                Material heartGlowMat = new(envGlowMat);
                heartGlowMat.DisableKeyword("ENABLE_HEIGHT_FOG");
                heartGlowMat.name = "<3";
                heartGlowMat.color = Color.cyan;
                heart.GetComponent<Renderer>().material = heartGlowMat;
                heart.GetComponent<MaterialPropertyBlockColorSetter>().SetField("_property", "_Color");
            }
        }
    }
}
