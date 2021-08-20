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
        Texture2D result = inputTexture.GenerateCleanTexture2D();

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

}
