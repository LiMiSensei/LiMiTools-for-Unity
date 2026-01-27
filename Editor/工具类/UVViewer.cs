using UnityEditor;
using UnityEngine;
using System.IO;
/*已记录Bug：
 *  无法动态更换UV颜色
 *  包含父子级物体无法全部显示
 *  没有选中依旧能保存纹理
 *  靠近会导致性能下降
 */

public class UVViewer : EditorWindow
{
   
    private Mesh targetMesh;
    private Material uvDisplayMaterial;
    
    private int selectedUVChannel = 0;
    private float uvScale = 2.0f;
    private Color uvColor = Color.green;
   
    private GameObject oldObject;
    private Texture2D uvTexture2;
    private bool isOne = false;
    private string tpath = "Assets";
    private string debugpreview = "";
    

    [MenuItem("LiMi/工具/UV查看器 V1.0")]
    public static void ShowWindow()
    {
        var window = GetWindow<UVViewer>();
        window.titleContent.text = "UV查看器";
        window.minSize = new Vector2(300, 400);
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        Selection.selectionChanged += OnSelectionChanged;
        LoadMaterial();
        UpdateTargetMesh();
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        Selection.selectionChanged -= OnSelectionChanged;
        oldObject?.SetActive(true);
    }

    void OnGUI()
    {
        //UV显示设置
        selectedUVChannel = EditorGUILayout.IntSlider("UV通道", selectedUVChannel, 0, 3);
        uvScale = EditorGUILayout.Slider("UV大小", uvScale, 0.1f, 5.0f);
        uvColor = EditorGUILayout.ColorField("UV颜色", uvColor);
        EditorGUILayout.Space();
        
        // 目标网格信息
        if (targetMesh != null)
        {
            EditorGUILayout.LabelField("当前网格:", EditorStyles.boldLabel);
            EditorGUILayout.ObjectField("网格：", targetMesh, typeof(Mesh), false);
            EditorGUILayout.LabelField("顶点: " + targetMesh.vertexCount);
            EditorGUILayout.LabelField("三角形: " + targetMesh.triangles.Length / 3);
            EditorGUILayout.LabelField("UV数: " + GetUVCount(targetMesh));
        }
        else EditorGUILayout.HelpBox("选择一个网格对象来显示uv", MessageType.Info);
       
        EditorGUILayout.Space();

        //两个按钮 选择路径 & 保存贴图
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button($"选中路径：{tpath}"))
        {
            tpath = SelectionPath();
        }
        if (GUILayout.Button("保存UV纹理"))
        {
            UnityTextureSave(uvTexture2,tpath,"UV_Preview");
        }
        EditorGUILayout.EndHorizontal();
        
        //状态输出
        EditorGUILayout.LabelField("状态:", EditorStyles.boldLabel);
        EditorGUILayout.TextArea(debugpreview, GUILayout.Height(60));
        
        //作者信息
        GUILayout.FlexibleSpace();
        GUILayout.Label("@LiMi  Version:1.0.1  Updated:2025-09-21", EditorStyles.miniLabel);
    }

    //订阅事件
    void OnSceneGUI(SceneView sceneView)
    {
        if (targetMesh == null || uvDisplayMaterial == null)
        {
            oldObject?.SetActive(true);
            return;
        }
        // 获取选中的对象
        GameObject selectedObject = Selection.activeGameObject;
        // 如果没有选中任何对象
        if (selectedObject == null)
        {
            oldObject?.SetActive(true);
            return;
        }

        // 如果选中了新对象
        if (selectedObject != oldObject)
        {
           
            UpdateTargetMesh();
            debugpreview = $"更新了贴图:{targetMesh.name}";
            uvTexture2 = GetUVTexture();
            oldObject?.SetActive(true);
            oldObject = selectedObject;
        }

        // 隐藏当前选中的对象
        selectedObject.SetActive(false);
        
        // 设置材质属性
        uvDisplayMaterial.SetTexture("_MainTex",uvTexture2);
        uvDisplayMaterial.SetColor("_Color", uvColor);
        uvDisplayMaterial.SetFloat("_Scale", uvScale);
        uvDisplayMaterial.SetInt("_UVChannel", selectedUVChannel);
        
        // 绘制UV
        Matrix4x4 matrix = selectedObject.transform.localToWorldMatrix;
        Graphics.DrawMesh(targetMesh, matrix, uvDisplayMaterial,0);
    }

    //订阅事件
    void OnSelectionChanged()
    {
        UpdateTargetMesh();
        Repaint();
    }

    //刷新网格
    void UpdateTargetMesh()
    {
        targetMesh = null;
        
        if (Selection.activeGameObject != null)
        {
            MeshFilter meshFilter = Selection.activeGameObject.GetComponent<MeshFilter>();
            SkinnedMeshRenderer skinnedRenderer = Selection.activeGameObject.GetComponent<SkinnedMeshRenderer>();
            
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                targetMesh = meshFilter.sharedMesh;
            }
            else if (skinnedRenderer != null && skinnedRenderer.sharedMesh != null)
            {
                targetMesh = skinnedRenderer.sharedMesh;
            }
        }
    }

    //加载材质球
    void LoadMaterial()
    {
        // 创建或获取UV显示材质
        Shader uvShader = Shader.Find("Hidden/Internal-UVDisplay");
        if (uvShader == null)
        {
            uvShader = CreateUVShader();
        }
        
        uvDisplayMaterial = new Material(uvShader);
        uvDisplayMaterial.hideFlags = HideFlags.HideAndDontSave;
    }

    //Shader
    Shader CreateUVShader()
    {
        string shaderCode = @"Shader ""Hidden/Internal-UVDisplay""
{
    Properties
    {
        _Color (""Color"", Color) = (1,1,1,1)
        _Scale (""Scale"", Float) = 1.0
        _UVChannel (""UV Channel"", Int) = 0
        _MainTex(""Texture2D"", 2D) = ""white"" {}
    }
    
    SubShader
    {
        Tags { ""RenderType"" = ""Transparent"" ""Queue""=""Transparent"" }
        Pass
        {
            ZWrite Off
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include ""UnityCG.cginc""
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float2 uv3 : TEXCOORD3;
            };
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            sampler2D _MainTex;
            fixed4 _Color;
            float _Scale;
            int _UVChannel;
            
            v2f vert (appdata v)
            {
                v2f o;
                float2 selectedUV;
                     if (_UVChannel == 0) selectedUV = v.uv0;
                else if (_UVChannel == 1) selectedUV = v.uv1;
                else if (_UVChannel == 2) selectedUV = v.uv2;
                else selectedUV = v.uv3;
                o.vertex = UnityObjectToClipPos(float4(selectedUV * 2.0 - 1.0, 0, 1) * _Scale);
                o.uv = selectedUV;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float4 mainTex = tex2D(_MainTex,i.uv);
                clip(mainTex.a - 0.1);
                return mainTex;
            }
            ENDCG
        }
    }
}";

        // 创建临时shader
        Shader shader = ShaderUtil.CreateShaderAsset(shaderCode);
        shader.hideFlags = HideFlags.HideAndDontSave;
        return shader;
    }

    //获取UV
    int GetUVCount(Mesh mesh)
    {
        int count = 0;
        if (mesh.uv != null && mesh.uv.Length > 0) count++;
        if (mesh.uv2 != null && mesh.uv2.Length > 0) count++;
        if (mesh.uv3 != null && mesh.uv3.Length > 0) count++;
        if (mesh.uv4 != null && mesh.uv4.Length > 0) count++;
        if (mesh.uv5 != null && mesh.uv5.Length > 0) count++;
        if (mesh.uv6 != null && mesh.uv6.Length > 0) count++;
        if (mesh.uv7 != null && mesh.uv7.Length > 0) count++;
        if (mesh.uv8 != null && mesh.uv8.Length > 0) count++;
        return count;
    }
    
    //项目选中路径
    string SelectionPath()
    {
        // 1. 获取选中的资源（过滤掉非资产对象）
        Object[] selectedObjects = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
        // 2. 处理空选择情况
        if (selectedObjects.Length == 0)
        {
            debugpreview = "没有选择文件或文件夹";
            return "Assets";
        }
        // 3. 获取第一个选中对象的GUID和路径
        string guid = Selection.assetGUIDs[0];
        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
        // 4. 统一路径格式并处理文件夹/文件
        if (AssetDatabase.IsValidFolder(assetPath))
        {
            // 文件夹直接使用原路径（自动统一为正斜杠）
            debugpreview = $"文件路径为：{assetPath}";
            return assetPath;
            
        }
        else
        {
            // 文件获取父目录并确保使用正斜杠
            debugpreview = $"文件路径为：{Path.GetDirectoryName(assetPath).Replace("\\", "/")}";
            return  Path.GetDirectoryName(assetPath).Replace("\\", "/");
        }
    }
    
    //直接获取UV图
    Texture2D GetUVTexture()
    {
        // 创建UV布局预览图
        Texture2D uvTexture1 = new Texture2D(1024, 1024);
        Color[] pixels = new Color[1024 * 1024];
        
        // 绘制UV三角形
        Vector2[] uvs = GetUVsForChannel(targetMesh, selectedUVChannel);
        int[] triangles = targetMesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector2 uv1 = uvs[triangles[i]];
            Vector2 uv2 = uvs[triangles[i + 1]];
            Vector2 uv3 = uvs[triangles[i + 2]];
            DrawUVTriangle(uvTexture1, pixels, uv1, uv2, uv3, uvColor);
        }
        uvTexture1.SetPixels(pixels);
        uvTexture1.Apply();
        return uvTexture1;
    }
    
    //获取UV
    Vector2[] GetUVsForChannel(Mesh mesh, int channel)
    {
        switch (channel)
        {
            case 0: return mesh.uv;
            case 1: return mesh.uv2;
            case 2: return mesh.uv3;
            case 3: return mesh.uv4;
            default: return mesh.uv;
        }
    }
    
    //转换UV并绘制线绘制线
    void DrawUVTriangle(Texture2D texture, Color[] pixels, Vector2 uv1, Vector2 uv2, Vector2 uv3, Color color)
    {
        // 将UV坐标转换为像素坐标
        int x1 = (int)(uv1.x * 1024);
        int y1 = (int)(uv1.y * 1024);
        int x2 = (int)(uv2.x * 1024);
        int y2 = (int)(uv2.y * 1024);
        int x3 = (int)(uv3.x * 1024);
        int y3 = (int)(uv3.y * 1024);
        
        // 绘制三角形边线
        DrawLine(texture, pixels, x1, y1, x2, y2, color);
        DrawLine(texture, pixels, x2, y2, x3, y3, color);
        DrawLine(texture, pixels, x3, y3, x1, y1, color);
    }
    
    //绘制线
    void DrawLine(Texture2D texture, Color[] pixels, int x1, int y1, int x2, int y2, Color color)
    {
        int dx = Mathf.Abs(x2 - x1);
        int dy = Mathf.Abs(y2 - y1);
        int sx = (x1 < x2) ? 1 : -1;
        int sy = (y1 < y2) ? 1 : -1;
        int err = dx - dy;
        
        while (true)
        {
            if (x1 >= 0 && x1 < 1024 && y1 >= 0 && y1 < 1024)
            {
                pixels[y1 * 1024 + x1] = color;
            }
            if (x1 == x2 && y1 == y2) break;
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x1 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y1 += sy;
            }
        }
    }

    //保存贴图方法
    void UnityTextureSave(Texture2D texture,string path,string name)
    {
        //安全判断
        if(texture == null)return;
        if (!Directory.Exists(path))path = "Assets";
        if(name.Length == 0) name = "Texture";
        
        //保存
        byte[] pngData = texture.EncodeToPNG();
        string fullPath = Path.Combine(path,name+".png");
        File.WriteAllBytes(fullPath, pngData);
        
        //ping
        AssetDatabase.Refresh();
        Object obj = AssetDatabase.LoadAssetAtPath(fullPath, typeof(Object));
        EditorGUIUtility.PingObject(obj);
        debugpreview = "保存成功";
    }
    
}
