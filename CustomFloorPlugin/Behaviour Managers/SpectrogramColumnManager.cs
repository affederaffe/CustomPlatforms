using BS_Utils.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomFloorPlugin {


    /// <summary>
    /// Instantiable manager class for <see cref="Spectrogram"/>s that handles creation of <see cref="SpectrogramColumns"/> and updating their <see cref="BasicSpectrogramData"/> references
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1812:Avoid unistantiated internal classes", Justification = "Instantiated by Unity")]
    internal class SpectrogramColumnManager:MonoBehaviour {


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
            foreach(Spectrogram spec in localDescriptors) {
                SpectrogramColumns specCol = spec.gameObject.AddComponent<SpectrogramColumns>();
                PlatformManager.SpawnedComponents.Add(specCol);

                ReflectionUtil.SetPrivateField(specCol, "_columnPrefab", spec.columnPrefab);
                ReflectionUtil.SetPrivateField(specCol, "_separator", spec.separator);
                ReflectionUtil.SetPrivateField(specCol, "_minHeight", spec.minHeight);
                ReflectionUtil.SetPrivateField(specCol, "_maxHeight", spec.maxHeight);
                ReflectionUtil.SetPrivateField(specCol, "_columnWidth", spec.columnWidth);
                ReflectionUtil.SetPrivateField(specCol, "_columnDepth", spec.columnDepth);

                spectrogramColumns.Add(specCol);
                columnDescriptors.Add(spec);
            }
        }


        /// <summary>
        /// Passes <see cref="BasicSpectrogramData"/> on to all <see cref="SpectrogramColumns"/><br/>
        /// </summary>
        internal void UpdateSpectrogramDataProvider() {
            BasicSpectrogramData[] datas = Resources.FindObjectsOfTypeAll<BasicSpectrogramData>();
            if(datas.Length != 0) {
                BasicSpectrogramData spectrogramData = datas.FirstOrDefault();
                if(spectrogramData != null) {
                    foreach(SpectrogramColumns specCol in spectrogramColumns) {
                        ReflectionUtil.SetPrivateField(specCol, "_spectrogramData", spectrogramData);
                    }
                }
            }
        }
    }
}
