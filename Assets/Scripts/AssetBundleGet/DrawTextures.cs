using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class DrawTextures : MonoBehaviour
{
    [SerializeField] private RawImage _img = null;

    private async void Start()
    {
        Texture[] result = null;

        StartCoroutine(
            GetAssetFromAssetBundle.GetAssetsAsync<Texture>(
                "AssetBundles/StandaloneWindows64/lightfield",
                (r) => result = r
            )
        );
        

        _img.texture = await Task.Run(() => {
            while (result == null);
            return result[0];
        }); ;
    }
}
