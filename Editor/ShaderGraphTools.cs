using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class ShaderGraphTools : EditorWindow
{
    [MenuItem("LiMi/工具/ShaderGraph优化工具 v1.0")]
    static void Open()
    {
        var window = EditorWindow.GetWindow<ShaderGraphTools>();
        window.titleContent.text = "ShaderGraph优化插件";
        window.Show();
        window.Scan();//直接找
    }
    //---------------数据-----------------
    string path = "Assets";                   //路径
    private string[] shaderGraphFiles = new string[]{};        //存放ShaderGraph文件
    
    //----------------功能---------------
    Vector2 scrollPos;        //窗口滚动区域
    string statusMessage = "";//功能提示
    bool isShuaXing = false;
    private bool isShsdowCast = false;
    void OnEnable()
    {
        
    }

    private void OnGUI()
    {
        //Help
        EditorGUILayout.HelpBox("遍历指定Asset文件夹的ShaderGraph文件 批量修改参数", MessageType.Info);
        //路劲选择
        EditorGUILayout.BeginHorizontal();
        {
            path = EditorGUILayout.TextArea(path);
            if (GUILayout.Button("选择路径")) path = SelectionPath();
        }
        EditorGUILayout.EndHorizontal();
        //重新扫描  清空
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("重新扫描")) Scan(); //执行扫描
            if (GUILayout.Button("清空")) ClearAll();//清空
        }
        EditorGUILayout.EndHorizontal();
        //功能按钮
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("精度设置为Half")) HalfPrecision();
            isShsdowCast = EditorGUILayout.Toggle(isShsdowCast);
            if (GUILayout.Button("关闭ShadowCast")) CastShadows(isShsdowCast);
        }
        EditorGUILayout.EndHorizontal();
        //列表
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos,GUILayout.Height(210));
        {
            for (int i = 0; i < shaderGraphFiles.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(Path.GetFileName(shaderGraphFiles[i]));
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndScrollView();
        //状态输出
        EditorGUILayout.LabelField("状态:", EditorStyles.boldLabel);
        EditorGUILayout.TextArea(statusMessage, GUILayout.Height(60));
        
        
        
        
        
        //强制刷新
        Repaint();
    }

    /*
     *
     * 
     */


    //关闭/开启CastShadows
    void CastShadows(bool isOpen)
    {
        int processed = 0;
        int modifiedCount = 0;
        foreach (string fullPath in shaderGraphFiles)
        {
            try
            {
                string content = File.ReadAllText(fullPath, Encoding.UTF8);
                string pattern = isOpen?" \"m_CastShadows\": true,":" \"m_CastShadows\": false,";
                string replacement =isOpen? " \"m_CastShadows\": false,":" \"m_CastShadows\": true,";
                if (Regex.IsMatch(content, pattern))
                {
                    string newContent = Regex.Replace(content, pattern, replacement);
                    File.WriteAllText(fullPath, newContent, Encoding.UTF8);
                    modifiedCount++;
                    Debug.LogFormat("已修改：{0}", GetRelativePath(fullPath));
                }
                else
                {
                    Debug.LogFormat("未修改（没有找到匹配项）：{0}", GetRelativePath(fullPath));
                }
                processed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogErrorFormat("处理文件 {0} 时出错：{1}", GetRelativePath(fullPath), ex.Message);
            }
        }
        AssetDatabase.Refresh();
    }
    //修改精度
    void HalfPrecision()
    {
        int processed = 0;
        int modifiedCount = 0;
        foreach (string fullPath in shaderGraphFiles)
        {
            try
            {
                string content = File.ReadAllText(fullPath, Encoding.UTF8);
                string customFunction = "Custom Function";
                string pattern = "\"m_GraphPrecision\": 1";
                string replacement = "\"m_GraphPrecision\": 2";
                if (Regex.IsMatch(content, pattern) && !Regex.IsMatch(content, customFunction))
                {
                    string newContent = Regex.Replace(content, pattern, replacement);
                    File.WriteAllText(fullPath, newContent, Encoding.UTF8);
                    modifiedCount++;
                    Debug.LogFormat("已修改：{0}", GetRelativePath(fullPath));
                }
                else
                {
                    Debug.LogFormat("未修改（没有找到匹配项或包含Custom Function不可修改）：{0}", GetRelativePath(fullPath));
                }
                processed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogErrorFormat("处理文件 {0} 时出错：{1}", GetRelativePath(fullPath), ex.Message);
            }
        }
        AssetDatabase.Refresh();
    }
    //扫描路径
    void Scan()
    {
        statusMessage = "正在扫描";
        //将绝对路径转系统路径；
        string relativeFolderPath = Path.GetFullPath(path);
        //获取所有.shadergraph文件
        shaderGraphFiles = Directory.GetFiles(relativeFolderPath, "*.shadergraph", SearchOption.AllDirectories);
        statusMessage = $"找到{shaderGraphFiles.Length}个";
    }
    //清理
    void ClearAll()
    {
        shaderGraphFiles = Array.Empty<string>();
        statusMessage = "已清空";
    }
    //项目文件夹选择
    string SelectionPath()
    {
        // 1. 获取选中的资源（过滤掉非资产对象）
        Object[] selectedObjects = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
        // 2. 处理空选择情况
        if (selectedObjects.Length == 0)
        {
            statusMessage = "请选择项目窗口的文件或文件夹来指定路径哦\n返回：Assets";
            return "Assets";
        }
        // 3. 获取第一个选中对象的GUID和路径
        string guid = Selection.assetGUIDs[0];
        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
        // 4. 统一路径格式并处理文件夹/文件
        if (AssetDatabase.IsValidFolder(assetPath))
        {
            // 文件夹直接使用原路径（自动统一为正斜杠）
            statusMessage = $"返回：{assetPath}";
            return assetPath;
            
        }
        else
        {
            // 文件获取父目录并确保使用正斜杠
            statusMessage = $"返回：{Path.GetDirectoryName(assetPath).Replace("\\", "/")}";
            return  Path.GetDirectoryName(assetPath).Replace("\\", "/");
        }
    }
    //绝对转相对
    string GetRelativePath(string fullPath)
    {
        string assetsPath = Application.dataPath.Replace("\\", "/");
        string path = fullPath.Replace("\\", "/");
        int idx = path.IndexOf("/Assets/", System.StringComparison.Ordinal);
        if (idx >= 0)
        {
            return path.Substring(idx + 1); // 去掉前面的 '/'，得到 "Assets/..."
        }
        return path;
    }
    void OnDisable()
    {
        
    }
}
