using System;
using UnityEngine;

// Token: 0x0200035C RID: 860
public class SpectrogramColumns : MonoBehaviour
{
    protected void Start()
    {
        this.CreateColums();
    }
    
    protected void Update()
    {
        float[] processedSamples;
        if (_spectrogramData == null) {
            processedSamples = new float[64];
            for (int i = 0; i < processedSamples.Length; i++)
            {
                processedSamples[i] = (Mathf.Sin((float)i / 64 * 9 * Mathf.PI + 1.4f* Mathf.PI) + 1.2f)/25;
            }
        }
        else
        {
            processedSamples = this._spectrogramData.ProcessedSamples.ToArray();
        }
        
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
    
    private void CreateColums()
    {
        _columnTransforms = new Transform[128];
        for (int i = 0; i < 64; i++)
        {
            _columnTransforms[i] = CreateColumn(_separator * i);
            _columnTransforms[i + 64] = CreateColumn(-_separator * (i + 1));
        }
    }
    
    private Transform CreateColumn(Vector3 pos)
    {
        GameObject gameObject = Instantiate<GameObject>(_columnPrefab, base.transform);
        CustomFloorPlugin.PlatformManager.SpawnedObjects.Add(gameObject);
        gameObject.transform.localPosition = pos;
        gameObject.transform.localScale = new Vector3(_columnWidth, _minHeight, _columnDepth);
        return gameObject.transform;
    }
#pragma warning disable CS0649
    [SerializeField]
    private GameObject _columnPrefab;
#pragma warning restore CS0649
    [SerializeField]
    private Vector3 _separator = new Vector3(0f, 0f, 1f);
    
    [SerializeField]
    private float _minHeight = 1f;

    [SerializeField]
    private float _maxHeight = 10f;
    
    [SerializeField]
    private float _columnWidth = 1f;
    
    [SerializeField]
    private float _columnDepth = 1f;
#pragma warning disable CS0649
    private BasicSpectrogramData _spectrogramData;
#pragma warning restore CS0649
    private Transform[] _columnTransforms;
}
