using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SobelFilter : MonoBehaviour
{
    [SerializeField] RawImage _beforeImage = null;
    [SerializeField] RawImage _afterImage = null;

    [SerializeField] private ComputeShader _computeShader = null;
    [SerializeField] private Texture _tex = null;

    [SerializeField] private float[] _edgeColor = new float[] { 1, 1, 1, 1 };
    [SerializeField] private float _threshold = .2f;
    [SerializeField] private float _sensitivity = 1;

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
        if (_tex == null && _beforeImage != null)
        {
            _tex = _beforeImage.texture;

        }
        else if (_beforeImage == null && _tex != null)
        {
            _beforeImage.texture = _tex;
        }
        else if(_beforeImage == null && _tex == null)
        {
            Debug.Log("SET BEFORE RAWIMAGE OR TEXTURE IN SOBEL FILTER.");
            return;
        }

        if (_afterImage == null)
        {
            Debug.Log("SET AFTER RAWIMAGE IN SOBEL FILTER.");
            return;
        }

        Debug.Log("RAN");

        int[] resolution = new int[] { _tex.width, _tex.height };

        Dictionary<string, object> computeShaderParams = new Dictionary<string, object>();
        computeShaderParams.Add("EdgeColor", _edgeColor);
        computeShaderParams.Add("Threshold", _threshold);
        computeShaderParams.Add("Sensitivity", _sensitivity);
        computeShaderParams.Add("Resolution", resolution);

        var result = ComputeShaderApplier.RunComputeShader(_computeShader, _tex, computeShaderParams);

        _afterImage.texture = result;
    }
}
