using System.Linq;

using CustomFloorPlugin.Interfaces;

using IPA.Utilities;

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

        private static readonly FieldAccessor<ParametricBoxController, MeshRenderer>.Accessor _meshRendererAccessor = FieldAccessor<ParametricBoxController, MeshRenderer>.GetAccessor("_meshRenderer");
        private static readonly FieldAccessor<TubeBloomPrePassLight, float>.Accessor _centerAccessor = FieldAccessor<TubeBloomPrePassLight, float>.GetAccessor("_center");
        private static readonly FieldAccessor<TubeBloomPrePassLight, float>.Accessor _colorAlphaMultiplierAccessor = FieldAccessor<TubeBloomPrePassLight, float>.GetAccessor("_colorAlphaMultiplier");
        private static readonly FieldAccessor<TubeBloomPrePassLight, float>.Accessor _boostToWhiteAccessor = FieldAccessor<TubeBloomPrePassLight, float>.GetAccessor("_boostToWhite");
        private static readonly FieldAccessor<TubeBloomPrePassLight, ParametricBoxController>.Accessor _parametricBoxControllerAccessor = FieldAccessor<TubeBloomPrePassLight, ParametricBoxController>.GetAccessor("_parametricBoxController");
        private static readonly FieldAccessor<TubeBloomPrePassLight, BoolSO?>.Accessor _mainEffectPostProcessEnabledAccessor = FieldAccessor<TubeBloomPrePassLight, BoolSO?>.GetAccessor("_mainEffectPostProcessEnabled");
        private static readonly FieldAccessor<TubeBloomPrePassLightWithId, TubeBloomPrePassLight>.Accessor _tubeBloomPrePassLightAccessor = FieldAccessor<TubeBloomPrePassLightWithId, TubeBloomPrePassLight>.GetAccessor("_tubeBloomPrePassLight");
        private static readonly FieldAccessor<TubeBloomPrePassLightWithId, bool>.Accessor _setOnlyOnceAccessor = FieldAccessor<TubeBloomPrePassLightWithId, bool>.GetAccessor("_setOnlyOnce");
        private static readonly FieldAccessor<BloomPrePassLight, BloomPrePassLightTypeSO>.Accessor _lightTypeAccessor = FieldAccessor<BloomPrePassLight, BloomPrePassLightTypeSO>.GetAccessor("_lightType");
        private static readonly FieldAccessor<LightWithIdMonoBehaviour, int>.Accessor _idAccessor = FieldAccessor<LightWithIdMonoBehaviour, int>.GetAccessor("_ID");
        private static readonly FieldAccessor<LightWithIdMonoBehaviour, LightWithIdManager?>.Accessor _lightManagerAccessor = FieldAccessor<LightWithIdMonoBehaviour, LightWithIdManager?>.GetAccessor("_lightManager");
        private static readonly FieldAccessor<MaterialPropertyBlockController, Renderer[]>.Accessor _renderersAccessor = FieldAccessor<MaterialPropertyBlockController, Renderer[]>.GetAccessor("_renderers");
        private static readonly FieldAccessor<MaterialPropertyBlockColorSetter, string>.Accessor _propertyAccessor = FieldAccessor<MaterialPropertyBlockColorSetter, string>.GetAccessor("_property");
        private static readonly FieldAccessor<InstancedMaterialLightWithId, MaterialPropertyBlockColorSetter>.Accessor _materialPropertyBlockColorSetterAccessor = FieldAccessor<InstancedMaterialLightWithId, MaterialPropertyBlockColorSetter>.GetAccessor("_materialPropertyBlockColorSetter");
        private static readonly FieldAccessor<InstancedMaterialLightWithId, float>.Accessor _intensityAccessor = FieldAccessor<InstancedMaterialLightWithId, float>.GetAccessor("_intensity");

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
                        _setOnlyOnceAccessor(ref _tubeBloomPrePassLightWithId) = true;
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
                    _meshRendererAccessor(ref parametricBoxController) = renderer;
                    _centerAccessor(ref tubeBloomPrePassLight) = center;
                    _colorAlphaMultiplierAccessor(ref tubeBloomPrePassLight) = colorAlphaMultiplier;
                    _boostToWhiteAccessor(ref tubeBloomPrePassLight) = boostToWhite;
                    _parametricBoxControllerAccessor(ref tubeBloomPrePassLight) = parametricBoxController;
                    _mainEffectPostProcessEnabledAccessor(ref tubeBloomPrePassLight) = _postProcessEnabled;
                    tubeBloomPrePassLight.width = width * 2;
                    tubeBloomPrePassLight.length = length;
                    tubeBloomPrePassLight.bloomFogIntensityMultiplier = bloomFogIntensityMultiplier;
                    _tubeBloomPrePassLightAccessor(ref _tubeBloomPrePassLightWithId) = tubeBloomPrePassLight;
                    BloomPrePassLight bloomPrePassLight = tubeBloomPrePassLight;
                    _lightTypeAccessor(ref bloomPrePassLight) = BloomPrePassLight.bloomLightsDict.Keys.First(static x => x.name == "AddBloomPrePassLightType");
                    LightWithIdMonoBehaviour lightWithIdMonoBehaviour = _tubeBloomPrePassLightWithId;
                    _idAccessor(ref lightWithIdMonoBehaviour) = (int)lightsID;
                    _tubeBloomPrePassLightWithId.ColorWasSet(color);
                    boxLight.SetActive(true);
                }
                else
                {
                    Renderer renderer = GetComponent<Renderer>();
                    renderer.sharedMaterial = _materialSwapper!.OpaqueGlowMaterial;
                    MaterialPropertyBlockController materialPropertyBlockController = gameObject.AddComponent<MaterialPropertyBlockController>();
                    _renderersAccessor(ref materialPropertyBlockController) = new[] { renderer };
                    MaterialPropertyBlockColorSetter materialPropertyBlockColorSetter = gameObject.AddComponent<MaterialPropertyBlockColorSetter>();
                    materialPropertyBlockColorSetter.materialPropertyBlockController = materialPropertyBlockController;
                    _propertyAccessor(ref materialPropertyBlockColorSetter) = "_Color";
                    _instancedMaterialLightWithId = gameObject.AddComponent<InstancedMaterialLightWithId>();
                    _materialPropertyBlockColorSetterAccessor(ref _instancedMaterialLightWithId) = materialPropertyBlockColorSetter;
                    _intensityAccessor(ref _instancedMaterialLightWithId) = 1.25f;
                    LightWithIdMonoBehaviour lightWithIdMonoBehaviour = _instancedMaterialLightWithId;
                    _idAccessor(ref lightWithIdMonoBehaviour) = (int)lightsID;
                    _instancedMaterialLightWithId.ColorWasSet(color);
                }
            }

            LightWithIdMonoBehaviour? lightWithIdMonoBehaviour1 = _instancedMaterialLightWithId is not null ? _instancedMaterialLightWithId : _tubeBloomPrePassLightWithId;
            _lightManagerAccessor(ref lightWithIdMonoBehaviour1!) = _lightWithIdManager;
            gameObject.SetActive(activeSelf);
        }

        public void PlatformDisabled() => _instancedMaterialLightWithId?.ColorWasSet(color);

        private static Mesh? _mesh;
        private static Mesh Mesh => _mesh ??= new Mesh
        {
            vertices = new Vector3[]
            {
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
            },
            triangles = new[]
            {
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
            }
        };
    }
}
