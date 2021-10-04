using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RGB2HSV_S : MonoBehaviour
{
    [SerializeField] RawImage _beforeImage = null;
    [SerializeField] RawImage _afterImage = null;

    [SerializeField] private ComputeShader _computeShader = null;

    [SerializeField] [Range(1, 50)] private int _update = 5;

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

        var result = ComputeShaderApplier.RunComputeShader(_computeShader, _beforeImage.texture, computeShaderParams);

        _afterImage.texture = result;
    }
}
