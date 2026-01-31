using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace LiMiTools.Editor.ArtTools
{
    public class PackagAssets : EditorWindow
    {
        private string path = "Assets/LiMiTools/Editor/ArtTools";

        public class Tese : ScriptableObject
        {
            
        }
        
        
        
        MaterialProperty[] props;
        Material material = null;
        
        
        public void DrawGUI()
        {
            material= EditorGUILayout.ObjectField("",material, typeof(Material), false,GUILayout.MinWidth(1)) as Material;
           
            
            
            


            if (GUILayout.Button("packag")) PackagAssetsNow();
        }

        void PackagAssetsNow()
        {
            //主资源添加子资源
            Color[] c = new Color[]{new Color(0.2f,0.2f,0.2f)};
            var tex = new Texture2D(1, 1);
            tex.SetPixels(c);
            tex.Apply();
            var mat = new Material(material);
            var mat2 = new Material(material);
            AssetDatabase.CreateAsset(mat, $"{path}/N.asset");
            AssetDatabase.AddObjectToAsset(mat2,$"{path}/N.asset" );
            AssetDatabase.AddObjectToAsset(tex,$"{path}/N.asset" );
            
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);
            
            //// ========== 1. 创建并保存主资源（必须先有主资源，核心前提） ==========
            //MainConfig mainConfig = ScriptableObject.CreateInstance<MainConfig>();
            //mainConfig.configName = "技能主配置";
            //// 主资源保存路径（自定义，确保路径存在）
            //string mainAssetPath = "Assets/LiMiTools/Editor/ArtTools/Main.png";
            //// 创建主资源文件（使其成为有效Asset）
            //AssetDatabase.CreateAsset(mainConfig, mainAssetPath);
//
            //// ========== 2. 创建子资源（全新未关联的对象） ==========
            //SubConfig subConfig1 = ScriptableObject.CreateInstance<SubConfig>();
            //subConfig1.id = 1;
            //subConfig1.desc = "普攻配置";
//
            //SubConfig subConfig2 = ScriptableObject.CreateInstance<SubConfig>();
            //subConfig2.id = 2;
            //subConfig2.desc = "大招配置";
//
            //// ========== 3. 核心：将子资源嵌入主资源 ==========
            //AssetDatabase.AddObjectToAsset(subConfig1, mainConfig); // 嵌入子资源1
            //AssetDatabase.AddObjectToAsset(subConfig2, mainConfig); // 嵌入子资源2
//
            //// ========== 4. 必做：保存所有资源修改（否则失效） ==========
            //AssetDatabase.SaveAssets();
            //// 可选：刷新Project窗口，立即看到效果
            //AssetDatabase.Refresh();
//
            //Debug.Log("主资源+子资源创建成功，路径：" + mainAssetPath);
        }
        
        
         T JSONSerialization<T>(T obj)
        {
            var json = JsonConvert.SerializeObject(obj,Formatting.Indented);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
