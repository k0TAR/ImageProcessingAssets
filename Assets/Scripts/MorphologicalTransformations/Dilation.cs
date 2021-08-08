using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dilation : MonoBehaviour
{
    [SerializeField] RawImage _beforeImage = null;
    [SerializeField] RawImage _afterImage = null;

    [SerializeField] private ComputeShader _computeShader = null;

    [SerializeField] [Range(1, 20)] private int _dilationIteration = 2;

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
        for (int i = 1; i < _dilationIteration; i++)
        {
            result = ComputeShaderApplier.RunComputeShader(_computeShader, result);
        }
        _afterImage.texture = result;
    }
}
