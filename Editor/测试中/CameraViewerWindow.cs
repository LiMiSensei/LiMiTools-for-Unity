using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class CameraViewerWindow : EditorWindow
{
    [MenuItem("LiMi/测试/Camera Viewer")]
    public static void ShowWindow()
    {
        GetWindow<CameraViewerWindow>("Camera Viewer");
    }

    private Camera targetCamera;
    private RenderTexture renderTexture;
    private Vector2 scrollPosition;
    private float scale = 1.0f;
    private bool showSettings = true;
    private bool autoRefresh = true;
    private float refreshRate = 0.033f; // 30 FPS
    private double lastRefreshTime;

    private void OnEnable()
    {
        // 创建默认的渲染纹理
        CreateRenderTexture(512, 512);
        
        // 开始刷新协程
        EditorApplication.update += UpdateView;
    }

    private void OnDisable()
    {
        EditorApplication.update -= UpdateView;
        if (renderTexture != null)
        {
            renderTexture.Release();
            DestroyImmediate(renderTexture);
        }
    }

    private void UpdateView()
    {
        if (autoRefresh && EditorApplication.timeSinceStartup - lastRefreshTime > refreshRate)
        {
            lastRefreshTime = EditorApplication.timeSinceStartup;
            Repaint();
        }
    }

    private void CreateRenderTexture(int width, int height)
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            DestroyImmediate(renderTexture);
        }

        renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
        {
            antiAliasing = 4,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        renderTexture.Create();
    }

    private void OnGUI()
    {
        DrawToolbar();
        DrawCameraSelection();
        
        if (showSettings)
        {
            DrawSettingsPanel();
        }

        DrawCameraView();

        // 处理拖拽缩放
        HandleZoomEvents();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
        {
            Repaint();
        }
        
        autoRefresh = GUILayout.Toggle(autoRefresh, "Auto Refresh", EditorStyles.toolbarButton);
        
        if (GUILayout.Button("Settings", EditorStyles.toolbarButton))
        {
            showSettings = !showSettings;
        }
        
        if (GUILayout.Button("Save Snapshot", EditorStyles.toolbarButton))
        {
            SaveSnapshot();
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawCameraSelection()
    {
        EditorGUILayout.BeginHorizontal();
        
        EditorGUILayout.LabelField("Target Camera:", GUILayout.Width(100));
        targetCamera = (Camera)EditorGUILayout.ObjectField(targetCamera, typeof(Camera), true);
        
        if (GUILayout.Button("Find Main", GUILayout.Width(80)))
        {
            targetCamera = Camera.main;
        }
        
        if (GUILayout.Button("Scene View", GUILayout.Width(80)))
        {
            targetCamera = SceneView.lastActiveSceneView?.camera;
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawSettingsPanel()
    {
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("Display Settings", EditorStyles.boldLabel);
        
        scale = EditorGUILayout.Slider("Zoom Scale", scale, 0.1f, 3f);
        refreshRate = EditorGUILayout.Slider("Refresh Rate (s)", refreshRate, 0.016f, 1f);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("640x480"))
        {
            CreateRenderTexture(640, 480);
        }
        if (GUILayout.Button("1024x768"))
        {
            CreateRenderTexture(1024, 768);
        }
        if (GUILayout.Button("1920x1080"))
        {
            CreateRenderTexture(1920, 1080);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }

    private void DrawCameraView()
    {
        if (targetCamera == null)
        {
            EditorGUILayout.HelpBox("Please select a camera to view.", MessageType.Info);
            return;
        }

        if (renderTexture == null)
        {
            CreateRenderTexture(512, 512);
        }

        
        // 渲染摄像机画面
        RenderCamera();

        // 计算显示区域
        Rect viewRect = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        
        // 绘制摄像机画面
        GUI.DrawTexture(viewRect, renderTexture, ScaleMode.ScaleToFit, true);
        
        // 绘制信息覆盖层
        DrawOverlayInfo(viewRect);
    }

    private void RenderCamera()
    {
        if (targetCamera != null && renderTexture != null)
        {
            // 保存原始目标纹理
            RenderTexture originalTarget = targetCamera.targetTexture;
            
            // 设置渲染目标
            targetCamera.targetTexture = renderTexture;
            
            // 渲染摄像机
            targetCamera.Render();
            
            // 恢复原始目标纹理
            targetCamera.targetTexture = originalTarget;
        }
    }

    private void DrawOverlayInfo(Rect viewRect)
    {
        // 在画面右上角显示信息
        Rect infoRect = new Rect(viewRect.xMax - 200, viewRect.y + 10, 190, 60);
        EditorGUI.DrawRect(infoRect, new Color(0, 0, 0, 0.8f));
        
        GUIStyle infoStyle = new GUIStyle(EditorStyles.label)
        {
            normal = { textColor = Color.white },
            fontSize = 10
        };
        
        GUI.Label(new Rect(infoRect.x + 5, infoRect.y + 5, infoRect.width - 10, 20), 
                 $"Camera: {targetCamera.name}", infoStyle);
        GUI.Label(new Rect(infoRect.x + 5, infoRect.y + 25, infoRect.width - 10, 20), 
                 $"Resolution: {renderTexture.width}x{renderTexture.height}", infoStyle);
        GUI.Label(new Rect(infoRect.x + 5, infoRect.y + 45, infoRect.width - 10, 20), 
                 $"Scale: {scale:F2}x", infoStyle);
    }

    private void HandleZoomEvents()
    {
        Event currentEvent = Event.current;
        
        if (currentEvent.type == EventType.ScrollWheel)
        {
            scale = Mathf.Clamp(scale - currentEvent.delta.y * 0.01f, 0.1f, 3f);
            currentEvent.Use();
        }
    }

    private void SaveSnapshot()
    {
        if (renderTexture == null) return;

        string path = EditorUtility.SaveFilePanel(
            "Save Camera Snapshot",
            Application.dataPath,
            $"CameraSnapshot_{System.DateTime.Now:yyyyMMdd_HHmmss}.png",
            "png"
        );

        if (!string.IsNullOrEmpty(path))
        {
            // 创建临时纹理来读取渲染纹理
            Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
            RenderTexture.active = renderTexture;
            tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            byte[] bytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
            DestroyImmediate(tex);

            Debug.Log($"Snapshot saved to: {path}");
            
            // 如果是Assets路径，刷新数据库
            if (path.StartsWith(Application.dataPath))
            {
                string assetPath = "Assets" + path.Substring(Application.dataPath.Length);
                AssetDatabase.Refresh();
            }
        }
    }
}
