using UnityEngine;


namespace CustomFloorPlugin 
{
    public class SpectrogramMaterial : MonoBehaviour 
    {
        [Header("The Array property (uniform float arrayName[64])")]
        public string PropertyName;

        [Header("The global intensity (float valueName)")]
        public string AveragePropertyName;
    }
}