
using UnityEditor;
using UnityEngine;


public class ArtToolsTA
{
    #region 数据成员

    private bool isOpen = false;
    string[] texture2DPath = new string[]{"Assets/"};
    #endregion
    public void Deaw()
    {
        isOpen = EditorGUILayout.BeginFoldoutHeaderGroup(isOpen,"目录跳转");
        if (isOpen)
        {
            if (GUILayout.Button("<UNK>"))
            {
            
            }
            if (GUILayout.Button("<UNK>"))
            {
            
            }
            if (GUILayout.Button("查找文件"))
            {
                string[] _guids = AssetDatabase.FindAssets("t:texture2D",texture2DPath);
                string _subpath = AssetDatabase.GUIDToAssetPath(_guids[0]);
                Object _o  = AssetDatabase.LoadAssetAtPath(_subpath, typeof(Object));
                Selection.activeObject = _o;
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
    }
}
