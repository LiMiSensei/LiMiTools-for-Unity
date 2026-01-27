using UnityEditor;
using UnityEngine;

public class TextureCompareWindow : EditorWindow
{
    private Texture2D _textureA;
    private Texture2D _textureB;
    private float _splitPosition = 0.5f;
    private CompareMode _compareMode = CompareMode.SideBySide;
    
    private enum CompareMode
    {
        SideBySide,
        SplitScreen,
        Difference,
        AlphaOverlay
    }
    
    [MenuItem("LiMi/测试/Texture Compare")]
    public static void ShowWindow()
    {
        GetWindow<TextureCompareWindow>("Texture Compare");
    }
    
    private void OnGUI()
    {
        DrawToolbar();
        
        if (_textureA == null || _textureB == null)
        {
            EditorGUILayout.HelpBox("请加载两个纹理进行比较", MessageType.Info);
            return;
        }
        
        DrawTextureComparison();
        DrawDifferenceInfo();
    }
    
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        _textureA = (Texture2D)EditorGUILayout.ObjectField(_textureA, typeof(Texture2D), false);
        _textureB = (Texture2D)EditorGUILayout.ObjectField(_textureB, typeof(Texture2D), false);
        
        GUILayout.FlexibleSpace();
        
        _compareMode = (CompareMode)EditorGUILayout.EnumPopup(_compareMode, GUILayout.Width(150));
        
        if (_compareMode == CompareMode.SplitScreen)
        {
            EditorGUILayout.LabelField("分割位置:", GUILayout.Width(70));
            _splitPosition = EditorGUILayout.Slider(_splitPosition, 0f, 1f, GUILayout.Width(150));
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawTextureComparison()
    {
        Rect displayRect = GUILayoutUtility.GetRect(position.width - 20, position.height - 100);
        
        switch (_compareMode)
        {
            case CompareMode.SideBySide:
                DrawSideBySide(displayRect);
                break;
            case CompareMode.SplitScreen:
                DrawSplitScreen(displayRect);
                break;
            case CompareMode.Difference:
                DrawDifference(displayRect);
                break;
            case CompareMode.AlphaOverlay:
                DrawAlphaOverlay(displayRect);
                break;
        }
    }
    
    private void DrawSideBySide(Rect displayRect)
    {
        float halfWidth = displayRect.width / 2 - 10;
        float height = Mathf.Min(displayRect.height, halfWidth * _textureA.height / _textureA.width);
        
        Rect rectA = new Rect(displayRect.x, displayRect.y, halfWidth, height);
        Rect rectB = new Rect(displayRect.x + halfWidth + 20, displayRect.y, halfWidth, height);
        
        GUI.DrawTexture(rectA, _textureA, ScaleMode.ScaleToFit);
        GUI.DrawTexture(rectB, _textureB, ScaleMode.ScaleToFit);
        
        // 标签
        GUI.Label(new Rect(rectA.x, rectA.y - 20, rectA.width, 20), "纹理 A");
        GUI.Label(new Rect(rectB.x, rectB.y - 20, rectB.width, 20), "纹理 B");
    }
    
    private void DrawSplitScreen(Rect displayRect)
    {
        float height = Mathf.Min(displayRect.height, displayRect.width * _textureA.height / _textureA.width);
        Rect textureRect = new Rect(displayRect.x, displayRect.y, displayRect.width, height);
        
        // 分割线位置
        float splitX = textureRect.x + textureRect.width * _splitPosition;
        
        // 绘制纹理A的左侧部分
        Rect rectA = new Rect(textureRect.x, textureRect.y, splitX - textureRect.x, textureRect.height);
        GUI.DrawTextureWithTexCoords(rectA, _textureA, 
            new Rect(0, 0, _splitPosition, 1));
        
        // 绘制纹理B的右侧部分
        Rect rectB = new Rect(splitX, textureRect.y, textureRect.width - (splitX - textureRect.x), textureRect.height);
        GUI.DrawTextureWithTexCoords(rectB, _textureB, 
            new Rect(_splitPosition, 0, 1 - _splitPosition, 1));
        
        // 绘制分割线
        Handles.BeginGUI();
        Handles.color = Color.red;
        Handles.DrawLine(
            new Vector3(splitX, textureRect.y),
            new Vector3(splitX, textureRect.y + textureRect.height)
        );
        Handles.EndGUI();
        
        // 分割线拖动
        EditorGUIUtility.AddCursorRect(textureRect, MouseCursor.ResizeHorizontal);
        if (Event.current.type == EventType.MouseDrag && textureRect.Contains(Event.current.mousePosition))
        {
            _splitPosition = (Event.current.mousePosition.x - textureRect.x) / textureRect.width;
            Repaint();
        }
    }
    
    private void DrawDifference(Rect displayRect)
    {
        float height = Mathf.Min(displayRect.height, displayRect.width * _textureA.height / _textureA.width);
        Rect textureRect = new Rect(displayRect.x, displayRect.y, displayRect.width, height);
        
        // 创建差异纹理
        Texture2D diffTexture = CreateDifferenceTexture(_textureA, _textureB);
        GUI.DrawTexture(textureRect, diffTexture, ScaleMode.ScaleToFit);
        
        // 显示差异百分比
        float diffPercent = CalculateDifferencePercentage(_textureA, _textureB);
        GUI.Label(new Rect(textureRect.x, textureRect.y - 20, textureRect.width, 20),
            $"差异度: {diffPercent:F2}%");
    }
    
    private void DrawAlphaOverlay(Rect displayRect)
    {
        float height = Mathf.Min(displayRect.height, displayRect.width * _textureA.height / _textureA.width);
        Rect textureRect = new Rect(displayRect.x, displayRect.y, displayRect.width, height);
        
        // 先绘制纹理B作为背景
        GUI.DrawTexture(textureRect, _textureB, ScaleMode.ScaleToFit);
        
        // 使用混合模式绘制纹理A
        GUI.color = new Color(1, 1, 1, 0.5f);
        GUI.DrawTexture(textureRect, _textureA, ScaleMode.ScaleToFit);
        GUI.color = Color.white;
    }
    
    private Texture2D CreateDifferenceTexture(Texture2D texA, Texture2D texB)
    {
        int width = Mathf.Min(texA.width, texB.width);
        int height = Mathf.Min(texA.height, texB.height);
        
        Texture2D diffTexture = new Texture2D(width, height);
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color colorA = texA.GetPixel(x, y);
                Color colorB = texB.GetPixel(x, y);
                
                // 计算颜色差异
                float diffR = Mathf.Abs(colorA.r - colorB.r);
                float diffG = Mathf.Abs(colorA.g - colorB.g);
                float diffB = Mathf.Abs(colorA.b - colorB.b);
                float diffA = Mathf.Abs(colorA.a - colorB.a);
                
                // 差异越大，颜色越红
                Color diffColor = new Color(diffR, diffG, diffB, diffA);
                diffTexture.SetPixel(x, y, diffColor);
            }
        }
        
        diffTexture.Apply();
        return diffTexture;
    }
    
    private float CalculateDifferencePercentage(Texture2D texA, Texture2D texB)
    {
        int width = Mathf.Min(texA.width, texB.width);
        int height = Mathf.Min(texA.height, texB.height);
        
        float totalDiff = 0f;
        int pixelCount = width * height;
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color colorA = texA.GetPixel(x, y);
                Color colorB = texB.GetPixel(x, y);
                
                float diffR = Mathf.Abs(colorA.r - colorB.r);
                float diffG = Mathf.Abs(colorA.g - colorB.g);
                float diffB = Mathf.Abs(colorA.b - colorB.b);
                
                totalDiff += (diffR + diffG + diffB) / 3f;
            }
        }
        
        return (totalDiff / pixelCount) * 100f;
    }
    
    private void DrawDifferenceInfo()
    {
        if (_textureA != null && _textureB != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("比较信息", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            
            EditorGUILayout.LabelField($"纹理A尺寸: {_textureA.width} x {_textureA.height}");
            EditorGUILayout.LabelField($"纹理B尺寸: {_textureB.width} x {_textureB.height}");
            
            if (_textureA.width != _textureB.width || _textureA.height != _textureB.height)
            {
                EditorGUILayout.HelpBox("纹理尺寸不同！", MessageType.Warning);
            }
            
            float diffPercent = CalculateDifferencePercentage(_textureA, _textureB);
            EditorGUILayout.LabelField($"差异度: {diffPercent:F2}%");
            
            EditorGUILayout.EndVertical();
        }
    }
}