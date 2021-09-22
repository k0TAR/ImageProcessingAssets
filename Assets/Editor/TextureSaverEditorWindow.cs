using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEngine.UI;

public class TextureSaverEditorWindow : EditorWindow
{
    [MenuItem("Editor/TextureSave")]
    private static void Create()
    {
        // 生成
        GetWindow<TextureSaverEditorWindow>("Save Texture");
    }

    Texture2D _savingTexture = null;
    RawImage _savingRawImage = null;
    string ASSET_PATH = "";
    string _texture_name = "";

    private void OnGUI()
    {
        ASSET_PATH = $"{Application.dataPath}/Textures/";
        Color defaultColor = GUI.backgroundColor;
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUI.backgroundColor = Color.gray;
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Select Texture");
            }
            GUI.backgroundColor = defaultColor;

            _savingRawImage = (RawImage)EditorGUILayout.ObjectField(
            "Saving Texture:",
            _savingRawImage,
            typeof(RawImage),
            true);
        }
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUI.backgroundColor = Color.gray;
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Saving Directory");
            }
            GUI.backgroundColor = defaultColor;

            

            using (new GUILayout.HorizontalScope(GUI.skin.box))
            {
                _texture_name = EditorGUILayout.TextField("Texture Name", _texture_name);
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Save Texture"))
                {
                    if(_savingRawImage != null && _texture_name.Length > 0)
                    {
                        SaveImage(_savingRawImage.texture.ToTexture2D(), _texture_name);
                    }
                }
            }
            GUI.backgroundColor = Color.yellow;

            if (_savingRawImage == null)
            {
                EditorGUILayout.HelpBox("Select Texture", MessageType.Error);
            }
            if (_texture_name.Length <= 0)
            {
                EditorGUILayout.HelpBox("Type in Saving File Name", MessageType.Error);
            }

            GUI.backgroundColor = Color.gray;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.wordWrap = true;
            GUILayout.Label("PATH: " + ASSET_PATH + _texture_name + ".png", style);
        }
    }

    public void SaveImage(Texture2D savingTexture, string name)
    {
        File.WriteAllBytes(
            ASSET_PATH + $"{name}.png",
            savingTexture.EncodeToPNG());
    }
}
