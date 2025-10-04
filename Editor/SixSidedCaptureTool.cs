using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class SixSidedCaptureTool : EditorWindow
{
    private GameObject targetObject;
    private Bounds objectBounds;
    private List<Texture2D> captureTextures = new List<Texture2D>();
    private Vector2 scrollPosition;
    private int resolution = 1024;
    private bool includeBackground = true;
    private Color backgroundColor = Color.clear;
    private string savePath = "Assets/Screenshots/";
    private bool useTransparentBackground = true;
    
    // 用于保存和恢复层的状态
    private int originalLayer;
    private Dictionary<GameObject, int> originalLayers = new Dictionary<GameObject, int>();
    private const int CAPTURE_LAYER = 31; // 使用最高层31，通常这个层不会被使用

    [MenuItem("LiMi的工具/六面捕获工具 V0.1")]
    static void ShowWindow()
    {
        GetWindow<SixSidedCaptureTool>("六面捕获工具");
    }

    void OnEnable()
    {
        Selection.selectionChanged += OnSelectionChanged;
        OnSelectionChanged();
    }

    void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChanged;
    }

    void OnSelectionChanged()
    {
        if (Selection.activeGameObject != targetObject)
        {
            targetObject = Selection.activeGameObject;
            CalculateBounds();
            Repaint();
        }
    }

    //计算包围盒
    void CalculateBounds()
    {
        if (targetObject != null)
        {
            Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                objectBounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    objectBounds.Encapsulate(renderers[i].bounds);
                }
            }
            else
            {
                objectBounds = new Bounds(targetObject.transform.position, Vector3.one);
            }
        }
        else
        {
            objectBounds = new Bounds();
        }
    }

    void OnGUI()
    {
        
        // 目标对象选择
        EditorGUILayout.BeginVertical(GUI.skin.box);
        {
             targetObject = (GameObject)EditorGUILayout.ObjectField("目标对象", targetObject, typeof(GameObject), true);
        }
        EditorGUILayout.EndVertical();
        
        // 显示包围盒信息
        EditorGUILayout.BeginVertical(GUI.skin.box);
        {
            EditorGUILayout.LabelField("包围盒信息", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField($"中心: {objectBounds.center}");
            EditorGUILayout.LabelField($"尺寸: {objectBounds.size}");
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // 设置选项
        EditorGUILayout.BeginVertical(GUI.skin.box);
        {
            EditorGUILayout.LabelField("截图设置", EditorStyles.miniBoldLabel);
            resolution = EditorGUILayout.IntSlider("分辨率", resolution, 256, 4096);
            includeBackground = EditorGUILayout.Toggle("包含背景", includeBackground);
            if (includeBackground)
            {
                backgroundColor = EditorGUILayout.ColorField("背景颜色", backgroundColor);
                useTransparentBackground = EditorGUILayout.Toggle("透明背景", useTransparentBackground);
            }
            EditorGUILayout.BeginHorizontal();
            {
                savePath = EditorGUILayout.TextField("保存路径", savePath);
                if (GUILayout.Button("选择路径"))
                {
                    savePath = SelectionPath();
                }
            }
            EditorGUILayout.EndHorizontal();
            
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        // 操作按钮
        if (targetObject != null)
        {
            
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("计算包围盒", GUILayout.Height(30))) CalculateBounds();
        
                if (GUILayout.Button("截取六面图", GUILayout.Height(30))) CaptureAllSides();
        
                if (GUILayout.Button("保存所有图片", GUILayout.Height(30)) && captureTextures.Count > 0) SaveAllTextures();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
        
        // 显示截图结果
        if (captureTextures.Count > 0)
        {
            DisplayCaptureResults();
        }
        //作者信息
        //作者信息
        GUILayout.FlexibleSpace();
        GUILayout.Label("@LiMi  Version:0.1.0  Updated:2025-09-22", EditorStyles.miniLabel);
    }
//---------------------------------------------------------------------------------------------------------------------//
    void CaptureAllSides()
    {
        captureTextures.Clear();

        try
        {
            // 将目标对象移动到专用层
            MoveObjectToCaptureLayer();
            
            // 六个方向：前、后、左、右、上、下
            Vector3[] directions = {
                Vector3.forward, Vector3.back,
                Vector3.left, Vector3.right,
                Vector3.up, Vector3.down
            };

            string[] directionNames = { "Front", "Back", "Left", "Right", "Top", "Bottom" };

            // 创建6个临时相机
            List<GameObject> tempCameras = new List<GameObject>();

            for (int i = 0; i < directions.Length; i++)
            {
                GameObject tempCamera = CreateTempCamera(directions[i], directionNames[i]);
                if (tempCamera != null)
                {
                    tempCameras.Add(tempCamera);
                }
            }

            // 一次性渲染所有相机
            RenderAllCameras(tempCameras, directionNames);

            // 清理临时相机
            foreach (var camera in tempCameras)
            {
                DestroyImmediate(camera);
            }
        }
        finally
        {
            // 恢复对象的原始层
            RestoreObjectLayers();
        }

        Debug.Log($"成功截取 {captureTextures.Count} 张图片");
    }

    void MoveObjectToCaptureLayer()
    {
        originalLayers.Clear();
        
        // 保存并设置目标对象及其所有子对象的层
        SetLayerRecursively(targetObject, CAPTURE_LAYER, originalLayers);
    }

    //设置层
    void SetLayerRecursively(GameObject obj, int layer, Dictionary<GameObject, int> layerDict)
    {
        if (obj == null) return;
        
        // 保存原始层
        layerDict[obj] = obj.layer;
        
        // 设置新层
        obj.layer = layer;
        
        // 递归设置所有子对象
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer, layerDict);
        }
    }

    //恢复层
    void RestoreObjectLayers()
    {
        // 恢复所有对象的原始层
        foreach (var kvp in originalLayers)
        {
            if (kvp.Key != null)
            {
                kvp.Key.layer = kvp.Value;
            }
        }
        originalLayers.Clear();
    }

    //项目选中路径
    string SelectionPath()
    {
        // 1. 获取选中的资源（过滤掉非资产对象）
        Object[] selectedObjects = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
        // 2. 处理空选择情况
        if (selectedObjects.Length == 0)
        {
            return "Assets";
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
            // 文件获取父目录并确保使用正斜
            return  Path.GetDirectoryName(assetPath).Replace("\\", "/");
        }
    }
    
    //创建摄像机
    GameObject CreateTempCamera(Vector3 direction, string cameraName)
    {
        if (targetObject == null) return null;

        // 创建临时相机
        GameObject tempCameraGO = new GameObject($"TempCamera_{cameraName}");
        Camera tempCamera = tempCameraGO.AddComponent<Camera>();

        // 设置相机参数
        SetupCaptureCamera(tempCamera, direction);

        return tempCameraGO;
    }

    void SetupCaptureCamera(Camera camera, Vector3 direction)
    {
        // 计算相机位置 - 确保完全包含包围盒
        float maxSize = Mathf.Max(objectBounds.size.x, objectBounds.size.y, objectBounds.size.z);
        float distance = maxSize * 1.5f;
        
        Vector3 cameraPosition = objectBounds.center + direction.normalized * distance;
        
        camera.transform.position = cameraPosition;
        camera.transform.rotation = Quaternion.LookRotation(-direction);
        
        // 设置正交投影
        camera.orthographic = true;
        camera.orthographicSize = maxSize * 0.6f;
        
        // 设置裁切平面
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = distance * 2f;
        
        // 清除标志和背景
        if (includeBackground)
        {
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = useTransparentBackground ? new Color(0, 0, 0, 0) : backgroundColor;
        }
        else
        {
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0, 0, 0, 0);
        }
        
        // 只渲染专用层
        camera.cullingMask = 1 << CAPTURE_LAYER;
        
        // 禁用相机组件，我们手动控制渲染
        camera.enabled = false;
    }

    void RenderAllCameras(List<GameObject> tempCameras, string[] directionNames)
    {
        for (int i = 0; i < tempCameras.Count; i++)
        {
            Camera camera = tempCameras[i].GetComponent<Camera>();
            if (camera != null)
            {
                Texture2D capture = RenderCameraToTexture(camera, directionNames[i]);
                if (capture != null)
                {
                    captureTextures.Add(capture);
                }
            }
        }
    }

    Texture2D RenderCameraToTexture(Camera camera, string directionName)
    {
        // 创建渲染纹理
        RenderTexture renderTexture = new RenderTexture(resolution, resolution, 24);
        camera.targetTexture = renderTexture;
        
        // 手动渲染
        camera.Render();
        
        // 读取到Texture2D
        Texture2D texture = new Texture2D(resolution, resolution, 
            useTransparentBackground ? TextureFormat.RGBA32 : TextureFormat.RGB24, false);
        
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        texture.Apply();
        
        // 清理
        camera.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(renderTexture);
        
        texture.name = $"{targetObject.name}_{directionName}";
        
        return texture;
    }

    //显示截图结果
    void DisplayCaptureResults()
    {
        EditorGUILayout.LabelField("截图结果", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < Mathf.Min(3, captureTextures.Count); i++)
        {
            DisplayTexturePreview(captureTextures[i], GetDirectionName(i));
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        for (int i = 3; i < Mathf.Min(6, captureTextures.Count); i++)
        {
            DisplayTexturePreview(captureTextures[i], GetDirectionName(i));
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
    }

    //截图显示框
    void DisplayTexturePreview(Texture2D texture, string label)
    {
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(200), GUILayout.Height(220));
        
        EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel, GUILayout.Width(180));
        
        // 显示纹理预览
        Rect previewRect = GUILayoutUtility.GetRect(180, 180);
        
        // 为透明背景 添加棋盘格背景
        if (useTransparentBackground || !includeBackground)
        {
            DrawCheckerboardBackground(previewRect);
        }
        
        EditorGUI.DrawPreviewTexture(previewRect, texture);
        
        // 显示尺寸信息
        EditorGUILayout.LabelField($"{texture.width}×{texture.height}", EditorStyles.miniLabel);
        
        // 操作按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("查看", GUILayout.Width(60)))
        {
            ShowTextureInWindow(texture);
        }
        
        if (GUILayout.Button("保存", GUILayout.Width(60)))
        {
            SaveTexture(texture);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }

    //截图
    void DrawCheckerboardBackground(Rect rect)
    {
        // 创建棋盘格样式
        Texture2D checkerboard = CreateCheckerboardTexture(16, new Color(0.8f, 0.8f, 0.8f, 0f), new Color(0.6f, 0.6f, 0.6f, 1f));
        GUI.DrawTexture(rect, checkerboard, ScaleMode.StretchToFill);
        DestroyImmediate(checkerboard);
    }

    //棋盘格
    Texture2D CreateCheckerboardTexture(int size, Color color1, Color color2)
    {
        Texture2D texture = new Texture2D(size * 2, size * 2);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                bool isEven = ((x / size) + (y / size)) % 2 == 0;
                texture.SetPixel(x, y, isEven ? color1 : color2);
            }
        }
        texture.Apply();
        return texture;
    }

    //名称
    string GetDirectionName(int index)
    {
        string[] names = { "前视图", "后视图", "左视图", "右视图", "顶视图", "底视图" };
        return index < names.Length ? names[index] : $"方向{index}";
    }

    //重新打开新窗口
    void ShowTextureInWindow(Texture2D texture)
    {
        TextureViewerWindow.ShowWindow(texture);
    }

    //保存图像
    void SaveTexture(Texture2D texture)
    {
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        string filePath = Path.Combine(savePath, texture.name + ".png");
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);
        
        AssetDatabase.Refresh();
        TextureImporter importer;
        importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
        importer.alphaIsTransparency = true;
        AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
        Debug.Log($"图片已保存: {filePath}");
    }

    //保存所有图像
    void SaveAllTextures()
    {
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        TextureImporter importer;
        
        foreach (Texture2D texture in captureTextures)
        {
            //写入图像
            string filePath = Path.Combine(savePath, texture.name + ".png");
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);
            
            //导入设置
            AssetDatabase.Refresh();
            importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
            importer.alphaIsTransparency = true;
            AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
        }
        Debug.Log($"所有图片已保存到: {savePath}");
    }
}


//--------------------------------------------------------------------------------------------------------------------//
public class TextureViewerWindow : EditorWindow
{
    private Texture2D textureToView;
    private Vector2 scrollPosition;
    private float zoomLevel = 1f;
    private bool useTransparentBackground;

    public static void ShowWindow(Texture2D texture)
    {
        TextureViewerWindow window = GetWindow<TextureViewerWindow>("Texture Viewer");
        window.textureToView = texture;
        window.titleContent = new GUIContent(texture.name);
        window.useTransparentBackground = texture.format == TextureFormat.RGBA32;
    }

    void OnGUI()
    {
        if (textureToView == null)
        {
            EditorGUILayout.HelpBox("没有纹理可显示", MessageType.Info);
            return;
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"尺寸: {textureToView.width} × {textureToView.height}", GUILayout.Width(200));
        EditorGUILayout.LabelField($"格式: {textureToView.format}", GUILayout.Width(150));
        zoomLevel = EditorGUILayout.Slider("缩放", zoomLevel, 0.1f, 3f);
        EditorGUILayout.EndHorizontal();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        int displayWidth = Mathf.RoundToInt(textureToView.width * zoomLevel);
        int displayHeight = Mathf.RoundToInt(textureToView.height * zoomLevel);
        
        Rect textureRect = GUILayoutUtility.GetRect(displayWidth, displayHeight);
        
        // 为透明背景添加棋盘格
        if (useTransparentBackground)
        {
            DrawCheckerboardBackground(textureRect);
        }
        
        GUI.DrawTexture(textureRect, textureToView, ScaleMode.ScaleToFit);
        
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        if (GUILayout.Button("保存图片"))
        {
            SaveTexture();
        }
    }

    void DrawCheckerboardBackground(Rect rect)
    {
        Texture2D checkerboard = CreateCheckerboardTexture(16, new Color(0.8f, 0.8f, 0.8f, 1f), new Color(0.6f, 0.6f, 0.6f, 1f));
        GUI.DrawTexture(rect, checkerboard, ScaleMode.StretchToFill);
        DestroyImmediate(checkerboard);
    }

    Texture2D CreateCheckerboardTexture(int size, Color color1, Color color2)
    {
        Texture2D texture = new Texture2D(size * 2, size * 2);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                bool isEven = ((x / size) + (y / size)) % 2 == 0;
                texture.SetPixel(x, y, isEven ? color1 : color2);
            }
        }
        texture.Apply();
        return texture;
    }

    void SaveTexture()
    {
        string path = EditorUtility.SaveFilePanel("保存纹理", "Assets/", textureToView.name, "png");
        if (!string.IsNullOrEmpty(path))
        {
            byte[] bytes = textureToView.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
        }
    }
}
