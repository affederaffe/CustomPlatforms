using System.IO;

using UnityEditor;
using UnityEngine;


namespace CustomFloorPlugin 
{
    [CustomEditor(typeof(CustomPlatform))]
    public class CustomPlatformEditor : Editor 
    {
        CustomPlatform customPlat;

        private void OnEnable() 
        {
            customPlat = (CustomPlatform)target;
        }

        public override void OnInspectorGUI() 
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Export")) 
            {
                string path = EditorUtility.SaveFilePanel("Save Platform file", string.Empty, customPlat.platName + ".plat", "plat");

                BuildTargetGroup selectedBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
                BuildTarget activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;

                if (!string.IsNullOrEmpty(path)) 
                {
                    string fileName = Path.GetFileName(path);
                    string folderPath = Path.GetDirectoryName(path);

                    PrefabUtility.SaveAsPrefabAsset(customPlat.gameObject, "Assets/_CustomPlatform.prefab");
                    AssetBundleBuild assetBundleBuild = default;
                    assetBundleBuild.assetBundleName = fileName;
                    assetBundleBuild.assetNames = new string[] { "Assets/_CustomPlatform.prefab" };

                    BuildPipeline.BuildAssetBundles(Application.temporaryCachePath, new AssetBundleBuild[] { assetBundleBuild }, BuildAssetBundleOptions.ForceRebuildAssetBundle, EditorUserBuildSettings.activeBuildTarget);
                    EditorPrefs.SetString("currentBuildingAssetBundlePath", folderPath);
                    EditorUserBuildSettings.SwitchActiveBuildTarget(selectedBuildTargetGroup, activeBuildTarget);

                    AssetDatabase.DeleteAsset("Assets/_CustomPlatform.prefab");

                    if (File.Exists(path))
                        File.Delete(path);

                    // Unity seems to save the file in lower case, which is a problem on Linux, as file systems are case sensitive there
                    File.Move(Path.Combine(Application.temporaryCachePath, fileName.ToLowerInvariant()), path);

                    AssetDatabase.Refresh();

                    EditorUtility.DisplayDialog("Exportation Successful!", "Exportation Successful!", "OK");
                }
                else 
                {
                    EditorUtility.DisplayDialog("Exportation Failed!", "Path is invalid.", "OK");
                }

            }
        }
    }
}
