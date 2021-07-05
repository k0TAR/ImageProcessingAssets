using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ComputeShaderApplier : MonoBehaviour
{
    [SerializeField] RawImage _before = null;
    [SerializeField] RawImage _after = null;

    [SerializeField] private ComputeShader _computeShader = null;
    [SerializeField] private Texture2D _tex = null;

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
    private class ComputeShaderParameter<T>
    {
        public T _data;
        public ComputeShaderParameter(T input){
            _data = input;
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


    private RenderTexture RunComputeShader(
        ComputeShader comp, 
        Texture2D input)
    {
        //RenderTextureの初期化
        //Initializing Render Texture
        var result = GenerateRenderTexture(input);

        //Kernel Indexの取得
        //Getting Kernel Index
        var kernelIndex = 0;// comp.FindKernel("Gaussian");

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

    private RenderTexture RunComputeShader(
        ComputeShader comp, 
        Texture2D input, 
        Dictionary<string, object> parameters)
    {
        //RenderTextureの初期化
        //Initializing Render Texture
        var result = GenerateRenderTexture(input);

        //Kernel Indexの取得
        //Getting Kernel Index
        var kernelIndex = 0;// comp.FindKernel("Gaussian");

        //1つのグループの中に何個のスレッドがあるか
        //How many threads in one group
        ThreadSize threadSize = new ThreadSize();
        comp.GetKernelThreadGroupSizes(kernelIndex,
            out threadSize.x, out threadSize.y, out threadSize.z);

        //GPUにデータをコピー
        //Copying data to GPU
        comp.SetTexture(kernelIndex, "Input", input);
        comp.SetTexture(kernelIndex, "Result", result);

        foreach(var p in parameters)
        {
            if (p.Value is Texture)
            {
                comp.SetTexture(kernelIndex, p.Key, (Texture)p.Value);
            } else if(p.Value.GetType() == typeof(int))
            {
                comp.SetInt(p.Key, (int)p.Value);
            }
            else if(p.Value.GetType() == typeof(float))
            {
                comp.SetFloat(p.Key, (float)p.Value);
            }
        }

        //GPUで処理を実行
        //Process in GPU
        comp.Dispatch(kernelIndex,
            input.width / (int)threadSize.x,
            input.height / (int)threadSize.y,
            (int)threadSize.z
        );

        return result;
    }

    private RenderTexture RunComputeShader(
        ComputeShader comp, 
        Texture2D input, 
        int[] grid, 
        Dictionary<string, object> parameters)
    {
        //RenderTextureの初期化
        //Initializing Render Texture
        var result = GenerateRenderTexture(input, grid);

        //Kernel Indexの取得
        //Getting Kernel Index
        var kernelIndex = 0;// comp.FindKernel("Gaussian");

        //1つのグループの中に何個のスレッドがあるか
        //How many threads in one group
        ThreadSize threadSize = new ThreadSize();
        comp.GetKernelThreadGroupSizes(kernelIndex,
            out threadSize.x, out threadSize.y, out threadSize.z);

        //GPUにデータをコピー
        //Copying data to GPU
        comp.SetTexture(kernelIndex, "Input", input);
        comp.SetTexture(kernelIndex, "Result", result);
        comp.SetInts("Grid", grid);

        foreach (var p in parameters)
        {
            if (p.Value is Texture)
            {
                comp.SetTexture(kernelIndex, p.Key, (Texture)p.Value);
            }
            else if (p.Value.GetType() == typeof(int))
            {
                comp.SetInt(p.Key, (int)p.Value);
            }
            else if (p.Value.GetType() == typeof(float))
            {
                comp.SetFloat(p.Key, (float)p.Value);
            }
        }

        //GPUで処理を実行
        //Process in GPU
        comp.Dispatch(kernelIndex,
            (input.width / (int)threadSize.x) / grid[0],
            (input.height / (int)threadSize.y) / grid[1],
            (int)threadSize.z
        );

        return result;
    }



    private RenderTexture GenerateRenderTexture(Texture2D input)
    {
        var result = new RenderTexture(input.width, input.height, 0, RenderTextureFormat.ARGB32);
        result.enableRandomWrite = true;
        result.filterMode = FilterMode.Point;
        result.Create();
        return result;
    }
    private RenderTexture GenerateRenderTexture(Texture2D input, int[] grid)
    {
        var result = new RenderTexture(input.width / grid[0], input.height / grid[1], 0, RenderTextureFormat.ARGB32);
        result.enableRandomWrite = true;
        result.filterMode = FilterMode.Point;
        result.Create();
        return result;
    }
}

