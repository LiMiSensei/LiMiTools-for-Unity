using System;
using UnityEditor;
using UnityEngine;

namespace LiMiTools.Editor.ArtTools
{
    public class SplitMergeTexture : EditorWindow
    {
        private Vector2 _scrollPos = Vector2.zero;
        private string _textureName = "texture";
        
        private string[] _channel = { "R","G","B","A" };
        private int[] _channelPos = {0,0,0,0};
        private bool[] _isInversion = {false,false,false,false};
        
        private float debug = 0f;
        
        //用于下中4中图像的通道预览
        private Texture2D[] _textureTexs = new Texture2D[4];
        private Texture2D[] _channelTexs = new Texture2D[4];
        
        
        //用于左下角预览
        private Texture2D _previewtexture;
        private Texture2D _oldPreviewtexture;
        private Texture2D Previewtexture
        {
            get
            {
                return _previewtexture;
            }
            set
            {
                if (value != _oldPreviewtexture)
                {
                    _oldPreviewtexture = value;
                    _previewtexture = value;
                    if(value != null)_channelTexs = TextureSplitRGBA(value);
                }
            }
        }
        
       
        void OnEnable()
        {
            
        }
        void OnDisable()
        {
      
        }
        void OnDestroy()
        {
            
        }

        public void DrawGUI()
        {
            EditorGUILayout.HelpBox("所有纹理统一以2次幂分离,合并,修改大小.注意", MessageType.Info);
            
            
            
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos,EditorStyles.helpBox);
            {
                TextureSplitGUI();
                EditorGUILayout.HelpBox(ArtToolCommon.Instance.Message, MessageType.Info);
                TextureMergeGUI();
            }
            EditorGUILayout.EndScrollView();
            
            
            
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("", EditorStyles.centeredGreyMiniLabel,GUILayout.MinWidth(10));
                EditorGUILayout.BeginVertical(EditorStyles.helpBox,GUILayout.Width(position.width),GUILayout.MinWidth(10));
                {
                    ArtToolCommon.Instance.FilterMode = (FilterMode)EditorGUILayout.EnumPopup("图像过滤模式：",ArtToolCommon.Instance.FilterMode,GUILayout.MinWidth(10));
                    ArtToolCommon.Instance.sRGB = EditorGUILayout.ToggleLeft("sRGB",ArtToolCommon.Instance.sRGB,GUILayout.MinWidth(10));
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox,GUILayout.Height(50));//水平布局
            {
                if (GUILayout.Button("选择路径",GUILayout.ExpandHeight(true))) ArtToolCommon.Instance.Path = ArtToolCore.SelectionPath();
                ArtToolCommon.Instance.Path = EditorGUILayout.TextField(ArtToolCommon.Instance.Path);
                EditorGUILayout.LabelField("Name:",EditorStyles.centeredGreyMiniLabel,GUILayout.Width(38));
                _textureName = EditorGUILayout.TextField(_textureName);
                if (GUILayout.Button("Save",GUILayout.ExpandHeight(true)))
                {
                    if(_previewtexture != null)
                        ArtToolCore.SaveTexture(ArtToolCore.TextureSizeOnGPU(ArtToolCommon.Instance.TextureSizeGPU,_previewtexture),_textureName,ArtToolCommon.Instance.Path);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        void TextureSplitGUI()
        {
            
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            for (int i = 0; i < _channel.Length; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    _textureTexs[i] = EditorGUILayout.ObjectField("",_textureTexs[i], typeof(Texture2D), false,GUILayout.MinWidth(10)) as Texture2D;
                    _channelPos[i] = GUILayout.SelectionGrid(_channelPos[i],_channel,1,GUILayout.MinWidth(10));
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("反转",GUILayout.Width(25),GUILayout.MinWidth(10));
                        _isInversion[i] = EditorGUILayout.Toggle(_isInversion[i],GUI.skin.button,GUILayout.Width(25),GUILayout.MinWidth(10));
                        if (GUILayout.Button("直接拆分",GUILayout.MinWidth(10)))
                        {
                            if (_textureTexs[i] != null)
                                ArtToolCore.SaveTexture(TextureSplit(_textureTexs[i],i),$"{_textureName}_{i}",ArtToolCommon.Instance.Path);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        void TextureMergeGUI()
        {
            
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox,GUILayout.Width(position.width),GUILayout.MinWidth(10));
                {
                    Rect texRect = GUILayoutUtility.GetRect(1, position.width);
                    if(Previewtexture != null)EditorGUI.DrawPreviewTexture(texRect,Previewtexture);
                    Previewtexture = EditorGUILayout.ObjectField(Previewtexture,typeof(Texture2D),false) as Texture2D;
                }
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox,GUILayout.MinHeight(position.width * 1.082f),GUILayout.MaxWidth(position.width * 0.284f));
                {
                    
                    for (int i = 0; i < _channelTexs.Length; i++)
                    {
                        if(_channelTexs[i] != null)GUILayout.Label(_channelTexs[i],GUILayout.Width(position.width * 0.26f),GUILayout.Height(position.width * 0.26f));
                    }
                    GUILayout.Space(1);
                }
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.BeginVertical();
                {
                    //三按钮
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox,GUILayout.Height(50));
                    {
                        if (GUILayout.Button("上推RGBA↑↑↑↑",GUILayout.ExpandHeight(true),GUILayout.MinWidth(10)))
                        {
                            Array.Copy(_channelTexs,_textureTexs,_channelPos.Length);//防止引用类型
                        }
                        if (GUILayout.Button("上推纹理 ↑",GUILayout.ExpandHeight(true),GUILayout.MinWidth(10)))
                        {
                            for (int i = 0; i < _channelTexs.Length; i++)
                            {
                                _textureTexs[i] = null;
                                _textureTexs[i] = _previewtexture;
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    if(GUILayout.Button("合并↓",GUILayout.MinWidth(10)))
                    {
                        Previewtexture = TextureMerge(_textureTexs);
                    }
                    
                    
                    //俩长宽控制的滑动条
                    var vector2ints = new Vector2Int((int)Mathf.Log(ArtToolCommon.Instance.TextureSizeGPU.x, 2), (int)Mathf.Log(ArtToolCommon.Instance.TextureSizeGPU.y, 2));
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    {
                        GUILayout.Label("长(次幂)",GUILayout.Width(60),GUILayout.MinWidth(10));
                        vector2ints.x = EditorGUILayout.IntSlider(vector2ints.x,0,12);
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    {
                        GUILayout.Label("宽(次幂)",GUILayout.Width(60),GUILayout.MinWidth(10));
                        vector2ints.y = EditorGUILayout.IntSlider(vector2ints.y,0,12);
                    }
                    EditorGUILayout.EndHorizontal();
                    ArtToolCommon.Instance.TextureSizeGPU = new Vector2Int((int)Mathf.Pow(2,vector2ints.x),(int)Mathf.Pow(2,vector2ints.y));
                    
                    debug = EditorGUILayout.Slider(debug,0f,2,GUILayout.MinWidth(10));
                    EditorGUILayout.LabelField($"大小为：{ArtToolCommon.Instance.TextureSizeGPU.x} * {ArtToolCommon.Instance.TextureSizeGPU.y}",GUILayout.MinWidth(10));

                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }




        
        
        //四张合成一张
        Texture2D TextureMerge(Texture2D[] textures)
        {
            if (textures == null || textures.Length < 4) return null;
            var size = ArtToolCommon.Instance.TextureSizeGPU;
            Texture2D outTex = new Texture2D(size.x, size.y);
            int pixelCount = size.x * size.y;
            Color[] finalColors = new Color[pixelCount];
            Color[][] texturePixels = new Color[4][];
            try
            {
                for (int i = 0; i < 4; i++)
                {
                    if (textures[i] == null) return null;
                    Texture2D resizedTex = ArtToolCore.TextureSizeOnGPU(size, textures[i]);
                    texturePixels[i] = resizedTex.GetPixels();
                }
                for (int pixelIdx = 0; pixelIdx < pixelCount; pixelIdx++)
                {
                    Color pixel = new Color();
                    for (int channel = 0; channel < 4; channel++)
                    {
                        Color[] srcPixels = texturePixels[channel];
                        int srcChannel = _channelPos[channel];
                        bool isInvert = _isInversion[channel];
                        float channelValue = srcChannel switch
                        {
                            0 => srcPixels[pixelIdx].r,
                            1 => srcPixels[pixelIdx].g,
                            2 => srcPixels[pixelIdx].b,
                            3 => srcPixels[pixelIdx].a,
                            _ => 0f
                        };
                        channelValue = isInvert ? 1f - channelValue : channelValue;
                        switch (channel)
                        {
                            case 0: pixel.r = channelValue; break;
                            case 1: pixel.g = channelValue; break;
                            case 2: pixel.b = channelValue; break;
                            case 3: pixel.a = channelValue; break;
                        }
                    }
                    finalColors[pixelIdx] = pixel;
                }
            }
            finally
            {
                for (int i = 0; i < 4; i++)
                {
                    texturePixels[i] = null;
                }
            }
            outTex.SetPixels(finalColors);
            outTex.Apply();
            return outTex;
        }
        
        //单张拆分
        Texture2D TextureSplit(Texture2D texture,int item)
        {
            if (texture == null) return null;
            texture = ArtToolCore.TextureSizeOnGPU(ArtToolCommon.Instance.TextureSizeGPU, texture);
            var colors = texture.GetPixels();
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i].r = _channelPos[item] switch
                {
                    0 => _isInversion[item] ?1f - colors[i].r:colors[i].r,
                    1 => _isInversion[item] ?1f - colors[i].g:colors[i].g,
                    2 => _isInversion[item] ?1f - colors[i].b:colors[i].b,
                    3 => _isInversion[item] ?1f - colors[i].a:colors[i].a
                };
                colors[i].g = _channelPos[item] switch
                {
                    0 => _isInversion[item] ?1f - colors[i].r:colors[i].r,
                    1 => _isInversion[item] ?1f - colors[i].g:colors[i].g,
                    2 => _isInversion[item] ?1f - colors[i].b:colors[i].b,
                    3 => _isInversion[item] ?1f - colors[i].a:colors[i].a
                };
                colors[i].b = _channelPos[item] switch
                {
                    0 => _isInversion[item] ?1f - colors[i].r:colors[i].r,
                    1 => _isInversion[item] ?1f - colors[i].g:colors[i].g,
                    2 => _isInversion[item] ?1f - colors[i].b:colors[i].b,
                    3 => _isInversion[item] ?1f - colors[i].a:colors[i].a
                };
                colors[i].a = _channelPos[item] switch
                {
                    0 => _isInversion[item] ?1f - colors[i].r:colors[i].r,
                    1 => _isInversion[item] ?1f - colors[i].g:colors[i].g,
                    2 => _isInversion[item] ?1f - colors[i].b:colors[i].b,
                    3 => _isInversion[item] ?1f - colors[i].a:colors[i].a
                };
            }
            texture.SetPixels(colors);
            texture.Apply();
            
            return texture;
        }
        
        //拆分RGBA
        Texture2D[] TextureSplitRGBA(Texture2D texture)
        {
            if (texture == null) return null;
            var size = ArtToolCommon.Instance.TextureSizeGPU;
            Texture2D[] splitTextures = new Texture2D[4];
            int pixelCount = size.x * size.y;
            for (int i = 0; i < 4; i++)
            {
                splitTextures[i] = new Texture2D(size.x, size.y);
            }
            Texture2D resizedTexture = ArtToolCore.TextureSizeOnGPU(size, texture);
            Color[] sourcePixels = resizedTexture.GetPixels();
            try
            {
                Color[] tempPixels = new Color[pixelCount];
                for (int pixelIdx = 0; pixelIdx < pixelCount; pixelIdx++)
                {
                    Color srcPixel = sourcePixels[pixelIdx];
                    float r = srcPixel.r;
                    float g = srcPixel.g;
                    float b = srcPixel.b;
                    float a = srcPixel.a;
                    tempPixels[pixelIdx] = new Color(r, r, r, r); // R通道
                }
                splitTextures[0].SetPixels(tempPixels);
                splitTextures[0].Apply();
                for (int pixelIdx = 0; pixelIdx < pixelCount; pixelIdx++)
                {
                    float g = sourcePixels[pixelIdx].g;
                    tempPixels[pixelIdx] = new Color(g, g, g, g);
                }
                splitTextures[1].SetPixels(tempPixels);
                splitTextures[1].Apply();
                for (int pixelIdx = 0; pixelIdx < pixelCount; pixelIdx++)
                {
                    float b = sourcePixels[pixelIdx].b;
                    tempPixels[pixelIdx] = new Color(b, b, b, b);
                }
                splitTextures[2].SetPixels(tempPixels);
                splitTextures[2].Apply();
                for (int pixelIdx = 0; pixelIdx < pixelCount; pixelIdx++)
                {
                    float a = sourcePixels[pixelIdx].a;
                    tempPixels[pixelIdx] = new Color(a, a, a, a);
                }
                splitTextures[3].SetPixels(tempPixels);
                splitTextures[3].Apply();
            }
            finally
            {
                sourcePixels = null;
            }

            return splitTextures;
        }
        
    }
}
