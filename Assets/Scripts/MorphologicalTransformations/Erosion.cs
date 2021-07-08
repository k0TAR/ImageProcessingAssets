using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Erosion : MonoBehaviour
{
    [SerializeField] RawImage _beforeImage = null;
    [SerializeField] RawImage _afterImage = null;

    [SerializeField] private ComputeShader _computeShader = null;
    [SerializeField] private Texture _tex = null;

    [SerializeField] [Range(1, 20)] private int _erosionIteration = 2;

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

        var result = ComputeShaderApplier.RunComputeShader(_computeShader, _tex);
        for (int i = 1; i < _erosionIteration; i++)
        {
            result = ComputeShaderApplier.RunComputeShader(_computeShader, result);
        }

        _afterImage.texture = result;
    }
}
