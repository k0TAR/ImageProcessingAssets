using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public static class GetAssetFromAssetBundle
{
    private static string _asset_bundle_path = Application.streamingAssetsPath + "/";

    public static IEnumerator GetAssetsAsync<T>(string path, Action<T[]> Callback)
    {
        
        Debug.Log("START GETTING ASSETS.");
        
        AssetBundleCreateRequest loadFileAsync = AssetBundle.LoadFromFileAsync(_asset_bundle_path + path);
        while (!loadFileAsync.isDone)
        {
            Debug.Log("LoadFromFileAsync progress: " + (loadFileAsync.progress * 100.0f) + "%");
            yield return null;
        }
        AssetBundle assetBundle = loadFileAsync.assetBundle;
        AssetBundleRequest loadAssetAsync = assetBundle.LoadAllAssetsAsync<T>();
        while (!loadAssetAsync.isDone)
        {
            Debug.Log("LoadAllAssetsAsync progress: " + (loadAssetAsync.progress * 100.0f) + "%");
            yield return null;
        }
        T[] assets = loadAssetAsync.allAssets.Cast<T>().ToArray();

        Callback(assets);
        Debug.Log("GETTING ASSETS END.");
    }

    public static void GetAssets<T>(string path, Action<T[]> Callback)
    {

    }
}
