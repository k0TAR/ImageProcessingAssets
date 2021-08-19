using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CpuTextureEditor
{
    /// <summary>
    /// calculates the passed algorithm for each pixels in the texture on CPU.
    /// </summary>
    /// <param name="inputTexture">Input texture.</param>
    /// <param name="algorithm">The algorithm that will be applied. <br />
    /// Algorithm takes a texture (Texture2D), applying pixel coordinate (int2).</param>
    /// <returns>The result of the texture.</returns>
    public static Texture2D CalculateEachPixel(Texture2D inputTexture, System.Func<Texture2D, Vector2Int, Color> algorithm)
    {
        Texture2D result = ToTexture2D(GenerateRenderTexture(inputTexture));

        int width = inputTexture.width;
        int height = inputTexture.height;

        Vector2Int coord = new Vector2Int(0, 0);
        for (coord.x = 0; coord.x < width; coord += Vector2Int.right)
        {
            for(coord.y = 0; coord.y < height; coord += Vector2Int.up)
            {
                Color settingColor = algorithm(inputTexture, coord);
                result.SetPixel(coord.x, coord.y, settingColor);
            }
        }

        result.Apply();
        return result;
    }



    public static RenderTexture GenerateRenderTexture(Texture input)
    {
        var result = new RenderTexture(input.width, input.height, 0, RenderTextureFormat.ARGB32);
        result.enableRandomWrite = true;
        result.filterMode = FilterMode.Point;
        result.Create();
        return result;
    }

    public static Texture2D ToTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        
        // Remember currently active render texture
        RenderTexture currentActiveRT = RenderTexture.active;
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.filterMode = FilterMode.Point;
        tex.Apply();

        RenderTexture.active = currentActiveRT;
        return tex;
    }
}
