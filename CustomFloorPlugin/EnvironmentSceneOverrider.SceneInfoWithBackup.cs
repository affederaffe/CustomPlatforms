using CustomFloorPlugin.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomFloorPlugin {
    internal static partial class EnvironmentSceneOverrider {
        private class SceneInfoWithBackup {
            internal readonly string BackupName;
            internal string SceneName {
                get {
                    return SceneInfo.sceneName;
                }
                set {
                    SceneInfo.SetPrivateField("_sceneName", value);
                }
            }
            private readonly SceneInfo SceneInfo;

            internal SceneInfoWithBackup(SceneInfo sceneInfo) {
                BackupName = sceneInfo.sceneName;
                SceneInfo = sceneInfo;
            }

            internal static List<SceneInfoWithBackup> GetAllEnvs() {
                List<SceneInfo> rawList = Resources.FindObjectsOfTypeAll<SceneInfo>().Where(
                    x => x.name.EndsWith("EnvironmentSceneInfo", Constants.StrInv)
                    && !(x.name.StartsWith("Menu", Constants.StrInv) || x.name.StartsWith("Tutorial", Constants.StrInv))
                    ).ToList();
                List<SceneInfoWithBackup> list = new List<SceneInfoWithBackup>();
                foreach(SceneInfo sceneInfo in rawList) {
                    list.Add(new SceneInfoWithBackup(sceneInfo));
                }
                return list;
            }
        }
        private static SceneInfoWithBackup EnvWithName(string name) {
            return allSceneInfos.First(x => x.SceneName.StartsWith(name, Constants.StrInv));
        }
    }
}
