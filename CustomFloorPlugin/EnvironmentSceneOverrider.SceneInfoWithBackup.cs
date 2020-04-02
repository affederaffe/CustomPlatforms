using IPA.Utilities;


namespace CustomFloorPlugin {


    internal static partial class EnvironmentSceneOverrider {


        /// <summary>
        /// This class wraps a <see cref="global::SceneInfo"/> and saves additional backup information.
        /// </summary>
        private class SceneInfoWithBackup {


            /// <summary>
            /// The sring originally contained in <see cref="SceneName"/>
            /// </summary>
            internal readonly string BackupName;


            /// <summary>
            /// An interface for <see cref="SceneInfo.sceneName"/>, but with added setter
            /// </summary>
            internal string SceneName {
                get {
                    return SceneInfo.sceneName;
                }
                set {
                    SceneInfo.SetField("_sceneName", value);
                }
            }


            /// <summary>
            /// The wrapped <see cref="global::SceneInfo"/>
            /// </summary>
            private readonly SceneInfo SceneInfo;


            /// <summary>
            /// Wraps a <see cref="global::SceneInfo"/> into a <see cref="SceneInfoWithBackup"/><br/>
            /// The resulting object contains additional backup data
            /// </summary>
            /// <param name="sceneInfo">The <see cref="global::SceneInfo"/> to wrap</param>
            internal SceneInfoWithBackup(SceneInfo sceneInfo) {
                BackupName = sceneInfo.sceneName;
                SceneInfo = sceneInfo;
            }
        }
    }
}