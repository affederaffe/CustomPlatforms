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

        private BasicSpectrogramData spectrogramData;
        private Renderer renderer;

        [Inject]
        public void Construct([InjectOptional] BasicSpectrogramData newData)
        {
            spectrogramData = newData;
        }

        void INotifyPlatformEnabled.PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void Start()
        {
            renderer = gameObject.GetComponent<Renderer>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void Update()
        {
            if (spectrogramData != null && renderer != null)
            {
                float average = 0.0f;
                for (int i = 0; i < 64; i++)
                {
                    average += spectrogramData.ProcessedSamples[i];
                }
                average /= 64.0f;

                foreach (Material mat in renderer.materials)
                {
                    mat.SetFloatArray(PropertyName, spectrogramData.ProcessedSamples);
                    mat.SetFloat(AveragePropertyName, average);
                }
            }
        }
    }
}