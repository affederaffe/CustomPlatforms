using UnityEngine;
using UnityEngine.Serialization;


namespace CustomFloorPlugin
{
    [RequireComponent(typeof(Renderer))]
    public class SpectrogramMaterial : MonoBehaviour
    {
        [Header("The Array property (uniform float arrayName[64])")]
        [FormerlySerializedAs("PropertyName")] public string? propertyName;
        [Header("The global intensity (float valueName)")]
        [FormerlySerializedAs("AveragePropertyName")] public string? averagePropertyName;
    }
}