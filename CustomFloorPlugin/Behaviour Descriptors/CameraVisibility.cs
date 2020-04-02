using UnityEngine;


namespace CustomFloorPlugin {


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Too old to change")]
    public class CameraVisibility:MonoBehaviour {
        public enum VisibilityMode { Default, HeadsetOnly, ThirdPersonOnly };

        public VisibilityMode visibilityMode;
        public bool affectChildren;

        // --------------------------- 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void Awake() {
            int layer = gameObject.layer;

            switch(visibilityMode) {
                case VisibilityMode.Default:
                    return;
                case VisibilityMode.HeadsetOnly:
                    layer = CameraVisibilityManager.OnlyInHeadset;
                    break;
                case VisibilityMode.ThirdPersonOnly:
                    layer = CameraVisibilityManager.OnlyInThirdPerson;
                    break;
            }
            if(affectChildren) {
                SetChildrenToLayer(gameObject, layer);
            } else {
                gameObject.layer = layer;
            }

            CameraVisibilityManager.SetCameraMasks();
        }

        // Recursively set the layer of an object and all children in its hierarchy
        private void SetChildrenToLayer(GameObject gameObject, int layer) {
            gameObject.layer = layer;
            foreach(Transform child in transform) {
                SetChildrenToLayer(child.gameObject, layer);
            }
        }
    }
}