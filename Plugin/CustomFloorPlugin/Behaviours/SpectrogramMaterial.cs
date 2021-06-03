using CustomFloorPlugin.Interfaces;

using UnityEngine;

using Zenject;


// ReSharper disable once CheckNamespace
namespace CustomFloorPlugin
{
    [RequireComponent(typeof(Renderer))]
    public class SpectrogramMaterial : MonoBehaviour, INotifyPlatformEnabled
    {
        // ReSharper disable InconsistentNaming
        [Header("The Array property (uniform float arrayName[64])")]
        public string? PropertyName;
        [Header("The global intensity (float valueName)")]
        public string? AveragePropertyName;
        // ReSharper restore InconsistentNaming

        private BasicSpectrogramData? _basicSpectrogramData;

        private Renderer Renderer => _renderer ??= GetComponent<Renderer>();
        private Renderer? _renderer;

        [Inject]
        public void Construct([InjectOptional] BasicSpectrogramData basicSpectrogramData)
        {
            _basicSpectrogramData = basicSpectrogramData;
        }

        public void PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            enabled = _basicSpectrogramData is not null;
        }

        private void Update()
        {
            float average = 0f;
            for (int i = 0; i < 64; i++)
                average += _basicSpectrogramData!.ProcessedSamples[i];
            average /= 64.0f;

            foreach (Material mat in Renderer.materials)
            {
                mat.SetFloatArray(PropertyName, _basicSpectrogramData!.ProcessedSamples);
                mat.SetFloat(AveragePropertyName, average);
            }
        }
    }
}