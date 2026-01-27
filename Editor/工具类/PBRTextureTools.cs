using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


public class PBRTextureTools : EditorWindow
{
   [MenuItem("LiMi/工具/图像处理工具 v1.0")]
   static void Open()
   {
      var window = (PBRTextureTools) GetWindow(typeof(PBRTextureTools),true,"图像处理工具v1.0");
      window.maxSize = new Vector2(450, 1000);
      window.Show();
     
      
   }
   
   //基础色贴图相关
   bool isBase =  false;
   private Texture2D BaseTexture;
   private string baseColorPath = "点击选择保存路径";
   private string baseColorNewPath = "";
   private string baseColorName = "BaseColor";
   private Vector2Int BaseColorSize = new Vector2Int(9,9);
   private Vector2Int m_BaseColorSize;
   bool isBaseName = false;
   //Mask贴图相关
   bool isMask = false;
   bool isRounessMap = false;
   private Texture2D MaskTexture;
   private Texture2D TextureR;
   private Texture2D TextureG;
   private Texture2D TextureB;
   private Texture2D TextureA;
   private Vector2Int MaskColorSize = new Vector2Int(9,9);
   private Vector2Int m_MaskColorSize;
   string MaskTextureName = "MaskColor";
   string MaskTexturePath = "点击选择保存路径";
   private string MaskTextureNewPath = "";
   //法线相关
   bool isNormal = false;
   private Texture2D NormalTexture;
   
   //分离图像
   bool isSplit = false;
   //Texture2D SplitTexture;
   Texture2D MergeTexture;
   Texture2D OldMergeTexture;
   string MergeTextureName = "MergeTexture";
   private string MergeTexturePath = "点击选择路径";
   private string m_MergeTexturePath = "";
   private Vector2Int MergeTextureSize = new Vector2Int(9,9);
   private Vector2Int m_MergeTextureSize;
   Texture2D SplitTextureR;
   Texture2D SplitTextureG;
   Texture2D SplitTextureB;
   Texture2D SplitTextureA;
   

   private void OnEnable()
   {
      BaseTexture   = GetMainTexture(1);
      MaskTexture   = GetMainTexture(1);
      TextureR      = GetMainTexture(1);
      TextureG      = GetMainTexture(1);
      TextureB      = GetMainTexture(0);
      TextureA      = GetMainTexture(1);
      NormalTexture = GetMainTexture(3);
   }

   private void OnGUI()
   {
      #region 基础色贴图

      //基础色贴图
      isBase = EditorGUILayout.BeginFoldoutHeaderGroup(isBase, "贴图大小修改：2次幂");
      if (isBase)
      {
         EditorGUILayout.BeginHorizontal();
         {
            EditorGUILayout.BeginVertical();
            BaseTexture =  (Texture2D)EditorGUILayout.ObjectField(BaseTexture,typeof(Texture2D),GUILayout.Width(100));
            GUILayout.Label( BaseTexture,GUILayout.Height(100));
            EditorGUILayout.EndVertical();
            EditorGUILayout.TextField("HSV");
         }
         EditorGUILayout.EndHorizontal();
            
         EditorGUILayout.BeginVertical();
         {
            m_BaseColorSize.x =(EditorGUILayout.IntSlider(m_BaseColorSize.x,5,12));
            m_BaseColorSize.y =(EditorGUILayout.IntSlider(m_BaseColorSize.y,5,12));
            BaseColorSize.x = (int)Mathf.Pow(2, m_BaseColorSize.x);
            BaseColorSize.y = (int)Mathf.Pow(2, m_BaseColorSize.y);
            
            if (BaseTexture!= null)
            {
               EditorGUILayout.LabelField(BaseTexture.width +"X"+ BaseTexture.height + " => "+BaseColorSize.x +"X" +BaseColorSize.y);
               //EditorGUILayout.LabelField(BaseColorSize.ToString());
            }
         }
         EditorGUILayout.EndVertical();
         
         if (GUILayout.Button(baseColorPath))
         {
            baseColorPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(Selection.activeObject));
         }

         baseColorName = EditorGUILayout.TextArea(baseColorName);
         if (GUILayout.Button("选择选中的名字"))
         {
            if(Selection.activeObject == null) return;
            baseColorName = Selection.activeObject.name;
         }
         
         isBaseName = EditorGUILayout.ToggleLeft("是否BaseMap名",isBaseName);
         if(GUILayout.Button("执行大小"))
         {
            Texture2D a1 = TextureScalerGPU(BaseColorSize, BaseTexture);
            SaveTexture(baseColorName,baseColorPath,isBaseName,a1,ref baseColorNewPath);
         }
            
      }
      EditorGUILayout.EndFoldoutHeaderGroup();

      #endregion

      #region Maskt贴图

      //Maskt贴图
      isMask = EditorGUILayout.BeginFoldoutHeaderGroup(isMask, "Maskt贴图流合并：R金属 G：AO B：Detail A：平滑度");
      if (isMask)
      {
         EditorGUILayout.BeginHorizontal(style:"box",GUILayout.Width(400));
         {
            EditorGUILayout.BeginVertical();
            {
               EditorGUILayout.LabelField("选择金属度贴图",GUILayout.Width(100));
               TextureR =  (Texture2D)EditorGUILayout.ObjectField(TextureR,typeof(Texture2D),GUILayout.Width(100));
               GUILayout.Label( TextureR,GUILayout.Width(100),GUILayout.Height(100));
            }
            EditorGUILayout.EndVertical();
         
            EditorGUILayout.BeginVertical();
            {
               EditorGUILayout.LabelField("选择AO贴图",GUILayout.Width(100));
               TextureG =  (Texture2D)EditorGUILayout.ObjectField(TextureG,typeof(Texture2D),GUILayout.Width(100));
               GUILayout.Label( TextureG,GUILayout.Width(100),GUILayout.Height(100));
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical();
            {
               EditorGUILayout.LabelField("选择DetailMask贴图",GUILayout.Width(100));
               TextureB =  (Texture2D)EditorGUILayout.ObjectField(TextureB,typeof(Texture2D),GUILayout.Width(100));
               GUILayout.Label( TextureB,GUILayout.Width(100),GUILayout.Height(100));
            }
            EditorGUILayout.EndVertical();
         
            EditorGUILayout.BeginVertical();
            {
               isRounessMap = EditorGUILayout.ToggleLeft("是粗糙度贴图？",isRounessMap,GUILayout.Width(100));
               TextureA =  (Texture2D)EditorGUILayout.ObjectField(TextureA,typeof(Texture2D),GUILayout.Width(100));
               GUILayout.Label( TextureA,GUILayout.Width(100),GUILayout.Height(100));
            }
            EditorGUILayout.EndVertical();
         }
         EditorGUILayout.EndHorizontal();
         //----------------------
         
        
         
         
         EditorGUILayout.BeginHorizontal();
         {
            GUILayout.Label( MaskTexture,style:"box",GUILayout.Width(100),GUILayout.Height(100));
            EditorGUILayout.BeginVertical(GUILayout.Width(300));//垂直布局
            {
               EditorGUILayout.BeginVertical();//放滑条  //垂直布局
               {
                  MaskColorSize.x =(EditorGUILayout.IntSlider(MaskColorSize.x,5,12));
                  MaskColorSize.y =(EditorGUILayout.IntSlider(MaskColorSize.y,5,12));
                  m_MaskColorSize.x = (int)Mathf.Pow(2, MaskColorSize.x);
                  m_MaskColorSize.y = (int)Mathf.Pow(2, MaskColorSize.y);
               }
               EditorGUILayout.EndVertical();
               
               EditorGUILayout.BeginHorizontal();//水平 //大小和是否粗糙度贴图
               {
                  if (MaskTexture!= null)
                  {
                     EditorGUILayout.LabelField(MaskTexture.width +"X"+ MaskTexture.height 
                     + " => "+m_MaskColorSize.x +"X" +m_MaskColorSize.y);
                  }
                  
               }
               EditorGUILayout.EndVertical();
               
               EditorGUILayout.BeginHorizontal(); //水平 //放选择保存路径
               {
                  if (GUILayout.Button(MaskTexturePath))
                  {
                     MaskTexturePath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(Selection.activeObject));
                  }
               }
               EditorGUILayout.EndVertical();
               EditorGUILayout.BeginHorizontal(); //水平 //放贴图名字和执行按钮
               {
                  baseColorName = EditorGUILayout.TextArea(baseColorName);
                  if (GUILayout.Button("执行合并"))
                  {
                     Texture2D[] ccc = { TextureR, TextureG, TextureB, TextureA };
                     MaskTexture = MaskTextureHB(ccc, m_MaskColorSize,isRounessMap);
                     SaveTexture(MaskTextureName, MaskTexturePath, false, MaskTexture,ref MaskTextureNewPath);
                     SettingInput(MaskTextureNewPath,false);
                  }
               }
               EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
         }
         EditorGUILayout.EndVertical();
         
         
         
         
               
               
         
            
               
         
            
         
      }
      EditorGUILayout.EndFoldoutHeaderGroup();

         #endregion

      #region 法线贴图

         ////------------------------------------------------法线贴图------------------------------------------------------------
         //isNormal = EditorGUILayout.BeginFoldoutHeaderGroup(isNormal, "法线贴图");
      //
         //if (isNormal)
         //{
         //   EditorGUILayout.BeginVertical();
         //   GUILayout.Label( NormalTexture,GUILayout.Height(100));
         //   EditorGUILayout.EndVertical();
         //}
         //EditorGUILayout.EndFoldoutHeaderGroup();
         

      #endregion
      
      #region 拆分贴图
      
      
      isSplit = EditorGUILayout.BeginFoldoutHeaderGroup(isSplit, "图像通道拆分");
      {
         if (isSplit)
         {
            EditorGUILayout.BeginVertical(); //垂直
            {
               EditorGUILayout.BeginHorizontal(); //水平 放图片和那啥按钮
               {
                  EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(false));
                  {
                     MergeTexture =  (Texture2D)EditorGUILayout.ObjectField(MergeTexture,typeof(Texture2D),GUILayout.Width(100));
                     if (OldMergeTexture != MergeTexture && MergeTexture!=null)
                     {
                        string path = AssetDatabase.GetAssetPath(MergeTexture);
                        TextureImporter textureImporter = (TextureImporter)TextureImporter.GetAtPath(path);
                        textureImporter.isReadable = true;
                        textureImporter.alphaIsTransparency = true;
                        AssetDatabase.ImportAsset(path);
                     }
                     GUILayout.Label(MergeTexture, GUILayout.Height(100),GUILayout.Width(100));
                  }
                  EditorGUILayout.EndVertical();
                  
                  EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(false)); //垂直
                  {
                     if (MergeTexture == null)
                     {
                        EditorGUILayout.LabelField("还没有放置贴图哦");
                     }
                     else
                     {
                        EditorGUILayout.LabelField("贴图大小："+MergeTexture.width + "X"+ MergeTexture.height);
                     }
                     if (GUILayout.Button(MergeTexturePath))
                     {
                        MergeTexturePath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(Selection.activeObject));
                     }
                     MergeTextureName = EditorGUILayout.TextArea(MergeTextureName);
                  }
                  EditorGUILayout.EndVertical();
               }
               EditorGUILayout.EndHorizontal();
               
               
               EditorGUILayout.BeginHorizontal(); //水平 放四个图
               {
                  EditorGUILayout.BeginVertical(GUILayout.Width(100)); //垂直
                  {

                     if (MergeTexture != null)
                     {
                        if(OldMergeTexture != MergeTexture)SplitTextureFF(MergeTexture, 0, ref SplitTextureR);
                        GUILayout.Label(SplitTextureR, GUILayout.Height(100), GUILayout.Width(100));
                        if (GUILayout.Button("保存R通道"))
                        {
                           SaveTexture(MergeTextureName + "_R", MergeTexturePath,false, SplitTextureR, ref m_MergeTexturePath);
                        }
                     }
                  }
                  EditorGUILayout.EndVertical();
                  
                  EditorGUILayout.BeginVertical(GUILayout.Width(100)); //垂直
                  {
                     if (MergeTexture != null  )
                     {
                        if(OldMergeTexture != MergeTexture)SplitTextureFF(MergeTexture, 1, ref SplitTextureG);
                        GUILayout.Label(SplitTextureG, GUILayout.Height(100),GUILayout.Width(100));
                        if (GUILayout.Button("保存G通道"))
                        {
                           SaveTexture(MergeTextureName + "_G", MergeTexturePath,false, SplitTextureG, ref m_MergeTexturePath);
                        }
                     }
                  }
                  EditorGUILayout.EndVertical();
                  
                  EditorGUILayout.BeginVertical(GUILayout.Width(100)); //垂直
                  {
                     if (MergeTexture != null  )
                     {
                        if(OldMergeTexture != MergeTexture)SplitTextureFF(MergeTexture, 2, ref SplitTextureB);
                        GUILayout.Label(SplitTextureB, GUILayout.Height(100),GUILayout.Width(100));
                        if (GUILayout.Button("保存B通道"))
                        {
                           SaveTexture(MergeTextureName + "_B", MergeTexturePath,false, SplitTextureB, ref m_MergeTexturePath);
                        }
                     }
                  }
                  EditorGUILayout.EndVertical();
                  
                  EditorGUILayout.BeginVertical(GUILayout.Width(100)); //垂直
                  {
                     if (MergeTexture != null  )
                     {
                        if(OldMergeTexture != MergeTexture)SplitTextureFF(MergeTexture, 3, ref SplitTextureA);
                        GUILayout.Label(SplitTextureA, GUILayout.Height(100),GUILayout.Width(100));
                        if (GUILayout.Button("保存A通道"))
                        {
                           SaveTexture(MergeTextureName + "_A", MergeTexturePath,false, SplitTextureA, ref m_MergeTexturePath);
                        }
                     }
                  }
                  EditorGUILayout.EndVertical();
                  OldMergeTexture = MergeTexture;
               }
               EditorGUILayout.EndHorizontal();
               
            }
            EditorGUILayout.EndVertical();
   
            
         }
      }
      EditorGUILayout.EndFoldoutHeaderGroup();
      #endregion
   }

   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   
   //-----------------------------------------------------------------------------------------------------------------
   Texture2D TextureScalerGPU(Vector2Int size,Texture2D oldTexture)//-----------------------------------------设置贴图大小
   {
      RenderTexture rt = RenderTexture.GetTemporary(size.x, size.y, 0,RenderTextureFormat.ARGB32);
      Graphics.Blit(oldTexture, rt);
      Texture2D result = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
      RenderTexture.active = rt;
      result.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
      result.Apply();
      RenderTexture.ReleaseTemporary(rt);
      return result;
   }

   void SaveTexture(string name,string path,bool isName,Texture2D texture,ref string newPath) //-----------------保存贴图
   {
     
      byte[] pngData = texture.EncodeToPNG();
      string fullPath = "";
      if (pngData != null)
      {
         
         if (isName)
         {
            fullPath = Path.Combine(path,name +"_BaseMap"+".png");
         }
         else
         { 
            fullPath = Path.Combine(path,name+".png");
         }
         
         File.WriteAllBytes(fullPath, pngData);
         AssetDatabase.Refresh();
         Object obj = AssetDatabase.LoadAssetAtPath(fullPath, typeof(Object));
         EditorGUIUtility.PingObject(obj);
         
      }
      newPath = fullPath;
   }

   Texture2D MaskTextureHB(Texture2D[] otherTexture, Vector2Int size,bool isRounessMap) //--------------------------合并
   {

      if (otherTexture.Length < 3) return Texture2D.blackTexture;

      Texture2D[] Chear = new Texture2D[4];
      Texture2D MaskTexture = new Texture2D(size.x, size.y, TextureFormat.ARGB32, false);
      for (int i = 0; i < 4; i++)
      {
         RenderTexture RT1 = RenderTexture.GetTemporary(size.x, size.y, 
            0, RenderTextureFormat.ARGB32);
         Graphics.Blit(otherTexture[i], RT1);
         Chear[i] = new Texture2D(RT1.width, RT1.height, TextureFormat.ARGB32, false);
         RenderTexture.active = RT1;
         Chear[i].ReadPixels(new Rect(0, 0, RT1.width, RT1.height), 0, 0);
         Chear[i].Apply();
         
         RenderTexture.ReleaseTemporary(RT1);
      }

      Color[] m1 = Chear[0].GetPixels();
      Color[] m2 = Chear[1].GetPixels();
      Color[] m3 = Chear[2].GetPixels();
      Color[] m4 = Chear[3].GetPixels();
      Color[] m5 = new Color[size.x * size.y];
      for (int i = 0; i < m5.Length; i++)
      {
         if (isRounessMap)
         {
            m5[i] = new Color(m1[i].r, m2[i].r, m3[i].r, 1 - m4[i].r);
         }
         else
         {
            m5[i] = new Color(m1[i].r, m2[i].r, m3[i].r, m4[i].r);
         }
         
      }
      MaskTexture.SetPixels(m5);
      MaskTexture.Apply();
      return MaskTexture;

   }

   void SplitTextureFF(Texture2D mainTexture, int TD,ref Texture2D NewTexture) //------------------------------------拆分通道
   {
      Texture2D a1  = new Texture2D(mainTexture.width, mainTexture.height, TextureFormat.ARGB32, false);
      // 获取所有像素
      try
      {
         Color[] pixels = mainTexture.GetPixels();
         Color[] NewPixels = new Color[pixels.Length];
         // 提取R通道
         for (int i = 0; i < pixels.Length; i++)
         {
            float p;
            switch (TD)
            {
               case 0: p = pixels[i].r;break;
               case 1: p = pixels[i].g;break;
               case 2: p = pixels[i].b;break;
               case 3: p = pixels[i].a;break;
               default:  p = pixels[i].r;break;
            }
            NewPixels[i] = new Color(p, p, p, 1);
         }

         a1.SetPixels(NewPixels);
         a1.Apply();
      }
      catch
      {
         a1 = mainTexture;
      }

      NewTexture = a1;

   }

   void SettingInput(string texturePath,bool sRgb)//------------------------------------------------------------导入设置
   {
      TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
      textureImporter.sRGBTexture = sRgb;
   }
   
   Texture2D GetMainTexture(int model)//-----------------------------------------------------------------------初始化贴图
   {
      Texture2D blackTexture = new Texture2D(1,1);
      Color color;
      switch (model)
      {
         case 0:color = Color.black;break;
         case 1:color = Color.white;break;
         case 2:color = Color.gray;break;
         case 3:color = Color.blue;break;
         default: color = Color.black;break;
      }
      blackTexture.SetPixel(1,1,color);
      blackTexture.Apply();
      return blackTexture;
   }

}
