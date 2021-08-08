using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class GaussianFilter : MonoBehaviour
{
    [SerializeField] float _sigma = 1f;
    [SerializeField] int _filterSize = 5;


    [SerializeField] RawImage _beforeImage = null;
    [SerializeField] RawImage _afterImage = null;

    [SerializeField] private ComputeShader _computeShader = null;
    private void Start()
    {
        if (!SystemInfo.supportsComputeShaders)
        {
            Debug.LogError("Does not support Compute Shader.");
            return;
        }

    }

    private void OnValidate()
    {
        if (!ComputeShaderApplier.IsInitializationEnough(ref _beforeImage, ref _afterImage, this)) return;


        float[] filter = new float[_filterSize * _filterSize];
        float weight = 0;
        for(int x = 0; x < _filterSize; x++)
        {
            for(int y = 0; y < _filterSize; y++)
            {
                filter[ (_filterSize - 1) * x + y] = Gaussian(x, y, _sigma);
                weight += filter[(_filterSize - 1) * x + y];
            }
        }

        ComputeBuffer filterBuffer = new ComputeBuffer(
            _filterSize * _filterSize, 
            System.Runtime.InteropServices.Marshal.SizeOf(typeof(float)) 
            );
        filterBuffer.SetData(filter);

        Dictionary<string, object> computeShaderParams= new Dictionary<string, object>();
        computeShaderParams.Add("Filter", filterBuffer);
        computeShaderParams.Add("FilterSize", _filterSize);
        computeShaderParams.Add("Weight", weight);

        var result = ComputeShaderApplier.RunComputeShader(_computeShader, _beforeImage.texture, computeShaderParams);

        _afterImage.texture = result;

        filterBuffer.Release();
        filterBuffer = null;
    }

    private float Gaussian(int x, int y, float sigma)
    {
        return (1 / (2 * Mathf.PI * sigma * sigma)) * Mathf.Exp( -(x*x + y*y) / (2.0f * sigma * sigma) ); //
    }
}
