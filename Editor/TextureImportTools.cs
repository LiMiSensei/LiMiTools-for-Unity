// File: Assets/Editor/TextureFolderIndexerWindow.cs
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class TextureImportTools : EditorWindow
{
    private Vector2 scrollPos;
    private string rootFolder = "Assets";
    private int maxTextureSize = 1024; // 默认最大尺寸，0 表示不限制
    private string[] textureExtensions = new string[] { ".png", ".jpg", ".jpeg", ".tga", ".bmp", ".psd", ".gif", ".exr", ".hdr", ".tif", ".tiff", ".texture", ".png" };
    private List<TextureInfo> textures = new List<TextureInfo>();
    private string statusMessage = "";

    [MenuItem("LiMi的工具/图像导入设置 V0.1")]
    public static void ShowWindow()
    {
        var window = GetWindow<TextureImportTools>("图像导入设置");
        window.minSize = new Vector2(700, 450);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Texture FolderIndexer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("遍历指定根文件夹及其子文件夹中的所有纹理文件，列出并可选设置全局最大纹理尺寸（MaxTextureSize）以控制导入时的最大贴图尺寸。", MessageType.Info);

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("根文件夹", GUILayout.Width(60));
        rootFolder = EditorGUILayout.TextField(rootFolder);
        if (GUILayout.Button("选择", GUILayout.Width(60)))
        {
            string selected = EditorUtility.OpenFolderPanel("选择根文件夹", Application.dataPath, "");
            if (!string.IsNullOrEmpty(selected))
            {
                if (selected.StartsWith(Application.dataPath))
                {
                    rootFolder = "Assets" + selected.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "请选择项目内的文件夹。", "确定");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("最大贴图边长（像素）", GUILayout.Width(170));
        maxTextureSize = EditorGUILayout.IntField(maxTextureSize);
        if (GUILayout.Button("应用全局设置", GUILayout.Width(140)))
        {
            ApplyGlobalTextureSize();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        if (GUILayout.Button("扫描纹理并生成列表", GUILayout.Height(28)))
        {
            ScanTexturesInFolder();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"纹理总数: {textures.Count}", EditorStyles.label);
        EditorGUILayout.Space();

        // 列表
        if (textures.Count > 0)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("选择", GUILayout.Width(40));
            EditorGUILayout.LabelField("纹理名称", GUILayout.Width(260));
            EditorGUILayout.LabelField("路径", GUILayout.Width(320));
            EditorGUILayout.LabelField("原始尺寸", GUILayout.Width(90));
            EditorGUILayout.LabelField("导入 MaxSize", GUILayout.Width(90));
            EditorGUILayout.EndHorizontal();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(210));
            foreach (var t in textures)
            {
                EditorGUILayout.BeginHorizontal();
                t.isSelected = EditorGUILayout.Toggle(t.isSelected, GUILayout.Width(40));
                EditorGUILayout.LabelField(t.texture.name, GUILayout.Width(260));
                EditorGUILayout.LabelField(t.assetPath, GUILayout.Width(320));
                EditorGUILayout.LabelField($"{t.width}x{t.height}", GUILayout.Width(90));
                EditorGUILayout.LabelField(t.maxTextureSize.ToString(), GUILayout.Width(90));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            if (GUILayout.Button("应用选中纹理的 MaxSize 设置", GUILayout.Height(28)))
            {
                ApplySelectedTexturesSize();
            }
            EditorGUILayout.Space();
        }
        else
        {
            EditorGUILayout.HelpBox("请点击“扫描纹理并生成列表”来生成纹理列表。", MessageType.Info);
        }

        EditorGUILayout.Space();
        // 状态信息
        if (!string.IsNullOrEmpty(statusMessage))
        {
            EditorGUILayout.HelpBox(statusMessage, MessageType.Info);
        }
    }

    // 扫描根文件夹下的纹理资产
    private void ScanTexturesInFolder()
    {
        textures.Clear();
        statusMessage = "正在扫描，请稍候...";

        if (string.IsNullOrEmpty(rootFolder))
        {
            statusMessage = "根文件夹无效，请设置正确路径。";
            return;
        }

        string absoluteRoot = GetAbsolutePath(rootFolder);
        if (!Directory.Exists(absoluteRoot))
        {
            statusMessage = $"文件夹不存在: {rootFolder}";
            return;
        }

        // 获取所有资产路径
        string[] assetPaths = AssetDatabase.GetAllAssetPaths();
        HashSet<string> textureAssetPaths = new HashSet<string>();

        foreach (string path in assetPaths)
        {
            if (!path.StartsWith(rootFolder)) continue;

            string ext = Path.GetExtension(path).ToLower();
            if (System.Array.Exists(textureExtensions, e => e == ext))
            {
                textureAssetPaths.Add(path);
            }
        }

        textures.Capacity = textureAssetPaths.Count;
        foreach (var path in textureAssetPaths)
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
            {
                // 读取导入设置以获取最大尺寸信息
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                int currentMaxSize = importer != null ? importer.maxTextureSize : 0;

                textures.Add(new TextureInfo
                {
                    texture = tex,
                    assetPath = path,
                    width = tex.width,
                    height = tex.height,
                    maxTextureSize = currentMaxSize > 0 ? currentMaxSize : maxTextureSize,
                    isSelected = true
                });
            }
        }

        // 按尺寸或名称排序以便查看
        textures.Sort((a, b) => b.width * b.height.CompareTo(a.width * a.height));
        statusMessage = $"扫描完成，找到 {textures.Count} 张纹理。";
        Repaint();
    }

    // 将全局设置应用到所有纹理导入设置中
    private void ApplyGlobalTextureSize()
    {
        if (maxTextureSize <= 0)
        {
            statusMessage = "最大尺寸应为正整数。";
            return;
        }

        // 将全局尺寸写入一个临时结果，等待应用到纹理
        foreach (var t in textures)
        {
            t.pendingMaxSize = maxTextureSize;
        }

        statusMessage = $"已将全局最大贴图尺寸设为 {maxTextureSize}，请点击“应用选中纹理的 MaxSize 设置”以实际修改导入设置。";
        Repaint();
    }

    // 将选中纹理的导入 MaxSize 设置应用到 Import 设置中
    private void ApplySelectedTexturesSize()
    {
        int updated = 0;
        int skipped = 0;

        foreach (var t in textures)
        {
            if (!t.isSelected) continue;

            string path = t.assetPath;
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                skipped++;
                continue;
            }

            int targetSize = t.pendingMaxSize > 0 ? t.pendingMaxSize : maxTextureSize;
            if (targetSize <= 0) targetSize = 1024; // 兜底

            if (importer.maxTextureSize != targetSize)
            {
                importer.maxTextureSize = targetSize;
                importer.SaveAndReimport();
                t.maxTextureSize = targetSize;
                updated++;
            }
            else
            {
                // 已是目标尺寸
            }

            // 额外：如果需要，可以勾选强制重新生成贴图
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        statusMessage = $"更新完成：修改了 {updated} 张纹理的 MaxSize，跳过 {skipped} 张纹理。";
        // 重新扫描以获取正确的当前尺寸
        ScanTexturesInFolder();
    }

    private string GetAbsolutePath(string projectPath)
    {
        if (Path.IsPathRooted(projectPath))
            return projectPath;
        return Path.Combine(Application.dataPath, projectPath.Substring("Assets".Length)).Replace('\\', '/');
    }

    [System.Serializable]
    private class TextureInfo
    {
        public Texture2D texture;
        public string assetPath;
        public int width;
        public int height;
        public int maxTextureSize;
        public bool isSelected;
        public int pendingMaxSize; // 需要应用的目标尺寸（来自全局设置）
    }
}
