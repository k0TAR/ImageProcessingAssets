using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PixelPropagate : MonoBehaviour
{
    [SerializeField] RawImage _beforeImage = null;
    [SerializeField] RawImage _afterImage = null;

    [SerializeField] private ComputeShader _computeShader = null;

    [SerializeField] [Range(1, 10)] private int _divider = 2;
    [SerializeField] [Range(1, 50)] private int _propagate_range = 5;

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
        computeShaderParams.Add("Divider", _divider);
        computeShaderParams.Add("PropagateRange", _propagate_range);

        var result = ComputeShaderApplier.RunComputeShader(_computeShader, _beforeImage.texture, computeShaderParams);

        _afterImage.texture = result;
    }
}
