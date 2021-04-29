using System.Collections.Generic;

using UnityEngine;

using Zenject;


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

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.green;

            for (int i = -64; i < 64; i++)
            {
                Vector3 zOffset = i * separator;
                if (columnPrefab != null)
                {
                    foreach (Renderer r in columnPrefab.GetComponentsInChildren<Renderer>())
                    {
                        Bounds bounds = r.bounds;
                        Gizmos.DrawCube(zOffset + bounds.center, bounds.size);
                    }
                }
                else
                {
                    Gizmos.DrawCube(zOffset, Vector3.one * 0.5f);
                }
            }
        }

        [Inject]
        public void Construct(MaterialSwapper materialSwapper, [InjectOptional] BasicSpectrogramData basicSpectrogramData)
        {
            _materialSwapper = materialSwapper;
            _basicSpectrogramData = basicSpectrogramData;
        }

        public async void PlatformEnabled(DiContainer container)
        {
            if (columnPrefab == null) return;
            container.Inject(this);
            await _materialSwapper!.ReplaceMaterials(columnPrefab);
            if (_columnTransforms == null) CreateColumns();
            foreach (INotifyPlatformEnabled notifyEnable in GetComponentsInChildren<INotifyPlatformEnabled>(true))
            {
                if ((Object)notifyEnable != this)
                    notifyEnable.PlatformEnabled(container);
            }
        }

        public void PlatformDisabled()
        {
            foreach (INotifyPlatformDisabled notifyDisable in GetComponentsInChildren<INotifyPlatformDisabled>(true))
            {
                if ((Object)notifyDisable != this)
                    notifyDisable.PlatformDisabled();
            }
        }

        /// <summary>
        /// Updates all columns heights.<br/>
        /// [Unity calls this once per frame!]
        /// </summary>
        private void Update()
        {
            IList<float> processedSamples = _basicSpectrogramData != null ? _basicSpectrogramData.ProcessedSamples : FallbackSamples;
            for (int i = 0; i < processedSamples.Count; i++)
            {
                _columnTransforms![i].localScale = new Vector3(columnWidth, Mathf.Lerp(minHeight, maxHeight, processedSamples[i]), columnDepth);
                _columnTransforms![i + 64].localScale = new Vector3(columnWidth, Mathf.Lerp(minHeight, maxHeight, processedSamples[i]), columnDepth);
            }
        }

        /// <summary>
        /// Creates all Columns using the <see cref="columnPrefab"/>
        /// </summary>
        private void CreateColumns()
        {
            _columnTransforms = new Transform[128];
            for (int i = 0; i < 64; i++)
            {
                _columnTransforms[i] = CreateColumn(separator * i);
                _columnTransforms[i + 64] = CreateColumn(-separator * (i + 1));
            }
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
            column.transform.localScale = new Vector3(columnWidth, minHeight, columnDepth);
            return column.transform;
        }

        /// <summary>
        /// Spectrogram fallback data
        /// </summary>
        private static IList<float> FallbackSamples => _fallbackSamples ??= CreateFallbackSamples();
        private static float[]? _fallbackSamples;

        private static float[] CreateFallbackSamples()
        {
            float[] samples = new float[64];
            for (int i = 0; i < samples.Length; i++)
                samples[i] = (Mathf.Sin(0.4f * i - 0.5f * Mathf.PI) + 1) / 2;
            return samples;
        }
    }
}