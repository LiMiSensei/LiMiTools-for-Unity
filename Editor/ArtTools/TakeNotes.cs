using System;
using System.Collections.Generic;
using System.IO;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace LiMiTools.Editor.ArtTools
{
    public class TakeNotes : EditorWindow
    {
        /// <summary>
        /// 这个工具所存的数据，不是文件的数据，用于存放UI相关或是临时变量
        /// </summary>
        class ToolsData
        {
            //基本数据
            public string SearchBar = "";
            public bool isModify = false;//状态切换
            public string FilePath = "Assets/LiMiTools/Editor/ArtTools/Note";

            public int ModifyID = 0;
            //左栏
            public Vector2 LeftScroll = Vector2.zero;
            public int _leftSelectID = 0;

            public int LeftSelectID
            {
                get{return _leftSelectID;}
                set
                {
                    if (_leftSelectID != value)
                    {
                        _leftSelectID = value;
                        isModify = false;
                    }
                    
                }
            }
            public string[] LeftCategory;
            public string[] _GUIDs = new string[0];

            public string[] GUIDs
            {
                get{return _GUIDs;}
                set
                {
                    _GUIDs = value;
                    LeftCategory = new string[value.Length];
                    for (int i = 0; i < _GUIDs.Length; i++)
                    {
                        LeftCategory[i] = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(_GUIDs[i]));
                    }
                }
            }
            
            public string FileName = "我的笔记";
            //右栏
            public Vector2 RightScroll = Vector2.zero;
            public int RightSelectID = 0;
            public string[]RightCategory = new string[0];
            
            
            public string Title = "";   //临时标题
            public string Content = ""; //临时内容
        }
        
        
        /// <summary>
        /// 真正处理数据相关
        /// </summary>
        class Page
        {
            public int ID;
            public string Name;
            public List<Note> Notes = new List<Note>();
            
            public class Note
            {
                
                
                private string _title;
                public string Title//标题
                {
                    get{return _title;}
                    set{_title = value;}
                }
                private string _content;

                public string Content//内容
                {
                    get{return _content;}
                    set{_content = value;}
                }
                public string DateTime;//创建时间
            }
            
        }
        
        private static GUIStyle styleContent;
        private static GUIStyle StyleContent
        {
            get
            {
                if (styleContent == null)
                {
                    styleContent = new GUIStyle("label");
                    styleContent.wordWrap = true;
                    styleContent.richText = true;
                    styleContent.fontSize = 16;
                }
                return styleContent;
            }
        }
        //主按钮样式
        private static GUIStyle styleTitle;
        private static GUIStyle StyleTitle
        {
            get
            {
                if (styleTitle == null)
                {
                    styleTitle = new GUIStyle("label");
                    styleTitle.alignment = TextAnchor.MiddleLeft;
                    styleTitle.wordWrap = false;
                    styleTitle.fontStyle = FontStyle.Bold;
                    styleTitle.fontSize = 18;
                }
                return styleTitle;
            }
        }
        
        List<Page> pages = new List<Page>(); //序号就是
        ToolsData TData = new ToolsData();
        
        
        //制作打包成Asset的工具
        
        
        void OnEnable()
        {
            pages = ReadFile();
        }

        void OnDisable()
        {
            
        }





        void ResetTools()
        {
            TData.isModify = false;
            
            
            TData.Title = "";
            TData.Content = "";
        }

        
        
        
        
//======================================================================================================================//

        #region File函数
        List<Page> ReadFile()
        {
            pages.Clear();
            //加载路径下的所有txt文件 //GUID
            TData.GUIDs = AssetDatabase.FindAssets("t:TextAsset",new string[] { TData.FilePath });
            //遍历所有文件进行反序列化
            foreach (var guid in TData.GUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                //加载为textAsset文件
                var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                //读取textAsset文件转为Page
               
                //反序列化
                var page = JsonConvert.DeserializeObject<Page>(textAsset.text);
                pages.Add(page);
            }
            return pages;
        }
        void CreateFile()//新建分类
        {
            char[] invalidChars = { '\\', '/', ':', '*', '?', '\"', '<', '>', '|' };
            if (TData.FileName == "" || TData.FileName.IndexOfAny(invalidChars) >= 0)
            {
                Debug.Log("名字不合法，请修改名称后重试");
                return;
            }
            //新建Page
            var newFile = new Page()
            {
                Name = TData.FileName,
            };
            //pages.Add(newFile);
            //将Page序列化为文本
            var json = JsonConvert.SerializeObject(newFile,Formatting.Indented);
            //将新建的TextAsset内容文本设置为序列化的内容
            var textAsset = new TextAsset(json);
            //如果没有这个文件夹就新建
            if (!Directory.Exists(TData.FilePath))
            {
                Directory.CreateDirectory(TData.FilePath);
            }
            //写入这个文件   
            var fileName = AssetDatabase.GenerateUniqueAssetPath($"{TData.FilePath}/{newFile.Name}.asset");
            //创建文件
            AssetDatabase.CreateAsset(textAsset,fileName);

            ReadFile();
        }
        void DeleteFile()
        {
            if(TData.GUIDs.Length <= 0)return;
            var path = AssetDatabase.GUIDToAssetPath(TData.GUIDs[TData.LeftSelectID]);
            ResetTools();
            TData.LeftSelectID = Mathf.Max(TData.LeftSelectID - 1,0);
            AssetDatabase.DeleteAsset(path);
            
            ReadFile();
        }
        
        #endregion
//=====================================================================================================================//

        #region Note函数
        void AddNote()
        {
            if(TData.GUIDs.Length <= 0)return;
           
            var thisTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Page.Note note = new Page.Note()
            {
                Title = TData.Title,
                Content = TData.Content,
                DateTime = thisTime,
            };
            pages[TData.LeftSelectID].Notes.Add(note);
            ResetTools();
            SaveNote();
        }

        void ModifyNote(int i)
        {
            
            if (TData.isModify) //点击修改按钮时状态 同时将ID传入
            {
                TData.ModifyID = i;
                TData.Title = pages [TData.LeftSelectID].Notes[i].Title;
                TData.Content = pages [TData.LeftSelectID].Notes[i].Content;
            }
            else              //确认修改时用上修改ID来修改
            {
                pages[TData.LeftSelectID].Notes[TData.ModifyID].Title = TData.Title;
                pages [TData.LeftSelectID].Notes[TData.ModifyID].Content = TData.Content;
                SaveNote();
            }
        }

        void SaveNote()
        {
            if(TData.GUIDs.Length <= 0)return;
            //获取要写入的文件的路径
            var path = AssetDatabase.GUIDToAssetPath(TData.GUIDs[TData.LeftSelectID]);
            //得到Page数据
            var pageData = pages[TData.LeftSelectID];
            //转成Json并写入文件
            var json = JsonConvert.SerializeObject(pageData,Formatting.Indented);
            var textAsset = new TextAsset(json);
            AssetDatabase.CreateAsset(textAsset,path);
        }

        void DeleteNote()
        {
            
        }
        #endregion
//=====================================================================================================================//
        void FileGUI()
        {
            //下面的按钮
            EditorGUILayout.BeginHorizontal();
            {
                TData.SearchBar = EditorGUILayout.DelayedTextField(TData.SearchBar,EditorStyles.toolbarSearchField);
                if (TData.SearchBar.Length > 0)
                {
                    
                }
                
            }
            EditorGUILayout.EndHorizontal();
            //选择面板
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            {
                TData.LeftScroll = EditorGUILayout.BeginScrollView(TData.LeftScroll);
                {
                    if(TData.GUIDs.Length != 0) TData.LeftSelectID = GUILayout.SelectionGrid(TData.LeftSelectID, TData.LeftCategory,1,EditorStyles.toolbarButton);
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
            //新建文本名字
            TData.FileName = EditorGUILayout.TextField(TData.FileName);
            //下面的按钮
            EditorGUILayout.BeginHorizontal();
            {
                if(GUILayout.Button("新建文件"))CreateFile();
                if (GUILayout.Button("删除文件"))
                {
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(TData.GUIDs[TData.LeftSelectID]));
                    
                    if (EditorUtility.DisplayDialog("删除文档",
                            $"你确认要删除\"{TData.LeftCategory[TData.LeftSelectID]}\"吗？此操作不可撤回", "确认删除", "取消"))
                    {
                       
                        DeleteFile();
                    }
                }
               
            }
            EditorGUILayout.EndHorizontal();


            //--分类：删除  增加  排序x
        }
        void NoteGUI()
        {
            if (TData.GUIDs.Length > 0)
            {
                TData.RightScroll = EditorGUILayout.BeginScrollView(TData.RightScroll);
                {
                    for (int i = 0; i < pages[TData.LeftSelectID].Notes.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                        {
                            //主要内容显示
                            EditorGUILayout.BeginVertical();
                            {
                                EditorGUILayout.SelectableLabel(pages[TData.LeftSelectID].Notes[i].Title,StyleTitle);
                                EditorGUILayout.TextArea(pages[TData.LeftSelectID].Notes[i].Content,StyleContent);
                                EditorGUILayout.SelectableLabel(pages[TData.LeftSelectID].Notes[i].DateTime);
                            }
                            EditorGUILayout.EndVertical();
                            
                            //右边的按钮
                            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(40));
                            {
                                if (!TData.isModify)
                                {
                                    if (GUILayout.Button("X"))
                                    {
                                        if(EditorUtility.DisplayDialog("删除笔记",
                                            $"你确认删除:\n \"{pages [TData.LeftSelectID].Notes[i].Title}\" \n此操作不可撤回", "确认删除", "取消"))
                                        {
                                            pages [TData.LeftSelectID].Notes.RemoveAt(i);
                                            SaveNote();
                                        };
                                        
                                    }
                                    if (GUILayout.Button("↑↓"))
                                    {
                                        
                                        TData.isModify = true;
                                        ModifyNote(i);
                                        
                                    };
                                    if (GUILayout.Button("↑↑"))
                                    {
                                        if (i > 0)
                                        {
                                            //析构换位；
                                            (pages[TData.LeftSelectID].Notes[i-1], pages[TData.LeftSelectID].Notes[i]) = (pages[TData.LeftSelectID].Notes[i], pages[TData.LeftSelectID].Notes[i-1]);
                                            SaveNote();
                                        }
                                    };
                                    if(GUILayout.Button("↓↓"))
                                    {
                                        if (i < pages[TData.LeftSelectID].Notes.Count-1)
                                        {
                                            //析构换位；
                                            (pages[TData.LeftSelectID].Notes[i+1], pages[TData.LeftSelectID].Notes[i]) = (pages[TData.LeftSelectID].Notes[i], pages[TData.LeftSelectID].Notes[i+1]);
                                            SaveNote();
                                        }
                                    };
                                }
                            }
                            EditorGUILayout.EndVertical();
                            
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space(10);
                    }
                }
                EditorGUILayout.EndScrollView();
                
                EditorGUILayout.Space(5);
                
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("标\n题",GUILayout.Height(40),GUILayout.Width(15));
                        TData.Title = EditorGUILayout.TextArea(TData.Title,GUILayout.Height(40));
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(1);
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("内\n容",GUILayout.Height(40),GUILayout.Width(15));
                        TData.Content = EditorGUILayout.TextArea(TData.Content,GUILayout.Height(100));
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
                
                if (TData.isModify)
                {
                    if (GUILayout.Button("确认修改"))
                    {
                        TData.isModify = false;
                        ModifyNote(0);
                    }
                }
                else
                {
                    if (GUILayout.Button("创建"))AddNote() ;
                }
                


            }
            
        }
        public void DrawGUI()
        {
            EditorGUILayout.BeginHorizontal();
            {
                //左栏
                EditorGUILayout.BeginVertical(EditorStyles.helpBox,GUILayout.ExpandHeight(true),GUILayout.Width(position.width * 0.5f));
                {
                    FileGUI();
                }
                EditorGUILayout.EndVertical();
                
                //右栏
                EditorGUILayout.BeginVertical(EditorStyles.helpBox,GUILayout.ExpandHeight(true));
                {
                     NoteGUI();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
