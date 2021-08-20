using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureExtensions
{
    public static RenderTexture ToRenderTexture(this Texture input)
    {
        var result = new RenderTexture(input.width, input.height, 0, RenderTextureFormat.ARGB32);
        result.enableRandomWrite = true;
        result.filterMode = FilterMode.Point;
        Graphics.Blit(input, result);
        result.Create();
        return result;
    }

    public static RenderTexture ToRenderTexture(this Texture input, int[] grid)
    {
        var result = new RenderTexture(input.width / grid[0], input.height / grid[1], 0, RenderTextureFormat.ARGB32);
        result.enableRandomWrite = true;
        result.filterMode = FilterMode.Point;
        Graphics.Blit(input, result);
        result.Create();
        return result;
    }

    public static RenderTexture GenerateCleanRenderTexture(this Texture input)
    {
        var result = new RenderTexture(input.width, input.height, 0, RenderTextureFormat.ARGB32);
        result.enableRandomWrite = true;
        result.filterMode = FilterMode.Point;
        result.Create();
        return result;
    }
    public static RenderTexture GenerateCleanRenderTexture(this Texture input, int[] grid)
    {
        var result = new RenderTexture(input.width / grid[0], input.height / grid[1], 0, RenderTextureFormat.ARGB32);
        result.enableRandomWrite = true;
        result.filterMode = FilterMode.Point;
        result.Create();
        return result;
    }

    public static Texture2D ToTexture2D(this Texture input)
    {
        var rTexture = input.ToRenderTexture();
        return rTexture.ToTexture2D();
    }

    public static Texture2D GenerateCleanTexture2D(this Texture input)
    {
        var rTexture = input.GenerateCleanRenderTexture();
        return rTexture.ToTexture2D();
    }
}
