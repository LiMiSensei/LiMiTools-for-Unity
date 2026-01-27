// File: Assets/Editor/MaterialInstancingEnablerWindow.cs
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;

public class MaterialGPUInstancing : EditorWindow
{
    
    private Vector2 scrollPos; // 窗口滚动区域
    private List<MaterialInfo> materials = new List<MaterialInfo>(); // 扫描结果
    private bool selectAll = true; // 全选开关
    private string statusMessage = ""; // 运行状态提示

    [MenuItem("LiMi/工具/启用GPU实例化 V1.0")]
    public static void ShowWindow()
    {
        var win = GetWindow<MaterialGPUInstancing>();
        win.titleContent.text = "启用GPU实例化";
        win.minSize = new Vector2(600, 400);
        win.Show();
        // 一次打开就执行扫描
        win.ScanSceneMaterials();
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("场景材质GPU实例化开关", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("扫描场景中所有对象使用的材质，显示是否支持Instancing，并提供一键开启选中的材质的实例化开关。", MessageType.Info);
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("重新扫描场景材质", GUILayout.Height(28))) ScanSceneMaterials();

            if (GUILayout.Button("启用选中材质的Instancing", GUILayout.Height(28))) EnableSelectedInstancing();
            
            if (GUILayout.Button("清空结果", GUILayout.Height(28))) {materials.Clear();statusMessage = "结果已清空。"; } 
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        
        if (materials.Count > 0)
        {
            EditorGUILayout.BeginHorizontal();
            {
                bool newSelectAll = EditorGUILayout.ToggleLeft("全选/取消全选", selectAll, GUILayout.Width(180));
                if (newSelectAll != selectAll)
                {
                    selectAll = newSelectAll;
                    foreach (var m in materials) m.isSelected = selectAll;
                }
                EditorGUILayout.LabelField($"材质数量: {materials.Count}", EditorStyles.label);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("选中", GUILayout.Width(40));
                EditorGUILayout.LabelField("材质名称", GUILayout.Width(260));
                EditorGUILayout.LabelField("支持Instancing", GUILayout.Width(120));
                EditorGUILayout.LabelField("当前启用", GUILayout.Width(90));
                EditorGUILayout.LabelField("对象引用数", GUILayout.Width(90));
            }
            EditorGUILayout.EndHorizontal();
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            {
                foreach (var info in materials)
                {
                    EditorGUILayout.BeginHorizontal();
                    info.isSelected = EditorGUILayout.Toggle(info.isSelected, GUILayout.Width(40));
                    EditorGUILayout.LabelField(info.material.name, GUILayout.Width(260));
                    EditorGUILayout.LabelField(info.supportsInstancing ? "是" : "否", GUILayout.Width(110));
                    EditorGUILayout.LabelField(info.isEnabled ? "启用" : "未启用", GUILayout.Width(90));
                    EditorGUILayout.LabelField(info.objectCount.ToString(), GUILayout.Width(90));
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();
        }
        else
        {
            EditorGUILayout.HelpBox("尚未扫描到材质，请点击“重新扫描场景材质”按钮。", MessageType.Info);
        }
        EditorGUILayout.Space();
        
        // 状态输出
        if (!string.IsNullOrEmpty(statusMessage))
        {
            EditorGUILayout.LabelField("状态:", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(statusMessage, GUILayout.Height(60));
        }
    }

    // 扫描场景中所有材质及其对象引用信息
    void ScanSceneMaterials()
    {
        materials.Clear();
        statusMessage = "正在扫描场景材质，请稍候...";

        // 收集场景中所有渲染器及其使用的材料
        Renderer[] renderers = FindObjectsOfType<Renderer>(true);
        Dictionary<Material, List<GameObject>> usage = new Dictionary<Material, List<GameObject>>();

        foreach (var rend in renderers)
        {
            if (rend == null) continue;
            // 使用 sharedMaterials 以避免影响运行时实例化
            foreach (Material mat in rend.sharedMaterials)
            {
                if (mat == null) continue;
                if (!usage.ContainsKey(mat))
                    usage[mat] = new List<GameObject>();
                usage[mat].Add(rend.gameObject);
            }
        }

        materials.Capacity = usage.Count;
        foreach (var kvp in usage)
        {
            Material mat = kvp.Key;
            bool supportsInstancing = MaterialSupportsInstancing(mat);
            bool isEnabled = mat.enableInstancing;
            materials.Add(new MaterialInfo
            {
                material = mat,
                supportsInstancing = supportsInstancing,
                isEnabled = isEnabled,
                objectCount = kvp.Value.Count,
                isSelected = true
            });
        }

        // 默认全选
        selectAll = true;

        // 按对象数降序
        materials.Sort((a, b) => b.objectCount.CompareTo(a.objectCount));

        statusMessage = $"扫描完成，找到 {materials.Count} 种材质。";
    }

    // 启用选中材质的 GPU Instancing
    void EnableSelectedInstancing()
    {
        if (materials.Count == 0)
        {
            statusMessage = "未发现材质，请先扫描。";
            return;
        }

        int changed = 0;
        int skipped = 0;
        StringBuilder sb = new StringBuilder();

        foreach (var info in materials)
        {
            if (!info.isSelected) continue;
            if (!info.supportsInstancing)
            {
                sb.AppendLine($"跳过 (不支持): {info.material.name}");
                skipped++;
                continue;
            }
            if (!info.isEnabled)
            {
                info.material.enableInstancing = true;
                EditorUtility.SetDirty(info.material);
                changed++;
                sb.AppendLine($"启用 Instancing: {info.material.name}");
            }
            else
            {
                sb.AppendLine($"已启用: {info.material.name}");
                // 计数也视为已处理
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        statusMessage = $"操作完成：启用 {changed} 个材质，跳过 {skipped} 个材质。\n" + sb.ToString();
        // 重新扫描，以刷新启用状态
        ScanSceneMaterials();
    }

    // 简单的检测：材质是否支持 Instancing
    bool MaterialSupportsInstancing(Material mat)
    {
        if (mat == null) return false;

        // 直接检查材质的 shader 是否包含 instancing 相关变体
        Shader shader = mat.shader;
        if (shader == null) return false;

        // 优先从 shader 的名字判断（一些简单 shader 会有 "Instancing" 词）
        string shaderName = shader.name.ToLower();
        if (shaderName.Contains("instancing") || shaderName.Contains("gpu instancing"))
        {
            return true;
        }

        // 简单文本扫描：读取着色器源码，查找 instancing 变体声明
        string path = AssetDatabase.GetAssetPath(shader);
        if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
        {
            try
            {
                string code = System.IO.File.ReadAllText(path);
                if (code.Contains("instancing") || code.Contains("multi_compile_instancing") || code.Contains("UNITY_DEFINE_INSTANCED_PROP"))
                    return true;
            }
            catch { /* 忽略读取错误 */ }
        }

        // 作为兜底，默认认为现代着色器都支持实例化，但某些自定义着色器可能不支持
        // 为避免误判，这里还会看材质是否已经启用 instancing
        // 但如果 shader 不支持，启用也不会生效，需要用户确认
        return mat.enableInstancing;
    }

    // 数据结构：材质信息
    [System.Serializable]
    public class MaterialInfo
    {
        public Material material;
        public bool supportsInstancing;
        public bool isEnabled;
        public int objectCount;
        public bool isSelected;
    }

    // 扩展容量属性，避免频繁创建
    private List<MaterialInfo> MaterialsFlattenCopy()
    {
        return materials;
    }

    // 小工具：在改动后自动刷新
    void OnInspectorUpdate()
    {
        Repaint();
    }
    
}
