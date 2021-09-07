using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SobelGradMap : MonoBehaviour
{
    [SerializeField] RawImage _beforeImage = null;
    [SerializeField] RawImage _afterImage = null;

    [SerializeField] private ComputeShader _computeShader = null;
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

        var result = ComputeShaderApplier.RunComputeShader(_computeShader, _beforeImage.texture);

        _afterImage.texture = result;


    }

}
