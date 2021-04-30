using UnityEngine.Events;
using UnityEngine.Serialization;


namespace CustomFloorPlugin 
{
    public class SaberSliceFilter : EventFilterBehaviour 
    {
        public enum SaberType
        {
            RightSaber,
            LeftSaber
        }

        public SaberType saberType;
        [FormerlySerializedAs("SaberSlice")] public UnityEvent? saberSlice;
    }
}