using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GaborFilter : MonoBehaviour
{
    [SerializeField] RawImage _beforeImage = null;
    [SerializeField] RawImage _afterImage = null;

    [SerializeField] private ComputeShader _computeShader = null;
    [SerializeField] private bool _alphaOn = false;
    [SerializeField] [Range(0, 10)] private float _sigma = .1f;
    [SerializeField] [Range(0, 10)] private float _theta = .1f;
    [SerializeField] [Range(0, 10)]  private float _lambda = .1f;
    [SerializeField] [Range(0, 10)]  private float _gamma = .1f;
    [SerializeField] [Range(0, 10)]  private float _psi = .1f;

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

        Dictionary<string, object> computeShaderParams = new Dictionary<string, object>();
        computeShaderParams.Add("Sigma", _sigma);
        computeShaderParams.Add("Theta", _theta);
        computeShaderParams.Add("Lambda", _lambda);
        computeShaderParams.Add("Gamma", _gamma);
        computeShaderParams.Add("Psi", _psi);

        var result = ComputeShaderApplier.RunComputeShader(_computeShader, _beforeImage.texture, computeShaderParams);

        _afterImage.texture = result;


    }
}
