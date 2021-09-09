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

        public async void PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            bool activeSelf = gameObject.activeSelf;
            gameObject.SetActive(false);

            if (_instancedMaterialLightWithId is null && _tubeBloomPrePassLightWithId is null)
            {
                (_, Material transparentGlowMaterial, Material opaqueGlowMaterial) =
                    await _materialSwapper!.MaterialsTask;
                Mesh mesh = GetComponent<MeshFilter>().mesh;
                if (mesh.vertexCount == 0)
                {
                    _tubeBloomPrePassLightWithId = gameObject.AddComponent<TubeBloomPrePassLightWithId>();
                    TubeBloomPrePassLight tubeBloomPrePassLight = gameObject.AddComponent<TubeBloomPrePassLight>();
                    GameObject boxLight = new("BoxLight");
                    boxLight.SetActive(false);
                    boxLight.transform.SetParent(transform);
                    MeshRenderer renderer = boxLight.AddComponent<MeshRenderer>();
                    renderer.sharedMaterial = transparentGlowMaterial;
                    ParametricBoxController parametricBoxController = boxLight.AddComponent<ParametricBoxController>();
                    parametricBoxController.SetField("_meshRenderer", renderer);
                    tubeBloomPrePassLight.SetField("_center", center);
                    tubeBloomPrePassLight.SetField("_parametricBoxController", parametricBoxController);
                    tubeBloomPrePassLight.SetField("_mainEffectPostProcessEnabled", _postProcessEnabled);
                    tubeBloomPrePassLight.width = width;
                    tubeBloomPrePassLight.length = length;
                    _tubeBloomPrePassLightWithId.SetField("_tubeBloomPrePassLight", tubeBloomPrePassLight);
                    ((BloomPrePassLight)tubeBloomPrePassLight).SetField("_lightType", BloomPrePassLight.bloomLightsDict.Keys.First(x => x.name == "AddBloomPrePassLightType"));
                    ((LightWithIdMonoBehaviour)_tubeBloomPrePassLightWithId).SetField("_ID", (int)lightsID);
                    _tubeBloomPrePassLightWithId.ColorWasSet(color);
                    boxLight.SetActive(true);
                }
                else
                {
                    Renderer renderer = GetComponent<Renderer>();
                    renderer.sharedMaterial = opaqueGlowMaterial;
                    MaterialPropertyBlockController materialPropertyBlockController = gameObject.AddComponent<MaterialPropertyBlockController>();
                    materialPropertyBlockController.SetField("_renderers", new[] { renderer });
                    MaterialPropertyBlockColorSetter materialPropertyBlockColorSetter = gameObject.AddComponent<MaterialPropertyBlockColorSetter>();
                    materialPropertyBlockColorSetter.materialPropertyBlockController = materialPropertyBlockController;
                    materialPropertyBlockColorSetter.SetField("_property", "_Color");
                    _instancedMaterialLightWithId = gameObject.AddComponent<InstancedMaterialLightWithId>();
                    _instancedMaterialLightWithId.SetField("_materialPropertyBlockColorSetter", materialPropertyBlockColorSetter);
                    _instancedMaterialLightWithId.SetField("_intensity", 1.25f);
                    ((LightWithIdMonoBehaviour)_instancedMaterialLightWithId).SetField("_ID", (int)lightsID);
                    _instancedMaterialLightWithId.ColorWasSet(color);
                }
            }

            LightWithIdMonoBehaviour light = _instancedMaterialLightWithId is null ? _tubeBloomPrePassLightWithId! : _instancedMaterialLightWithId!;
            light.SetField("_lightManager", _lightWithIdManager);
            gameObject.SetActive(activeSelf);
        }

        public void PlatformDisabled()
        {
            if (_instancedMaterialLightWithId is null) return;
            _instancedMaterialLightWithId.ColorWasSet(color);
        }
    }
}