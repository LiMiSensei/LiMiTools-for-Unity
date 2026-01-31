using UnityEditor;
using UnityEngine;

namespace LiMiTools.Editor.ArtTools
{
    public sealed class ArtToolCommon
    {
        
        //报错信息
        public string Message = "";
        //图像覆盖模式
        public bool isCovering = false;
        //图像过滤模式
        public FilterMode FilterMode = FilterMode.Bilinear;
        //sRGB
        public bool sRGB = true;
        //存储路径
        private string _path = "Assets";
        public string Path
        {
            get
            {
                _path = EditorPrefs.GetString("ArtToolCommon_path","Assets");
                return _path;
            }
            set
            {
                _path = value;
                EditorPrefs.SetString("ArtToolCommon_path",_path);//存储位置
            }
        }
        //Gradient
        private Gradient[] _gradients;

        public Gradient[] Gradients
        {
            get{return _gradients;}
            set { _gradients = value; }
        }
        //AnimationCurve
        private AnimationCurve[] _animationCurves;

        public AnimationCurve[] AnimationCurves
        {
            get{return _animationCurves;}
            set { _animationCurves = value; }
        }
        //纹理长宽
        private Vector2Int _textureSize = new Vector2Int(128, 1);
        public Vector2Int TextureSize
        {
            get
            {
                _textureSize.x = EditorPrefs.GetInt("ArtToolCommon_textureSizeX", 128);
                _textureSize.y = EditorPrefs.GetInt("ArtToolCommon_textureSizeY", 128);
                return _textureSize;
                
            }
            set
            {
                _textureSize.x = Mathf.Clamp(value.x, 1, 4096);
                _textureSize.y = Mathf.Clamp(value.y,1,4096); 
                EditorPrefs.SetInt("ArtToolCommon_textureSizeX", _textureSize.x);
                EditorPrefs.SetInt("ArtToolCommon_textureSizeY", _textureSize.y);
            }  
        }
        
        //GPU处理图像大小只能以2次幂调整大小
        private Vector2Int _textureSizeGPU = new Vector2Int(128, 128);
        public Vector2Int TextureSizeGPU
        {
            get
            {
                _textureSizeGPU.x = EditorPrefs.GetInt("ArtToolCommon_textureSizeGPUX", 128);
                _textureSizeGPU.y = EditorPrefs.GetInt("ArtToolCommon_textureSizeGPUY", 128);
                return _textureSizeGPU;
                
            }
            set
            {
                _textureSizeGPU.x = Mathf.Clamp(value.x, 1, 4096);
                _textureSizeGPU.y = Mathf.Clamp(value.y,1,4096); 
                EditorPrefs.SetInt("ArtToolCommon_textureSizeGPUX", _textureSizeGPU.x);
                EditorPrefs.SetInt("ArtToolCommon_textureSizeGPUY", _textureSizeGPU.y);
            }  
        }
    
        //----------------------------------------------- 初始化 ------------------------------------------------------//
        private ArtToolCommon()// 初始化非托管资源等
        {
            _gradients = new Gradient[10];
            _animationCurves = new AnimationCurve[10];
            for (int i = 0; i < 10; i++)
            {
                _animationCurves[i] ??= AnimationCurve.Linear(0, 0, 1, 1);
                _gradients[i] ??= new Gradient();
            }
        }
        //------------------------------------------------------------------------------------------------------------//
    
    
    
    
        //------------------------------------------------------------------------------------------------------------//
        private static ArtToolCommon instance;
        private static readonly object _lock = new object();
        public static ArtToolCommon Instance
        {
            get
            {
                if (instance == null)
                    lock (_lock)
                        if (instance == null)
                            instance = new ArtToolCommon();
                return instance;
            }
        }
    }
}
