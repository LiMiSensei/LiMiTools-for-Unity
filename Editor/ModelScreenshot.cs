using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class ModelScreenshot : EditorWindow
{
    private static List<GameObject> gameObjectsList = new List<GameObject>();
    private static Color backgroundColor = new Color(0, 0, 0, 0);


    private Texture2D dirUpMap;//上
    private Texture2D dirDownMap;//下
    private Texture2D dirLeftMap;//左
    private Texture2D dirRightMap;//右
    private Texture2D dirFrontMap;//前
    private Texture2D dirBackMap;//后

    [MenuItem("LiMi/工具/截取选中模型 V0.1")]
    static void Init()
    {
        var window = GetWindow(typeof(ModelScreenshot));
        window.Show();
    }

    private void OnEnable()
    {
        dirUpMap = Texture2D.blackTexture;
        dirDownMap = Texture2D.blackTexture;
        dirLeftMap = Texture2D.blackTexture;
        dirRightMap = Texture2D.blackTexture;
        dirFrontMap = Texture2D.blackTexture;
        dirBackMap = Texture2D.blackTexture;
    }

    private void OnGUI()
    {
        
        EditorGUILayout.BeginHorizontal(GUILayout.Height(200));//水平
        {
            GUILayout.Label(dirLeftMap);//左
            GUILayout.Label(dirFrontMap);//前
            GUILayout.Label(dirRightMap);//右
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal(GUILayout.Height(200));//水平
        {
            GUILayout.Label(dirUpMap);//上
            GUILayout.Label(dirBackMap);//后
            GUILayout.Label(dirDownMap);//下
        }
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("刷新"))
        {
            
        }
        if (GUILayout.Button("路径"))
        {
            
        }
        if (GUILayout.Button("保存"))
        {
            CaptureSelected();
        }
        if (GUILayout.Button("保存外部"))
        {
            CaptureSelected();
        }
        
    }

    //
    public void CaptureSelected()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("请先选中需要截图的模型");
            return;
        }

        // 保存当前选择
        gameObjectsList.Clear();
        gameObjectsList.AddRange(Selection.gameObjects);

        // 创建临时截图相机
        SetupCamera(out Camera renderCamera, out Light renderLight);
        
        // 设置所有选中对象的图层
        SetObjectsLayer(Selection.gameObjects, 31);
        
        // 渲染并保存
        RenderAndSave(renderCamera);
        
        // 恢复设置
        Cleanup(renderCamera, renderLight);
        
        Debug.Log("截图完成！");
    }

    //
    static void SetupCamera(out Camera renderCamera, out Light renderLight)
    {
        // 创建临时相机
        GameObject cameraGO = new GameObject("Screenshot Camera");
        renderCamera = cameraGO.AddComponent<Camera>();
        
        // 计算包围盒
        Bounds bounds = CalculateSelectedBounds();
        
        // 配置相机
        renderCamera.orthographic = true;
        renderCamera.orthographicSize = bounds.extents.magnitude * 1.2f;
        renderCamera.nearClipPlane = 0.01f;
        renderCamera.farClipPlane = bounds.size.magnitude * 3f;
        renderCamera.backgroundColor = backgroundColor;
        renderCamera.clearFlags = CameraClearFlags.SolidColor;
        renderCamera.cullingMask = 1 << 31; // 只渲染第31层
        
        // 定位相机
        cameraGO.transform.position = bounds.center + Vector3.back * bounds.size.magnitude * 1.5f;
        cameraGO.transform.LookAt(bounds.center);
        
        // 添加光源
        GameObject lightGO = new GameObject("Screenshot Light");
        renderLight = lightGO.AddComponent<Light>();
        renderLight.type = LightType.Directional;
        renderLight.intensity = 1f;
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);
    }

    //
    static Bounds CalculateSelectedBounds()
    {
        Bounds bounds = new Bounds();
        bool initialized = false;
        
        foreach (var go in gameObjectsList)
        {
            foreach (var renderer in go.GetComponentsInChildren<Renderer>())
            {
                if (!initialized)
                {
                    bounds = renderer.bounds;
                    initialized = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
        }
        
        return bounds;
    }

    //
    static void SetObjectsLayer(GameObject[] objects, int layer)
    {
        foreach (var go in objects)
        {
            SetLayerRecursively(go, layer);
        }
    }

    //
    static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    //
    static void RenderAndSave(Camera renderCamera)
    {
        // 创建临时RenderTexture
        int resolution = 2048;
        RenderTexture rt = new RenderTexture(resolution, resolution, 32, RenderTextureFormat.ARGB32);
        renderCamera.targetTexture = rt;
        
        // ��染
        Texture2D screenshot = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false);
        renderCamera.Render();
        
        // 读取像素
        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        screenshot.Apply();
        
        // 保存为PNG
        byte[] bytes = screenshot.EncodeToPNG();
        string path = EditorUtility.SaveFilePanel(
            "保存截图",
            "",
            "ModelScreenshot.png",
            "png");
        
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllBytes(path, bytes);
        }
        
        // 清理
        RenderTexture.active = null;
        renderCamera.targetTexture = null;
        DestroyImmediate(rt);
        DestroyImmediate(screenshot);
    }

    //
    static void Cleanup(Camera renderCamera, Light renderLight)
    {
        // 恢复原始层级
        SetObjectsLayer(gameObjectsList.ToArray(), 0);
        
        // 销毁临时对象
        DestroyImmediate(renderCamera.gameObject);
        if (renderLight != null) DestroyImmediate(renderLight.gameObject);
    }
}
