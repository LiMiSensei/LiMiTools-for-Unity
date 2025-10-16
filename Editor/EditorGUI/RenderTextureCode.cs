using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class RenderTextureCode : EditorWindow
{
    [MenuItem("RenderTextureCode/RenderTextureCode")]
    static void Init()
    {
        RenderTextureCode window = (RenderTextureCode)EditorWindow.GetWindow(typeof(RenderTextureCode), 
            true, 
            "渲染纹理");
        window.Show();
    }

    private RenderTexture rt;
    int antiAliasing;
    private Object GO = null;
    void OnGUI()
    {
        GO = EditorGUILayout.ObjectField(GO, typeof(Object), true);
        rt = GO is RenderTexture?GO as RenderTexture:null;
        RenderTexture rt1 = RenderTexture.GetTemporary(512, 512);
        
        antiAliasing = EditorGUILayout.DelayedIntField("RenderTexture 的抗锯齿级别:",antiAliasing);//修改
        rt.antiAliasing = Mathf.Clamp(antiAliasing, 1, 8);
        //rt.antiAliasing = Mathf.Clamp(antiAliasing,1,8);
        EditorGUILayout.LabelField("RenderTexture 的抗锯齿级别:"+ rt.antiAliasing);
        EditorGUILayout.LabelField("如果设置了该标志，则自动生成多级渐进纹理级别:"+ rt.autoGenerateMips);
        EditorGUILayout.LabelField("如果为 true 且 antiAliasing 大于 1，则默认不解析渲染纹理:"+ rt.bindTextureMS);
        EditorGUILayout.LabelField("渲染纹理的颜色缓冲区（只读）:"+ rt.colorBuffer);
        EditorGUILayout.LabelField("渲染纹理深度缓冲区的精度:"+ rt.depth);
        EditorGUILayout.LabelField("渲染纹理的深度/模板缓冲区:"+ rt.depthBuffer);
        EditorGUILayout.LabelField("深度/模板缓冲区的格式:"+ rt.depthStencilFormat);
        EditorGUILayout.LabelField("此结构包含创建 RenderTexture 需要的所有信息:"+ rt.descriptor);
        EditorGUILayout.LabelField("渲染纹理的维度（类型）:"+ rt.dimension);
        EditorGUILayout.LabelField("在 Shader Model 5.0 级别着色器上启用随机访问写入该渲染纹理:"+ rt.enableRandomWrite);
        EditorGUILayout.LabelField("渲染纹理的颜色格式:"+ rt.graphicsFormat);
        EditorGUILayout.LabelField("渲染纹理的高度:"+ rt.height);
        EditorGUILayout.LabelField("渲染纹理的宽度:"+ rt.width);
        EditorGUILayout.LabelField("渲染纹理无记忆模式属性:"+ rt.memorylessMode);
        EditorGUILayout.LabelField("该渲染纹理是否使用 sRGB 读/写转换？（只读）:"+ rt.sRGB);
        EditorGUILayout.LabelField("可以封装在RenderTexture中的模具数据的格式:"+ rt.stencilFormat);
        EditorGUILayout.LabelField("当此标志设置为true时，渲染纹理将设置为由动态分辨率系统使用:"+ rt.useDynamicScale);
        EditorGUILayout.LabelField("当此标志设置为true时，渲染纹理将设置为由动态分辨率系统使用:"+ rt.useDynamicScaleExplicit);
        EditorGUILayout.LabelField("如果设置该标志，则渲染纹理具有多级渐进纹理:"+ rt.useMipMap);
        EditorGUILayout.LabelField("3D 渲染纹理的体积范围或数组纹理的切片数:"+ rt.volumeDepth);
        EditorGUILayout.LabelField("如果该 RenderTexture 是在立体渲染中使用的 VR 眼睛纹理，则该属性决定将进行哪些特殊渲染（如果有）:"+ rt.vrUsage);
        EditorGUILayout.LabelField("bool 如果创建了纹理，则为 true，否则为 false:"+ rt.Create());
        EditorGUILayout.LabelField("是否实际创建了该渲染纹理？:"+ rt.IsCreated());
        if (GUILayout.Button("执行"))
        {
            System.GC.Collect();
            //Debug.Log(rt.antiAliasing);
            EditorUtility.UnloadUnusedAssetsImmediate();
        }
//

        RenderTexture.ReleaseTemporary(rt1);
        rt.Release();
    }
}
