using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;


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
        [FormerlySerializedAs("SaberSlice")] public UnityEvent? saberSlice;
    }
}