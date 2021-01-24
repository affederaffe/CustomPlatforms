using System.Collections.Generic;

using IPA.Utilities;

using UnityEngine;

using Zenject;


namespace CustomFloorPlugin {


    /// <summary>
    /// Instantiable manager class for <see cref="Spectrogram"/>s that handles creation of <see cref="SpectrogramColumns"/> and updating their <see cref="BasicSpectrogramData"/> references
    /// </summary>
    internal class SpectrogramColumnManager : MonoBehaviour {

        [InjectOptional]
        private readonly BasicSpectrogramData _basicSpectrogramData;

        /// <summary>
        /// <see cref="List{T}"/> of known <see cref="Spectrogram"/>s under a <see cref="CustomPlatform"/>
        /// </summary>
        private List<Spectrogram> columnDescriptors;


        /// <summary>
        /// <see cref="List{T}"/> of known <see cref="SpectrogramColumns"/> created by this <see cref="SpectrogramColumnManager"/> under a <see cref="CustomPlatform"/>
        /// </summary>
        private List<SpectrogramColumns> spectrogramColumns;


        /// <summary>
        /// Updates the Provider for Spectogram Data when this object becomes active<br/>
        /// [Unity calls this when the <see cref="MonoBehaviour"/> becomes active in the hierachy]
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnEnable() {
            UpdateSpectrogramDataProvider();
        }


        /// <summary>
        /// Creates <see cref="SpectrogramColumns"/> for each <see cref="Spectrogram"/> on the given <paramref name="gameObject"/>
        /// </summary>
        /// <param name="gameObject">What <see cref="GameObject"/> to create <see cref="SpectrogramColumns"/> for</param>
        internal void CreateColumns(GameObject gameObject) {
            spectrogramColumns = new List<SpectrogramColumns>();
            columnDescriptors = new List<Spectrogram>();

            Spectrogram[] localDescriptors = gameObject.GetComponentsInChildren<Spectrogram>(true);
            foreach (Spectrogram spec in localDescriptors) {
                SpectrogramColumns specCol = spec.gameObject.AddComponent<SpectrogramColumns>();
                PlatformManager.SpawnedComponents.Add(specCol);

                MaterialSwapper.ReplaceMaterials(spec.columnPrefab);

                specCol.SetField("_columnPrefab", spec.columnPrefab);
                specCol.SetField("_separator", spec.separator);
                specCol.SetField("_minHeight", spec.minHeight);
                specCol.SetField("_maxHeight", spec.maxHeight);
                specCol.SetField("_columnWidth", spec.columnWidth);
                specCol.SetField("_columnDepth", spec.columnDepth);
                specCol.SetField("_amount", spec.amount);

                spectrogramColumns.Add(specCol);
                columnDescriptors.Add(spec);
            }
        }


        /// <summary>
        /// Passes <see cref="BasicSpectrogramData"/> on to all <see cref="SpectrogramColumns"/><br/>
        /// </summary>
        internal void UpdateSpectrogramDataProvider() {
            if (_basicSpectrogramData != null) {
                foreach (SpectrogramColumns specCol in spectrogramColumns) {
                    specCol.SetField("_spectrogramData", _basicSpectrogramData);
                }
            }
        }
    }
}