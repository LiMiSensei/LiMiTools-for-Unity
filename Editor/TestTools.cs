using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class TestTools : EditorWindow
{
    [MenuItem("CShaderp/核心")]
    static void OpenWindow()
    {
        var window = EditorWindow.GetWindow<TestTools>();
        window.Show();
    }

    bool isRunning = true;

    static object obj = new object();
    private void OnEnable()
    {
       
    }
    private void OnGUI()
    {
        if(GUILayout.Button("开启线程"))
        {
           
            Thread t = new Thread(ThreadFunction);
            t.IsBackground = true;
            t.Start();
        }

        if (GUILayout.Button("终止线程"));
        {
            isRunning = !isRunning;
        }
        
    }

    void ThreadFunction()
    {
        lock (obj)//检查这个obj有没有被其他地方锁住
        {
            
        }
        
       
        
    }

    
}
