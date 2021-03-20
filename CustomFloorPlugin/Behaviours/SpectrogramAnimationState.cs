using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    public class SpectrogramAnimationState : MonoBehaviour, INotifyPlatformEnabled
    {
        public AnimationClip animationClip;
        [Header("0: Low Frequency, 63 High Frequency")]
        [Range(0, 63)]
        public int sample;
        [Header("Use the average of all samples, ignoring specified sample")]
        public bool averageAllSamples;

        private PlatformManager _platformManager;
        private BasicSpectrogramData _basicSpectrogramData;
        private Animation _animation;

        [Inject]
        public void Construct(PlatformManager platformManager, [InjectOptional] BasicSpectrogramData basicSpectrogramData)
        {
            _platformManager = platformManager;
            _basicSpectrogramData = basicSpectrogramData;
        }

        void INotifyPlatformEnabled.PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void Update()
        {
            if (animationClip != null)
            {
                _animation = GetComponent<Animation>();
                if (_animation == null)
                {
                    _animation = gameObject.AddComponent<Animation>();
                    _platformManager.spawnedObjects.Add(_animation);
                    _animation.AddClip(animationClip, "clip");
                    _animation.Play("clip");
                    _animation["clip"].speed = 0;
                }

                if (_basicSpectrogramData != null)
                {
                    float average = 0.0f;
                    for (int i = 0; i < 64; i++)
                    {
                        average += _basicSpectrogramData.ProcessedSamples[i];
                    }
                    average /= 64.0f;

                    float value = averageAllSamples ? average : _basicSpectrogramData.ProcessedSamples[sample];

                    value *= 5f;
                    if (value > 1f)
                    {
                        value = 1f;
                    }
                    value = Mathf.Pow(value, 2f);

                    _animation["clip"].time = value * _animation["clip"].length;

                }
            }
        }
    }
}