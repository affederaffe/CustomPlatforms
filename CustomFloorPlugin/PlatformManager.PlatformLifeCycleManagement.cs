using UnityEngine;
using static CustomFloorPlugin.Utilities.Logging;
using static CustomFloorPlugin.Utilities.BeatSaberSearching;
using static CustomFloorPlugin.Constants;

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
                if(!GetCurrentEnvironment().name.StartsWith("Menu", StrInv)) {
                    platformSpawned = true;
                }
                DestroyCustomObjects();
                Log("Switching to " + AllPlatforms[index].name);
                activePlatform?.gameObject.SetActive(false);
                NotifyPlatform(NotifyType.Disable);

                if(index != 0) {
                    activePlatform = AllPlatforms[index % AllPlatforms.Count];
                    activePlatform.gameObject.SetActive(true);
                    PlatformLoader.AddManagers(activePlatform);
                    NotifyPlatform(NotifyType.Enable);
                    SpawnCustomObjects();
                    EnvironmentArranger.RearrangeEnvironment();
                } else {
                    activePlatform = null;
                }
                Instance.StartCoroutine(HideForPlatformAfterOneFrame(activePlatform ?? AllPlatforms[0]));
            }
            private static void NotifyPlatform(NotifyType type) {
                NotifyOnEnableOrDisable[] things = activePlatform?.gameObject?.GetComponentsInChildren<NotifyOnEnableOrDisable>(true);
                if(things != null) {
                    foreach(NotifyOnEnableOrDisable thing in things) {
                        if(type == NotifyType.Disable) {
                            thing.PlatformDisabled();
                        } else {
                            Log("Calling Enables, expecting more than one entry in the queue after this...");
                            thing.PlatformEnabled();
                        }
                    } 
                }
            }

            private static void SpawnCustomObjects() {
                Log("Members in SpawnQueue: " + SpawnQueue.GetInvocationList().Length);
                Log(SpawnQueue.GetInvocationList()[0].Method.DeclaringType.Name);
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
            private enum NotifyType {
                Enable = 0,
                Disable = 1
            }
        }
    }
}
