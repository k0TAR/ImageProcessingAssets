using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Thinning2OnePixel : MonoBehaviour
{
    [SerializeField] RawImage _beforeImage = null;
    [SerializeField] RawImage _afterImage = null;

    [SerializeField] private ComputeShader _computeShader = null;
    [SerializeField] private bool _alphaOn = false;
    [SerializeField] [Range(1, 15)] int _thinningIteration = 5;


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
        computeShaderParams.Add("AlphaOn", Convert.ToInt32(_alphaOn));

        //Debug.Log("===Thinning2OnePixel on GPU START===");
        //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        //sw.Start();

        var result = ComputeShaderApplier.RunComputeShader(_computeShader, _beforeImage.texture, computeShaderParams); ;
        for (int i = 1; i < _thinningIteration; i++)
        {
            result = ComputeShaderApplier.RunComputeShader(_computeShader, result, computeShaderParams);
        }

        _afterImage.texture = result;

        //sw.Stop();
        //Debug.Log("Iteration: " + _thinningIteration + ", Processing Time: " + sw.ElapsedMilliseconds + " ms");
    }
}
