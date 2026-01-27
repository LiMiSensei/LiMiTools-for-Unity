using UnityEditor;
using UnityEngine;

public class TextureSelectionExample : EditorWindow
{
    private Texture2D _texture1;
    private Texture2D _texture2;
    private Texture2D _texture3;
    private Texture2D _texture4;

    [MenuItem("LiMi/测试/Texture Selection Example")]
    public static void ShowWindow()
    {
        GetWindow<TextureSelectionExample>("Texture Selection Example");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("方法1: 使用 EditorGUILayout.ObjectField");
        _texture1 = (Texture2D)EditorGUILayout.ObjectField("纹理1", _texture1, typeof(Texture2D), false);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("方法2: 使用按钮和文件对话框");
        EditorGUILayout.BeginHorizontal();
        _texture2 = (Texture2D)EditorGUILayout.ObjectField("纹理2", _texture2, typeof(Texture2D), false);
        if (GUILayout.Button("选择", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFilePanel("选择纹理", "Assets", "png,jpg,jpeg,bmp,tga");
            if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
                _texture2 = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("方法3: 使用弹出对象选择器");
        EditorGUILayout.BeginHorizontal();
        _texture3 = (Texture2D)EditorGUILayout.ObjectField("纹理3", _texture3, typeof(Texture2D), false);
        if (GUILayout.Button("弹出", GUILayout.Width(60)))
        {
            EditorGUIUtility.ShowObjectPicker<Texture2D>(_texture3, false, "", EditorGUIUtility.GetControlID(FocusType.Passive));
        }
        EditorGUILayout.EndHorizontal();

        // 处理对象选择器的回调
        if (Event.current.commandName == "ObjectSelectorUpdated")
        {
            _texture3 = EditorGUIUtility.GetObjectPickerObject() as Texture2D;
            Repaint();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("方法4: 使用自定义纹理选择窗口");
        EditorGUILayout.BeginHorizontal();
        _texture4 = (Texture2D)EditorGUILayout.ObjectField("纹理4", _texture4, typeof(Texture2D), false);
        if (GUILayout.Button("从文件夹选择", GUILayout.Width(100)))
        {
            //TextureSelectorWindow.ShowWindow("Assets", (texture) =>
            //{
            //    _texture4 = texture;
            //    Repaint();
            //});
        }
        EditorGUILayout.EndHorizontal();

        // 显示预览
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("预览");
        Texture2D[] textures = { _texture1, _texture2, _texture3, _texture4 };
        for (int i = 0; i < textures.Length; i++)
        {
            if (textures[i] != null)
            {
                EditorGUILayout.LabelField($"纹理{i+1}: {textures[i].name}");
                Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(100));
                EditorGUI.DrawPreviewTexture(rect, textures[i]);
            }
        }
    }
}