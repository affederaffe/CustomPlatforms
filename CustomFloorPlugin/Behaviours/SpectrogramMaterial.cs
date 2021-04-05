using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    public class SpectrogramMaterial : MonoBehaviour, INotifyPlatformEnabled
    {
        [Header("The Array property (uniform float arrayName[64])")]
        public string PropertyName;
        [Header("The global intensity (float valueName)")]
        public string AveragePropertyName;

        private BasicSpectrogramData _basicSpectrogramData;

        private Renderer Renderer => _Renderer ??= GetComponent<Renderer>();
        private Renderer _Renderer;

        [Inject]
        public void Construct([InjectOptional] BasicSpectrogramData basicSpectrogramData)
        {
            _basicSpectrogramData = basicSpectrogramData;
        }

        void INotifyPlatformEnabled.PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void Update()
        {
            if (_basicSpectrogramData != null && Renderer != null)
            {
                float average = 0.0f;
                for (int i = 0; i < 64; i++)
                {
                    average += _basicSpectrogramData.ProcessedSamples[i];
                }
                average /= 64.0f;

                foreach (Material mat in Renderer.materials)
                {
                    mat.SetFloatArray(PropertyName, _basicSpectrogramData.ProcessedSamples);
                    mat.SetFloat(AveragePropertyName, average);
                }
            }
        }
    }
}