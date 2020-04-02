using UnityEngine;


namespace CustomFloorPlugin {


    /// <summary>
    /// Utility class, provides functionality for <see cref="CameraVisibility"/>s
    /// </summary>
    internal static class CameraVisibilityManager {


        /// <summary>
        /// Constant for the ThirdPerson Layer
        /// </summary>
        internal const int OnlyInThirdPerson = 3;


        /// <summary>
        /// Constant for the HeadSet Layer
        /// </summary>
        internal const int OnlyInHeadset = 4;


        /// <summary>
        /// Sets Main-<see cref="Camera"/>s <see cref="Camera.cullingMask"/>
        /// </summary>
        internal static void SetCameraMasks() {
            Camera.main.cullingMask &= ~(1 << OnlyInThirdPerson);
            Camera.main.cullingMask |= 1 << OnlyInHeadset;
        }
    }
}