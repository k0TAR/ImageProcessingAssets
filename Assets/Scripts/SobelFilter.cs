using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SobelFilter : MonoBehaviour
{
    [SerializeField] RawImage _beforeImage = null;
    [SerializeField] RawImage _afterImage = null;

    [SerializeField] private ComputeShader _computeShader = null;

    [SerializeField] private Color _edgeColor = Color.white;
    [SerializeField] [Range(0, 2)] private float _threshold = .2f;
    [SerializeField] [Range(0, 5)] private float _sensitivity = 1;

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
        if ( !ComputeShaderApplier.IsInitializationEnough(ref _beforeImage, ref _afterImage, this) ) return;

        float[] edgeColorVector = new float[4] { _edgeColor.r, _edgeColor.g, _edgeColor.b, _edgeColor.a};
        int[] resolution = new int[] { _beforeImage.texture.width, _beforeImage.texture.height };

        Dictionary<string, object> computeShaderParams = new Dictionary<string, object>();
        computeShaderParams.Add("EdgeColor", edgeColorVector);
        computeShaderParams.Add("Threshold", _threshold);
        computeShaderParams.Add("Sensitivity", _sensitivity);
        computeShaderParams.Add("Resolution", resolution);

        var result = ComputeShaderApplier.RunComputeShader(_computeShader, _beforeImage.texture, computeShaderParams);

        _afterImage.texture = result;
    }
}
