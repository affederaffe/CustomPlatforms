using UnityEngine.Events;


namespace CustomFloorPlugin 
{
    public class SaberSliceFilter : EventFilterBehaviour 
    {
        public enum SaberType
        {
            RightSaber,
            LeftSaber
        }

        public SaberType saberType = SaberType.LeftSaber;
        public UnityEvent SaberSlice;
    }
}