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
        private static LightWithIdManager _lightManager;
        

        private void OnDrawGizmos() {
            Gizmos.color = color;
            Gizmos.matrix = transform.localToWorldMatrix;
            Vector3 cubeCenter = Vector3.up * (0.5f - center) * length;
            Gizmos.DrawCube(cubeCenter, new Vector3(2 * width, length, 2 * width));
        }

        // ----------------
        private bool IsMesh = false;

        private static TubeBloomPrePassLight _prefab;
        internal static TubeBloomPrePassLight prefab {
            get {
                if(_prefab == null) {
                    Plugin.Log("Setting Prefab!");
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

        private void Awake() {

        }

        private void GameAwake() {
            tubeBloomLight = Instantiate(prefab);
            tubeBloomLight.transform.SetParent(transform);
            PlatformManager.SpawnedObjects.Add(tubeBloomLight.gameObject);

            tubeBloomLight.transform.localRotation = Quaternion.identity;
            tubeBloomLight.transform.localPosition = Vector3.zero;
            tubeBloomLight.transform.localScale = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);


            if(GetComponent<MeshFilter>().mesh.vertexCount == 0) {
                GetComponent<MeshRenderer>().enabled = false;
                //Traverse.Create(tubeBloomLight).Field("_registeredWithLightType")
            } else {
                // swap for MeshBloomPrePassLight
                IsMesh = true;
                tubeBloomLight.gameObject.SetActive(false);
                MeshBloomPrePassLight meshbloom = ReflectionUtil.CopyComponent(tubeBloomLight, typeof(TubeBloomPrePassLight), typeof(MeshBloomPrePassLight), tubeBloomLight.gameObject) as MeshBloomPrePassLight;
                meshbloom.Init(GetComponent<Renderer>());
                DestroyImmediate(tubeBloomLight);
                tubeBloomLight = meshbloom;
                tubeBloomLight.gameObject.SetActive(true);
            }
            tubeBloomLight.gameObject.SetActive(false);

            var lightWithId = tubeBloomLight.GetComponent<LightWithId>();
            if(lightWithId) {

                lightWithId.SetPrivateField("_tubeBloomPrePassLight", tubeBloomLight);
                Traverse.Create(lightWithId).Field<int>("_ID").Value = (int)lightsID;
                Traverse.Create(lightWithId).Field<LightWithIdManager>("_lighManager").Value = PlatformManager.LightManager;
            }

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