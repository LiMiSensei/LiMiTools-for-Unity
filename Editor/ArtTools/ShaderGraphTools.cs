using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LiMiTools.Editor.ArtTools
{
    public class ShaderGraphTools : EditorWindow
    {
        #region 制作用于列表的数据组
        private enum MaterialType //材质类型
        {
            Decal,
            Unlit,
            Fullscreen,
            SpriteCustomLit,
            Lit,
            SixWaySmokeLit,
            SpriteUnlit,
            PhysicalHair,
            SpriteLit,
            Canvas,
        }
        private enum Precision  //四种精度
        {
            Inherit,
            Single,
            Half,
            UseGraphPrecision
        }
        
        #endregion
        private string[] _categoryNames =
        {
            "ShaderGraph","SubShader"
        };
        private int _selectedCategory = 0;//选择部分
        private Vector2 _scrollPos = Vector2.zero;
        private Vector2 _miniWindows = Vector2.zero;
        
        
        private string searchKeyword = "";      //搜索栏 
        
    
        private class GraphData
        {
            public bool isSelected;              //是否选中
            private string _name;                //名称
            public string Name
            { get { return _name; }
                set
                {
                    char[] invalidChars = { '\\', '/', ':', '*', '?', '\"', '<', '>', '|' };
                    if (value.IndexOfAny(invalidChars) < 0) _name = value; 
                }
            }
            public string path;                 //路径

            public string shaderFile;           //存放文本
            
            public Precision precision;         //精度
            public MaterialType type;           //材质类型
            public int usageQuantity;          //使用数量
            public List<Material> materials;
                            
            public string m_Path;        //搜索路径
        }

        //增、删、查、改、遍历
        private Dictionary<string, GraphData> _gamesObjFiles;  //GUID
        private Dictionary<string, GraphData> _oldObjFiles;   //GUID
        
            
            
        ArtToolCommon data;
        private void OnEnable()
        {
            _gamesObjFiles = new Dictionary<string, GraphData>();
            _oldObjFiles = new Dictionary<string, GraphData>();
            data = ArtToolCommon.Instance;
        }

        
        
        
        
        
        
        
        
        
        
        
        
        //------------------------------------------- GUI -----------------------------------------------------//
        public void DrawGUI()
        {
            _selectedCategory = GUILayout.SelectionGrid(_selectedCategory,_categoryNames,2,EditorStyles.toolbarButton,GUILayout.MinWidth(10));
            
            searchKeyword = EditorGUILayout.TextField("搜索关键词", searchKeyword,GUILayout.MinWidth(10));
            if(GUILayout.Button($"Selection Path: {data.Path}",GUILayout.MinWidth(10))) data.Path = ArtToolCore.SelectionPath();
            
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("扫描",GUILayout.MinWidth(10))) AddList();
                if (GUILayout.Button("清空",GUILayout.MinWidth(10))) ClearList();
            }
            EditorGUILayout.EndHorizontal();

            
            
            switch (_selectedCategory)
            {
                case 0: SG_GUI(); break;
                case 1: SB_GUI(); break;
            }
            
            
            EditorGUILayout.LabelField("状态:", EditorStyles.boldLabel,GUILayout.MinWidth(10));
            EditorGUILayout.TextArea(data.Message, GUILayout.Height(60),GUILayout.MinWidth(10));
            if (GUILayout.Button("应用修改",GUILayout.MinWidth(10))) ApplyChanges();

        }
        
        void SG_GUI()
        {
            //标题栏目
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Name:",GUILayout.Width(100),GUILayout.MaxWidth(100),GUILayout.MinWidth(10));
                EditorGUILayout.LabelField("Path:",GUILayout.Width(200),GUILayout.MaxWidth(200),GUILayout.MinWidth(10));
                EditorGUILayout.LabelField("m_Path:",GUILayout.Width(200),GUILayout.MaxWidth(200),GUILayout.MinWidth(10));

            }
            EditorGUILayout.EndHorizontal();
            
            //内容表
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos,EditorStyles.helpBox);
            {
                foreach (var key in _gamesObjFiles.Keys)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            if (_gamesObjFiles[key].Name.Contains(searchKeyword) || searchKeyword.Length == 0)
                            {
                                _gamesObjFiles[key].Name = EditorGUILayout.TextArea(_gamesObjFiles[key].Name, GUILayout.MaxWidth(100));
                                EditorGUILayout.SelectableLabel(_gamesObjFiles[key].path, GUILayout.MaxWidth(200));
                                EditorGUILayout.SelectableLabel(_gamesObjFiles[key].m_Path,GUILayout.MaxWidth(200));
                                EditorGUILayout.LabelField($"使用数量：{_gamesObjFiles[key].usageQuantity}");
                                GUILayout.Button("查看材质表");
                                if(GUILayout.Button("Open"))OpenShaderGraph(key);
                                if(GUILayout.Button("Selection"));
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        int columns = 5;
                        int num = 0;
                        for (var i = 0; i < (_gamesObjFiles[key].materials.Count / columns) +1  ;i++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            for (var j = 0; j < columns; j++)
                            {
                                if(num == _gamesObjFiles[key].materials.Count) break;
                                EditorGUILayout.ObjectField(_gamesObjFiles[key].materials[num], typeof(Material), false, GUILayout.MaxWidth(100));
                                num++;
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        
                    }
                    EditorGUILayout.EndVertical();
                }
                
            }
            EditorGUILayout.EndScrollView();
        }
        void SB_GUI()
        {
            //标题栏目
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Name:",GUILayout.Width(100),GUILayout.MaxWidth(100),GUILayout.MinWidth(10));
                EditorGUILayout.LabelField("Path:",GUILayout.Width(200),GUILayout.MaxWidth(200),GUILayout.MinWidth(10));
                EditorGUILayout.LabelField("m_Path:",GUILayout.Width(200),GUILayout.MaxWidth(200),GUILayout.MinWidth(10));

            }
            EditorGUILayout.EndHorizontal();
            
            //内容表
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos,EditorStyles.helpBox);
            {
                
                foreach (var key in _gamesObjFiles.Keys)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        if (_gamesObjFiles[key].Name.Contains(searchKeyword) || searchKeyword.Length == 0)
                        {
                            _gamesObjFiles[key].Name   = EditorGUILayout.TextArea(_gamesObjFiles[key].Name,  GUILayout.MaxWidth(100),GUILayout.MinWidth(10));
                            EditorGUILayout.SelectableLabel                      (_gamesObjFiles[key].path,  GUILayout.MaxWidth(200),GUILayout.MinWidth(10));
                            _gamesObjFiles[key].m_Path = EditorGUILayout.TextArea(_gamesObjFiles[key].m_Path,GUILayout.MaxWidth(200),GUILayout.MinWidth(10));
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
               
            }
            EditorGUILayout.EndScrollView();
        }


       
        //-------------------------------------------------------------------------------------------------------------//
        void AddList()
        {
            ClearList();
            //获取要搜索的目录
            var relativeFolderPath = Path.GetFullPath(data.Path); 
            //得到的目录
            var systemPath = Directory.GetFiles(relativeFolderPath,_selectedCategory==0? "*.shadergraph":"*.shadersubgraph", SearchOption.AllDirectories);
            
            
            foreach (var item in systemPath)
            {
                var file = File.ReadAllText(item, Encoding.UTF8);
                var unityPath = ArtToolCore.SystemToUnityPath(item);
                var gUID = AssetDatabase.AssetPathToGUID(unityPath);
                var name = Path.GetFileNameWithoutExtension(unityPath);
                var material = FindShaderAllMaterial(unityPath);
                var m_Path = ReadJsonValue(file, "m_Path");
                _gamesObjFiles.Add(gUID,new GraphData()
                {
                    Name = name,
                    path = unityPath,
                    shaderFile = file,
                    materials = material,
                    usageQuantity = material.Count,
                    m_Path = m_Path,
                });
                _oldObjFiles.Add(gUID,new GraphData()
                {
                    Name = name,
                    path = unityPath,
                    shaderFile = file,
                    materials = material,
                    usageQuantity = material.Count,
                    m_Path = m_Path,
                });
            }
           
        }

        void ClearList()
        {
            _oldObjFiles?.Clear();
            _gamesObjFiles?.Clear();
        }

        void ApplyChanges()
        {
            foreach (var key in _gamesObjFiles.Keys)
            {
                //修改名字
                if (_gamesObjFiles[key].Name != _oldObjFiles[key].Name)
                {
                    GUIDToReName(key,_gamesObjFiles[key].Name);
                }
                //修改m_Path
                if (_gamesObjFiles[key].path != _oldObjFiles[key].path)
                {
                    
                }
                    
                
            }
        }

        //用GUID设置名字
        void GUIDToReName(string guid,string newName)
        {
            //1 先读文件路径
            var path = AssetDatabase.GUIDToAssetPath(guid);                            //获取 文件路径
            //2 拆分
            var directoryName = Path.GetDirectoryName(path);                           //文件 上面的路径
            var fileNameWE = Path.GetFileNameWithoutExtension(path);             //获取 文件的名字
            var fileName = Path.GetFileName(path);                                     //获取 文件的名字带扩展的
            //3 替换
            var newfileName = fileName.Replace(fileNameWE, newName);                   //将带扩展的的名字 改名字
            //4 组合新路径
            var newpath = Path.Combine(directoryName, newfileName).Replace("\\","/");  //组合新路径 Unity路径
            //5 如果这个路径有新名字就加1直到没有了
            string newpath_1 = newpath;
            string newpath_2 = AssetDatabase.GenerateUniqueAssetPath(newpath_1);
            var newNewName = Path.GetFileNameWithoutExtension(newpath_2); 
            //6 设置名字
            AssetDatabase.RenameAsset(path, newNewName);
        }
        
        //打开文件的方法
        void OpenShaderGraph(string guid)                                    
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrEmpty(path))
            {
                Object shaderAsset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (shaderAsset != null)
                {
                    Selection.activeObject = shaderAsset;
                    EditorGUIUtility.PingObject(shaderAsset);
                    AssetDatabase.OpenAsset(shaderAsset);
                }
            }
        }

        List<Material> FindShaderAllMaterial(string path)
        {  
            List<Material> usedMaterials = new List<Material>();
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
            var materialGuids = AssetDatabase.FindAssets("t:Material",new [] {"Assets"});
            for (int i = 0; i < materialGuids.Length; i++)
            {
                Material material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(materialGuids[i]));
                if (material != null && material.shader != null)
                {
                    if (shader == material.shader)
                    {
                        usedMaterials.Add(material);
                    }
                }
            }
            return usedMaterials;
        }
        
        
        private string ReadJsonValue(string json,string content)                            //解析JSON的某一行的值
        {
            //value = m_
            int mPathIndex = json.IndexOf($"\"{content}\":", StringComparison.Ordinal);
            if (mPathIndex == -1) return null;
            int valueStart = json.IndexOf('"', mPathIndex + 8) + 1; // 跳过 "m_Path":"
            int valueEnd = json.IndexOf('"', valueStart);
            if (valueStart > 0 && valueEnd > valueStart)
            {
                return json.Substring(valueStart, valueEnd - valueStart);
            }
            return null;
        }
        
        private string ReplaceJsonValue(string json,string content, string value)          //修改JSON方法
        {
            //value
            int mPathIndex = json.IndexOf($"\"{content}\":", StringComparison.Ordinal);
            if (mPathIndex == -1) return "未找到";
            
            int valueStart = json.IndexOf('"', mPathIndex + 8) + 1;
            int valueEnd = json.IndexOf('"', valueStart);
            if (valueStart <= 0 || valueEnd <= valueStart) return "m_Path 值格式错误";
            
            // 构建新的JSON
            StringBuilder sb = new StringBuilder();
            sb.Append(json.Substring(0, valueStart));
            sb.Append(value);
            sb.Append(json.Substring(valueEnd));
            return sb.ToString();
        }

      
    }
}
