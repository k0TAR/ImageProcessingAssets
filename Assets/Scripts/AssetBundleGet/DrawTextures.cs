using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class DrawTextures : MonoBehaviour
{
    [SerializeField] private RawImage _img = null;
    private const int uvCountSide = 17;

    private Vector2[,] uvs = new Vector2[uvCountSide, uvCountSide];
    private Texture[,] textures;


    private async void Awake()
    {
        Texture[] result = null;

        StartCoroutine(
            GetAssetFromAssetBundle.GetAssetsAsync<Texture>(
                "AssetBundles/StandaloneWindows64/lightfield",
                (r) => result = r
            )
        );
        

        await Task.Run(() => {
            while (result == null);
        });

        _img.texture = result[0];

        for (int i = 0; i < uvCountSide; i++)
        {
            for(int j = 0; j < uvCountSide; j++)
            {
                var tex = result[i * uvCountSide + j];
                var numberArray = Regex.Matches(tex.name, @"[+-]?\d+(?:\.\d+)?")
                .Cast<Match>()
                .Select(m => float.Parse(m.Value))
                .ToArray();

                uvs[i,j] = new Vector2(numberArray[2], numberArray[3]);
                Debug.Log(uvs[i,j]);
            }
        }

    }
}
