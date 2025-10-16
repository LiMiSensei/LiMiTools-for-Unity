using UnityEditor;
using UnityEngine;

public class ArtToolsActice 
{
    public void Deaw()
    {
        if (GUILayout.Button("<UNK>"))
        {
            GameObject go = Selection.activeGameObject;
            Undo.RecordObject(go, "Deaw");//记录数据，并允许撤回
            go.SetActive(!go.activeSelf);
            go.isStatic = !go.isStatic;
            go.layer = LayerMask.NameToLayer("Default");
            go.tag = "Default";
            StaticEditorFlags flags = StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic;//设置静态图层
            GameObjectUtility.SetStaticEditorFlags(go, flags);       //设置静态图层
            var a = System.Convert.ToString(1, 2);//进制转换
        }
       
        
    }
}
