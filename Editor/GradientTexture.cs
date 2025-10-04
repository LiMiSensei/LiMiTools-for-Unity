using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class GradientTexture : EditorWindow
{
    

    [MenuItem("LiMi的工具/渐变条工具 V1.1")]
    static void Open()
    {
        var window = GetWindow<GradientTexture>("渐变图生成器");
        
        window.Show();
    }
    
    private Gradient[] _gradient;//要初始化并填充数据
    private Texture2D _previewTexture;
   
    private int HeightSacle = 1;
    Material _material;
    private string _texturePath = "Assets/Texture2D";
    string _textureName = "Remap";
    bool isSelected = false;
    bool isPreview = false;
    bool isFG_TH = false;
    bool isQZSX = false;
    Vector2Int textureSize = new Vector2Int(128, 1);
    
    FilterMode _myEnum = FilterMode.Point;
    

    void OnEnable()
    {
        _gradient = new Gradient[10];
        for (int i = 0; i < _gradient.Length; i++)
        {
            _gradient[i] = new Gradient();
        }
       
    }
    void OnGUI()
    {
        // 1. 渐变图选择控件
        if (!isPreview)
        {
            for (int i = 0; i < HeightSacle; i++)
            {
                _gradient[HeightSacle - 1 - i] = EditorGUILayout.GradientField("渐变图", _gradient[HeightSacle - 1 - i]);
            }
        }
        // 2. 图像设置
        EditorGUILayout.BeginVertical("box");
            isPreview = EditorGUILayout.BeginFoldoutHeaderGroup(isPreview, "图像设置");
                if (isPreview)
                {
                    EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("长度：",GUILayout.Width(30));
                        textureSize.x =  Mathf.Clamp(EditorGUILayout.IntField(  textureSize.x,GUILayout.Width(200)),1,4096);
                        GUILayout.Label("长度：",GUILayout.Width(30));
                        textureSize.y =   Mathf.Clamp(EditorGUILayout.IntField(  textureSize.y,GUILayout.Width(200)),1,4096);
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("渐变条数量&宽度倍率");
                        HeightSacle = EditorGUILayout.IntSlider(HeightSacle, 1, 10);
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                        
                        _myEnum = (FilterMode)EditorGUILayout.EnumPopup("图像过滤模式：",_myEnum);
                       
                    EditorGUILayout.EndHorizontal();
                }
            EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.EndVertical();
        //4. 名称
        EditorGUILayout.BeginHorizontal();
        _textureName = EditorGUILayout.TextArea(_textureName);
        if (GUILayout.Button("选择名称"))
        {
            _textureName = Selection.activeObject.name;
        }

        if(GUILayout.Button(isFG_TH?"替换图像":"覆盖图像"))
        {
            isFG_TH = !isFG_TH;
        }
        EditorGUILayout.EndHorizontal();
        // 3. 路劲以及保存
        EditorGUILayout.BeginHorizontal("box");
            _texturePath =  EditorGUILayout.TextField( _texturePath);
            if (GUILayout.Button("选择路径"))
            {
                 _texturePath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(Selection.activeObject));
            }
            if (GUILayout.Button("应用&保存"))
            {
                TextureSave();
            }
        EditorGUILayout.EndHorizontal();
        isQZSX = GUILayout.Toggle(isQZSX,"强制刷新");
        double editortime = EditorApplication.timeSinceStartup;
        
        if (isQZSX && Mathf.Sin((float)editortime) == 0.98)
        {
            Repaint();
            TextureSave();
        }
        //5. 显示
        EditorGUILayout.LabelField("图像大小："+textureSize.x+"X"+textureSize.y*HeightSacle);
        if (_previewTexture != null)
        {
            EditorGUILayout.LabelField("Main Texture:",style:"box");
            Rect texRect = GUILayoutUtility.GetRect(100, 100);
            EditorGUI.DrawPreviewTexture(texRect, _previewTexture);
        }
       //6. 作者信息
       GUILayout.Label("© 2023 YourName", EditorStyles.miniLabel);
       EditorGUILayout.LabelField("© 2023 YourName", EditorStyles.miniLabel);
        
    }


    void TextureSave()
    {
        string path = "";
        //制作
        Color[] colors = new Color[textureSize.x *  textureSize.y *  HeightSacle];
        _previewTexture = new Texture2D(textureSize.x, textureSize.y *  HeightSacle, TextureFormat.ARGB32, false);
        _previewTexture.filterMode = _myEnum;
        int cache = 0;
        for (int i = 0; i < HeightSacle; i++) //10
        {
            for (int j = 0; j < textureSize.y; j++) //128
            {
                for (int k = 0; k < textureSize.x; k++)//1024
                {
                    colors[cache] = _gradient[i].Evaluate((float)k / textureSize.x);
                    cache++;
                }
                
            }
        }
        _previewTexture.SetPixels(colors);
        _previewTexture.Apply();

        //保存
        byte[] pngData = _previewTexture.EncodeToPNG();
        if (pngData != null)
        {

            if (_textureName.Length == 0)
            {
                _textureName = "Remap";
            }
            string fullPath = Path.Combine(_texturePath,_textureName+".png");
            File.WriteAllBytes(fullPath, pngData);
            AssetDatabase.Refresh();
            path = fullPath;
            Debug.Log(path);
        }

        if (!isQZSX)
        {
            //ping
            Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
            EditorGUIUtility.PingObject(obj);
        }
        

    }
    
}
