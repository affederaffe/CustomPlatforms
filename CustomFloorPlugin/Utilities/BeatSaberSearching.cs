using CustomFloorPlugin.Exceptions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomFloorPlugin.Utilities {
    internal static class BeatSaberSearching {
        /// <exception cref="EnvironmentSceneNotFoundException"></exception>
        internal static Scene GetCurrentEnvironment() {
            Scene scene = new Scene();
            Scene environmentScene = scene;
            for(int i = 0; i < SceneManager.sceneCount; i++) {
                scene = SceneManager.GetSceneAt(i);
                if(scene.name.EndsWith("Environment", Constants.StrInv)) {
                    if(!environmentScene.IsValid() || environmentScene.name.StartsWith("Menu", Constants.StrInv))
                        environmentScene = scene;
                }
            }
            if(environmentScene.IsValid()) {
                return environmentScene;
            }
            throw new EnvironmentSceneNotFoundException();
        }
        /// <exception cref="ManagerNotFoundException"></exception>
        internal static LightWithIdManager FindLightWithIdManager() {
            Scene? scene;
            try {
                scene = GetCurrentEnvironment();
            } catch(EnvironmentSceneNotFoundException e) {
                throw new ManagerNotFoundException(e);
            }

            LightWithIdManager manager = null;
            void RecursiveFindManager(GameObject directParent) {
                for(int i = 0; i < directParent.transform.childCount; i++) {
                    GameObject child = directParent.transform.GetChild(i).gameObject;
                    if(child.GetComponent<LightWithIdManager>() != null) {
                        manager = child.GetComponent<LightWithIdManager>();
                    }
                    if(child.transform.childCount != 0) {
                        RecursiveFindManager(child);
                    }
                }
            }
            GameObject[] roots = scene?.GetRootGameObjects();
            foreach(GameObject root in roots) {
                RecursiveFindManager(root);
            }
            if(manager != null) {
                return manager;
            } else {
                throw new ManagerNotFoundException();
            }
        }
    }
}
