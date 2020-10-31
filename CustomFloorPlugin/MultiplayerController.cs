using CustomFloorPlugin.Utilities;
using UnityEngine;

using static CustomFloorPlugin.Utilities.BeatSaberSearching;

namespace CustomFloorPlugin
{
    public static partial class PlatformManager {


        /// <summary>
        /// This Class only exists to check if the Player is currently spectating. 
        /// Why did I make a Class for this? I don't know.
        /// </summary
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1812:Avoid unistantiated internal classes", Justification = "Instantiated by Unity")]
        private class MultiplayerController : MonoBehaviour {


            /// <summary>
            /// Checks if the Player is spectating by trying to find an element of the spectator menu<br/>
            /// [Called by Unity]<br/>
            /// [Called once per frame!]
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
            private void Update() {
                if (IsSpectating() && PlatformManager.CurrentPlatformIndex != 0) {
                    PlatformManager.ChangeToPlatform(0);
                }
            }

            internal static bool IsSpectating() {
                if (GetCurrentEnvironment().name.StartsWith("Multiplayer", GlobalCollection.STR_INV) && GameObject.Find("Environment/GrandstandSpectatingSpot/MultiplayerLocalInactivePlayerController(Clone)/MultiplayerLocalInactivePlayerInGameMenuViewController/MenuWrapper/Canvas/MainBar") != null) {
                    return true;
                } else {
                    return false;
                }
            }
        }
    }
}
