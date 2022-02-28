using System.Linq;

using CustomFloorPlugin.Interfaces;

using UnityEngine;

using Zenject;


// ReSharper disable once CheckNamespace
namespace CustomFloorPlugin
{
    public class SpectrogramAnimationState : MonoBehaviour, INotifyPlatformEnabled
    {
        public AnimationClip? animationClip;
        [Header("0: Low Frequency, 63 High Frequency")]
        [Range(0, 63)]
        public int sample;
        [Header("Use the average of all samples, ignoring specified sample")]
        public bool averageAllSamples;

        private BasicSpectrogramData? _basicSpectrogramData;

        private Animation? _animation;

        [Inject]
        public void Construct([InjectOptional] BasicSpectrogramData basicSpectrogramData)
        {
            _basicSpectrogramData = basicSpectrogramData;
        }

        public void PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            enabled = animationClip is not null && _basicSpectrogramData is not null;
            if (_animation is null)
            {
                _animation = gameObject.AddComponent<Animation>();
                _animation.AddClip(animationClip, "clip");
            }

            _animation.Play("clip");
            _animation["clip"].speed = 0;
        }

        public void Update()
        {
            float value = averageAllSamples ? _basicSpectrogramData!.ProcessedSamples.Average() : _basicSpectrogramData!.ProcessedSamples[sample];
            value *= 5f;
            value = Mathf.Clamp01(value);
            value = Mathf.Pow(value, 2f);
            _animation!["clip"].time = value * _animation!["clip"].length;
        }
    }
}