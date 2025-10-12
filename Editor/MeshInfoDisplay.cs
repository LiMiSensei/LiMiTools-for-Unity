using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[InitializeOnLoad]
public class MeshInfoDisplay
{
    private static bool _showMeshInfo = true;
    private static GUIStyle _style;
    private static Vector3 _lastCameraPosition;
    private static Quaternion _lastCameraRotation;
    
    // 新增缓存系统
    private static Dictionary<GameObject, (int meshCount, int subMeshCount, int totalTriangles, int totalVertices)> _meshCache = 
        new Dictionary<GameObject, (int, int, int, int)>();
    private static int _lastSelectionHash; // 用于检测选择变化

    [MenuItem("LiMi/辅助/模型统计信息 &m")]
    public static void ToggleDisplay()
    {
        _showMeshInfo = !_showMeshInfo;
        ClearCache(); // 切换显示时清空缓存
        SceneView.RepaintAll();
    }

    [MenuItem("LiMi/辅助/模型统计信息 &m", true)]
    private static bool ToggleDisplayValidate()
    {
        Menu.SetChecked("LiMi/辅助/模型统计信息 &m", _showMeshInfo);
        return true;
    }
    
    static MeshInfoDisplay()
    {
        _style = new GUIStyle()
        {
            fontSize = 11,
            richText = true,
            padding = new RectOffset(5, 5, 5, 5),
            normal = { 
                textColor = Color.yellow,
                background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.8f)) 
            }
        };

        SceneView.duringSceneGui += OnSceneGUI;
        EditorApplication.playModeStateChanged += OnPlayModeChange;
        Selection.selectionChanged += OnSelectionChanged; // 新增选择变化事件
    }

    private static void OnPlayModeChange(PlayModeStateChange state)
    {
        ClearCache();
        SceneView.RepaintAll();
    }

    private static void OnSelectionChanged()
    {
        UpdateMeshCache();
        SceneView.RepaintAll();
    }

    private static void ClearCache()
    {
        _meshCache.Clear();
        _lastSelectionHash = 0;
    }

    private static void UpdateMeshCache()
    {
        int newHash = GetSelectionHash();
        if (newHash == _lastSelectionHash) return;
        
        _lastSelectionHash = newHash;
        _meshCache.Clear();

        foreach (var go in Selection.gameObjects)
        {
            if (!go) continue;
            _meshCache[go] = CalculateMeshStats(go);
        }
    }

    private static int GetSelectionHash()
    {
        int hash = 0;
        foreach (var go in Selection.gameObjects)
        {
            if (!go) continue;
            hash ^= go.GetInstanceID();
        }
        return hash;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (!_showMeshInfo || Selection.gameObjects.Length == 0)
            return;
        if (Camera.current != null && 
            (Camera.current.transform.position != _lastCameraPosition || 
             Camera.current.transform.rotation != _lastCameraRotation))
        {
            _lastCameraPosition = Camera.current.transform.position;
            _lastCameraRotation = Camera.current.transform.rotation;
            SceneView.RepaintAll();
        }
        Handles.BeginGUI();
        foreach (var go in Selection.gameObjects)
        {
            if (!go || !_meshCache.TryGetValue(go, out var stats)) continue;
            if (stats.totalTriangles == 0) continue;

            Vector3 screenPos = Camera.current.WorldToScreenPoint(go.transform.position);
            if (screenPos.z <= 0) continue; // 背面剔除

            Rect rect = new Rect(
                screenPos.x - 50, 
                SceneView.currentDrawingSceneView.position.height - screenPos.y - 50, 
                200, 
                80
            );

            string info = $"名称：<b>{go.name}</b>\n" +
                         $"网格: <color=#FF8080>{stats.meshCount}</color>\n" +
                         $"子网格: <color=#80FF80>{stats.subMeshCount}</color>\n" +
                         $"顶点: <color=#FF80FF>{stats.totalVertices:N0}</color>\n"+
                         $"三角面: <color=#8080FF>{stats.totalTriangles:N0}</color>";

            GUI.Box(rect, info, _style);
            
        }
        Handles.EndGUI();

        
    }

    private static Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    private static (int meshCount, int subMeshCount, int totalTriangles, int totalVertices) CalculateMeshStats(GameObject go)
    {
        int triangles = 0;
        int vertices = 0;
        int meshCount = 0;
        int subMeshCount = 0;

        foreach (var filter in go.GetComponentsInChildren<MeshFilter>())
        {
            if (!filter.sharedMesh) continue;
            meshCount++;
            subMeshCount += filter.sharedMesh.subMeshCount;
            triangles += filter.sharedMesh.triangles.Length / 3;
            vertices += filter.sharedMesh.vertexCount;
        }

        foreach (var renderer in go.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (!renderer.sharedMesh) continue;
            meshCount++;
            subMeshCount += renderer.sharedMesh.subMeshCount;
            triangles += renderer.sharedMesh.triangles.Length / 3;
            vertices += renderer.sharedMesh.vertexCount;
        }

        return (meshCount, subMeshCount, triangles, vertices);
    }
}
