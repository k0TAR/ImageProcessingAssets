using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AssetBundleEditor : EditorWindow
{
    static string _root_path = "AssetBundles";
    static string _variant = "assetbundle";

    [SerializeField] private List<UnityEngine.Object> _inputAssets;

    [MenuItem("Asset Bundle/Build Settings(Windows64)", false)]
    private static void OpenWindow()
    {
        GetWindow<AssetBundleEditor>("Asset Bundle Editor");
    }

    private void OnGUI()
    {
        Color defaultColor = GUI.backgroundColor;

        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUI.backgroundColor = Color.gray;
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Assign assets that are going tobe bundled.");
            }
            GUI.backgroundColor = defaultColor;
            using (new GUILayout.HorizontalScope())
            {
                var so = new SerializedObject(this);
                so.Update();
                EditorGUILayout.PropertyField(so.FindProperty("_inputAssets"), true);
                so.ApplyModifiedProperties();
            }
        }
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUI.backgroundColor = Color.gray;
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Designate the destination directory.");
            }
            GUI.backgroundColor = defaultColor;

            GUILayout.Label("Path: " + _root_path);

            using (new GUILayout.HorizontalScope(GUI.skin.box))
            {
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Build Asset Bundle"))
                {
                    BuildAssetBundle();
                }
                GUI.backgroundColor = defaultColor;
            }
        }
    }

    private void BuildAssetBundle()
    {

    }

    static private void BuildAssetBundlesForWindows64()
    {
        BuildTarget target_platform = BuildTarget.StandaloneWindows64;

        var output_path = System.IO.Path.Combine(_root_path, target_platform.ToString());

        if (System.IO.Directory.Exists(output_path) == false)
        {
            System.IO.Directory.CreateDirectory(output_path);
        }

        var asset_bundle_build_list = new List<AssetBundleBuild>();
        foreach (string asset_bundle_name in AssetDatabase.GetAllAssetBundleNames())
        {
            var builder = new AssetBundleBuild();
            builder.assetBundleName = asset_bundle_name;
            builder.assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(builder.assetBundleName);
            builder.assetBundleVariant = _variant;
            asset_bundle_build_list.Add(builder);
        }

        if (asset_bundle_build_list.Count > 0)
        {
            BuildPipeline.BuildAssetBundles(
                output_path,
                asset_bundle_build_list.ToArray(),
                BuildAssetBundleOptions.ChunkBasedCompression,
                target_platform
            );
        }
    }
}
