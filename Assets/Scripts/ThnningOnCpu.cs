using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThnningOnCpu : MonoBehaviour
{
    [SerializeField] RawImage _beforeImage = null;
    [SerializeField] RawImage _afterImage = null;

    //[SerializeField] private bool _alphaOn = false;
    //[SerializeField] [Range(1, 10)] int _thinningIteration = 5;


    private void Start()
    {

    }

    private void OnValidate()
    {
        if (_beforeImage == null || _afterImage == null)
        {
            return;
        }
        else 
        { 
            _afterImage.texture = null; 
        }

        Texture mainTexture = _beforeImage.texture;
        Texture2D texture2D = new Texture2D(mainTexture.width, mainTexture.height, TextureFormat.RGBA32, false);

        RenderTexture currentRT = RenderTexture.active;

        RenderTexture renderTexture = new RenderTexture(mainTexture.width, mainTexture.height, 32);
        Graphics.Blit(mainTexture, renderTexture);

        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        RenderTexture.active = currentRT;


        _afterImage.texture = CpuTextureEditor.CalculateEachPixel(texture2D, ThinningAlgorithm);

    }

    private Color ThinningAlgorithm(Texture2D input, Vector2Int coord)
    {
        Color center = input.GetPixel(coord.x, coord.y);


        return center;
    }
}
