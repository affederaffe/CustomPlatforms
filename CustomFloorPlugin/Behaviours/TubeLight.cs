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
        public float fogIntensityMultiplier = 0.15f;
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

        private PlatformManager _platformManager;
        private LightWithIdManager _lightManager;

        private TubeBloomPrePassLight tubeBloomLight;
        private GameObject iHeartBeatSaber;

        [Inject]
        public void Construct(PlatformManager platformManager, LightWithIdManager lightManager)
        {
            _platformManager = platformManager;
            _lightManager = lightManager;
        }

        void INotifyPlatformEnabled.PlatformEnabled(DiContainer container)
        {
            // Skip if the light is already initialized.
            // This can happen if a platform uses a GameObject instead of a prefab for a TrackRing
            if (tubeBloomLight != null || iHeartBeatSaber != null)
                return;

            container.Inject(this);
            GetComponent<MeshRenderer>().enabled = false;
            if (GetComponent<MeshFilter>().mesh.vertexCount == 0)
            {
                tubeBloomLight = Instantiate(_platformManager.lightSource.GetComponent<TubeBloomPrePassLight>());
                _platformManager.spawnedObjects.Add(tubeBloomLight.gameObject);

                tubeBloomLight.transform.parent = transform;
                tubeBloomLight.transform.localRotation = Quaternion.identity;
                tubeBloomLight.transform.localPosition = Vector3.zero;
                tubeBloomLight.transform.localScale = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);

                tubeBloomLight.gameObject.SetActive(false);

                TubeBloomPrePassLightWithId lightWithId = tubeBloomLight.GetComponent<TubeBloomPrePassLightWithId>();
                lightWithId.SetField("_tubeBloomPrePassLight", tubeBloomLight);
                (lightWithId as LightWithIdMonoBehaviour).SetField("_ID", (int)lightsID);
                (lightWithId as LightWithIdMonoBehaviour).SetField("_lightManager", _lightManager);

                tubeBloomLight.SetField("_width", width * 2);
                tubeBloomLight.SetField("_length", length);
                tubeBloomLight.SetField("_center", center);
                tubeBloomLight.SetField("_transform", tubeBloomLight.transform);
                tubeBloomLight.SetField("_maxAlpha", 0.1f);
                tubeBloomLight.SetField("_bloomFogIntensityMultiplier", fogIntensityMultiplier);

                ParametricBoxController parabox = tubeBloomLight.GetComponentInChildren<ParametricBoxController>();
                tubeBloomLight.SetField("_parametricBoxController", parabox);

                Parametric3SliceSpriteController parasprite = tubeBloomLight.GetComponentInChildren<Parametric3SliceSpriteController>();
                tubeBloomLight.SetField("_dynamic3SliceSprite", parasprite);
                parasprite.Init();
                parasprite.GetComponent<MeshRenderer>().enabled = false;

                SetColorToDefault();
                tubeBloomLight.gameObject.SetActive(true);

            }
            else
            {
                iHeartBeatSaber = Instantiate(_platformManager.heart);
                _platformManager.spawnedObjects.Add(iHeartBeatSaber);
                iHeartBeatSaber.transform.parent = transform;
                iHeartBeatSaber.transform.position = transform.position;
                iHeartBeatSaber.transform.localScale = Vector3.one;
                iHeartBeatSaber.transform.rotation = transform.rotation;
                InstancedMaterialLightWithId lightWithId = iHeartBeatSaber.GetComponent<InstancedMaterialLightWithId>();
                (lightWithId as LightWithIdMonoBehaviour).SetField("_ID", (int)lightsID);
                (lightWithId as LightWithIdMonoBehaviour).SetField("_lightManager", _lightManager);
                lightWithId.SetField("_minAlpha", 0f);
                iHeartBeatSaber.GetComponent<MeshFilter>().mesh = GetComponent<MeshFilter>().mesh;
                iHeartBeatSaber.SetActive(true);
            }
        }

        private void SetColorToDefault()
        {
            tubeBloomLight.color = color * 0.9f;
            tubeBloomLight.Refresh();
        }
    }
}