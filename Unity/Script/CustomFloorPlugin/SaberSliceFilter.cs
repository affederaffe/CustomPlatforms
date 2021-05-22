using UnityEngine;
using UnityEngine.Events;


namespace CustomFloorPlugin
{
    public class SaberSliceFilter : MonoBehaviour
    {
        public enum SaberType
        {
            LeftSaber,
            RightSaber
        }

        public SaberType saberType;
        // ReSharper disable once InconsistentNaming
        public UnityEvent? SaberSlice;
    }
}