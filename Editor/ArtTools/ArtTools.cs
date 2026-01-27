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
    
        private string[] _categoryNames =
        {
            "Create Texture", "Split and Merge","测试1","测试1","测试1","测试1","测试1","测试1","测试1","测试1","测试1","测试1","测试1"
            ,"测试1","测试1","测试1","测试1","测试1","测试1","测试1","测试1","测试1","测试1","测试1","测试1","测试1","测试1","测试1","测试1"
        };
        private int _selectedCategory = 0;//选择部分
        private Vector2 _scrollPos = Vector2.zero;
        
        private Action action;
    
        //其他工具指引
        private CreateTextureTool _createTextureTool;
        private SplitMergeTexture _splitMergeTexture;
   
        private void OnEnable()
        {
            _createTextureTool = new CreateTextureTool();
            _splitMergeTexture = new SplitMergeTexture();
            //存储位置
            _selectedCategory = EditorPrefs.GetInt("SelectedCategory",0);
        }
        void OnDisable()
        {
            CoreUtils.Destroy(_createTextureTool);
            CoreUtils.Destroy(_splitMergeTexture);
            //存储位置
            EditorPrefs.SetInt("SelectedCategory",_selectedCategory);
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
                        _selectedCategory = GUILayout.SelectionGrid(_selectedCategory,_categoryNames,1);
                    }
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();
            
                EditorGUILayout.BeginVertical(EditorStyles.helpBox,GUILayout.ExpandHeight(true));//垂直布局
                {
                    switch (_selectedCategory)
                    {
                        case 0: _createTextureTool.DrawGUI(); break;
                        case 1: _splitMergeTexture.DrawGUI(); break;
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Label("Updata:226.01.26 by:LiMi",EditorStyles.centeredGreyMiniLabel);
        }
    }
}
