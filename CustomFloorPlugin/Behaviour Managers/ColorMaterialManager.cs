using System.Collections.Generic;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    internal class ColorMaterialManager : MonoBehaviour
    {
        [InjectOptional]
        private readonly ColorManager _colorManager;

        private List<ColorMaterial> colorMaterials;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnEnable()
        {
            UpdateColorManagerProvider();
        }

        internal void UpdateMaterials(GameObject go)
        {
            colorMaterials = new List<ColorMaterial>();

            foreach (ColorMaterial col in go.GetComponentsInChildren<ColorMaterial>())
            {
                colorMaterials.Add(col);
            }
        }

        internal void UpdateColorManagerProvider()
        {
            if (_colorManager != null)
            {
                foreach (ColorMaterial colMat in colorMaterials)
                {
                    colMat.SetColorManager(_colorManager);
                }
            }
        }
    }
}
