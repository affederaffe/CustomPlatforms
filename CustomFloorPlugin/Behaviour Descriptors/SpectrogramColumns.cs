using CustomFloorPlugin;

using UnityEngine;


[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Unity can't deserialize values onto readonly fields")]
public class SpectrogramColumns : MonoBehaviour
{
    /// <summary>
    /// Prefab for individual columns
    /// </summary>
    [SerializeField]
    private GameObject _columnPrefab = null;

    /// <summary>
    /// The added offset between columns
    /// </summary>
    [SerializeField]
    private Vector3 _separator = new Vector3(0f, 0f, 1f);

    /// <summary>
    /// Minimum height of the individual columns, reached at dead silence on the channel
    /// </summary>
    [SerializeField]
    private float _minHeight = 1f;

    /// <summary>
    /// Maximum height of the individual columns, reached at peak channel volume
    /// </summary>
    [SerializeField]
    private float _maxHeight = 10f;

    /// <summary>
    /// Width of the individual columns, always applies
    /// </summary>
    [SerializeField]
    private float _columnWidth = 1f;

    /// <summary>
    /// Depth of the individual columns, always applies
    /// </summary>
    [SerializeField]
    private float _columnDepth = 1f;

    /// <summary>
    /// Set via reflection
    /// [Acquired from BeatSaber]
    /// </summary>
    private BasicSpectrogramData _spectrogramData;

    private LightWithIdManager _lightWithIdManager;

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

    /// <summary>
    /// Unity calls this initializer function once at the beginning of a <see cref="MonoBehaviour"/>s life.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
    private void Start()
    {
        CreateColums();
    }

    /// <summary>
    /// Updates all columns heights.<br/>
    /// [Unity calls this once per frame!]
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
    private void Update()
    {
        float[] processedSamples = _spectrogramData?.ProcessedSamples.ToArray() ?? FallbackSamples;

        for (int i = 0; i < processedSamples.Length; i++)
        {
            float num = processedSamples[i] * (5f + i * 0.07f);
            if (num > 1f)
            {
                num = 1f;
            }
            num = Mathf.Pow(num, 2f);
            _columnTransforms[i].localScale = new Vector3(_columnWidth, Mathf.Lerp(_minHeight, _maxHeight, num) + i * 0.1f, _columnDepth);
            _columnTransforms[i + 64].localScale = new Vector3(_columnWidth, Mathf.Lerp(_minHeight, _maxHeight, num), _columnDepth);
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
            _columnTransforms[i] = CreateColumn(_separator * i);
            _columnTransforms[i + 64] = CreateColumn(-_separator * (i + 1));
        }
    }

    /// <summary>
    /// Creates a column and returns its <see cref="Transform"/>
    /// </summary>
    /// <param name="pos">Where to create the column(local space <see cref="Vector3"/> offset)</param>
    /// <returns></returns>
    private Transform CreateColumn(Vector3 pos)
    {
        GameObject gameObject = Instantiate(_columnPrefab, transform);
        foreach (TubeLight tubeLight in gameObject.GetComponentsInChildren<TubeLight>())
        {
            tubeLight.GameAwake(_lightWithIdManager);
        }
        PlatformManager.SpawnedObjects.Add(gameObject);
        gameObject.transform.localPosition = pos;
        gameObject.transform.localScale = new Vector3(_columnWidth, _minHeight, _columnDepth);
        return gameObject.transform;
    }
}