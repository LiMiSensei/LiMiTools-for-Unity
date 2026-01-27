using UnityEditor;
using UnityEngine;

public class AdvancedTextureViewer : EditorWindow
{
    private Texture2D _texture;
    private float _zoomLevel = 1f;
    private Vector2 _scrollPosition;
    private Vector2 _textureScrollPosition;
    private FilterMode _filterMode = FilterMode.Bilinear;
    private bool _showAlphaChannel = false;
    private bool _showMipMaps = false;
    
    [MenuItem("LiMi/测试/Advanced Texture Viewer")]
    public static void ShowWindow()
    {
        GetWindow<AdvancedTextureViewer>("Advanced Texture Viewer");
    }
    
    private void OnGUI()
    {
        DrawToolbar();
        
        if (_texture == null)
        {
            EditorGUILayout.HelpBox("没有纹理加载。使用工具栏按钮加载纹理。", MessageType.Info);
            return;
        }
        
        DrawTextureDisplay();
        DrawTextureInfo();
    }
    
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        if (GUILayout.Button("加载纹理", EditorStyles.toolbarButton))
        {
            LoadTexture();
        }
        
        if (GUILayout.Button("创建测试纹理", EditorStyles.toolbarButton))
        {
            CreateTestTexture();
        }
        
        if (_texture != null)
        {
            if (GUILayout.Button("保存纹理", EditorStyles.toolbarButton))
            {
                SaveTexture();
            }
        }
        
        GUILayout.FlexibleSpace();
        
        // 缩放控制
        GUILayout.Label("缩放:", EditorStyles.miniLabel);
        _zoomLevel = EditorGUILayout.Slider(_zoomLevel, 0.1f, 5f, GUILayout.Width(150));
        
        EditorGUILayout.EndHorizontal();
        
        // 高级选项
        EditorGUILayout.BeginHorizontal();
        
        _filterMode = (FilterMode)EditorGUILayout.EnumPopup("过滤模式", _filterMode);
        _showAlphaChannel = EditorGUILayout.Toggle("显示Alpha通道", _showAlphaChannel);
        _showMipMaps = EditorGUILayout.Toggle("显示MipMaps", _showMipMaps);
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawTextureDisplay()
    {
        // 计算显示区域
        Rect displayRect = GUILayoutUtility.GetRect(position.width - 20, position.height - 200);
        
        // 纹理显示区域
        Rect textureRect = new Rect(0, 0, _texture.width * _zoomLevel, _texture.height * _zoomLevel);
        
        // 滚动视图
        _scrollPosition = GUI.BeginScrollView(displayRect, _scrollPosition, textureRect);
        
        // 创建临时渲染纹理来处理Alpha通道和过滤
        RenderTexture renderTexture = RenderTexture.GetTemporary(
            (int)(_texture.width * _zoomLevel), 
            (int)(_texture.height * _zoomLevel));
        
        // 设置过滤模式
        _texture.filterMode = _filterMode;
        
        // 绘制纹理
        if (_showAlphaChannel)
        {
            // 显示Alpha通道
            Material mat = new Material(Shader.Find("Hidden/ShowAlpha"));
            Graphics.Blit(_texture, renderTexture, mat);
            GUI.DrawTexture(textureRect, renderTexture);
            DestroyImmediate(mat);
        }
        else
        {
            GUI.DrawTexture(textureRect, _texture, ScaleMode.ScaleToFit);
        }
        
        RenderTexture.ReleaseTemporary(renderTexture);
        
        // 绘制网格线（如果缩放足够大）
        if (_zoomLevel > 2f)
        {
            DrawGrid(textureRect);
        }
        
        // 绘制像素信息（当鼠标悬停时）
        DrawPixelInfo(displayRect, textureRect);
        
        GUI.EndScrollView();
        
        // 缩放控制按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("重置缩放", GUILayout.Width(100)))
        {
            _zoomLevel = 1f;
            _scrollPosition = Vector2.zero;
        }
        
        if (GUILayout.Button("适合窗口", GUILayout.Width(100)))
        {
            FitToWindow(displayRect);
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawGrid(Rect textureRect)
    {
        Handles.BeginGUI();
        
        Color gridColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        Handles.color = gridColor;
        
        // 绘制垂直线
        for (int x = 0; x <= _texture.width; x++)
        {
            float posX = textureRect.x + x * _zoomLevel;
            Handles.DrawLine(
                new Vector3(posX, textureRect.y),
                new Vector3(posX, textureRect.y + textureRect.height)
            );
        }
        
        // 绘制水平线
        for (int y = 0; y <= _texture.height; y++)
        {
            float posY = textureRect.y + y * _zoomLevel;
            Handles.DrawLine(
                new Vector3(textureRect.x, posY),
                new Vector3(textureRect.x + textureRect.width, posY)
            );
        }
        
        Handles.EndGUI();
    }
    
    private void DrawPixelInfo(Rect displayRect, Rect textureRect)
    {
        Event evt = Event.current;
        Vector2 mousePos = evt.mousePosition;
        
        if (displayRect.Contains(mousePos))
        {
            // 计算纹理坐标
            Vector2 texturePos = (mousePos - textureRect.position + _scrollPosition) / _zoomLevel;
            
            if (texturePos.x >= 0 && texturePos.x < _texture.width &&
                texturePos.y >= 0 && texturePos.y < _texture.height)
            {
                int x = Mathf.FloorToInt(texturePos.x);
                int y = Mathf.FloorToInt(texturePos.y);
                
                Color pixelColor = _texture.GetPixel(x, y);
                
                // 绘制工具提示
                Rect tooltipRect = new Rect(mousePos.x + 15, mousePos.y - 30, 200, 60);
                GUI.Box(tooltipRect, "");
                
                GUI.Label(new Rect(tooltipRect.x + 5, tooltipRect.y + 5, 190, 20),
                    $"位置: ({x}, {y})");
                GUI.Label(new Rect(tooltipRect.x + 5, tooltipRect.y + 25, 190, 20),
                    $"颜色: R:{pixelColor.r:F2} G:{pixelColor.g:F2} B:{pixelColor.b:F2}");
                GUI.Label(new Rect(tooltipRect.x + 5, tooltipRect.y + 45, 190, 20),
                    $"Alpha: {pixelColor.a:F2}");
                
                // 绘制颜色预览
                Rect colorRect = new Rect(tooltipRect.x + 160, tooltipRect.y + 5, 30, 50);
                EditorGUI.DrawRect(colorRect, pixelColor);
                
                // 左键点击设置像素颜色
                if (evt.type == EventType.MouseDown && evt.button == 0)
                {
                    SetPixelColor(x, y, Color.white);
                    evt.Use();
                }
                
                // 右键点击获取颜色
                if (evt.type == EventType.MouseDown && evt.button == 1)
                {
                    CopyColorToClipboard(pixelColor);
                    evt.Use();
                }
            }
        }
    }
    
    private void DrawTextureInfo()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("纹理信息", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(GUI.skin.box);
        
        EditorGUILayout.LabelField($"名称: {_texture.name}");
        EditorGUILayout.LabelField($"尺寸: {_texture.width} x {_texture.height}");
        EditorGUILayout.LabelField($"格式: {_texture.format}");
        EditorGUILayout.LabelField($"MipMap计数: {_texture.mipmapCount}");
        EditorGUILayout.LabelField($"内存大小: {CalculateTextureSize(_texture)} MB");
        
        EditorGUILayout.EndVertical();
    }
    
    private void LoadTexture()
    {
        string path = EditorUtility.OpenFilePanel("选择纹理", "Assets", "png,jpg,jpeg,bmp,tga,tif,psd");
        if (!string.IsNullOrEmpty(path))
        {
            if (path.StartsWith(Application.dataPath))
            {
                string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
                _texture = AssetDatabase.LoadAssetAtPath<Texture2D>(relativePath);
            }
            else
            {
                // 加载外部纹理
                byte[] fileData = System.IO.File.ReadAllBytes(path);
                _texture = new Texture2D(2, 2);
                _texture.LoadImage(fileData);
                _texture.name = System.IO.Path.GetFileNameWithoutExtension(path);
            }
            
            if (_texture != null)
            {
                _zoomLevel = 1f;
                _scrollPosition = Vector2.zero;
                Debug.Log($"加载纹理: {_texture.name} ({_texture.width}x{_texture.height})");
            }
        }
    }
    
    private void CreateTestTexture()
    {
        int width = 256;
        int height = 256;
        _texture = new Texture2D(width, height);
        
        // 创建测试图案
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color color = new Color(
                    (float)x / width,
                    (float)y / height,
                    (float)(x + y) / (width + height),
                    1.0f
                );
                
                // 添加网格效果
                if (x % 32 == 0 || y % 32 == 0)
                {
                    color = Color.Lerp(color, Color.black, 0.3f);
                }
                
                _texture.SetPixel(x, y, color);
            }
        }
        
        _texture.Apply();
        _texture.name = "测试纹理";
        _zoomLevel = 1f;
        _scrollPosition = Vector2.zero;
    }
    
    private void SaveTexture()
    {
        string path = EditorUtility.SaveFilePanel("保存纹理", "Assets", _texture.name, "png");
        if (!string.IsNullOrEmpty(path))
        {
            byte[] pngData = _texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, pngData);
            
            if (path.StartsWith(Application.dataPath))
            {
                string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
                AssetDatabase.ImportAsset(relativePath);
                Debug.Log($"纹理已保存: {relativePath}");
            }
            else
            {
                Debug.Log($"纹理已保存: {path}");
            }
        }
    }
    
    private void FitToWindow(Rect displayRect)
    {
        float fitWidth = displayRect.width / _texture.width;
        float fitHeight = displayRect.height / _texture.height;
        _zoomLevel = Mathf.Min(fitWidth, fitHeight);
        _scrollPosition = Vector2.zero;
    }
    
    private void SetPixelColor(int x, int y, Color color)
    {
        // 注意：这会修改纹理数据
        if (EditorUtility.DisplayDialog("修改像素", "确定要修改像素颜色吗？", "确定", "取消"))
        {
            _texture.SetPixel(x, y, color);
            _texture.Apply();
            Repaint();
        }
    }
    
    private void CopyColorToClipboard(Color color)
    {
        string colorString = $"new Color({color.r}f, {color.g}f, {color.b}f, {color.a}f)";
        EditorGUIUtility.systemCopyBuffer = colorString;
        Debug.Log($"颜色已复制到剪贴板: {colorString}");
    }
    
    private float CalculateTextureSize(Texture2D texture)
    {
        // 近似计算纹理内存大小
        int bitsPerPixel = 32; // RGBA32
        if (texture.format == TextureFormat.RGBA32) bitsPerPixel = 32;
        else if (texture.format == TextureFormat.RGB24) bitsPerPixel = 24;
        else if (texture.format == TextureFormat.RGBAFloat) bitsPerPixel = 128;
        
        long bytes = (long)texture.width * texture.height * bitsPerPixel / 8;
        
        // 考虑MipMaps
        if (texture.mipmapCount > 1)
        {
            bytes = (long)(bytes * 1.33f); // 近似值
        }
        
        return bytes / (1024f * 1024f); // 转换为MB
    }
}