using System;
using UnityEditor;
using UnityEngine;

public class ArtTools : EditorWindow
{
    [MenuItem("LiMi/测试/ArtTools")]
    public static void ShowWindow()
    {
        var window = GetWindow(typeof(ArtTools),false,"ArtTools");
        window.Show();
    }

    

    #region 数据成员

    string[] categoryNames = {"角色", "场景", "UI","特效","TA","其他","选中对象"};
    int selectedCategory = 0;
    private ArtToolsTA artToolsTA;
    private ArtToolsActice artToolsactive;
    private void OnEnable()
    {
        artToolsTA = new ArtToolsTA();
        artToolsactive = new ArtToolsActice();
        selectedCategory = EditorPrefs.GetInt("SelectedCategory",0);
        //selectedCategory = EditorPrefs.HasKey("SelectedCategory")?selectedCategory = EditorPrefs.GetInt("SelectedCategory"):0;;
    }
    #endregion
    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox,GUILayout.Width(position.width*0.15f),GUILayout.ExpandHeight(true));
            {
                selectedCategory = GUILayout.SelectionGrid(selectedCategory,categoryNames,1);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox,GUILayout.ExpandHeight(true));
            {
                switch (selectedCategory)
                {
                    case 0: GUILayout.TextArea("角色");break;
                    case 1: artToolsactive.Deaw();    break;
                    case 2: break;
                    case 3: break;  
                    case 4: artToolsTA.Deaw();         break;
                    case 5: break;
                    case 6: break;
                }
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.Label("Updata:225.07.18 by:LiMi",EditorStyles.centeredGreyMiniLabel);
    }

    void OnDisable()
    {
        EditorPrefs.SetInt("SelectedCategory",selectedCategory);
    }
}
