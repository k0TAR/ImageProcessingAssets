using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SobelFilter : MonoBehaviour
{
    [SerializeField] RawImage _before = null;
    [SerializeField] RawImage _after = null;

    [SerializeField] private ComputeShader _computeShader = null;
    [SerializeField] private Texture2D _tex = null;

    [SerializeField] private float[] _edgeColor = new float[] { 1, 1, 1, 1 };
    [SerializeField] private float _threshold;
    [SerializeField] private float _sensitivity;

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
        _before.texture = _tex;
        

        int[] resolution = new int[] { _tex.width, _tex.height };

        Dictionary<string, object> computeShaderParams = new Dictionary<string, object>();
        computeShaderParams.Add("EdgeColor", _edgeColor);
        computeShaderParams.Add("Threshold", _threshold);
        computeShaderParams.Add("Sensitivity", _sensitivity);
        computeShaderParams.Add("Resolution", resolution);

        var result = ComputeShaderApplier.RunComputeShader(_computeShader, _tex, computeShaderParams);

        _after.texture = result;
    }
}
