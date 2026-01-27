using System.IO;
using UnityEditor;
using UnityEngine;
// 存放实现方法，不存放变量或者其他数据
//

namespace LiMiTools.Editor.ArtTools
{
    public static class ArtToolCore
    {
        static char[] invalidChars = { '\\', '/', ':', '*', '?', '\"', '<', '>', '|' };

        
    
        //选择项目路径
        public static string SelectionPath()
        {
            // 1. 获取选中的资源（过滤掉非资产对象）
            Object[] selectedObjects = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
        
            // 2. 处理空选择情况
            if (selectedObjects.Length == 0)
            {
                ArtToolCommon.Instance.Message = "请点击项目任意文件&文件夹，目录将同步到此";
                return  "Assets/";
            }
            string guid = Selection.assetGUIDs[0]; // 3. 获取第一个选中对象的GUID和路径
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.IsValidFolder(assetPath))// 4. 统一路径格式并处理文件夹/文件
            {
                ArtToolCommon.Instance.Message = $"输出目录为{assetPath}";
                return assetPath; // 文件夹直接使用原路径（自动统一为正斜杠）
            }
            else
            {
                assetPath = System.IO.Path.GetDirectoryName(assetPath).Replace("\\", "/");// 文件获取父目录并确保使用正斜杠
                ArtToolCommon.Instance.Message = $"输出目录为{assetPath}";
                return assetPath;
            }
        }
    
        //保存图像
        public static void SaveTexture(Texture2D texture, string name, string path)
        {
            //安全检查 如果图像为空 || 如果名字不合法 "\/：*？\"<>|"       ||如果路径不合法
            if (texture == null)
            {
                ArtToolCommon.Instance.Message = $"保存失败，图像为null";
                return;
            };
            if (name.IndexOfAny(invalidChars) >= 0)
            {
                ArtToolCommon.Instance.Message = "保存失败，名字不合法 \"\\/：*？\\\"<>|\"";
                return;
            }
            if (!path.StartsWith("Assets"))
            {
                ArtToolCommon.Instance.Message = $"保存失败，路径不合法";
                return;
            }
            //写入纹理
            byte[] pngData = texture.EncodeToPNG();
            string fullPath = System.IO.Path.Combine(path,name+".png");
            File.WriteAllBytes(fullPath, pngData);
            //刷新资产
            AssetDatabase.Refresh();
            ArtToolCommon.Instance.Message = "保存成功";
            //ping
            Object obj = AssetDatabase.LoadAssetAtPath(fullPath, typeof(Object));
            TextureImporter importer = AssetImporter.GetAtPath(fullPath) as TextureImporter;
            importer.filterMode = ArtToolCommon.Instance.FilterMode;;
            importer.SaveAndReimport();
            EditorGUIUtility.PingObject(obj);
        }
        
        //GPU调整纹理大小
        public static Texture2D TextureSizeOnGPU(Vector2Int size, Texture2D oldTexture)
        {
            if (oldTexture == null)
                return null;
            RenderTexture rt = null;
            try
            {
                rt = RenderTexture.GetTemporary(size.x, size.y, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(oldTexture, rt);
                Texture2D result = new Texture2D(size.x, size.y, TextureFormat.ARGB32, false);
                RenderTexture previous = RenderTexture.active;
                try
                {
                    RenderTexture.active = rt;
                    result.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
                    result.Apply();
                }
                finally
                {
                    RenderTexture.active = previous;
                }
                return result;
            }
            finally
            {
                if (rt != null)
                    RenderTexture.ReleaseTemporary(rt);
            }
        }
    }
}
