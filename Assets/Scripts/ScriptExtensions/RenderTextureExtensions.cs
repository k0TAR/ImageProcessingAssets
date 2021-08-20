using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RenderTextureExtensions
{
    public static Texture2D ToTexture2D(this RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.ARGB32, false);

        var currentActiveRT = RenderTexture.active;
        RenderTexture.active = rTex;

        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.filterMode = FilterMode.Point;
        tex.Apply();

        RenderTexture.active = currentActiveRT;
        return tex;
    }
}
