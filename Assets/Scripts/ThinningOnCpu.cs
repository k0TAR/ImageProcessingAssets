using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThinningOnCpu : MonoBehaviour
{
    [SerializeField] RawImage _beforeImage = null;
    [SerializeField] RawImage _afterImage = null;

    //[SerializeField] private bool _alphaOn = false;
    [SerializeField] [Range(1, 10)] int _thinningIteration = 5;


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

        var result = CpuTextureEditor.CalculateEachPixel(_beforeImage.texture.ToTexture2D(), ThinningAlgorithm);
        for (int i = 1; i < _thinningIteration; i++)
        {
            result = CpuTextureEditor.CalculateEachPixel(result, ThinningAlgorithm);

        }
        _afterImage.texture = result;
    }

    private Color ThinningAlgorithm(Texture2D input, Vector2Int coord)
    {
        if (input.GetPixel(coord.x, coord.y).r == 0) return input.GetPixel(coord.x, coord.y);

        int width = input.width;
        int height = input.height;

        bool changeColor = false;
        bool noElimColor = false;

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
                        if (0 <= coord.x + i && coord.x + i < width && 0 <= coord.y + j && coord.y + j < height)
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
                
                eliminate |= matchsFilter;
                if (amountSetPixel8Neighbour - 2 == 4 && eliminate)
                {
                    changeColor = true;
                }
                if (eliminate) break;
            }
        }

        if (eliminate)
        {
            FilterRules noEliminationRules = CreateNoEliminationRules();
            for(int k = 0; k < noEliminationRules._rules.Count; k++)
            {
                bool matchsFilter = true;

                //default center pixel is i = 0, j = 0.
                //where 1 is default center.
                // 0 | 0 | 0 | 0
                // 0 | 1 | 0 | 0
                // 0 | 0 | 0 | 0
                // 0 | 0 | 0 | 0
                for (int i = -1; i < 3 && matchsFilter; i++)
                {
                    for (int j = -2; j < 2 && matchsFilter; j++)
                    {
                        float value;
                        if (0 <= coord.x + i && coord.x + i < width && 0 <= coord.y + j && coord.y + j < height)
                        {
                            value = input.GetPixel(
                                coord.x + i + (noEliminationRules._centers[k].x - 1),
                                coord.y + j + (noEliminationRules._centers[k].y - 1)
                                ).r;
                        }
                        else
                        {
                            value = 0;
                        }

                        if(noEliminationRules._rules[k][j * -1 + 1, i + 1] == -1)
                        {
                            continue;
                        }

                        matchsFilter &= value == noEliminationRules._rules[k][j * -1 + 1, i + 1];
                    }
                }

                eliminate &= !matchsFilter;
                if (!eliminate) {
                    noElimColor = true;
                    break;
                }
            }
        }
        

        Color centerColor = eliminate ? new Color(0, .2f, 0, 1) : input.GetPixel(coord.x, coord.y);

        centerColor = changeColor ? new Color(0,0,.4f,1) : centerColor;
        centerColor = noElimColor ? new Color(1, 0.6f, 0.4f) : centerColor;
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

    private FilterRules CreateNoEliminationRules()
    {
        List<Matrix4x4> noEliminationRules = new List<Matrix4x4> {
            new Matrix4x4(){
                m00=-1,m01=0,m02=-1,m03=-1,
                m10=1,m11=1,m12=1,m13=-1,
                m20=1,m21=1,m22=1,m23=-1,
                m30=-1,m31=0,m32=-1,m33=-1
            },
            new Matrix4x4(){
                m00=-1,m01=0,m02=0,m03=-1,
                m10=1,m11=1,m12=0,m13=-1,
                m20=0,m21=1,m22=0,m23=-1,
                m30=0,m31=0,m32=-1,m33=-1
            },
            new Matrix4x4(){
                m00=-1,m01=0,m02=0,m03=-1,
                m10=0,m11=1,m12=0,m13=-1,
                m20=0,m21=1,m22=1,m23=-1,
                m30=0,m31=0,m32=-1,m33=-1
            },
            new Matrix4x4(){
                m00=-1,m01=0,m02=0,m03=0,
                m10=0,m11=1,m12=1,m13=0,
                m20=0,m21=0,m22=1,m23=-1,
                m30=-1,m31=-1,m32=-1,m33=-1
            },
            new Matrix4x4(){
                m00=-1,m01=1,m02=1,m03=-1,
                m10=0,m11=1,m12=1,m13=0,
                m20=-1,m21=1,m22=1,m23=-1,
                m30=-1,m31=-1,m32=-1,m33=-1
            },
            new Matrix4x4(){
                m00=0,m01=0,m02=0,m03=-1,
                m10=0,m11=1,m12=1,m13=0,
                m20=-1,m21=1,m22=0,m23=0,
                m30=-1,m31=-1,m32=-1,m33=-1
            },
            new Matrix4x4(){
                m00=0,m01=0,m02=0,m03=0,
                m10=0,m11=1,m12=1,m13=0,
                m20=0,m21=1,m22=1,m23=0,
                m30=0,m31=0,m32=0,m33=0
            },
        };
        List<Vector2Int> center = new List<Vector2Int> {
            new Vector2Int(1,1),
            new Vector2Int(2,1),
            new Vector2Int(1,1),
            new Vector2Int(1,1),
            new Vector2Int(1,1),
            new Vector2Int(1,2),
            new Vector2Int(1,1),
        };

        return new FilterRules(noEliminationRules, center);
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
