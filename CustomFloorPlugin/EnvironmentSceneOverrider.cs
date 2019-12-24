using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BS_Utils.Utilities;

namespace CustomFloorPlugin
{
    public class EnvironmentSceneOverrider
    {
        public enum EnvOverrideMode { Off, Default, Nice, BigMirror, Triangle, KDA, Monstercat };

        public static EnvOverrideMode overrideMode = EnvOverrideMode.Off;
        private static Dictionary<EnvOverrideMode, EnvSceneInfo> envInfos;
        
        public static void GetSceneInfos()
        {
            var sceneInfos = Resources.FindObjectsOfTypeAll<SceneInfo>();
            envInfos = new Dictionary<EnvOverrideMode, EnvSceneInfo>();
            envInfos.Add(EnvOverrideMode.Default, new EnvSceneInfo("Default", sceneInfos.First(x => x.name == "DefaultEnvironmentSceneInfo")));
            envInfos.Add(EnvOverrideMode.Nice, new EnvSceneInfo("Nice", sceneInfos.First(x => x.name == "NiceEnvironmentSceneInfo")));
            envInfos.Add(EnvOverrideMode.BigMirror, new EnvSceneInfo("Big Mirror", sceneInfos.First(x => x.name == "BigMirrorEnvironmentSceneInfo")));
            envInfos.Add(EnvOverrideMode.Triangle, new EnvSceneInfo("Triangle", sceneInfos.First(x => x.name == "TriangleEnvironmentSceneInfo")));
            envInfos.Add(EnvOverrideMode.KDA, new EnvSceneInfo("KDA", sceneInfos.First(x => x.name == "KDAEnvironmentSceneInfo")));
            envInfos.Add(EnvOverrideMode.Monstercat, new EnvSceneInfo("Monstercat", sceneInfos.First(x => x.name == "MonstercatEnvironmentSceneInfo")));
        }

        public class EnvSceneInfo
        {
            public string sceneName { get; private set; }
            public string displayName { get; private set; }
            public SceneInfo sceneInfo { get; private set; }

            public EnvSceneInfo(string displayName, SceneInfo sceneInfo)
            {
                this.displayName = displayName;
                sceneName = sceneInfo.sceneName;
                this.sceneInfo = sceneInfo;
            }

            public void OverrideScene(EnvSceneInfo info)
            {
                sceneInfo.SetPrivateField("_sceneName", info.sceneName);
            }

            public void Revert()
            {
                sceneInfo.SetPrivateField("_sceneName", sceneName);
            }
        };

        public static void OverrideEnvironmentScene()
        {
            foreach (EnvSceneInfo info in envInfos.Values)
            {
                if (overrideMode == EnvOverrideMode.Off)
                {
                    info.Revert();
                }
                else
                {
                    info.OverrideScene(envInfos[overrideMode]);
                }
            }
        }

        public static List<object> OverrideModes()
        {
            List<object> ovs = new List<object>();
            ovs.Add(EnvOverrideMode.Off);
            ovs.Add(EnvOverrideMode.Default);
            ovs.Add(EnvOverrideMode.Nice);
            ovs.Add(EnvOverrideMode.BigMirror);
            ovs.Add(EnvOverrideMode.Triangle);
            ovs.Add(EnvOverrideMode.KDA);
            ovs.Add(EnvOverrideMode.Monstercat);
            return ovs;
        }

        public static string Name(EnvOverrideMode mode)
        {
            if (mode == EnvOverrideMode.Off) return "Off";
            return envInfos[mode].displayName;
        }
    }
}
