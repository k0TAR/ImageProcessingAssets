using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AssetBundleEditor : EditorWindow
{
    static string _root_path = "AssetBundles";

    [SerializeField] private UnityEngine.Object _inputAssetFolder = null;
    [SerializeField] private string _assetBundleName = "";

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
                GUILayout.Label("Assign asset folder that is going to be bundled.");
            }
            GUI.backgroundColor = defaultColor;
            using (new GUILayout.HorizontalScope())
            {
                var editorSO = new SerializedObject(this);
                editorSO.Update();
                EditorGUILayout.PropertyField(editorSO.FindProperty("_inputAssetFolder"), true);
                editorSO.ApplyModifiedProperties();
            }
        }

        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUI.backgroundColor = Color.gray;
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Input asset bundle name.");
            }
            GUI.backgroundColor = defaultColor;
            using (new GUILayout.HorizontalScope())
            {
                var editorSO = new SerializedObject(this);
                editorSO.Update();
                EditorGUILayout.PropertyField(editorSO.FindProperty("_assetBundleName"), true);
                editorSO.ApplyModifiedProperties();
            }
        }

        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUI.backgroundColor = Color.gray;
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Designated destination directory.");
            }
            GUI.backgroundColor = defaultColor;

            GUILayout.Label("Path: " + _root_path);

            using (new GUILayout.HorizontalScope(GUI.skin.box))
            {
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Build Asset Bundle"))
                {
                    BuildAssetBundlesForWindows64();
                }
                GUI.backgroundColor = defaultColor;
            }
        }

    }


    private void BuildAssetBundlesForWindows64()
    {
        BuildTarget target_platform = BuildTarget.StandaloneWindows64;

        var output_path = System.IO.Path.Combine(_root_path, target_platform.ToString());

        if (System.IO.Directory.Exists(output_path) == false)
        {
            System.IO.Directory.CreateDirectory(output_path);
        }

        string[] srcFilesPath = Directory.GetFileSystemEntries(AssetDatabase.GetAssetPath(_inputAssetFolder));
        foreach (string srcFilePath in srcFilesPath)
        {
            string fileName = Path.GetFileNameWithoutExtension(srcFilePath);
            if (srcFilePath.EndsWith("meta")) continue;
            AssetImporter importer = AssetImporter.GetAtPath(srcFilePath);
            importer.assetBundleName = _assetBundleName;
            importer.SaveAndReimport();
            if (importer != null)
            {
                importer.SetAssetBundleNameAndVariant(_assetBundleName, "");
            }
        }


        BuildPipeline.BuildAssetBundles(
                output_path,
                BuildAssetBundleOptions.ChunkBasedCompression,
                target_platform
            );

        foreach (string srcFilePath in srcFilesPath)
        {
            AssetImporter importer = AssetImporter.GetAtPath(srcFilePath);
            if (importer != null)
            {
                importer.SetAssetBundleNameAndVariant("", "");
            }
        }
    }
}
