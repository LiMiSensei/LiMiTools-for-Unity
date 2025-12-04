using UnityEditor;
using UnityEngine;
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
    void OnGUI()
    {
        
       
        
    }
    
}