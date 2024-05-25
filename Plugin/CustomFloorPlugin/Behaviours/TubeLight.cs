using System.Linq;

using CustomFloorPlugin.Interfaces;

using UnityEngine;

using Zenject;


// ReSharper disable once CheckNamespace
namespace CustomFloorPlugin
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class TubeLight : MonoBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
    {
        public enum LightsID
        {
            Static = 0,
            BackLights = 1,
            BigRingLights = 2,
            LeftLasers = 3,
            RightLasers = 4,
            TrackAndBottom = 5,
            Unused5 = 6,
            Unused6 = 7,
            Unused7 = 8,
            RingsRotationEffect = 9,
            RingsStepEffect = 10,
            Unused10 = 11,
            Unused11 = 12,
            RingSpeedLeft = 13,
            RingSpeedRight = 14,
            Unused14 = 15,
            Unused15 = 16
        }

        public float width = 0.5f;
        public float length = 1f;
        [Range(0, 1)] public float center = 0.5f;
        public Color color = Color.cyan;
        public float colorAlphaMultiplier = 1f;
        public float bloomFogIntensityMultiplier = 1f;
        public float boostToWhite;
        public LightsID lightsID;

        private MaterialSwapper? _materialSwapper;
        private BoolSO? _postProcessEnabled;
        private LightWithIdManager? _lightWithIdManager;

        private TubeBloomPrePassLightWithId? _tubeBloomPrePassLightWithId;
        private InstancedMaterialLightWithId? _instancedMaterialLightWithId;

        [Inject]
        public void Construct(MaterialSwapper materialSwapper, [Inject(Id = "PostProcessEnabled")] BoolSO postProcessEnabled, [InjectOptional] LightWithIdManager lightWithIdManager)
        {
            _materialSwapper = materialSwapper;
            _postProcessEnabled = postProcessEnabled;
            _lightWithIdManager = lightWithIdManager;
        }

        public void PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            bool activeSelf = gameObject.activeSelf;
            gameObject.SetActive(false);

            if (_instancedMaterialLightWithId is null && _tubeBloomPrePassLightWithId is null)
            {
                Mesh mesh = GetComponent<MeshFilter>().mesh;
                if (mesh.vertexCount == 0)
                {
                    _tubeBloomPrePassLightWithId = gameObject.AddComponent<TubeBloomPrePassLightWithId>();
                    TubeBloomPrePassLight tubeBloomPrePassLight = gameObject.AddComponent<TubeBloomPrePassLight>();
                    if (lightsID == LightsID.Static)
                        _tubeBloomPrePassLightWithId._setOnlyOnce = true;
                    GameObject boxLight = new("BoxLight");
                    boxLight.SetActive(false);
                    boxLight.layer = 13; // NeonLight
                    Transform boxLightTransform = boxLight.transform;
                    boxLightTransform.transform.SetParent(transform);
                    boxLightTransform.transform.localRotation = Quaternion.Euler(Vector3.zero);
                    MeshFilter meshFilter = boxLight.AddComponent<MeshFilter>();
                    meshFilter.mesh = Mesh;
                    MeshRenderer renderer = boxLight.AddComponent<MeshRenderer>();
                    renderer.sharedMaterial = _materialSwapper!.TransparentGlowMaterial;
                    ParametricBoxController parametricBoxController = boxLight.AddComponent<ParametricBoxController>();
                    parametricBoxController._meshRenderer = renderer;
                    tubeBloomPrePassLight._center = center;
                    tubeBloomPrePassLight._colorAlphaMultiplier = colorAlphaMultiplier;
                    tubeBloomPrePassLight._boostToWhite = boostToWhite;
                    tubeBloomPrePassLight._parametricBoxController = parametricBoxController;
                    tubeBloomPrePassLight._mainEffectPostProcessEnabled = _postProcessEnabled;
                    tubeBloomPrePassLight.width = width * 2;
                    tubeBloomPrePassLight.length = length;
                    tubeBloomPrePassLight.bloomFogIntensityMultiplier = bloomFogIntensityMultiplier;
                    _tubeBloomPrePassLightWithId._tubeBloomPrePassLight = tubeBloomPrePassLight;
                    BloomPrePassLight bloomPrePassLight = tubeBloomPrePassLight;
                    bloomPrePassLight._lightType = BloomPrePassLight.bloomLightsDict.Keys.First(static x => x.name == "AddBloomPrePassLightType");
                    LightWithIdMonoBehaviour lightWithIdMonoBehaviour = _tubeBloomPrePassLightWithId;
                    lightWithIdMonoBehaviour._ID = (int)lightsID;
                    _tubeBloomPrePassLightWithId.ColorWasSet(color);
                    boxLight.SetActive(true);
                }
                else
                {
                    Renderer renderer = GetComponent<Renderer>();
                    renderer.sharedMaterial = _materialSwapper!.OpaqueGlowMaterial;
                    MaterialPropertyBlockController materialPropertyBlockController = gameObject.AddComponent<MaterialPropertyBlockController>();
                    materialPropertyBlockController._renderers = [renderer];
                    MaterialPropertyBlockColorSetter materialPropertyBlockColorSetter = gameObject.AddComponent<MaterialPropertyBlockColorSetter>();
                    materialPropertyBlockColorSetter.materialPropertyBlockController = materialPropertyBlockController;
                    materialPropertyBlockColorSetter._property = "_Color";
                    _instancedMaterialLightWithId = gameObject.AddComponent<InstancedMaterialLightWithId>();
                    _instancedMaterialLightWithId._materialPropertyBlockColorSetter = materialPropertyBlockColorSetter;
                    _instancedMaterialLightWithId._intensity = 1.25f;
                    LightWithIdMonoBehaviour lightWithIdMonoBehaviour = _instancedMaterialLightWithId;
                    lightWithIdMonoBehaviour._ID = (int)lightsID;
                    _instancedMaterialLightWithId.ColorWasSet(color);
                }
            }

            LightWithIdMonoBehaviour? lightWithIdMonoBehaviour1 = _instancedMaterialLightWithId is not null ? _instancedMaterialLightWithId : _tubeBloomPrePassLightWithId;
            lightWithIdMonoBehaviour1!._lightManager = _lightWithIdManager;
            gameObject.SetActive(activeSelf);
        }

        public void PlatformDisabled() => _instancedMaterialLightWithId?.ColorWasSet(color);

        private static Mesh? _mesh;
        private static Mesh Mesh => _mesh ??= new Mesh
        {
            vertices =
            [
                new(1, -1, -1),
                new(1, -1, 1),
                new(1, 1, -1),
                new(1, 1, 1),
                new(-1, -1, -1),
                new(-1, -1, 1),
                new(-1, 1, -1),
                new(-1, 1, 1),
                new(1, 1, -1),
                new(1, 1, 1),
                new(-1, 1, -1),
                new(-1, 1, 1),
                new(-1, -1, -1),
                new(1, -1, -1),
                new(1, -1, 1),
                new(-1, -1, 1),
                new(1, 1, -1),
                new(1, -1, -1),
                new(-1, -1, -1),
                new(-1, 1, -1),
                new(-1, 1, 1),
                new(-1, -1, 1),
                new(1, -1, 1),
                new(1, 1, 1)
            ],
            triangles =
            [
                0, 2, 3,
                0, 3, 1,
                8, 6, 7,
                8, 7, 9,
                10, 4, 5,
                10, 5, 11,
                12, 13, 14,
                12, 14, 15,
                16, 17, 18,
                16, 18, 19,
                20, 21, 22,
                20, 22, 23
            ]
        };
    }
}
