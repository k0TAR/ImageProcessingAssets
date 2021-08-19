using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThnningOnCpu : MonoBehaviour
{
    [SerializeField] RawImage _beforeImage = null;
    [SerializeField] RawImage _afterImage = null;

    [SerializeField] private bool _alphaOn = false;
    //[SerializeField] [Range(1, 10)] int _thinningIteration = 5;


    private void Start()
    {
        _alphaOn = !_alphaOn;
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
        if (input.GetPixel(coord.x, coord.y).r == 0) return input.GetPixel(coord.x, coord.y);

        int width = input.width;
        int height = input.height;

        List<FilterRules> eliminationRules = CreateEliminationRules();

        int amountSetPixel8Neighbour = Count8NeighboursWithSetBits(input, coord);
        bool eliminate = false;

        if(1 < amountSetPixel8Neighbour && amountSetPixel8Neighbour < 8)
        {
            foreach (var elimRule in eliminationRules[amountSetPixel8Neighbour - 2]._rules)
            {
                bool matchsFilter = true;

                for (int i = -1; i < 2 && matchsFilter; i++)
                {
                    for (int j = -1; j < 2 && matchsFilter; j++)
                    {
                        float value;
                        if (0 < coord.x + i && coord.x + i < width && 0 < coord.y + j && coord.y + j < height)
                        {
                            value = input.GetPixel(coord.x + i, coord.y + j).r;
                        }
                        else
                        {
                            value = 0;
                        }
                        matchsFilter &= value == elimRule[j * -1 + 1,i + 1];
                    }
                }
                if(amountSetPixel8Neighbour - 2 == 5 && eliminate)
                {
                    Debug.Log(coord);
                }
                eliminate |= matchsFilter;
                if (eliminate) break;
            }
        }
        

        

        Color centerColor = eliminate ? new Color(0, 1, 0, 1) : input.GetPixel(coord.x, coord.y);


        return centerColor;
    }

    private struct FilterRules
    {
        public List<Matrix4x4> _rules;
        public List<Vector2Int> _centers;

        public FilterRules(List<Matrix4x4> rules, List<Vector2Int> centers)
        {
            _rules = rules;
            _centers = centers;
        }
    }

    private List<FilterRules> CreateEliminationRules()
    {
        List<FilterRules> eliminationRules = new List<FilterRules>();

        List<Matrix4x4> eliminationRule2 = new List<Matrix4x4> {
            new Matrix4x4(){
                m00=1,m01=1,m02=0,m03=0,
                m10=0,m11=1,m12=0,m13=0,
                m20=0,m21=0,m22=0,m23=0
            },
            new Matrix4x4(){
                m00=0,m01=1,m02=1,m03=0,
                m10=0,m11=1,m12=0,m13=0,
                m20=0,m21=0,m22=0,m23=0
            },
            new Matrix4x4(){
                m00=0,m01=0,m02=1,m03=0,
                m10=0,m11=1,m12=1,m13=0,
                m20=0,m21=0,m22=0,m23=0
            },
            new Matrix4x4(){
                m00=0,m01=0,m02=0,m03=0,
                m10=0,m11=1,m12=1,m13=0,
                m20=0,m21=0,m22=1,m23=0
            },
            new Matrix4x4(){
                m00=0,m01=0,m02=0,m03=0,
                m10=0,m11=1,m12=0,m13=0,
                m20=0,m21=1,m22=1,m23=0
            },
            new Matrix4x4(){
                m00=0,m01=0,m02=0,m03=0,
                m10=0,m11=1,m12=0,m13=0,
                m20=1,m21=1,m22=0,m23=0
            },
            new Matrix4x4(){
                m00=0,m01=0,m02=0,m03=0,
                m10=1,m11=1,m12=0,m13=0,
                m20=1,m21=0,m22=0,m23=0
            },
            new Matrix4x4(){
                m00=1,m01=0,m02=0,m03=0,
                m10=1,m11=1,m12=0,m13=0,
                m20=0,m21=0,m22=0,m23=0
            }
        };
        List<Vector2Int> center2 = new List<Vector2Int> { 
            new Vector2Int(1,1),
            new Vector2Int(1,1),
            new Vector2Int(1,1),
            new Vector2Int(1,1),
            new Vector2Int(1,1),
            new Vector2Int(1,1),
            new Vector2Int(1,1),
            new Vector2Int(1,1),
        };
        eliminationRules.Add( new FilterRules(eliminationRule2, center2) );

        List<Matrix4x4> eliminationRule3 = new List<Matrix4x4> {
            new Matrix4x4(){
                m00=1,m01=1,m02=1,m03=0,
                m10=0,m11=1,m12=0,m13=0,
                m20=0,m21=0,m22=0,m23=0
            },
            new Matrix4x4(){
                m00=0,m01=1,m02=1,m03=0,
                m10=0,m11=1,m12=1,m13=0,
                m20=0,m21=0,m22=0,m23=0
            },
            new Matrix4x4(){
                m00=0,m01=0,m02=1,m03=0,
                m10=0,m11=1,m12=1,m13=0,
                m20=0,m21=0,m22=1,m23=0
            },
            new Matrix4x4(){
                m00=0,m01=0,m02=0,m03=0,
                m10=0,m11=1,m12=1,m13=0,
                m20=0,m21=1,m22=1,m23=0
            },
            new Matrix4x4(){
                m00=0,m01=0,m02=0,m03=0,
                m10=0,m11=1,m12=0,m13=0,
                m20=1,m21=1,m22=1,m23=0
            },
            new Matrix4x4(){
                m00=0,m01=0,m02=0,m03=0,
                m10=1,m11=1,m12=0,m13=0,
                m20=1,m21=1,m22=0,m23=0
            },
            new Matrix4x4(){
                m00=1,m01=0,m02=0,m03=0,
                m10=1,m11=1,m12=0,m13=0,
                m20=1,m21=0,m22=0,m23=0
            },
            new Matrix4x4(){
                m00=1,m01=1,m02=0,m03=0,
                m10=1,m11=1,m12=0,m13=0,
                m20=0,m21=0,m22=0,m23=0
            },

            new Matrix4x4(){
                m00=0,m01=1,m02=0,m03=0,
                m10=1,m11=1,m12=0,m13=0,
                m20=1,m21=0,m22=0,m23=0
            },
            new Matrix4x4(){
                m00=0,m01=1,m02=0,m03=0,
                m10=0,m11=1,m12=1,m13=0,
                m20=0,m21=0,m22=1,m23=0
            },
            new Matrix4x4(){
                m00=1,m01=1,m02=0,m03=0,
                m10=0,m11=1,m12=1,m13=0,
                m20=0,m21=0,m22=0,m23=0
            },
            new Matrix4x4(){
                m00=0,m01=1,m02=1,m03=0,
                m10=1,m11=1,m12=0,m13=0,
                m20=0,m21=0,m22=0,m23=0
            },
        };
        List<Vector2Int> center3 = center2;
        eliminationRules.Add(new FilterRules(eliminationRule3, center3));

        List<Matrix4x4> eliminationRule4 = new List<Matrix4x4> {
            new Matrix4x4(){
                m00=1,m01=1,m02=1,m03=0,
                m10=0,m11=1,m12=1,m13=0,
                m20=0,m21=0,m22=0,m23=0
            },
            new Matrix4x4(){
                m00=0,m01=0,m02=1,m03=0,
                m10=0,m11=1,m12=1,m13=0,
                m20=0,m21=1,m22=1,m23=0
            },
            new Matrix4x4(){
                m00=0,m01=0,m02=0,m03=0,
                m10=1,m11=1,m12=0,m13=0,
                m20=1,m21=1,m22=1,m23=0
            },
            new Matrix4x4(){
                m00=1,m01=1,m02=0,m03=0,
                m10=1,m11=1,m12=0,m13=0,
                m20=1,m21=0,m22=0,m23=0
            },
            new Matrix4x4(){
                m00=1,m01=1,m02=1,m03=0,
                m10=1,m11=1,m12=0,m13=0,
                m20=0,m21=0,m22=0,m23=0
            },
            new Matrix4x4(){
                m00=0,m01=1,m02=1,m03=0,
                m10=0,m11=1,m12=1,m13=0,
                m20=0,m21=0,m22=1,m23=0
            },
            new Matrix4x4(){
                m00=0,m01=0,m02=0,m03=0,
                m10=0,m11=1,m12=1,m13=0,
                m20=1,m21=1,m22=1,m23=0
            },
            new Matrix4x4(){
                m00=1,m01=0,m02=0,m03=0,
                m10=1,m11=1,m12=0,m13=0,
                m20=1,m21=1,m22=0,m23=0
            },

            new Matrix4x4(){
                m00=0,m01=1,m02=1,m03=0,
                m10=1,m11=1,m12=0,m13=0,
                m20=1,m21=0,m22=0,m23=0
            },
            new Matrix4x4(){
                m00=1,m01=1,m02=0,m03=0,
                m10=0,m11=1,m12=1,m13=0,
                m20=0,m21=0,m22=1,m23=0
            },
        };
        List<Vector2Int> center4 = center2;
        eliminationRules.Add(new FilterRules(eliminationRule4, center4));

        List<Matrix4x4> eliminationRule5 = new List<Matrix4x4> {
            new Matrix4x4(){
                m00=1,m01=1,m02=1,m03=0,
                m10=0,m11=1,m12=1,m13=0,
                m20=0,m21=0,m22=1,m23=0
            },
            new Matrix4x4(){
                m00=0,m01=0,m02=1,m03=0,
                m10=0,m11=1,m12=1,m13=0,
                m20=1,m21=1,m22=1,m23=0
            },
            new Matrix4x4(){
                m00=1,m01=0,m02=0,m03=0,
                m10=1,m11=1,m12=0,m13=0,
                m20=1,m21=1,m22=1,m23=0
            },
            new Matrix4x4(){
                m00=1,m01=1,m02=1,m03=0,
                m10=1,m11=1,m12=0,m13=0,
                m20=1,m21=0,m22=0,m23=0
            },

            new Matrix4x4(){
                m00=1,m01=1,m02=1,m03=0,
                m10=1,m11=1,m12=1,m13=0,
                m20=0,m21=0,m22=0,m23=0
            },
            new Matrix4x4(){
                m00=0,m01=1,m02=1,m03=0,
                m10=0,m11=1,m12=1,m13=0,
                m20=0,m21=1,m22=1,m23=0
            },
            new Matrix4x4(){
                m00=0,m01=0,m02=0,m03=0,
                m10=1,m11=1,m12=1,m13=0,
                m20=1,m21=1,m22=1,m23=0
            },
            new Matrix4x4(){
                m00=1,m01=1,m02=0,m03=0,
                m10=1,m11=1,m12=0,m13=0,
                m20=1,m21=1,m22=0,m23=0
            },
        };
        List<Vector2Int> center5 = center2;
        eliminationRules.Add(new FilterRules(eliminationRule5, center5));

        List<Matrix4x4> eliminationRule6 = new List<Matrix4x4> {
            new Matrix4x4(){
                m00=1,m01=1,m02=1,m03=0,
                m10=1,m11=1,m12=1,m13=0,
                m20=0,m21=0,m22=1,m23=0
            },
            new Matrix4x4(){
                m00=0,m01=1,m02=1,m03=0,
                m10=0,m11=1,m12=1,m13=0,
                m20=1,m21=1,m22=1,m23=0
            },
            new Matrix4x4(){
                m00=1,m01=0,m02=0,m03=0,
                m10=1,m11=1,m12=1,m13=0,
                m20=1,m21=1,m22=1,m23=0
            },
            new Matrix4x4(){
                m00=1,m01=1,m02=1,m03=0,
                m10=1,m11=1,m12=0,m13=0,
                m20=1,m21=1,m22=0,m23=0
            },

            new Matrix4x4(){
                m00=1,m01=1,m02=1,m03=0,
                m10=1,m11=1,m12=1,m13=0,
                m20=1,m21=0,m22=0,m23=0
            },
            new Matrix4x4(){
                m00=1,m01=1,m02=1,m03=0,
                m10=0,m11=1,m12=1,m13=0,
                m20=0,m21=1,m22=1,m23=0
            },
            new Matrix4x4(){
                m00=0,m01=0,m02=1,m03=0,
                m10=1,m11=1,m12=1,m13=0,
                m20=1,m21=1,m22=1,m23=0
            },
            new Matrix4x4(){
                m00=1,m01=1,m02=0,m03=0,
                m10=1,m11=1,m12=0,m13=0,
                m20=1,m21=1,m22=1,m23=0
            },
        };
        List<Vector2Int> center6 = center2;
        eliminationRules.Add(new FilterRules(eliminationRule6, center6));


        List<Matrix4x4> eliminationRule7 = new List<Matrix4x4> {
            new Matrix4x4(){
                m00=1,m01=1,m02=1,m03=0,
                m10=1,m11=1,m12=1,m13=0,
                m20=1,m21=0,m22=1,m23=0
            },
            new Matrix4x4(){
                m00=1,m01=1,m02=1,m03=0,
                m10=0,m11=1,m12=1,m13=0,
                m20=1,m21=1,m22=1,m23=0
            },
            new Matrix4x4(){
                m00=1,m01=0,m02=1,m03=0,
                m10=1,m11=1,m12=1,m13=0,
                m20=1,m21=1,m22=1,m23=0
            },
            new Matrix4x4(){
                m00=1,m01=1,m02=1,m03=0,
                m10=1,m11=1,m12=0,m13=0,
                m20=1,m21=1,m22=1,m23=0
            },
        };
        List<Vector2Int> center7 = center2;
        eliminationRules.Add(new FilterRules(eliminationRule7, center7));


        return eliminationRules;
    }

    int Count8NeighboursWithSetBits(Texture2D input, Vector2Int pos)
    {
        int amount = 0;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (i == 0 & j == 0)
                {
                    continue;
                }
                amount += (int) input.GetPixel(pos.x + i, pos.y + j).r;
            }
        }
        return amount;
    }
}
