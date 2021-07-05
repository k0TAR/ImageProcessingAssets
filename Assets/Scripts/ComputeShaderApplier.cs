using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ComputeShaderApplier : MonoBehaviour
{
    [SerializeField] RawImage _before;
    [SerializeField] RawImage _after;

    [SerializeField] private ComputeShader _computeShader;
    [SerializeField] private Texture2D _tex;

    private struct ThreadSize
    {
        public uint x;
        public uint y;
        public uint z;

        public ThreadSize(uint x, uint y, uint z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

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

        var result = RunComputeShader(_computeShader, _tex);

        _after.texture = result;
    }


    private RenderTexture RunComputeShader(ComputeShader comp, Texture2D input)
    {
        //RenderTextureの初期化
        //Initializing Render Texture
        var result = new RenderTexture(input.width, input.height, 0, RenderTextureFormat.ARGB32);
        result.enableRandomWrite = true;
        result.filterMode = FilterMode.Point;
        result.Create();

        //Kernel Indexの取得
        //Getting Kernel Index
        var kernelIndex = 0;// comp.FindKernel("Gaussian");
        //print(kernelIndex);

        //1つのグループの中に何個のスレッドがあるか
        //How many threads in one group
        ThreadSize threadSize = new ThreadSize();
        comp.GetKernelThreadGroupSizes(kernelIndex,
            out threadSize.x, out threadSize.y, out threadSize.z);

        //GPUにデータをコピー
        //Copying data to GPU
        comp.SetTexture(kernelIndex, "Input", input);
        comp.SetTexture(kernelIndex, "Result", result);

        //GPUで処理を実行
        //Process in GPU
        comp.Dispatch(kernelIndex,
            input.width / (int)threadSize.x,
            input.height / (int)threadSize.y,
            (int)threadSize.z
        );

        return result;
    }


}
