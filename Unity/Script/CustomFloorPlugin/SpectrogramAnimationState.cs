using UnityEngine;


namespace CustomFloorPlugin 
{
    public class SpectrogramAnimationState : MonoBehaviour 
    {
        public AnimationClip? animationClip;

        [Header("0: Low Frequency, 63 High Frequency")]
        [Range(0f, 63f)]
        public int sample;

        [Header("Use the average of all samples, ignoring specified sample")]
        public bool averageAllSamples;
    }
}