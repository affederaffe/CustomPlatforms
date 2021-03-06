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

        private void Update()
        {
            float average = 0f;
            for (int i = 0; i < 64; i++)
                average += _basicSpectrogramData!.ProcessedSamples[i];
            average /= 64.0f;

            float value = averageAllSamples ? average : _basicSpectrogramData!.ProcessedSamples[sample];

            value *= 5f;
            if (value > 1f)
                value = 1f;

            value = Mathf.Pow(value, 2f);

            _animation!["clip"].time = value * _animation!["clip"].length;
        }
    }
}