using System.Collections.Generic;
using CustomFloorPlugin.Interfaces;
using UnityEngine;

using Zenject;


// ReSharper disable once CheckNamespace
namespace CustomFloorPlugin
{
    public class Spectrogram : MonoBehaviour, INotifyPlatformEnabled, INotifyPlatformDisabled
    {
        /// <summary>
        /// Prefab for individual columns
        /// </summary>
        public GameObject? columnPrefab;

        /// <summary>
        /// The added offset between columns
        /// </summary>
        public Vector3 separator = Vector3.forward;

        /// <summary>
        /// Minimum height of the individual columns, reached at dead silence on the channel
        /// </summary>
        public float minHeight = 1f;

        /// <summary>
        /// Maximum height of the individual columns, reached at peak channel volume
        /// </summary>
        public float maxHeight = 10f;

        /// <summary>
        /// Width of the individual columns, always applies
        /// </summary>
        public float columnWidth = 1f;

        /// <summary>
        /// Depth of the individual columns, always applies
        /// </summary>
        public float columnDepth = 1f;

        private MaterialSwapper? _materialSwapper;
        private BasicSpectrogramData? _basicSpectrogramData;

        private Transform[]? _columnTransforms;

        [Inject]
        public void Construct(MaterialSwapper materialSwapper, [InjectOptional] BasicSpectrogramData basicSpectrogramData)
        {
            _materialSwapper = materialSwapper;
            _basicSpectrogramData = basicSpectrogramData;
        }

        public async void PlatformEnabled(DiContainer container)
        {
            if (columnPrefab is null) return;
            container.Inject(this);
            if (_columnTransforms is null)
            {
                await _materialSwapper!.ReplaceMaterialsAsync(columnPrefab);
                _columnTransforms = CreateColumns();
                UpdateColumnHeights(FallbackSamples);
                foreach (INotifyPlatformEnabled notifyEnable in GetComponentsInChildren<INotifyPlatformEnabled>(true))
                {
                    if (!ReferenceEquals(this, notifyEnable))
                        notifyEnable?.PlatformEnabled(container);
                }
            }

            enabled = _basicSpectrogramData is not null;
        }

        public void PlatformDisabled()
        {
            UpdateColumnHeights(FallbackSamples);
        }

        /// <summary>
        /// Updates all columns heights with the processed samples
        /// [Unity calls this once per frame!]
        /// </summary>
        private void Update()
        {
            UpdateColumnHeights(_basicSpectrogramData!.ProcessedSamples);
        }

        /// <summary>
        /// Updates all columns heights
        /// </summary>
        private void UpdateColumnHeights(IList<float> samples)
        {
            for (int i = 0; i < samples.Count; i++)
            {
                _columnTransforms![i].localScale = new Vector3(columnWidth, Mathf.Lerp(minHeight, maxHeight, samples[i]), columnDepth);
                _columnTransforms![i + 64].localScale = new Vector3(columnWidth, Mathf.Lerp(minHeight, maxHeight, samples[i]), columnDepth);
            }
        }

        /// <summary>
        /// Creates all Columns using the <see cref="columnPrefab"/>
        /// </summary>
        private Transform[] CreateColumns()
        {
            Transform[] columnTransforms = new Transform[128];
            for (int i = 0; i < 64; i++)
            {
                columnTransforms[i] = CreateColumn(separator * i);
                columnTransforms[i + 64] = CreateColumn(-separator * (i + 1));
            }

            return columnTransforms;
        }

        /// <summary>
        /// Creates a column and returns its <see cref="Transform"/>
        /// </summary>
        /// <param name="pos">Where to create the column(local space <see cref="Vector3"/> offset)</param>
        /// <returns>The <see cref="Transform"/> of the created column</returns>
        private Transform CreateColumn(Vector3 pos)
        {
            GameObject column = Instantiate(columnPrefab!, transform);
            column.transform.localPosition = pos;
            return column.transform;
        }

        /// <summary>
        /// Spectrogram fallback data
        /// </summary>
        private static float[]? _fallbackSamples;
        private static IList<float> FallbackSamples
        {
            get
            {
                if (_fallbackSamples is not null) return _fallbackSamples;
                _fallbackSamples = new float[64];
                for (int i = 0; i < _fallbackSamples.Length; i++)
                    _fallbackSamples[i] = (Mathf.Sin((0.4f * i) - (0.5f * Mathf.PI)) + 1) / 2;
                return _fallbackSamples;
            }
        }
    }
}