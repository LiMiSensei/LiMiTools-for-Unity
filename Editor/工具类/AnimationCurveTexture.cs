using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
/* 
 * 
 */
public class AnimationCurveTexture : EditorWindow
{
  [MenuItem("LiMi/工具/曲线转贴图工具 V1.1")]
  static void OpenWindow()
  {
    var window = EditorWindow.GetWindow<AnimationCurveTexture>("曲线转贴图工具");
    window.Show();
  }
  //---------------------------------------------------------------------
  List<AnimationCurve> curesList = new List<AnimationCurve>();
  private int index = 1;
  private int maxList = 10;
  private Vector2Int textureSize = new Vector2Int(256, 1);
  private Texture2D animationCurveTexture;
  FilterMode textureModel = FilterMode.Point;
 
  private string texturePath = "Assets";
  private string textureName = "AnimationCurveTexture";
  
  Vector2 scrollPos;
  //----------------------------------------------------------------------
  void OnGUI()
  {
    scrollPos  = EditorGUILayout.BeginScrollView(scrollPos);
    //动态设置列表大小
    index = EditorGUILayout.IntSlider(index, 1, maxList);
    if (curesList.Count != maxList)
    {
      SetListCount(curesList, maxList);
    }
    
    //屏幕显示AnimationCurve
    for (int i = index-1; i >= 0 ; i--)
    {
      if(curesList[i] == null)curesList[i] = AnimationCurve.Linear(0, 0, 1, 1);
      curesList[i] = EditorGUILayout.CurveField(curesList[i]);
    }
    
    
    //设置长宽等
    EditorGUILayout.BeginHorizontal();//水平布局
    {
      EditorGUILayout.LabelField("长:",GUILayout.Width(30));
      textureSize.x = EditorGUILayout.IntField(textureSize.x);
      EditorGUILayout.LabelField("宽:",GUILayout.Width(30));
      textureSize.y = EditorGUILayout.IntField(textureSize.y);
    }
    EditorGUILayout.EndHorizontal();
    
    //设置图像过滤模式
    textureModel = (FilterMode)EditorGUILayout.EnumPopup("图像过滤模式：",textureModel);
    
    //设置路径和名称
    EditorGUILayout.BeginHorizontal();
    {
      if (GUILayout.Button($"保存路径:{texturePath}")) texturePath = SelectionPath();
      EditorGUILayout.LabelField("名称:",GUILayout.Width(30));
      textureName = EditorGUILayout.TextField(textureName);
    }
    EditorGUILayout.EndHorizontal();
    
    //按钮：执行生成 和 保存
    EditorGUILayout.BeginHorizontal();
    if (GUILayout.Button("生成"))animationCurveTexture = AddSize(textureSize,index,curesList);
    EditorGUILayout.EndHorizontal();
    
    //屏幕显示合成后的颜色
    if (animationCurveTexture != null)
    {
      Rect texRect = GUILayoutUtility.GetRect(100, 200);
      EditorGUI.DrawPreviewTexture(texRect, animationCurveTexture);
      GUILayout.Label(animationCurveTexture, GUILayout.Width(256), GUILayout.Height(1));
      if (GUILayout.Button("保存")) UnityTextureSave(animationCurveTexture, texturePath, textureName);
    }
    EditorGUILayout.EndScrollView();
    
    //作者信息
    GUILayout.FlexibleSpace();
    GUILayout.Label("@LiMi  Version:1.1.0  Updated:2025-09-21", EditorStyles.miniLabel);
  }
  
  //选择文件夹方法
  string SelectionPath()
  {
    // 1. 获取选中的资源（过滤掉非资产对象）
    Object[] selectedObjects = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
    // 2. 处理空选择情况
    if (selectedObjects.Length == 0)
    {
      return  "Assets";
    }
    // 3. 获取第一个选中对象的GUID和路径
    string guid = Selection.assetGUIDs[0];
    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
    // 4. 统一路径格式并处理文件夹/文件
    if (AssetDatabase.IsValidFolder(assetPath))
    {
      // 文件夹直接使用原路径（自动统一为正斜杠）
      return assetPath;
    }
    else
    {
      // 文件获取父目录并确保使用正斜杠
      return  Path.GetDirectoryName(assetPath).Replace("\\", "/");
    }
  }
  
  // 调整List到指定数量（泛型方法）
  void SetListCount<T>(List<T> list, int targetCount, T defaultValue = default)
  {
    int current = list.Count;
    
    if (targetCount > current)
    {
      // 扩容
      if (list.Capacity < targetCount)
        list.Capacity = targetCount; // 预分配内存
        
      list.AddRange(Enumerable.Repeat(defaultValue, targetCount - current));
    }
    else if (targetCount < current)
    {
      // 缩容
      list.RemoveRange(targetCount, current - targetCount);
    }
    // 数量相等时不操作
  }
  
  //主要算法
  Texture2D AddSize(Vector2Int size,int index,List<AnimationCurve> List)
  {
    Texture2D texture = new Texture2D(size.x, size.y * index);
    texture.filterMode = textureModel;
    Color[] a1 = new Color[size.x * size.y  * List.Count];
    int num = 0;
   
    for (int i = 0; i < index; i++)//1
      for (int j = 0; j < size.y; j++)//1
      for (int k = 0; k < size.x; k++)//256
      {
        float color = List[i].Evaluate((float)k/size.x);
        a1[num] = new Color(color, color, color, color);
        num++;
      }
    texture.SetPixels(a1);
    texture.Apply();
    return texture;
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
  }

}
