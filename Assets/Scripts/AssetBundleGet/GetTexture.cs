using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GetTexture : MonoBehaviour
{
    [SerializeField] RawImage _img = null;

    private void Start()
    {
        StartCoroutine(Download());
    }

    IEnumerator Download()
    {
        string asset_bundle_path = Application.streamingAssetsPath 
            + "/AssetBundles/StandaloneWindows64/lightfield";
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(asset_bundle_path);
        while (!request.isDone)
        {
            yield return null;
        }
        AssetBundle assetBundle = request.assetBundle;
        AssetBundleRequest images = assetBundle.LoadAllAssetsAsync<Texture>();
        Texture[] texs = images.allAssets.Cast<Texture>().ToArray();
        _img.texture = texs[0];
    }


}
