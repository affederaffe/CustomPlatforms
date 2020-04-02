using System;

using UnityEngine;

using static CustomFloorPlugin.Utilities.Logging;


namespace CustomFloorPlugin {


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Too old to change")]
    public class SpectrogramAnimationState:MonoBehaviour {
        public AnimationClip animationClip;
        [Header("0: Low Frequency, 63 High Frequency")]
        [Range(0, 63)]
        public int sample;
        [Header("Use the average of all samples, ignoring specified sample")]

        public bool averageAllSamples;

        private Animation animation;
        private BasicSpectrogramData spectrogramData;

        public void SetData(BasicSpectrogramData newData) {
            spectrogramData = newData;
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "I don't have enough information on what COULD be thrown here to edit this catch block")]
        void Update() {
            try {
                if(animationClip != null) {
                    animation = GetComponent<Animation>();
                    if(animation == null) {
                        animation = gameObject.AddComponent<Animation>();
                        PlatformManager.SpawnedComponents.Add(animation);
                        animation.AddClip(animationClip, "clip");
                        animation.Play("clip");
                        animation["clip"].speed = 0;
                    }

                    if(spectrogramData != null) {
                        float average = 0.0f;
                        for(int i = 0; i < 64; i++) {
                            average += spectrogramData.ProcessedSamples[i];
                        }
                        average /= 64.0f;

                        float value = averageAllSamples ? average : spectrogramData.ProcessedSamples[sample];

                        value *= 5f;
                        if(value > 1f) {
                            value = 1f;
                        }
                        value = Mathf.Pow(value, 2f);

                        animation["clip"].time = value * animation["clip"].length;

                    }
                }
            } catch(Exception e) {
                Log(e);
            }
        }
    }
}