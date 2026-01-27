using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class SubShaderEditor : EditorWindow
{
    [MenuItem("LiMi/工具/SubShader路径调整工具 v0.1")]
    static void Init()
    {
        GetWindow<SubShaderEditor>("SubShader路径调整工具").Show();
    }
//--------------------------------------------------全局变量----------------------------------------------------------------//
    private string path = "Assets"; //路径
    bool isOpen = true;             //选择栏目
    bool oldisOpen = true;          //老的选择栏
    Vector2 scrollPos;              //窗口滚动区域
    
    private bool[] m_bools = new bool[] {};             //存放选择器
    private string[] subShadersPath = new string[]{};   //存放subShader路径
    private string[] subShadersFile = new string[]{};   //存放subShader文本文件
    private string[] subShadersM_Path = new string[]{}; //存放subShader中m_Path的值
    
//---------------------------------------------GUI-------------------------------------------------------------------//
    void OnGUI()
    {
        //选择文件路径
        EditorGUILayout.HelpBox("先选中项目文件夹，再点击以下按钮返回路径", MessageType.Info);
        if (GUILayout.Button($"路径为：{path}")) path = SelectionPath();
        
        //清空和扫描按钮
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("扫描")) AddList(); //执行扫描
            if (GUILayout.Button("清空")) ClearList();;//清空
        }
        EditorGUILayout.EndHorizontal();
        
        //Bool  Name  m_Path 栏
        EditorGUILayout.BeginHorizontal(EditorStyles.linkLabel);
        {
            isOpen = EditorGUILayout.Toggle(isOpen,GUILayout.MaxWidth(20));
            if (oldisOpen != isOpen)
            { BoolTC(isOpen); oldisOpen = isOpen; }
            EditorGUILayout.LabelField("名称",GUILayout.MaxWidth(120));
            EditorGUILayout.LabelField("m_Path路径");
        }
        EditorGUILayout.EndHorizontal();
        
        //列表视图
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos,EditorStyles.helpBox,GUILayout.ExpandHeight(true));
        {
            ListView();
        }
        EditorGUILayout.EndScrollView();
        
        //应用修改
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("应用修改"))  WriteToFile();
        
        //作者信息
        GUILayout.Label("@LiMi  Version:0.1.0  Updated:2025-10-5", EditorStyles.miniLabel);

    }
//-------------------------------------------------主要方法---------------------------------------------------------------//
    void ListView()
    {
        for (int i = 0; i<subShadersPath.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();
            {
                m_bools[i] = EditorGUILayout.Toggle(m_bools[i],GUILayout.MaxWidth(20));
                EditorGUILayout.LabelField(Path.GetFileNameWithoutExtension(subShadersPath[i]),GUILayout.MaxWidth(120));
                subShadersM_Path[i] = EditorGUILayout.TextField(subShadersM_Path[i],GUILayout.MaxWidth(300));
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    //查找包含.shadersubgraph的文件
    void AddList()
    {
       //清除数组
       ClearList();
       
       //扫描数量  返回路径
       string relativeFolderPath = Path.GetFullPath(path);
       subShadersPath = Directory.GetFiles(relativeFolderPath, "*.shadersubgraph", SearchOption.AllDirectories);
       
       //以subShadersPath数量 初始化数组
       InitializationList(subShadersPath.Length);
      
       //写入数组
       FillList();
       
    }

    //初始化数组
    void InitializationList(int targetCount)
    {
        m_bools = new bool[targetCount] ;
        subShadersFile = new string[targetCount];
        subShadersM_Path = new string[targetCount];
    }
    
    //清空数组
    void ClearList()
    {
        subShadersPath = new string[]{};
        m_bools = new bool[] { };
        subShadersFile = new string[]{};
        subShadersM_Path = new string[]{};
        System.GC.Collect();
    }

    //填充bool
    void BoolTC(bool isG = true)
    {
        for (int i = 0; i < m_bools.Length; i++)
        {
            m_bools[i] = isG;
        }
    }

    //填充路径，UTF8，行号，m_Path
    void FillList()
    {
        BoolTC();
        for (int i = 0; i < subShadersPath.Length; i++)
        {
            try
            {
                //UTF文件
                subShadersFile[i] = File.ReadAllText(subShadersPath[i], Encoding.UTF8);
                //m_Path内容
                subShadersM_Path[i] = ExtractMPathValue(subShadersFile[i]);
            }
            catch (Exception e)
            {
                Console.WriteLine($"妈的有报错,第{i}的{Path.GetFileNameWithoutExtension(subShadersPath[i])}");
                throw;
            }
            
        }
    }
    //解析JSON的方法
    private string ExtractMPathValue(string json)
    {
        int mPathIndex = json.IndexOf("\"m_Path\":");
        if (mPathIndex == -1) return null;
        int valueStart = json.IndexOf('"', mPathIndex + 8) + 1; // 跳过 "m_Path":"
        int valueEnd = json.IndexOf('"', valueStart);
        if (valueStart > 0 && valueEnd > valueStart)
        {
            return json.Substring(valueStart, valueEnd - valueStart);
        }
        return null;
    }
    //修改JSON方法
    private string ReplaceMPathValue(string json, string newValue)
    {
        int mPathIndex = json.IndexOf("\"m_Path\":");
        if (mPathIndex == -1)
        {
            throw new System.Exception("未找到 m_Path 字段");
        }
        
        int valueStart = json.IndexOf('"', mPathIndex + 8) + 1;
        int valueEnd = json.IndexOf('"', valueStart);
        
        if (valueStart <= 0 || valueEnd <= valueStart)
        {
            throw new System.Exception("m_Path 值格式错误");
        }
        
        // 构建新的JSON
        StringBuilder sb = new StringBuilder();
        sb.Append(json.Substring(0, valueStart));
        sb.Append(newValue);
        sb.Append(json.Substring(valueEnd));
        
        return sb.ToString();
    }

    //写入文件
    void WriteToFile()
    {
        for (int i = 0; i < subShadersPath.Length; i++)
        {
            if (m_bools[i])
            {
                try
                {
                    subShadersFile[i] = ReplaceMPathValue(subShadersFile[i],subShadersM_Path[i]);
                    File.WriteAllText(subShadersPath[i], subShadersFile[i], Encoding.UTF8);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"妈的写入报错！第{i}的{Path.GetFileNameWithoutExtension(subShadersPath[i])}");
                    throw;
                }
                
            }
        }
        AssetDatabase.Refresh();
        AddList(); //执行扫描
    }
    
    //选择文件夹方法
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
//----------------------------------------------------------------------------------------------------------------------//
}


