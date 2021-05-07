using UnityEngine;


namespace CustomFloorPlugin
{
    public class CameraVisibility : MonoBehaviour
    {
        public enum VisibilityMode
        {
            Default,
            HeadsetOnly,
            ThirdPersonOnly
        };

        public VisibilityMode visibilityMode;
        public bool affectChildren;

        /// <summary>
        /// Constant for the ThirdPerson Layer
        /// </summary>
        private const int kOnlyInThirdPerson = 3;

        /// <summary>
        /// Constant for the HeadSet Layer
        /// </summary>
        private const int kOnlyInHeadset = 4;

        private void Awake()
        {
            int layer = gameObject.layer;

            switch (visibilityMode)
            {
                case VisibilityMode.Default:
                    return;
                case VisibilityMode.HeadsetOnly:
                    layer = kOnlyInHeadset;
                    break;
                case VisibilityMode.ThirdPersonOnly:
                    layer = kOnlyInThirdPerson;
                    break;
            }
            if (affectChildren)
            {
                SetChildrenToLayer(gameObject, layer);
            }
            else
            {
                gameObject.layer = layer;
            }

            SetCameraMasks();
        }

        // Recursively set the layer of an object and all children in its hierarchy
        private void SetChildrenToLayer(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in transform)
            {
                SetChildrenToLayer(child.gameObject, layer);
            }
        }

        private static readonly Camera mainCamera = Camera.main!;

        /// <summary>
        /// Sets Main-<see cref="Camera"/>s <see cref="Camera.cullingMask"/>
        /// </summary>
        private static void SetCameraMasks()
        {
            int cullingMask = mainCamera.cullingMask;
            cullingMask &= ~(1 << kOnlyInThirdPerson);
            cullingMask |= 1 << kOnlyInHeadset;
            mainCamera.cullingMask = cullingMask;
        }
    }
}