using UnityEngine;


namespace CustomFloorPlugin {
    public static partial class PlatformManager {


        /// <summary>
        /// This Class only exists to check if the Player is currently spectating. 
        /// Why did I make a Class for this? I don't know.
        /// </summary
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1812:Avoid unistantiated internal classes", Justification = "Instantiated by Unity")]
        private class MultiplayerController : MonoBehaviour {
            internal static bool disabledPlatformInMultiplayer = false;
            private const string specSpot = "Environment/GrandstandSpectatingSpot/MultiplayerLocalInactivePlayerController(Clone)/MultiplayerLocalInactivePlayerInGameMenuViewController/MenuWrapper/Canvas";
            private const string specSpotDuel1 = "Environment/GrandstandSpectatingSpot-Duel1/MultiplayerLocalInactivePlayerController(Clone)/MultiplayerLocalInactivePlayerInGameMenuViewController/MenuWrapper/Canvas";
            private const string specSpotDuel2 = "Environment/GrandstandSpectatingSpot-Duel2/MultiplayerLocalInactivePlayerController(Clone)/MultiplayerLocalInactivePlayerInGameMenuViewController/MenuWrapper/Canvas";

            /// <summary>
            /// Checks if the Player is spectating by trying to find an element of the spectator menu<br/>
            /// [Called by Unity]<br/>
            /// [Called once per frame!]
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
            private void Update() {
                if (IsSpectating() && !disabledPlatformInMultiplayer) {
                    PlatformManager.ChangeToPlatform(0);
                    disabledPlatformInMultiplayer = true;
                }
            }

            internal static bool IsSpectating() {
                if (GameObject.Find(specSpot) != null || GameObject.Find(specSpotDuel1) != null || GameObject.Find(specSpotDuel2)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
    }
}