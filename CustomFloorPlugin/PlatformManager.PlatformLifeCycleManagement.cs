using UnityEngine;
using static CustomFloorPlugin.Utilities.Logging;
using static CustomFloorPlugin.Utilities.BeatSaberSearching;

namespace CustomFloorPlugin {

    public sealed partial class PlatformManager {
        private static class PlatformLifeCycleManagement {
            internal static void InternalChangeToPlatform() {
                if(kyleBuffer.HasValue) {
                    InternalChangeToPlatform(kyleBuffer.Value);
                    kyleBuffer = null;
                } else {
                    InternalChangeToPlatform(CurrentPlatformIndex);
                }
            }
            internal static void InternalChangeToPlatform(int index) {
                if(!GetCurrentEnvironment().name.StartsWith("Menu", Constants.StrInv)) {
                    platformSpawned = true;
                }
                DestroyCustomObjects();
                Log("Switching to " + AllPlatforms[index].name);
                activePlatform?.gameObject.SetActive(false);
                if(index != 0) {
                    activePlatform = AllPlatforms[index % AllPlatforms.Count];
                    activePlatform.gameObject.SetActive(true);
                    PlatformLoader.AddManagers(activePlatform);
                    SpawnCustomObjects();
                    EnvironmentArranger.RearrangeEnvironment();
                } else {
                    activePlatform = null;
                }
                Instance.StartCoroutine(HideForPlatformAfterOneFrame(activePlatform ?? AllPlatforms[0]));

            }


            private static void SpawnCustomObjects() {
                SpawnQueue(FindLightWithIdManager());
            }
            private static void DestroyCustomObjects() {
                while(SpawnedObjects.Count != 0) {
                    GameObject gameObject = SpawnedObjects[0];
                    SpawnedObjects.Remove(gameObject);
                    Destroy(gameObject);
                }
                while(SpawnedComponents.Count != 0) {
                    Component component = SpawnedComponents[0];
                    SpawnedComponents.Remove(component);
                    Destroy(component);
                }
            }
        }
    }
}
