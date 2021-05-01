using IPA.Utilities;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class TubeLight : MonoBehaviour, INotifyPlatformEnabled
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
        [Range(0, 1)]
        public float center = 0.5f;
        public Color color = Color.cyan;
        public LightsID lightsID;

        private MaterialSwapper? _materialSwapper;
        private LightWithIdManager? _lightWithIdManager;

        private InstancedMaterialLightWithId? _instancedMaterialLightWithId;

        [Inject]
        public void Construct(MaterialSwapper materialSwapper, LightWithIdManager lightWithIdManager)
        {
            _materialSwapper = materialSwapper;
            _lightWithIdManager = lightWithIdManager;
        }

        public void PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            gameObject.SetActive(false);

            // Using 2 different light types was a mistake
            Mesh mesh = GetComponent<MeshFilter>().mesh;
            if (mesh.vertexCount == 0)
            {
                float y = (0.5f - center) * length * 2;
                mesh.vertices = new Vector3[]
                {
                    new(-width, (y-length)/2, -width),
                    new(width, (y-length)/2, -width),
                    new(width, (y+length)/2, -width),
                    new(-width, (y+length)/2, -width),
                    new(-width, (y+length)/2, width),
                    new(width, (y+length)/2, width),
                    new(width, (y-length)/2, width),
                    new(-width, (y-length)/2, width),
                };
                mesh.triangles = triangles;
            }

            if (_instancedMaterialLightWithId == null)
            {
                GetComponent<Renderer>().material = _materialSwapper!.MaterialsLoadingTask.Result.OpaqueGlowMaterial;
                MaterialPropertyBlockController materialPropertyBlockController = gameObject.AddComponent<MaterialPropertyBlockController>();
                materialPropertyBlockController.SetField("_renderers", new[] { GetComponent<Renderer>() });
                MaterialPropertyBlockColorSetter materialPropertyBlockColorSetter = gameObject.AddComponent<MaterialPropertyBlockColorSetter>();
                materialPropertyBlockColorSetter.materialPropertyBlockController = materialPropertyBlockController;
                materialPropertyBlockColorSetter.SetField("_property", "_Color");
                _instancedMaterialLightWithId = gameObject.AddComponent<InstancedMaterialLightWithId>();
                _instancedMaterialLightWithId.SetField("_materialPropertyBlockColorSetter", materialPropertyBlockColorSetter);
                _instancedMaterialLightWithId.SetField("_intensity", 1.4f);
                ((LightWithIdMonoBehaviour)_instancedMaterialLightWithId).SetField("_ID", (int)lightsID);
                _instancedMaterialLightWithId.ColorWasSet(color);
                gameObject.layer = 13;
            }

            ((LightWithIdMonoBehaviour)_instancedMaterialLightWithId).SetField("_lightManager", _lightWithIdManager);
            gameObject.SetActive(true);
        }

        private static readonly int[] triangles = {
            0, 2, 1, //face front
			0, 3, 2,
            2, 3, 4, //face top
			2, 4, 5,
            1, 2, 5, //face right
			1, 5, 6,
            0, 7, 4, //face left
			0, 4, 3,
            5, 4, 7, //face back
			5, 7, 6,
            0, 6, 7, //face bottom
			0, 1, 6
        };
    }
}