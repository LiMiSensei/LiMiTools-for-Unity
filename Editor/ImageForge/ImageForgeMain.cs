using System;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using Object = UnityEngine.Object;

public class ExampleScript : EditorWindow
{
    Rect windowRect = new Rect(20, 20, 120, 50);

    [MenuItem("54t4wrew/werw")]
    static void Open()
    {
        var window = GetWindow<ExampleScript>("编辑器风格窗口");
        window.titleContent = new GUIContent("主编辑器窗口");
        window.Show();
    }
    BoundsInt viewBounds;
   
    private string str = "1234";
    private Color color1 = Color.white;
    private AnimationCurve curve1 = AnimationCurve.Linear(0, 0, 1, 1);
    private bool b = true;
    private double d1 = 123123;
    private float f1 = 123.4f;
    private GameObject go1;
    Vector2 vector2 = Vector2.zero;
    Vector2Int vector2Int;
    Vector3 vector3 = Vector3.zero;
    Vector3Int vector3Int;
    Vector4 vector4 = Vector4.zero;
    private Rect rect1 = new Rect(20, 20, 120, 50);
    private Gradient gradient1 = new Gradient();
    private int i1 = 0;
    private float minvalue = 0f;
    private float maxvalue = 10f;
    private float minLimit = 2f;
    private float maxLimit = 20f;
    private Object o;
    private EditorTool[] m_Tools;
    private Tool currentTool = Tool.Move;
    private GUILayoutOption[] options;
    // 用于控制动画的目标值（0 = 关闭，1 = 打开）
    private float m_fadeTarget = 0f;
    // 当前动画进度（0~1），每帧靠插值平滑过渡
    private float m_fadeCurrent = 0.5f;

    // 动画速度（每秒变化的值），可自行调节
    private const float kFadeSpeed = 1f;
    private int m_SelectedTool = 0;
    private float m_knobValue = 50f;   // 初始值
    private long l1 = 1;

    void OnGUI()
    {
        
        EditorGUILayout.Space();

        // ---------- Knob ----------
        // 参数说明：
        //   value   : 当前数值
        //   min/max : 允许范围
        //   size    : Knob 大小 (宽, 高)
        //   label   : 描述文字（可为 null）
        l1 = EditorGUILayout.LongField(l1);


        // 让数值同步显示在下面的滑动条，以验证两者一致性
        EditorGUILayout.LabelField($"当前值: {m_knobValue:F1}");
        m_knobValue = EditorGUILayout.Slider(m_knobValue, 0f, 100f);
        
        EditorGUILayout.TagField("TagField");
    }

}