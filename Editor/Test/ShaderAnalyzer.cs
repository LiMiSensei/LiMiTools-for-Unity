using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class ShaderAnalyzer : EditorWindow
{
    [MenuItem("LiMi/Shader分析器")]
    public static void ShowWindow()
    {
        GetWindow<ShaderAnalyzer>("Shader分析器");
    }

    private List<MaterialInfo> materialInfos = new List<MaterialInfo>();
    private Vector2 scrollPosition;
    private bool showBuiltinShaders = false;
    private bool showShaderGraph = true;
    private bool showRegularShaders = true;
    private string searchKeyword = "";

    private class MaterialInfo
    {
        public Material material;
        public Shader shader;
        public ShaderType shaderType;
        public string shaderPath;
        public bool isBuiltin;
    }

    private enum ShaderType
    {
        ShaderGraph,
        RegularShader,
        BuiltinShader,
        Unknown
    }

    private void OnGUI()
    {
        DrawToolbar();
        DrawSettings();
        DrawMaterialList();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        if (GUILayout.Button("分析项目材质", EditorStyles.toolbarButton))
        {
            AnalyzeProjectMaterials();
        }
        
        if (GUILayout.Button("分析选中材质", EditorStyles.toolbarButton))
        {
            AnalyzeSelectedMaterials();
        }
        
        if (GUILayout.Button("导出CSV", EditorStyles.toolbarButton))
        {
            ExportToCSV();
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawSettings()
    {
        EditorGUILayout.BeginVertical("Box");
        
        EditorGUILayout.LabelField("筛选设置", EditorStyles.boldLabel);
        
        searchKeyword = EditorGUILayout.TextField("搜索关键词", searchKeyword);
        
        EditorGUILayout.Space();
        showShaderGraph = EditorGUILayout.Toggle("显示ShaderGraph", showShaderGraph);
        showRegularShaders = EditorGUILayout.Toggle("显示普通Shader", showRegularShaders);
        showBuiltinShaders = EditorGUILayout.Toggle("显示内置Shader", showBuiltinShaders);
        
        EditorGUILayout.EndVertical();
    }

    private void DrawMaterialList()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"材质列表 ({GetFilteredMaterials().Count})", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        foreach (var materialInfo in GetFilteredMaterials())
        {
            DrawMaterialItem(materialInfo);
        }
        
        EditorGUILayout.EndScrollView();
    }

    private void DrawMaterialItem(MaterialInfo info)
    {
        EditorGUILayout.BeginVertical("Box");
        
        // 材质名称和对象字段
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.ObjectField(info.material, typeof(Material), false, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();
        
        // Shader信息
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Shader:", GUILayout.Width(50));
        EditorGUILayout.ObjectField(info.shader, typeof(Shader), false, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();
        
        // Shader类型
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("类型:", GUILayout.Width(50));
        
        string typeText = "";
        Color typeColor = Color.white;
        
        switch (info.shaderType)
        {
            case ShaderType.ShaderGraph:
                typeText = "ShaderGraph";
                typeColor = new Color(0.2f, 0.8f, 0.4f); // 绿色
                break;
            case ShaderType.RegularShader:
                typeText = "普通Shader";
                typeColor = new Color(0.4f, 0.6f, 1f); // 蓝色
                break;
            case ShaderType.BuiltinShader:
                typeText = "内置Shader";
                typeColor = new Color(0.8f, 0.8f, 0.8f); // 灰色
                break;
            case ShaderType.Unknown:
                typeText = "未知类型";
                typeColor = new Color(1f, 0.4f, 0.4f); // 红色
                break;
        }
        
        GUI.color = typeColor;
        EditorGUILayout.LabelField(typeText, EditorStyles.boldLabel);
        GUI.color = Color.white;
        
        EditorGUILayout.EndHorizontal();
        
        // Shader路径
        if (!string.IsNullOrEmpty(info.shaderPath))
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("路径:", GUILayout.Width(50));
            EditorGUILayout.LabelField(info.shaderPath, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndHorizontal();
        }
        
        // 操作按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("选择材质"))
        {
            Selection.activeObject = info.material;
            EditorGUIUtility.PingObject(info.material);
        }
        
        if (GUILayout.Button("选择Shader"))
        {
            Selection.activeObject = info.shader;
            EditorGUIUtility.PingObject(info.shader);
        }
        
        if (GUILayout.Button("打开文件"))
        {
            OpenShaderFile(info);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }

    private void AnalyzeProjectMaterials()
    {
        materialInfos.Clear();
        
        string[] materialGuids = AssetDatabase.FindAssets("t:Material");
        int total = materialGuids.Length;
        
        for (int i = 0; i < total; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(materialGuids[i]);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            
            if (material != null && material.shader != null)
            {
                AnalyzeMaterial(material);
            }
            
            // 显示进度条
            if (i % 10 == 0)
            {
                EditorUtility.DisplayProgressBar("分析材质", $"正在分析 {i}/{total}", (float)i / total);
            }
        }
        
        EditorUtility.ClearProgressBar();
        materialInfos = materialInfos.OrderBy(m => m.shaderType).ThenBy(m => m.material.name).ToList();
        Repaint();
        
        Debug.Log($"分析完成！共找到 {materialInfos.Count} 个材质");
    }

    private void AnalyzeSelectedMaterials()
    {
        materialInfos.Clear();
        
        foreach (var obj in Selection.objects)
        {
            if (obj is Material material && material.shader != null)
            {
                AnalyzeMaterial(material);
            }
        }
        
        materialInfos = materialInfos.OrderBy(m => m.shaderType).ThenBy(m => m.material.name).ToList();
        Repaint();
        
        Debug.Log($"分析完成！共找到 {materialInfos.Count} 个选中材质");
    }

    private void AnalyzeMaterial(Material material)
    {
        Shader shader = material.shader;
        string shaderPath = AssetDatabase.GetAssetPath(shader);
        
        MaterialInfo info = new MaterialInfo
        {
            material = material,
            shader = shader,
            shaderPath = shaderPath,
            isBuiltin = IsBuiltinShader(shaderPath)
        };
        
        info.shaderType = DetermineShaderType(info);
        materialInfos.Add(info);
    }

    private ShaderType DetermineShaderType(MaterialInfo info)
    {
        if (info.isBuiltin)
        {
            return ShaderType.BuiltinShader;
        }
        
        if (string.IsNullOrEmpty(info.shaderPath))
        {
            return ShaderType.Unknown;
        }
        
        // 检查是否是ShaderGraph文件
        if (info.shaderPath.EndsWith(".shadergraph") || info.shaderPath.EndsWith(".shadersubgraph"))
        {
            return ShaderType.ShaderGraph;
        }
        
        // 检查Shader文件内容来判断是否是ShaderGraph
        try
        {
            string[] lines = File.ReadAllLines(info.shaderPath);
            foreach (string line in lines)
            {
                if (line.Contains("ShaderGraph") || line.Contains("m_IsAsset") || line.Contains("m_Text"))
                {
                    return ShaderType.ShaderGraph;
                }
            }
        }
        catch
        {
            // 文件读取失败，可能是内置Shader或其他情况
        }
        
        return ShaderType.RegularShader;
    }

    private bool IsBuiltinShader(string shaderPath)
    {
        return string.IsNullOrEmpty(shaderPath) || 
               shaderPath.StartsWith("Resources/") || 
               shaderPath.Contains("Built-in");
    }

    private List<MaterialInfo> GetFilteredMaterials()
    {
        IEnumerable<MaterialInfo> filtered = materialInfos;
        
        // 类型筛选
        if (!showShaderGraph) filtered = filtered.Where(m => m.shaderType != ShaderType.ShaderGraph);
        if (!showRegularShaders) filtered = filtered.Where(m => m.shaderType != ShaderType.RegularShader);
        if (!showBuiltinShaders) filtered = filtered.Where(m => m.shaderType != ShaderType.BuiltinShader);
        
        // 关键词搜索
        if (!string.IsNullOrEmpty(searchKeyword))
        {
            filtered = filtered.Where(m => 
                m.material.name.Contains(searchKeyword) || 
                m.shader.name.Contains(searchKeyword) ||
                (m.shaderPath != null && m.shaderPath.Contains(searchKeyword)));
        }
        
        return filtered.ToList();
    }

    private void OpenShaderFile(MaterialInfo info)
    {
        if (!string.IsNullOrEmpty(info.shaderPath) && !info.isBuiltin)
        {
            Object shaderAsset = AssetDatabase.LoadAssetAtPath<Object>(info.shaderPath);
            if (shaderAsset != null)
            {
                Selection.activeObject = shaderAsset;
                EditorGUIUtility.PingObject(shaderAsset);
                
                // 如果是ShaderGraph文件，尝试打开它
                if (info.shaderType == ShaderType.ShaderGraph && info.shaderPath.EndsWith(".shadergraph"))
                {
                    AssetDatabase.OpenAsset(shaderAsset);
                }
            }
        }
        else
        {
            Debug.LogWarning("无法打开内置Shader或路径无效的Shader");
        }
    }

    private void ExportToCSV()
    {
        string path = EditorUtility.SaveFilePanel("导出CSV", "", "shader_analysis.csv", "csv");
        if (string.IsNullOrEmpty(path)) return;
        
        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine("材质名称,Shader名称,Shader类型,Shader路径,是否内置");
            
            foreach (var info in materialInfos)
            {
                string typeName = info.shaderType switch
                {
                    ShaderType.ShaderGraph => "ShaderGraph",
                    ShaderType.RegularShader => "普通Shader",
                    ShaderType.BuiltinShader => "内置Shader",
                    _ => "未知"
                };
                
                writer.WriteLine($"\"{info.material.name}\",\"{info.shader.name}\",\"{typeName}\",\"{info.shaderPath}\",\"{info.isBuiltin}\"");
            }
        }
        
        Debug.Log($"CSV文件已导出到: {path}");
        EditorUtility.RevealInFinder(path);
    }
}
