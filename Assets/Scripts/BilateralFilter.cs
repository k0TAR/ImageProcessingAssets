using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BilateralFilter : MonoBehaviour
{
    [SerializeField] [Range(0, 10)] float _sigmaSpatial = 3f;
    [SerializeField] [Range(0, 10)] float _sigmaIntensity = .1f;
    [SerializeField] [Range(1, 15)] int _filterSize = 5;
    [SerializeField] [Range(1,10)] int _bfIteration = 6;


    [SerializeField] RawImage _beforeImage = null;
    [SerializeField] RawImage _afterImage = null;

    [SerializeField] private ComputeShader _computeShader = null;
    [SerializeField] private Texture _tex = null;
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
        if (!ComputeShaderApplier.IsInitializationEnough(ref _beforeImage, ref _afterImage, ref _tex, this)) return;


        float[] filter = new float[_filterSize * _filterSize];
        float weight = 0;
        for (int x = 0; x < _filterSize; x++)
        {
            for (int y = 0; y < _filterSize; y++)
            {
                filter[(_filterSize - 1) * x + y] = Gaussian(x, y, _sigmaSpatial);
                weight += filter[(_filterSize - 1) * x + y];
            }
        }

        ComputeBuffer filterBuffer = new ComputeBuffer(
            _filterSize * _filterSize,
            System.Runtime.InteropServices.Marshal.SizeOf(typeof(float))
            );
        filterBuffer.SetData(filter);

        Dictionary<string, object> computeShaderParams = new Dictionary<string, object>();
        computeShaderParams.Add("GaussianFilter", filterBuffer);
        computeShaderParams.Add("FilterSize", _filterSize);
        computeShaderParams.Add("Weight", weight);
        computeShaderParams.Add("Sigma", _sigmaIntensity);

        var result = ComputeShaderApplier.RunComputeShader(_computeShader, _tex, computeShaderParams); ; 
        for(int i = 1; i < _bfIteration; i++)
        {
            result = ComputeShaderApplier.RunComputeShader(_computeShader, result, computeShaderParams);
        }
          
        _afterImage.texture = result;

        filterBuffer.Dispose(); 
        filterBuffer = null;
    }

    private float Gaussian(int x, int y, float sigma)
    {
        return Mathf.Exp(-(x * x + y * y) / (2.0f * sigma * sigma)); //
    }
}
