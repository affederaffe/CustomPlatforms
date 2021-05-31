using UnityEngine;


namespace CustomFloorPlugin
{
    [RequireComponent(typeof(Renderer))]
    public class SpectrogramMaterial : MonoBehaviour
    {
        // ReSharper disable InconsistentNaming
        [Header("The Array property (uniform float arrayName[64])")]
        public string? PropertyName;
        [Header("The global intensity (float valueName)")]
        public string? AveragePropertyName;
        // ReSharper restore InconsistentNaming
    }
}