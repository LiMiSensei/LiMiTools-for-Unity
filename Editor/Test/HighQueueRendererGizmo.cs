using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class HighQueueRendererGizmo : EditorWindow
{
    [MenuItem("LiMi的工具/队列查看器 V0.1")]
    public static void ShowWindow()
    {
        GetWindow<HighQueueRendererGizmo>("队列查看器");
    }

    private int minQueue = 2000;
    private int maxQueue = 3000;
    private bool autoRefresh = true;
    private double lastRefreshTime;
    
    private List<RendererInfo> highQueueRenderers = new List<RendererInfo>();
    private Vector2 scrollPosition;

    private class RendererInfo
    {
        public Renderer renderer;
        public int queue;
        public Material[] materials;
    }

    private void OnEnable()
    {
        EditorApplication.update += OnUpdate;
        RefreshRenderers();
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnUpdate;
    }

    private void OnUpdate()
    {
        if (autoRefresh && EditorApplication.timeSinceStartup - lastRefreshTime > 2.0f)
        {
            lastRefreshTime = EditorApplication.timeSinceStartup;
            RefreshRenderers();
        }
    }

    private void OnGUI()
    {
        DrawSettingsPanel();
        DrawRendererList();
        //作者信息
        GUILayout.FlexibleSpace();
        GUILayout.Label("@LiMi  Version:0.1  Updated:2025-09-22", EditorStyles.miniLabel);
    }
    
    //------------------------------------------------------------------------------------------------------------------

    private void DrawSettingsPanel()
    {
        EditorGUILayout.BeginVertical("Box");
        {
            EditorGUILayout.LabelField("设置", EditorStyles.boldLabel);
        
            EditorGUILayout.LabelField("队列范围", EditorStyles.boldLabel);
            minQueue = EditorGUILayout.IntField("最小队列", minQueue);
            maxQueue = EditorGUILayout.IntField("最大队列", maxQueue);
        
            EditorGUILayout.Space();
            autoRefresh = EditorGUILayout.Toggle("自动刷新", autoRefresh);
        
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("立即刷新"))
                {
                    RefreshRenderers();
                }
                if (GUILayout.Button("全选"))
                {
                    SelectAllRenderers();
                }
                if (GUILayout.Button("清除选择"))
                {
                    Selection.activeObject = null;
                }
            }
            EditorGUILayout.EndHorizontal();

        }
        EditorGUILayout.EndVertical();
        
        
    }

    
    //List内容
    private void DrawRendererList()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal("Box");
        {
            EditorGUILayout.LabelField($"对象： ({highQueueRenderers.Count})", EditorStyles.boldLabel,GUILayout.Width(100));
            EditorGUILayout.LabelField("队列: ", EditorStyles.boldLabel,GUILayout.Width(50));
            EditorGUILayout.LabelField("材质球数量:", EditorStyles.boldLabel,GUILayout.Width(100));
        }
        EditorGUILayout.EndHorizontal();
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        {
            foreach (var rendererInfo in highQueueRenderers)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    // 显示渲染器对象字段
                    EditorGUILayout.ObjectField(rendererInfo.renderer, typeof(Renderer), true, GUILayout.Width(100));
            
                    // 显示队列数值
                    EditorGUILayout.LabelField(rendererInfo.queue.ToString(), GUILayout.Width(70));
                    
                    // 材质球数量
                    EditorGUILayout.LabelField(rendererInfo.materials.Length.ToString(),GUILayout.Width(30));
                    // 聚焦按钮
                    if (GUILayout.Button("选择", GUILayout.Width(60)))
                    {
                
                        FocusOnRenderer(rendererInfo.renderer);
                    }

                    
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndScrollView();
    }
//----------------------------------------------------------------------------------------------------------------------
    //刷新渲染组件
    private void RefreshRenderers()
    {
        highQueueRenderers.Clear();
        
        // 获取场景中所有渲染器
        var allRenderers = FindObjectsOfType<Renderer>();
        
        foreach (var renderer in allRenderers)
        {
            var materials = renderer.sharedMaterials;
            if (materials == null) continue;
            foreach (var material in materials)
            {
                if (material != null && material.renderQueue >= minQueue && material.renderQueue <= maxQueue)
                {
                    highQueueRenderers.Add(new RendererInfo
                    {
                        renderer = renderer,
                        queue = material.renderQueue,
                        materials = materials
                    });
                    break;
                }
            }
        }
        
        // 按队列排序
        highQueueRenderers = highQueueRenderers.OrderBy(r => r.queue).ToList();
        Repaint();
    }

    //全选按钮的功能
    private void SelectAllRenderers()
    {
        var selectedObjects = highQueueRenderers.Select(r => r.renderer.gameObject).ToArray();
        Selection.objects = selectedObjects;
    }

    
    // 聚焦功能 - 将场景视图摄像机对准选中的渲染器
    private void FocusOnRenderer(Renderer renderer)
    {
        if (renderer == null) return;
        // 获取场景视图
        
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null) return;
        
        // 选择对象
        Selection.activeGameObject = renderer.gameObject;
        
        if (Selection.activeTransform != null)
        {
            SceneView.lastActiveSceneView.FrameSelected();
            SceneView.lastActiveSceneView.size = 5f; 
        }
        // 确保对象在视图中可见
        EditorGUIUtility.PingObject(renderer.gameObject);
    }

    
}
