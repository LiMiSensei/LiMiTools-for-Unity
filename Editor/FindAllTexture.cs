using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/* 日志：目前 material 和 shader 没有写
 * 修复：
 * 问题：法线无法正常显示
 * 
 */
public class FindAllTexture : EditorWindow
{
    [MenuItem("LiMi的工具/查找所有图像 V1.1")]
    static void FindAllTextureWindows()
    {
        var window = EditorWindow.GetWindow<FindAllTexture>("查找所有贴图");
        window.Show();
    }
    
    
    
    //全局变量
    private List<Texture2D> textureList = new List<Texture2D>();
    private List<Material> materialLisst = new List<Material>();
    private List<Shader> shaderList = new List<Shader>();
    private Vector2 scrollPosition;
    private string SearchPath = "Assets";
    private enum FileType
    {
        Texture2D = 0,
        Material = 1,
        Shader = 2
    }
    private FileType fileType;
    
    
    void OnGUI()
    {
        //用于显示的
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        {
            foreach (var texture in textureList)
            {
                DisplayTextureItem(texture);
            }
        }
        EditorGUILayout.EndScrollView();
        
        //刷新方法
        EditorGUILayout.BeginHorizontal();
        {
            if(GUILayout.Button("选择文件夹："))
            {
                SearchPath = SelectionPath();
            }
            SearchPath = EditorGUILayout.TextArea(SearchPath);
            fileType = (FileType)EditorGUILayout.EnumPopup("文件类型：", fileType);
            if (GUILayout.Button("刷新&查找"))
            {
                //清空列表
                textureList.Clear();
                
                //查找对应资产ID
                string t1 = "Texture2D";
                switch (fileType)
                {
                    case FileType.Texture2D:t1 =  "Texture2D";break;
                    case FileType.Material:t1 =  "Material";break;
                    case FileType.Shader:t1 =  "Shader";break;
                    default: t1 = "Texture2D";break;
                }
                string[] guids2 = AssetDatabase.FindAssets($"t:{t1}", new[] {SearchPath});
                
                //-将ID转路径 并加到List列表里面
                foreach (string guid in guids2)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    if (tex != null) textureList.Add(tex);
                }
            }
        }
        EditorGUILayout.EndHorizontal();
      
    }

    

    //显示方法
    void DisplayTextureItem(Texture2D texture)
    {
        EditorGUILayout.BeginHorizontal(GUI.skin.box);
        {
            GUILayout.Label(texture, GUILayout.Width(64), GUILayout.Height(64));
            EditorGUILayout.BeginVertical();
            {
                FileInfo fileInfo = new FileInfo(AssetDatabase.GetAssetPath(texture));
                var fileSizeBytes = fileInfo.Length;
                EditorGUILayout.LabelField("名称: " + texture.name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("储存: " + fileSizeBytes.ToString());
                EditorGUILayout.LabelField("分辨率: " + texture.width + "x" + texture.height);
                EditorGUILayout.LabelField("格式: " + texture.format);
                EditorGUILayout.LabelField("路径：" + AssetDatabase.GetAssetPath(texture));
                
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("选择", GUILayout.Width(60)))
                    {
                        Selection.activeObject = texture;
                        EditorGUIUtility.PingObject(texture);
                    }
                    if (GUILayout.Button("查看", GUILayout.Width(60)))
                    {
                    
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(1);
    }
    
    //返回选中的文件夹
    string SelectionPath()
    {
        // 1. 获取选中的资源（过滤掉非资产对象）
        Object[] selectedObjects = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
        // 2. 处理空选择情况
        if (selectedObjects.Length == 0)
        {
            return  "Assets";
        }
        // 3. 获取第一个选中对象的GUID和路径
        string guid = Selection.assetGUIDs[0];
        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
        // 4. 统一路径格式并处理文件夹/文件
        if (AssetDatabase.IsValidFolder(assetPath))
        {
            // 文件夹直接使用原路径（自动统一为正斜杠）
            return assetPath;
        }
        else
        {
            // 文件获取父目录并确保使用正斜杠
            return  Path.GetDirectoryName(assetPath).Replace("\\", "/");
        }
    }
  
}
