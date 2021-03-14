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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
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

        /// <summary>
        /// Sets Main-<see cref="Camera"/>s <see cref="Camera.cullingMask"/>
        /// </summary>
        private void SetCameraMasks()
        {
            Camera.main.cullingMask &= ~(1 << kOnlyInThirdPerson);
            Camera.main.cullingMask |= 1 << kOnlyInHeadset;
        }

        // Recursively set the layer of an object and all children in its hierarchy
        private void SetChildrenToLayer(GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in transform)
            {
                SetChildrenToLayer(child.gameObject, layer);
            }
        }
    }
}