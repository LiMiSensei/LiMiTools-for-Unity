using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiMiTools.Editor.ArtTools
{
    public class ArtTools : EditorWindow
    {
        [MenuItem("TA工具箱/ArtTools")]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(ArtTools),false,"ArtTools");
            window.Show();
        }

    

        #region 数据成员

        enum ToolsName
        {
            CreateTexture,
            SplitMergeTexture,
            ShaderGraphTools,
            MaterialTools,
            
            TakeNotes,
            FlowMapBursh,
            UVView,
            DebugWindow,
            About,
        }
        ToolsName toolsName = ToolsName.CreateTexture;
        private string[] _categoryNames;
        
        
        
        private Vector2 _scrollPos = Vector2.zero;
        private Action action;
    
        //其他工具指引
        private CreateTextureTool _createTextureTool;
        private SplitMergeTexture _splitMergeTexture;
        private DebugWindow _debugWindow;
        private ShaderGraphTools _shaderGraphTools;
        private TakeNotes _takeNotes;
        private void OnEnable()
        {
            _categoryNames = Enum.GetNames(typeof(ToolsName));
            
            _createTextureTool = ScriptableObject.CreateInstance<CreateTextureTool>();
            _splitMergeTexture = ScriptableObject.CreateInstance<SplitMergeTexture>();
            _debugWindow       = ScriptableObject.CreateInstance<DebugWindow>();
            _shaderGraphTools  = ScriptableObject.CreateInstance<ShaderGraphTools>();
            _takeNotes         = ScriptableObject.CreateInstance<TakeNotes>();
            //存储位置
            toolsName = (ToolsName)EditorPrefs.GetInt("SelectedCategory",0);
        }
        void OnDisable()
        {
            CoreUtils.Destroy(_createTextureTool);
            CoreUtils.Destroy(_splitMergeTexture);
            CoreUtils.Destroy(_debugWindow);
            CoreUtils.Destroy(_shaderGraphTools);
            CoreUtils.Destroy(_takeNotes);
            //存储位置
            EditorPrefs.SetInt("SelectedCategory",(int)toolsName);
        }
        #endregion
        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();//水平布局
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox,GUILayout.Width(position.width*0.15f));
                {
                    _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
                    {
                        toolsName = (ToolsName)GUILayout.SelectionGrid((int)toolsName,_categoryNames,1);
                    }
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();
            
                EditorGUILayout.BeginVertical(EditorStyles.helpBox,GUILayout.ExpandHeight(true));//垂直布局
                {
                    switch (toolsName)
                    {
                        case ToolsName.CreateTexture     : _createTextureTool.DrawGUI();break;
                        case ToolsName.SplitMergeTexture : _splitMergeTexture.DrawGUI();break;
                        case ToolsName.ShaderGraphTools  : _shaderGraphTools .DrawGUI();break;
                        case ToolsName.MaterialTools     :                              break;
                        
                      
                        case ToolsName.DebugWindow       :       _debugWindow.DrawGUI();break;
                        case ToolsName.TakeNotes         :         _takeNotes.DrawGUI();break;
                        case ToolsName.FlowMapBursh      :                              break;
                        case ToolsName.UVView            :                              break;
                        case ToolsName.About             :                              break;
                        
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Label("Updata:226.01.26 by:LiMi",EditorStyles.centeredGreyMiniLabel);
        }
    }
}
