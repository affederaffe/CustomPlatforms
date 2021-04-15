using IPA.Utilities;

using UnityEngine;

using Zenject;


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
        [Range(0, 1)]
        public float center = 0.5f;
        public Color color = Color.white;
        public LightsID lightsID = LightsID.Static;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnDrawGizmos()
        {
            Gizmos.color = color;
            Gizmos.matrix = transform.localToWorldMatrix;
            Vector3 cubeCenter = Vector3.up * (0.5f - center) * length;
            Gizmos.DrawCube(cubeCenter, new Vector3(2 * width, length, 2 * width));
        }

        private AssetLoader? _assetLoader;
        private PlatformManager? _platformManager;
        private LightWithIdManager? _lightManager;

        private TubeBloomPrePassLight? _tubeBloomLight;
        private TubeBloomPrePassLightWithId? _tubeBloomLightWithId;
        private InstancedMaterialLightWithId? _iHeartBeatSaber;

        [Inject]
        public void Construct(AssetLoader assetLoader, PlatformManager platformManager, LightWithIdManager lightManager)
        {
            _assetLoader = assetLoader;
            _platformManager = platformManager;
            _lightManager = lightManager;
        }

        void INotifyPlatformEnabled.PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            GetComponent<MeshRenderer>().enabled = false;
            if (GetComponent<MeshFilter>().mesh.vertexCount == 0)
            {
                _tubeBloomLight = Instantiate(_assetLoader!.lightSource!.GetComponent<TubeBloomPrePassLight>());
                _platformManager!.spawnedObjects.Add(_tubeBloomLight.gameObject);

                _tubeBloomLight.transform.parent = transform;
                _tubeBloomLight.transform.localRotation = Quaternion.identity;
                _tubeBloomLight.transform.localPosition = Vector3.zero;
                _tubeBloomLight.transform.localScale = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);

                _tubeBloomLightWithId = _tubeBloomLight.GetComponent<TubeBloomPrePassLightWithId>();
                _tubeBloomLightWithId.SetField("_tubeBloomPrePassLight", _tubeBloomLight);
                (_tubeBloomLightWithId as LightWithIdMonoBehaviour).SetField("_ID", (int)lightsID);
                (_tubeBloomLightWithId as LightWithIdMonoBehaviour).SetField("_lightManager", _lightManager);

                _tubeBloomLight.SetField("_width", width * 2);
                _tubeBloomLight.SetField("_length", length);
                _tubeBloomLight.SetField("_center", center);
                _tubeBloomLight.SetField("_transform", _tubeBloomLight.transform);

                ParametricBoxController parabox = _tubeBloomLight.GetComponentInChildren<ParametricBoxController>();
                _tubeBloomLight.SetField("_parametricBoxController", parabox);

                Parametric3SliceSpriteController parasprite = _tubeBloomLight.GetComponentInChildren<Parametric3SliceSpriteController>();
                _tubeBloomLight.SetField("_dynamic3SliceSprite", parasprite);
                parasprite.Init();
                parasprite.GetComponent<MeshRenderer>().enabled = false;

                _tubeBloomLight.color = color * 0.9f;
                _tubeBloomLight.Refresh();

                _tubeBloomLight.gameObject.SetActive(true);
            }
            else
            {
                _iHeartBeatSaber = Instantiate(_assetLoader!.heart!.GetComponent<InstancedMaterialLightWithId>());
                _platformManager!.spawnedObjects.Add(_iHeartBeatSaber.gameObject);

                _iHeartBeatSaber.transform.parent = transform;
                _iHeartBeatSaber.transform.position = transform.position;
                _iHeartBeatSaber.transform.localScale = Vector3.one;
                _iHeartBeatSaber.transform.rotation = transform.rotation;

                (_iHeartBeatSaber as LightWithIdMonoBehaviour).SetField("_ID", (int)lightsID);
                (_iHeartBeatSaber as LightWithIdMonoBehaviour).SetField("_lightManager", _lightManager);
                _iHeartBeatSaber.GetComponent<MeshFilter>().mesh = GetComponent<MeshFilter>().mesh;
                _iHeartBeatSaber.gameObject.SetActive(true);
            }
        }

        void INotifyPlatformDisabled.PlatformDisabled()
        {
            if (_tubeBloomLight != null)
            {
                _tubeBloomLight.InvokeMethod<object, BloomPrePassLight>("UnregisterLight");
                _lightManager!.UnregisterLight(_tubeBloomLightWithId);
            }
            else if (_iHeartBeatSaber != null)
            {
                _lightManager!.UnregisterLight(_iHeartBeatSaber);
            }
        }
    }
}