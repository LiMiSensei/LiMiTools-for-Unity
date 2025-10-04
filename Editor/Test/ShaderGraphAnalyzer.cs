using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class ShaderGraphAnalyzer : EditorWindow
{
    [MenuItem("LiMi的测试/ShaderGraph使用分析")]
    public static void ShowWindow()
    {
        GetWindow<ShaderGraphAnalyzer>("ShaderGraph使用分析");
    }

    private List<ShaderGraphInfo> shaderGraphInfos = new List<ShaderGraphInfo>();
    private Vector2 scrollPosition;
    private string searchKeyword = "";
    private bool showUnused = true;
    private bool showUsed = true;
    private bool sortByUsageCount = true;

    private class ShaderGraphInfo
    {
        public Shader shader;
        public string shaderGraphPath;
        public List<Material> usedMaterials = new List<Material>();
        public int usageCount => usedMaterials.Count;
    }

    private void OnGUI()
    {
        DrawToolbar();
        DrawSettings();
        DrawShaderGraphList();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        if (GUILayout.Button("分析ShaderGraph使用情况", EditorStyles.toolbarButton))
        {
            AnalyzeShaderGraphUsage();
        }
        
        if (GUILayout.Button("导出CSV报告", EditorStyles.toolbarButton))
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
        showUsed = EditorGUILayout.Toggle("显示已使用的", showUsed);
        showUnused = EditorGUILayout.Toggle("显示未使用的", showUnused);
        sortByUsageCount = EditorGUILayout.Toggle("按使用数量排序", sortByUsageCount);
        
        EditorGUILayout.EndVertical();
    }

    private void DrawShaderGraphList()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"ShaderGraph文件 ({GetFilteredShaderGraphs().Count})", EditorStyles.boldLabel);
        
        // 统计信息
        int totalShaderGraphs = shaderGraphInfos.Count;
        int usedCount = shaderGraphInfos.Count(sg => sg.usageCount > 0);
        int unusedCount = totalShaderGraphs - usedCount;
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"总计: {totalShaderGraphs}", GUILayout.Width(100));
        EditorGUILayout.LabelField($"已使用: {usedCount}", GUILayout.Width(100));
        EditorGUILayout.LabelField($"未使用: {unusedCount}", GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        foreach (var shaderGraphInfo in GetFilteredShaderGraphs())
        {
            DrawShaderGraphItem(shaderGraphInfo);
        }
        
        EditorGUILayout.EndScrollView();
    }

    private void DrawShaderGraphItem(ShaderGraphInfo info)
    {
        EditorGUILayout.BeginVertical("Box");
        
        // ShaderGraph文件信息
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("ShaderGraph:", GUILayout.Width(80));
        
        // 显示Shader对象
        EditorGUILayout.ObjectField(info.shader, typeof(Shader), false, GUILayout.Width(200));
        
        // 使用数量显示（带颜色）
        GUI.color = info.usageCount > 0 ? new Color(0.2f, 0.8f, 0.4f) : new Color(1f, 0.4f, 0.4f);
        EditorGUILayout.LabelField($"使用数: {info.usageCount}", GUILayout.Width(80));
        GUI.color = Color.white;
        
        EditorGUILayout.EndHorizontal();
        
        // 文件路径
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("路径:", GUILayout.Width(80));
        EditorGUILayout.LabelField(info.shaderGraphPath, EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndHorizontal();
        
        // 操作按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("打开ShaderGraph", GUILayout.Width(120)))
        {
            OpenShaderGraphFile(info);
        }
        
        if (GUILayout.Button("选择文件", GUILayout.Width(80)))
        {
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(info.shaderGraphPath);
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
        
        if (GUILayout.Button("查看材质", GUILayout.Width(80)))
        {
            ShowMaterialsWindow(info);
        }
        EditorGUILayout.EndHorizontal();
        
        // 简略显示使用的材质（最多显示3个）
        if (info.usageCount > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("使用的材质:", EditorStyles.boldLabel);
            
            int showCount = Mathf.Min(3, info.usageCount);
            for (int i = 0; i < showCount; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(info.usedMaterials[i], typeof(Material), false, GUILayout.Width(250));
                
                if (GUILayout.Button("选择", GUILayout.Width(50)))
                {
                    Selection.activeObject = info.usedMaterials[i];
                    EditorGUIUtility.PingObject(info.usedMaterials[i]);
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (info.usageCount > 3)
            {
                EditorGUILayout.LabelField($"... 还有 {info.usageCount - 3} 个材质", EditorStyles.miniLabel);
            }
        }
        else
        {
            EditorGUILayout.LabelField("❌ 未被任何材质使用", EditorStyles.boldLabel);
        }
        
        EditorGUILayout.EndVertical();
    }

    private void AnalyzeShaderGraphUsage()
    {
        shaderGraphInfos.Clear();
        
        // 1. 首先找到所有的ShaderGraph文件
        string[] shaderGraphPaths = AssetDatabase.FindAssets("t:Shader")
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Where(path => IsShaderGraphFile(path))
            .ToArray();
        
        int totalGraphs = shaderGraphPaths.Length;
        Debug.Log($"找到 {totalGraphs} 个ShaderGraph文件");
        
        // 2. 为每个ShaderGraph创建信息
        for (int i = 0; i < totalGraphs; i++)
        {
            string path = shaderGraphPaths[i];
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
            
            if (shader != null)
            {
                ShaderGraphInfo info = new ShaderGraphInfo
                {
                    shader = shader,
                    shaderGraphPath = path,
                    usedMaterials = new List<Material>()
                };
                shaderGraphInfos.Add(info);
            }
            
            if (i % 10 == 0)
            {
                EditorUtility.DisplayProgressBar("分析ShaderGraph", $"处理 {i}/{totalGraphs}", (float)i / totalGraphs);
            }
        }
        
        // 3. 找到所有材质并匹配Shader
        string[] materialGuids = AssetDatabase.FindAssets("t:Material");
        int totalMaterials = materialGuids.Length;
        
        for (int i = 0; i < totalMaterials; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(materialGuids[i]);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            
            if (material != null && material.shader != null)
            {
                // 查找这个材质使用的Shader是否在ShaderGraph列表中
                var shaderGraphInfo = shaderGraphInfos.FirstOrDefault(sg => sg.shader == material.shader);
                if (shaderGraphInfo != null)
                {
                    shaderGraphInfo.usedMaterials.Add(material);
                }
            }
            
            if (i % 100 == 0)
            {
                EditorUtility.DisplayProgressBar("分析材质使用", $"分析 {i}/{totalMaterials}", (float)i / totalMaterials);
            }
        }
        
        EditorUtility.ClearProgressBar();
        
        // 排序
        if (sortByUsageCount)
        {
            shaderGraphInfos = shaderGraphInfos.OrderByDescending(sg => sg.usageCount).ThenBy(sg => sg.shader.name).ToList();
        }
        else
        {
            shaderGraphInfos = shaderGraphInfos.OrderBy(sg => sg.shader.name).ToList();
        }
        
        Repaint();
        
        Debug.Log($"分析完成！共分析 {shaderGraphInfos.Count} 个ShaderGraph文件");
    }

    private bool IsShaderGraphFile(string path)
    {
        // 通过文件扩展名判断
        if (path.EndsWith(".shadergraph") || path.EndsWith(".shadersubgraph"))
        {
            return true;
        }
        
        // 通过文件内容判断（备用方法）
        try
        {
            if (File.Exists(path))
            {
                string content = File.ReadAllText(path);
                return content.Contains("ShaderGraph") || content.Contains("m_IsAsset");
            }
        }
        catch
        {
            // 忽略读取错误
        }
        
        return false;
    }

    private List<ShaderGraphInfo> GetFilteredShaderGraphs()
    {
        IEnumerable<ShaderGraphInfo> filtered = shaderGraphInfos;
        
        // 使用状态筛选
        if (!showUsed) filtered = filtered.Where(sg => sg.usageCount == 0);
        if (!showUnused) filtered = filtered.Where(sg => sg.usageCount > 0);
        
        // 关键词搜索
        if (!string.IsNullOrEmpty(searchKeyword))
        {
            filtered = filtered.Where(sg => 
                sg.shader.name.Contains(searchKeyword) ||
                sg.shaderGraphPath.Contains(searchKeyword));
        }
        
        return filtered.ToList();
    }

    private void OpenShaderGraphFile(ShaderGraphInfo info)
    {
        Object shaderGraphAsset = AssetDatabase.LoadAssetAtPath<Object>(info.shaderGraphPath);
        if (shaderGraphAsset != null)
        {
            Selection.activeObject = shaderGraphAsset;
            EditorGUIUtility.PingObject(shaderGraphAsset);
            AssetDatabase.OpenAsset(shaderGraphAsset);
        }
    }

    private void ShowMaterialsWindow(ShaderGraphInfo info)
    {
        MaterialsListWindow.ShowWindow(info.shader.name, info.usedMaterials);
    }

    private void ExportToCSV()
    {
        string path = EditorUtility.SaveFilePanel("导出CSV报告", "", "shadergraph_usage.csv", "csv");
        if (string.IsNullOrEmpty(path)) return;
        
        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine("ShaderGraph名称,Shader路径,使用材质数量,材质列表");
            
            foreach (var info in shaderGraphInfos)
            {
                string materialNames = string.Join(";", info.usedMaterials.Select(m => m.name));
                writer.WriteLine($"\"{info.shader.name}\",\"{info.shaderGraphPath}\",{info.usageCount},\"{materialNames}\"");
            }
        }
        
        Debug.Log($"CSV报告已导出到: {path}");
        EditorUtility.RevealInFinder(path);
    }
}

// 材质列表显示窗口
public class MaterialsListWindow : EditorWindow
{
    private static List<Material> materials;
    private static string title;
    private Vector2 scrollPosition;

    public static void ShowWindow(string shaderName, List<Material> materialList)
    {
        MaterialsListWindow window = GetWindow<MaterialsListWindow>();
        window.titleContent = new GUIContent($"材质列表 - {shaderName}");
        materials = materialList;
        title = shaderName;
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField($"Shader: {title}", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"使用材质数量: {materials.Count}", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        foreach (var material in materials)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(material, typeof(Material), false, GUILayout.Width(300));
            
            if (GUILayout.Button("选择", GUILayout.Width(60)))
            {
                Selection.activeObject = material;
                EditorGUIUtility.PingObject(material);
            }
            
            if (GUILayout.Button("查看", GUILayout.Width(60)))
            {
                EditorUtility.OpenPropertyEditor(material);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndScrollView();
        
        if (GUILayout.Button("选择所有材质"))
        {
            Selection.objects = materials.ToArray();
        }
    }
}
