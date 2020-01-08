using CustomFloorPlugin.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Harmony;
using UnityEngine.SceneManagement;
using BS_Utils.Utilities;
using CustomFloorPlugin;
namespace CustomFloorPlugin {
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]

    public class TubeLight:MonoBehaviour {
        public enum LightsID {
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

        private void OnDrawGizmos() {
            Gizmos.color = color;
            Gizmos.matrix = transform.localToWorldMatrix;
            Vector3 cubeCenter = Vector3.up * (0.5f - center) * length;
            Gizmos.DrawCube(cubeCenter, new Vector3(2 * width, length, 2 * width));
        }

        // ----------------

        private static TubeBloomPrePassLight _prefab;
        internal static TubeBloomPrePassLight prefab {
            get {
                if(_prefab == null) {
                    try {
                        _prefab =
                            SceneManager
                            .GetSceneByName("MenuEnvironment")
                            .GetRootGameObjects()
                            .First<GameObject>(x => x.name == "Wrapper")
                            .transform
                            .Find("NearBuildingLeft/Neon")
                            .GetComponent<TubeBloomPrePassLight>();
                    } catch(InvalidOperationException) {
                        _prefab =
                            SceneManager
                            .GetSceneByName("MenuEnvironment")
                            .GetRootGameObjects()
                            .First<GameObject>(x => x.name == "RootContainer")
                            .transform
                            .Find("Wrapper/NearBuildingLeft/Neon")
                            .GetComponent<TubeBloomPrePassLight>();
                    }
                }
                return _prefab;
            }
        }
        private TubeBloomPrePassLight tubeBloomLight;
        private GameObject iHeartBeatSaber;
        internal void GameAwake() {

            GetComponent<MeshRenderer>().enabled = false;
            if(GetComponent<MeshFilter>().mesh.vertexCount == 0) {
                tubeBloomLight = Instantiate(prefab);
                tubeBloomLight.transform.parent = transform;
                PlatformManager.SpawnedObjects.Add(tubeBloomLight.gameObject);

                tubeBloomLight.transform.localRotation = Quaternion.identity;
                tubeBloomLight.transform.localPosition = Vector3.zero;
                tubeBloomLight.transform.localScale = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);

                tubeBloomLight.gameObject.SetActive(false);

                var lightWithId = tubeBloomLight.GetComponent<LightWithId>();
                if(lightWithId) {
                    lightWithId.SetPrivateField("_tubeBloomPrePassLight", tubeBloomLight);
                    lightWithId.SetPrivateField("_ID", (int)lightsID);
                    lightWithId.SetPrivateField("_lightManager", PlatformManager.LightManager);
                }
                // i broke it -.-
                //try to fix it, if you can't: roll back
                tubeBloomLight.SetPrivateField("_width", width * 2);
                tubeBloomLight.SetPrivateField("_length", length);
                tubeBloomLight.SetPrivateField("_center", center);
                tubeBloomLight.SetPrivateField("_transform", tubeBloomLight.transform);
                tubeBloomLight.SetPrivateField("_maxAlpha", 0.1f);

                var parabox = tubeBloomLight.GetComponentInChildren<ParametricBoxController>();
                tubeBloomLight.SetPrivateField("_parametricBoxController", parabox);

                var parasprite = tubeBloomLight.GetComponentInChildren<Parametric3SliceSpriteController>();
                tubeBloomLight.SetPrivateField("_dynamic3SliceSprite", parasprite);
                parasprite.Init();
                parasprite.GetComponent<MeshRenderer>().enabled = false;

                SetColorToDefault();
                tubeBloomLight.gameObject.SetActive(true);
            } else {
                // swap for <3
                iHeartBeatSaber = Instantiate(PlatformManager.Heart);
                PlatformManager.SpawnedObjects.Add(iHeartBeatSaber);
                iHeartBeatSaber.transform.parent = transform;
                iHeartBeatSaber.transform.position = transform.position;
                iHeartBeatSaber.transform.localScale = Vector3.one;
                iHeartBeatSaber.transform.rotation = transform.rotation;
                var lightWithId = iHeartBeatSaber.GetComponent<LightWithId>();
                if(lightWithId) {
                    lightWithId.SetPrivateField("_ID", (int)lightsID);
                    lightWithId.SetPrivateField("_lightManager", PlatformManager.LightManager);
                    lightWithId.SetPrivateField("_minAlpha", 0);
                }
                iHeartBeatSaber.GetComponent<MeshFilter>().mesh = GetComponent<MeshFilter>().mesh;
                iHeartBeatSaber.SetActive(true);
            }
        }
        IEnumerator<WaitForEndOfFrame> KerFuffel(TubeBloomPrePassLight tubeBloomLight) {
            yield return new WaitForEndOfFrame();
            tubeBloomLight.color = Color.black.ColorWithAlpha(0);
            tubeBloomLight.Refresh();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void OnEnable() {
            PlatformManager.SpawnQueue += GameAwake;
        }
        private void OnDisable() {
            PlatformManager.SpawnQueue -= GameAwake;
        }
        private void SetColorToDefault() {
            tubeBloomLight.color = color * 0.9f; //<-------//This, i need that!
            tubeBloomLight.Refresh();
        }
    }
}