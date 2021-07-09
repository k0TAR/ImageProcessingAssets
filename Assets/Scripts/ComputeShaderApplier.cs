using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ComputeShaderApplier 
{
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
    /*
    private class ComputeShaderParameter<T>
    {
        public T _data;
        public ComputeShaderParameter(T input){
            _data = input;
        }

    }*/

    public static bool IsInitializationEnough(
        ref RawImage beforeImage, 
        ref RawImage afterImage, 
        ref Texture usingTexture, 
        Object usingClass)
    {
        if (beforeImage == null && usingTexture == null)
        {
            Debug.Log($"SET BEFORE RAWIMAGE OR TEXTURE IN {usingClass.name}.");
            return false;
        }
        else if (usingTexture == null && beforeImage != null)
        {
            usingTexture = beforeImage.texture;

        }
        else if (usingTexture != null && beforeImage == null)
        {
            beforeImage.texture = usingTexture;
        }


        if (afterImage == null)
        {
            Debug.Log($"SET AFTER RAWIMAGE IN {usingClass.name}.");
            return false;
        }

        afterImage.texture = null;
        return true;
    }

    public static RenderTexture RunComputeShader(
        ComputeShader comp, 
        Texture input)
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


    public static RenderTexture RunComputeShader(
        ComputeShader comp, 
        Texture input, 
        Dictionary<string, object> kernelParams)
    {
        //RenderTextureの初期化
        //Initializing Render Texture
        var result = GenerateRenderTexture(input);

        //Kernel Indexの取得
        //Getting Kernel Index
        var kernelIndex = 0;// comp.FindKernel("GaussianFilter");

        //1つのグループの中に何個のスレッドがあるか
        //How many threads in one group
        ThreadSize threadSize = new ThreadSize();
        comp.GetKernelThreadGroupSizes(kernelIndex,
            out threadSize.x, out threadSize.y, out threadSize.z);

        //GPUにデータをコピー
        //Copying data to GPU
        comp.SetTexture(kernelIndex, "Input", input);
        comp.SetTexture(kernelIndex, "Result", result);

        DistinguishAndSetParams(comp, kernelIndex, kernelParams);


        //GPUで処理を実行
        //Process in GPU
        comp.Dispatch(kernelIndex,
            input.width / (int)threadSize.x,
            input.height / (int)threadSize.y,
            (int)threadSize.z
        );

        return result;
    }

    public static RenderTexture RunComputeShader(
        ComputeShader comp, 
        Texture input, 
        int[] grid, 
        Dictionary<string, object> kernelParams)
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

        DistinguishAndSetParams(comp, kernelIndex, kernelParams);

        //GPUで処理を実行
        //Process in GPU
        comp.Dispatch(kernelIndex,
            (input.width / (int)threadSize.x) / grid[0],
            (input.height / (int)threadSize.y) / grid[1],
            (int)threadSize.z
        );

        return result;
    }

    private static void DistinguishAndSetParams(
        ComputeShader comp, int kernelIndex, Dictionary<string, object> kernelParams
        )
    {
        foreach (var p in kernelParams)
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
            else if (p.Value.GetType() == typeof(int[]))
            {
                comp.SetInts(p.Key, (int[])p.Value);
            }
            else if (p.Value.GetType() == typeof(float[]))
            {
                comp.SetFloats(p.Key, (float[])p.Value);
            }
            else if(p.Value.GetType() == typeof(ComputeBuffer))
            {
                comp.SetBuffer(kernelIndex, p.Key, (ComputeBuffer)p.Value);
            }
        }
    }

    private static RenderTexture GenerateRenderTexture(Texture input)
    {
        var result = new RenderTexture(input.width, input.height, 0, RenderTextureFormat.ARGB32);
        result.enableRandomWrite = true;
        result.filterMode = FilterMode.Point;
        result.Create();
        return result;
    }
    private static RenderTexture GenerateRenderTexture(Texture input, int[] grid)
    {
        var result = new RenderTexture(input.width / grid[0], input.height / grid[1], 0, RenderTextureFormat.ARGB32);
        result.enableRandomWrite = true;
        result.filterMode = FilterMode.Point;
        result.Create();
        return result;
    }
}

