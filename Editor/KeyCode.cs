using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class KeyCode : EditorWindow
{
    [MenuItem("LiMi的指令/创建Cube _#a")] 
    static void CreateCube()
    {
        EditorApplication.ExecuteMenuItem("GameObject/3D Object/Cube");
    }
    
    [MenuItem("LiMi的指令/查找材质球 _#m")] 
    static void FindMaterialContext()
    {
        Renderer renderer = Selection.activeGameObject.GetComponent<Renderer>();
        EditorGUIUtility.PingObject(renderer.sharedMaterial); 
        EditorUtility.OpenPropertyEditor(renderer.sharedMaterial);
    }
    
    [MenuItem("LiMi的指令/快速聚焦 %#m")] // Ctrl+Shift+F快捷键
    static void FocusObject()
    {
        if (Selection.activeTransform != null)
        {
            SceneView.lastActiveSceneView.FrameSelected();
            // 高级参数：设置聚焦时的视野大小
            SceneView.lastActiveSceneView.size = 5f; 
        }
    }
    
    [MenuItem("LiMi的指令/场景截图")]
    static void CaptureSceneView()
    {
        SceneView sceneView = SceneView.lastActiveSceneView;
        //sceneView.camera.pixelRect = new Rect(0, 0,3840, 280);
        if (sceneView == null) return;
        // 创建临时RenderTexture
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        // 渲染场景
        Camera cam = sceneView.camera;
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        // 读取像素
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        // 保存为PNG
        byte[] bytes = tex.EncodeToPNG();
        string path = "Assets/场景截图/" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllBytes(path, bytes);
        // 清理
        cam.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);
        DestroyImmediate(tex);

        AssetDatabase.Refresh();
        Debug.Log($"截图已保存: {path}");
    }    
}
