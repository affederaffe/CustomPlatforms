using System.Collections.Generic;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    /// <summary>
    /// Instantiable manager class for <see cref="SpectrogramMaterial"/>s that handles updating their <see cref="BasicSpectrogramData"/> references
    /// </summary>
    internal class SpectrogramMaterialManager : MonoBehaviour
    {
        [InjectOptional]
        private readonly BasicSpectrogramData _basicSpectrogramData;

        /// <summary>
        /// <see cref="List{T}"/> of all known <see cref="SpectrogramMaterial"/>s under the gameObject this script is attached to.
        /// </summary>
        private List<SpectrogramMaterial> spectrogramMaterials;

        /// <summary>
        /// Updates the Provider for Spectogram Data when this object becomes active<br/>
        /// [Unity calls this when the <see cref="MonoBehaviour"/> becomes active in the hierachy]
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnEnable()
        {
            UpdateSpectrogramDataProvider();
        }

        /// <summary>
        /// Updates the <see cref="List{T}"/> of known <see cref="SpectrogramMaterial"/>s
        /// </summary>
        internal void UpdateMaterials(GameObject go)
        {
            spectrogramMaterials = new List<SpectrogramMaterial>();

            foreach (SpectrogramMaterial spec in go.GetComponentsInChildren<SpectrogramMaterial>())
            {
                spectrogramMaterials.Add(spec);
            }
        }

        /// <summary>
        /// Passes <see cref="BasicSpectrogramData"/> on to all <see cref="SpectrogramMaterial"/>s<br/>
        /// </summary>
        internal void UpdateSpectrogramDataProvider()
        {
            if (_basicSpectrogramData != null)
            {
                foreach (SpectrogramMaterial specMat in spectrogramMaterials)
                {
                    specMat.SetData(_basicSpectrogramData);
                }
            }
        }
    }
}