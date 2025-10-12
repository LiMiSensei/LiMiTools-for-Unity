using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class ShowWireframe
{
    
    [MenuItem("LiMi/辅助/模型辅助线 &d")]
    static void ToggleManual()
    {
        _enabled = !_enabled;
        EditorPrefs.SetBool(EDITOR_PREFS_KEY, _enabled);
        SceneView.RepaintAll();
    }
    
    [MenuItem("LiMi/辅助/模型辅助线 &d", true)]
    static bool ToggleManualValidate()
    {
        Menu.SetChecked("LiMi/辅助/模型辅助线 &d", _enabled);
        return true;
    }
    
    //--------------------------------------------------------------------------------------------------------------//
    private const string EDITOR_PREFS_KEY = "ShowWireframe";
    private static bool _enabled;
    private static float[] _pressTimes = new float[3];
    private static int _pressCount = 0;
    
    
    
    //-------------------------------------------------------------------------------------------------------------//
    static ShowWireframe()
    {
        _enabled = EditorPrefs.GetBool(EDITOR_PREFS_KEY, true);
        SceneView.duringSceneGui += OnSceneGUI;
    }
    
    static void OnSceneGUI(SceneView sceneView)
    {
        if (!_enabled) return;

        foreach (var obj in Selection.gameObjects)
        {
            DrawBoundsWithLabels(obj.transform);
        }
    }

    static void DrawBoundsWithLabels(Transform target)
    {
        var renderers = target.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;
        foreach (var r in renderers) bounds.Encapsulate(r.bounds);//获取包裹盒
        Handles.color = Color.green;
        Handles.DrawWireCube(bounds.center, bounds.size);
        Vector3 size = bounds.size;
        GUIStyle style = new GUIStyle(GUI.skin.label) //GUIskin控制
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.cyan },
            alignment = TextAnchor.MiddleCenter
        };
        Handles.Label(bounds.center + Vector3.right * (size.x/2 + 0.3f), $"长:{size.x:F2}m", style);//绘制文本
        Handles.Label(bounds.center + Vector3.up * (size.y/2 + 0.3f), $"高:{size.y:F2}m", style);
        Handles.Label(bounds.center + Vector3.forward * (size.z/2 + 0.3f), $"宽:{size.z:F2}m", style);
    }

    
}