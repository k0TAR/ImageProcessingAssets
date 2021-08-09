using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Binarization : MonoBehaviour
{
    [SerializeField] RawImage _beforeImage = null;
    [SerializeField] RawImage _afterImage = null;

    [SerializeField] private ComputeShader _computeShader = null;

    [SerializeField] [Range(0, 1)] private float _threshold = .2f;
    [SerializeField] private bool _alphaOn = false;

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
        computeShaderParams.Add("Threshold", _threshold);
        computeShaderParams.Add("AlphaOn", Convert.ToInt32(_alphaOn) );

        var result = ComputeShaderApplier.RunComputeShader(_computeShader, _beforeImage.texture, computeShaderParams);

        _afterImage.texture = result;
    }
}
