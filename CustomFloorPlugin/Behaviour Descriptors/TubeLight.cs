using CustomFloorPlugin.Util;
using CustomUI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Harmony;
using UnityEngine.SceneManagement;

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
        public static List<GameObject> SpawnedObjects = new List<GameObject>();

        private void OnDrawGizmos() {
            Gizmos.color = color;
            Gizmos.matrix = transform.localToWorldMatrix;
            Vector3 cubeCenter = Vector3.up * (0.5f - center) * length;
            Gizmos.DrawCube(cubeCenter, new Vector3(2 * width, length, 2 * width));
        }

        // ----------------
        //private GameObject tblwid;//Watch out! Incorrect Naming
        //public static GameObject tblwidpf = null;//Watch out! Incorrect Naming
        private bool IsMesh = false;
        public static TubeBloomPrePassLight prefab = null;

        private TubeBloomPrePassLight tubeBloomLight;

        private void Awake() {
            if(prefab == null) {
                Debug.Log("Setting Prefab!");
                prefab = GameObject.Find("Wrapper/NearBuildingLeft/Neon").GetComponent<TubeBloomPrePassLight>();
            }
        }
        
        private void GameAwake() {
            int i = 0;
            //if(tblwidpf == null) {
            //    tblwidpf = GameObject.Find("Wrapper/NearBuildingLeft/Neon");
            //}
            //tblwid = Instantiate(tblwidpf);

            //Type totbl = typeof(TubeBloomPrePassLight);
            //Type tombl = typeof(MeshBloomPrePassLight);
            //Traverse<TubeBloomPrePassLight> tblwid_tbl = Traverse.Create(tblwid.GetComponent<TubeBloomPrePassLightWithId>()).Field<TubeBloomPrePassLight>("_tubeBloomPrePassLight");
            //Traverse<TubeBloomPrePassLight> tblwidpf_tbl = Traverse.Create(tblwidpf.GetComponent<TubeBloomPrePassLightWithId>()).Field<TubeBloomPrePassLight>("_tubeBloomPrePassLight");

            //tblwid_tbl.Value = ReflectionUtil.CopyComponent(tblwidpf_tbl.Value, totbl, totbl, tblwid).GetComponent<TubeBloomPrePassLight>();
            //TubeLight[] localDescriptors = GetComponentsInChildren<TubeLight>(true);//??
            //if(localDescriptors == null) return;//??
            //tblwid.transform.parent = transform.parent;
            //tblwid.transform.localRotation = transform.localRotation;
            //tblwid.transform.localPosition = transform.localPosition;
            //tblwid.transform.localScale = transform.localScale;
            //if(GetComponent<MeshFilter>().mesh.vertexCount == 0) {
            //    GetComponent<MeshRenderer>().enabled = false;
            //} else {
            //    // swap for MeshBloomPrePassLight
            //    IsMesh = true;
            //    tblwid.SetActive(false);
            //    MeshBloomPrePassLight mbl = ReflectionUtil.CopyComponent(tblwid_tbl.Value, totbl, tombl, tblwid) as MeshBloomPrePassLight;
            //    mbl.Init(GetComponent<Renderer>());
            //    tblwid.SetActive(true);
            //    DestroyImmediate(tblwid_tbl.Value.gameObject);
            //    tblwid_tbl.Value = mbl;
            //}
            //tblwid.SetActive(false);
            // ----------------
            if(prefab == null) {
                Debug.Log("Setting Prefab!");
                prefab = GameObject.Find("Wrapper/NearBuildingLeft/Neon").GetComponent<TubeBloomPrePassLight>();
            }
            TubeLight tl = this;
            
            tubeBloomLight = Instantiate(prefab);
            
            tubeBloomLight.transform.SetParent(tl.transform);
            
            SpawnedObjects.Add(tubeBloomLight.gameObject);

            tubeBloomLight.transform.localRotation = Quaternion.identity;
            tubeBloomLight.transform.localPosition = Vector3.zero;
            tubeBloomLight.transform.localScale = new Vector3(1 / tl.transform.lossyScale.x, 1 / tl.transform.lossyScale.y, 1 / tl.transform.lossyScale.z);


            if(tl.GetComponent<MeshFilter>().mesh.vertexCount == 0) {
                tl.GetComponent<MeshRenderer>().enabled = false;
                //Traverse.Create(tubeBloomLight).Field("_registeredWithLightType")
            } else {
                // swap for MeshBloomPrePassLight
                IsMesh = true;
                tubeBloomLight.gameObject.SetActive(false);
                MeshBloomPrePassLight meshbloom = ReflectionUtil.CopyComponent(tubeBloomLight, typeof(TubeBloomPrePassLight), typeof(MeshBloomPrePassLight), tubeBloomLight.gameObject) as MeshBloomPrePassLight;
                meshbloom.Init(tl.GetComponent<Renderer>());
                tubeBloomLight.gameObject.SetActive(true);
                DestroyImmediate(tubeBloomLight);
                tubeBloomLight = meshbloom;
            }
            tubeBloomLight.gameObject.SetActive(false);
            
            var lightWithId = tubeBloomLight.GetComponent<LightWithId>();
            if(lightWithId) {

                lightWithId.SetPrivateField("_tubeBloomPrePassLight", tubeBloomLight);

                Traverse.Create(lightWithId).Field<int>("_ID").Value = (int)tl.lightsID;

                Traverse.Create(lightWithId).Field<LightWithIdManager>("_lighManager").Value = Plugin.LightWithId_Start_Patch.GameLightManager;
            }
            
            tubeBloomLight.SetPrivateField("_width", tl.width * 2);
            
            tubeBloomLight.SetPrivateField("_length", tl.length);
            
            tubeBloomLight.SetPrivateField("_center", tl.center);
            
            tubeBloomLight.SetPrivateField("_transform", tubeBloomLight.transform);
            
            tubeBloomLight.SetPrivateField("_maxAlpha", 0.1f);
            
            var parabox = tubeBloomLight.GetComponentInChildren<ParametricBoxController>();
            
            tubeBloomLight.SetPrivateField("_parametricBoxController", parabox);
            
            var parasprite = tubeBloomLight.GetComponentInChildren<Parametric3SliceSpriteController>();
            
            tubeBloomLight.SetPrivateField("_dynamic3SliceSprite", parasprite);
            
            parasprite.Init();
            
            parasprite.GetComponent<MeshRenderer>().enabled = false;
            
            tubeBloomLight.color = color * 0.9f;
            
            tubeBloomLight.Refresh();
            
            tubeBloomLight.gameObject.SetActive(true);
            

        }
        //private void dAwake() {
        //    var prefab = Resources.FindObjectsOfTypeAll<TubeBloomPrePassLight>().First(x => x.name == "Neon");

        //    TubeLight[] localDescriptors = GetComponentsInChildren<TubeLight>(true);

        //    if(localDescriptors == null) return;

        //    TubeLight tl = this;

        //    tubeBloomLight = Instantiate(prefab);
        //    tubeBloomLight.transform.SetParent(tl.transform);
        //    tubeBloomLight.transform.localRotation = Quaternion.identity;
        //    tubeBloomLight.transform.localPosition = Vector3.zero;
        //    tubeBloomLight.transform.localScale = new Vector3(1 / tl.transform.lossyScale.x, 1 / tl.transform.lossyScale.y, 1 / tl.transform.lossyScale.z);

        //    if(tl.GetComponent<MeshFilter>().mesh.vertexCount == 0) {
        //        tl.GetComponent<MeshRenderer>().enabled = false;
        //    } else {
        //        // swap for MeshBloomPrePassLight
        //        tubeBloomLight.gameObject.SetActive(false);
        //        MeshBloomPrePassLight meshbloom = ReflectionUtil.CopyComponent(tubeBloomLight, typeof(TubeBloomPrePassLight), typeof(MeshBloomPrePassLight), tubeBloomLight.gameObject) as MeshBloomPrePassLight;
        //        meshbloom.Init(tl.GetComponent<Renderer>());
        //        tubeBloomLight.gameObject.SetActive(true);
        //        DestroyImmediate(tubeBloomLight);
        //        tubeBloomLight = meshbloom;
        //    }
        //    tubeBloomLight.gameObject.SetActive(false);

        //    var lightWithId = tubeBloomLight.GetComponent<LightWithId>();
        //    if(lightWithId) {
        //        lightWithId.SetPrivateField("_tubeBloomPrePassLight", tubeBloomLight);
        //        var runtimeFields = typeof(LightWithId).GetTypeInfo().GetRuntimeFields();
        //        runtimeFields.First(f => f.Name == "_ID").SetValue(lightWithId, (int)tl.lightsID);
        //        //var lightManagers = Resources.FindObjectsOfTypeAll<LightWithIdManager>().FirstOrDefault();
        //        //lightManager = lightManagers.FirstOrDefault();

        //        runtimeFields.First(f => f.Name == "_lighManager").SetValue(lightWithId, Plugin.LightWithId_Start_Patch.GameLightManager);
        //    }

        //    tubeBloomLight.SetPrivateField("_width", tl.width * 2);
        //    tubeBloomLight.SetPrivateField("_length", tl.length);
        //    tubeBloomLight.SetPrivateField("_center", tl.center);
        //    tubeBloomLight.SetPrivateField("_transform", tubeBloomLight.transform);
        //    tubeBloomLight.SetPrivateField("_maxAlpha", 0.1f);
        //    var parabox = tubeBloomLight.GetComponentInChildren<ParametricBoxController>();
        //    tubeBloomLight.SetPrivateField("_parametricBoxController", parabox);
        //    var parasprite = tubeBloomLight.GetComponentInChildren<Parametric3SliceSpriteController>();
        //    tubeBloomLight.SetPrivateField("_dynamic3SliceSprite", parasprite);
        //    parasprite.Init();
        //    parasprite.GetComponent<MeshRenderer>().enabled = false;
        //    tubeBloomLight.color = color * 0.9f;
        //    //tubeBloomLight.Refresh();
        //    tubeBloomLight.gameObject.SetActive(true);
        //    //TubeLightManager.UpdateEventTubeLightList();
        //}

        private void GameSceneLoaded() {
            //GameAwake();
            //Debug.Log("Set ref to: " + Plugin.LightWithId_Start_Patch.GameLightManager.name);
            //var lightWithId = tubeBloomLight.GetComponent<LightWithId>();
            //if(lightWithId) {
            //    Traverse.Create(lightWithId).Field<LightWithIdManager>("_lighManager").Value = Plugin.LightWithId_Start_Patch.GameLightManager;
            //}
            //tubeBloomLight.gameObject.SetActive(true);
            //if(IsMesh) {
            //    Traverse i_light = Traverse.Create((MeshBloomPrePassLight)tubeBloomLight);
            //    Traverse<bool> _isRegistered = i_light.Field<bool>("_isRegistered");
            //    Debug.Log("name: " + tubeBloomLight.gameObject.name);
            //    Debug.Log("parent name: " + tubeBloomLight.transform.parent.gameObject.name.ToString());//Con.cat.King.Lel
            //    Debug.Log("scene name: " + tubeBloomLight.gameObject.scene.name);
            //    Debug.Log("before: _isRegistered: " + _isRegistered.Value.ToString());
            //    if(_isRegistered.Value == false) {
            //        //i_light.Method("RegisterLight");
            //        Debug.Log("after: _isRegistered: " + _isRegistered.Value.ToString());
            //    }
            //}

            //tubeBloomLight.Refresh();
            //StartCoroutine(KerFuffel(tubeBloomLight));
        }
        IEnumerator<WaitForEndOfFrame> KerFuffel(TubeBloomPrePassLight tubeBloomLight) {
            yield return new WaitForEndOfFrame();
            tubeBloomLight.color = Color.black.ColorWithAlpha(0);
            tubeBloomLight.Refresh();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        IEnumerator<WaitForEndOfFrame> ToggleBlooms(GameObject gameObject) {
            yield return new WaitForEndOfFrame();

            void RecursiveToggleBloomPrePass(GameObject directParent) {
                for(int i = 0; i < directParent.transform.childCount; i++) {
                    GameObject child = directParent.transform.GetChild(i).gameObject;
                    if(child.GetComponent<BloomPrePassLight>() != null) {
                        child.transform.parent = null;
                        child.SetActive(!child.activeSelf);
                        child.SetActive(!child.activeSelf);
                        child.transform.parent = directParent.transform;
                    }
                    if(child.transform.childCount != 0) {
                        RecursiveToggleBloomPrePass(child);
                    }
                }
            }

            RecursiveToggleBloomPrePass(gameObject);
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////
        


        private void OnEnable() {

            BSEvents.gameSceneLoaded += GameSceneLoaded;
            
            StartCoroutine(ToggleBlooms(gameObject));
            PlatformManager.SpawnQueue += GameAwake;
        }
        

        private void OnDisable() {
            BSEvents.gameSceneLoaded -= GameSceneLoaded;
            PlatformManager.SpawnQueue -= GameAwake;
        }

        private void SetColorToDefault() {
            tubeBloomLight.color = color * 0.9f;
            tubeBloomLight.Refresh();
        }
    }
}