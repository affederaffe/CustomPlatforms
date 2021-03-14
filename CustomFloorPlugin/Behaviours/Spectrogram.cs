using UnityEngine;

using Zenject;


namespace CustomFloorPlugin
{
    public class Spectrogram : MonoBehaviour, INotifyPlatformEnabled
    {
        /// <summary>
        /// Prefab for individual columns
        /// </summary>
        public GameObject columnPrefab;

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

        private MaterialSwapper _materialSwapper;
        private PlatformManager _platformManager;
        private BasicSpectrogramData _basicSpectrogramData;

        /// <summary>
        /// An array of all <see cref="Transform"/>s under a <see cref="Spectrogram"/>
        /// </summary>
        private Transform[] _columnTransforms;

        /// <summary>
        /// Spectogram fallback data
        /// </summary>
        private static float[] FallbackSamples
        {
            get
            {
                if (_FallbackSamples == null)
                {
                    _FallbackSamples = new float[64];
                    for (int i = 0; i < FallbackSamples.Length; i++)
                    {
                        FallbackSamples[i] = (Mathf.Sin((float)i / 64 * 9 * Mathf.PI + 1.4f * Mathf.PI) + 1.2f) / 25;
                    }
                }
                return _FallbackSamples;
            }
        }
        private static float[] _FallbackSamples;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.green;
            Vector3 zOffset;

            for (int i = -64; i < 64; i++)
            {
                zOffset = i * separator;
                if (columnPrefab != null)
                {
                    foreach (Renderer r in columnPrefab.GetComponentsInChildren<Renderer>())
                    {
                        Gizmos.DrawCube(zOffset + r.bounds.center, r.bounds.size);
                    }
                }
                else
                {
                    Gizmos.DrawCube(zOffset, Vector3.one * 0.5f);
                }
            }
        }

        [Inject]
        public void Construct(MaterialSwapper materialSwapper, PlatformManager platformManager, [InjectOptional] BasicSpectrogramData basicSpectrogramData)
        {
            _materialSwapper = materialSwapper;
            _platformManager = platformManager;
            _basicSpectrogramData = basicSpectrogramData;
        }

        void INotifyPlatformEnabled.PlatformEnabled(DiContainer container)
        {
            container.Inject(this);
            _materialSwapper.ReplaceMaterials(columnPrefab);
            CreateColums();
            // Distribute the event again over all new spawned children, and the children only to avoid recursion
            foreach (INotifyPlatformEnabled notifyEnable in GetComponentsInChildren<INotifyPlatformEnabled>(true))
            {
                if ((Object)notifyEnable != this)
                {
                    notifyEnable.PlatformEnabled(container);
                }
            }
        }

        /// <summary>
        /// Updates all columns heights.<br/>
        /// [Unity calls this once per frame!]
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void Update()
        {
            float[] processedSamples = _basicSpectrogramData?.ProcessedSamples.ToArray() ?? FallbackSamples;

            for (int i = 0; i < processedSamples.Length; i++)
            {
                float num = processedSamples[i] * (5f + i * 0.01f);
                if (num > 1f)
                    num = 1f;
                num = Mathf.Pow(num, 2f);
                _columnTransforms[i].localScale = new Vector3(columnWidth, Mathf.Lerp(minHeight, maxHeight, num) + i * 0.1f, columnDepth);
                _columnTransforms[i + 64].localScale = new Vector3(columnWidth, Mathf.Lerp(minHeight, maxHeight, num), columnDepth);
            }
        }

        /// <summary>
        /// Creates all Columns using the <see cref="_columnPrefab"/>
        /// </summary>
        private void CreateColums()
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
        /// <returns></returns>
        private Transform CreateColumn(Vector3 pos)
        {
            GameObject gameObject = Instantiate(columnPrefab, transform);
            _platformManager.spawnedObjects.Add(gameObject);
            gameObject.transform.localPosition = pos;
            gameObject.transform.localScale = new Vector3(columnWidth, minHeight, columnDepth);
            return gameObject.transform;
        }
    }
}