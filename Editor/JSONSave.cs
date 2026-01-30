using System;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public class JSONSave : EditorWindow
{
    [MenuItem("TA工具箱/Json")]
    static void OpenWindow()
    {
        GetWindow(typeof(JSONSave),false,"ArtTools").Show();
    }
    
    Vector2 _scrollPos = Vector2.zero;
   
    
    
    //值类型
    public struct NoteData
    {
        public string Title;
        public string Content;
    }
    
    Dictionary<NoteData,string> notes = new Dictionary<NoteData,string>();
    void OnEnable()
    {
        
    }

    void OnDisable()
    {
        
    }
    void OnGUI()
    {
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        {
            //foreach (var item in myStructs)
            //{
            //    EditorGUILayout.LabelField(item.Title);
            //    EditorGUILayout.LabelField(item.Content);
            //}
           
        }
        EditorGUILayout.EndScrollView();


        //临时变量
        //ttt.Title = EditorGUILayout.TextField(ttt.Title);
        //ttt.Content = EditorGUILayout.TextField(ttt.Content);
        //if (GUILayout.Button("添加"))
        //{
        //}
    }

    
}
