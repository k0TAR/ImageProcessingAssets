using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GaborFilter : MonoBehaviour
{
    [SerializeField] RawImage _beforeImage = null;
    [SerializeField] RawImage _afterImage = null;
    [SerializeField] RawImage _viewKernel = null;
    [SerializeField] private ComputeShader _kernelViewComputeShader = null;


    [SerializeField] private ComputeShader _computeShader = null;
    [SerializeField] private bool _useWeight = false;
    [SerializeField] [Range(3, 100)] private int _size = 3;
    [SerializeField] [Range(0, 50)] private float _sigma = .1f;
    [SerializeField] [Range(-3.14f, 3.14f)] private float _theta = .1f;
    [SerializeField] [Range(0, 50)] private float _lambda = .1f;
    [SerializeField] [Range(0, 50)] private float _gamma = .1f;
    [SerializeField] [Range(0, 10)] private float _psi = .1f;

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

        if(_size % 2 == 0)
        {
            _size -= 1;
        }

        float[] gaborKernel = GetGaborKernel(_size, _sigma, _theta, _lambda, _gamma, _psi);
        float weight = 0;

        foreach (var v in gaborKernel)
        {
            weight += v;
        }

        ComputeBuffer kernelBuffer = new ComputeBuffer(
            _size * _size,
            System.Runtime.InteropServices.Marshal.SizeOf(typeof(float))
            );
        kernelBuffer.SetData(gaborKernel);

        Dictionary<string, object> computeShaderParams = new Dictionary<string, object>();
        computeShaderParams.Add("GaborKernel", kernelBuffer);
        computeShaderParams.Add("KernelSize", _size);
        computeShaderParams.Add("Weight", weight);
        computeShaderParams.Add("UseWeightToAverageKernel", Convert.ToInt32(_useWeight));


        var result = ComputeShaderApplier.RunComputeShader(_computeShader, _beforeImage.texture, computeShaderParams);
        result.filterMode = FilterMode.Point;

        _afterImage.texture = result;

        Dictionary<string, object> kernelParams = new Dictionary<string, object>();
        kernelParams.Add("Size", _size);
        kernelParams.Add("Sigma", _sigma);
        kernelParams.Add("Theta", _theta);
        kernelParams.Add("Lambda", _lambda);
        kernelParams.Add("Gamma", _gamma);
        kernelParams.Add("Psi", _psi);

        var kernelTexture = ComputeShaderApplier.RunComputeShader(_kernelViewComputeShader, _beforeImage.texture, kernelParams);


        _viewKernel.texture = kernelTexture;


        kernelBuffer.Dispose();
        kernelBuffer = null;
    }


    float[] GetGaborKernel(int size, float sigma, float theta, float lambda, float gamma, float psi)
    {
        float[] gaborKernel = new float[size * size];


        float sigma_x = sigma;
        float sigma_y = sigma / gamma;
        float sig_x = -1 / (2 * sigma_x * sigma_x);
        float sig_y = -1 / (2 * sigma_y * sigma_y);
        float cscale = 2 * Mathf.PI / lambda;

        int min = -size / 2;
        int max = size / 2;

        for (int i = min; i <= max ; i++)
        {
            for (int j = min; j <= max; j++)
            {
                float xr = i * Mathf.Cos(theta) + j * Mathf.Sin(theta);
                float yr = -i * Mathf.Sin(theta) + j * Mathf.Cos(theta);

                gaborKernel[(i + max) * size + (j + max)] = Mathf.Exp(sig_x * xr * xr + sig_y * yr * yr ) * Mathf.Cos(cscale * xr + psi);

            }
        }

        return gaborKernel;
    }

}
