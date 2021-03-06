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
                string path = EditorUtility.SaveFilePanel("Save Platform file", "", customPlat.platName + ".plat", "plat");

                BuildTargetGroup selectedBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
                BuildTarget activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;

                if (!string.IsNullOrEmpty(path)) 
                {
                    string fileName = Path.GetFileName(path);
                    string folderPath = Path.GetDirectoryName(path);

                    PrefabUtility.SaveAsPrefabAsset(customPlat.gameObject, "Assets/_CustomPlatform.prefab");
                    AssetBundleBuild assetBundleBuild = default;
                    assetBundleBuild.assetBundleName = fileName;
                    assetBundleBuild.assetNames = new string[] 
                    {
                        "Assets/_CustomPlatform.prefab"
                    };

                    BuildPipeline.BuildAssetBundles(Application.temporaryCachePath, new AssetBundleBuild[] { assetBundleBuild }, BuildAssetBundleOptions.ForceRebuildAssetBundle, EditorUserBuildSettings.activeBuildTarget);
                    EditorPrefs.SetString("currentBuildingAssetBundlePath", folderPath);
                    EditorUserBuildSettings.SwitchActiveBuildTarget(selectedBuildTargetGroup, activeBuildTarget);

                    AssetDatabase.DeleteAsset("Assets/_CustomPlatform.prefab");

                    if (File.Exists(path))
                        File.Delete(path);

                    File.Move(Application.temporaryCachePath + "/" + fileName, path);

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