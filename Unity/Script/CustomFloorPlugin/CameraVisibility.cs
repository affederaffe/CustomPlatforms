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
    }
}