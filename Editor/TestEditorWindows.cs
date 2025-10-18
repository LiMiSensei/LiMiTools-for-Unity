using UnityEditor;
using UnityEngine;

public class TestWindows : EditorWindow
{
    [MenuItem("工具/测试1")]
    static void ShowWindow()
    {
        var window = GetWindow<TestWindows>();
        window.Show();//打开面板 
        window.title = "我的窗口";//窗口的名字
        //window.maxSize = new Vector2(450, 1000);//窗口的大小 还有其他方法可以设置
    }

    //写全局变量的部分
    private float size = 0f;
    
    void OnEnable()
    {
        //打开时
    }

    void OnDisable()
    {
        //销毁时
    }
    void OnGUI()
    {
        //写GUI的部分
        EditorGUILayout.LabelField("测试1");
        GUILayout.Button("测试2");
    }
}
