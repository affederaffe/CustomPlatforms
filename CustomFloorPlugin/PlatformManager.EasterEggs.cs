using IPA.Utilities;

using UnityEngine;

using static CustomFloorPlugin.Utilities.BeatSaberSearching;
using static CustomFloorPlugin.Utilities.Logging;


namespace CustomFloorPlugin {


    public static partial class PlatformManager {


        /// <summary>
        /// Is a mod without easter eggs a real mod?
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1812:Avoid unistantiated internal classes", Justification = "Instantiated by Unity")]
        private class EasterEggs:MonoBehaviour {


            /// <summary>
            /// Overrides the visibility settings for the heart and assigns it a <see cref="LightWithIdManager"/><br/>
            /// [Called by Unity]<br/>
            /// [Called once per frame!]
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
            private void Update() {
                if(Input.GetKeyDown(KeyCode.Keypad0)) {
                    Log();
                    Heart.SetActive(false);
                    Heart.GetComponent<LightWithId>().SetField("_lightManager", FindLightWithIdManager(GetCurrentEnvironment()));
                    Heart.SetActive(true);
                }
            }
        }
    }
}