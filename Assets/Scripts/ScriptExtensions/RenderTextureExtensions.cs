using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RenderTextureExtensions
{
    public static Texture2D ToTexture2D(this RenderTexture rTex)
    {
        Texture2D texture2D = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBA32, false);

        var currentActiveRT = RenderTexture.active;
        RenderTexture.active = rTex;

        texture2D.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        texture2D.filterMode = FilterMode.Point;
        texture2D.Apply();

        RenderTexture.active = currentActiveRT;
        return texture2D;
    }
}
