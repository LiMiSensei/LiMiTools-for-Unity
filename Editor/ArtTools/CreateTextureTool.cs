using UnityEditor;
using UnityEngine;

namespace LiMiTools.Editor.ArtTools
{
    public class CreateTextureTool : EditorWindow
    {
        private string[] _categoryNames =
        {
            "渐变条工具","曲线条工具"
        };
        private int _selectedCategory = 0;//选择部分
        private Texture2D previewTexture;
        private Vector2 _scrollPos = Vector2.zero;
        private string _textureName = "Texture";
        private int _number = 1;
        
        private Gradient[] _gradient;               //要初始化并填充数据
        private AnimationCurve[] _animationCurves;  //
        void OnEnable()
        {
            _gradient = ArtToolCommon.Instance.Gradients;
            _animationCurves = ArtToolCommon.Instance.AnimationCurves;

            _number = EditorPrefs.GetInt("CreateTextureTool_number", 1);
            _selectedCategory = EditorPrefs.GetInt("CreateTextureTool_selectedCategory",0);//存储位置
        }

        void OnDisable()
        {
            EditorPrefs.SetInt("CreateTextureTool_selectedCategory",_selectedCategory);//存储位置
            EditorPrefs.SetInt("CreateTextureTool_number", _number);
        }

        void OnDestroy()
        {
            
        }

        //-------------------------------------------------------------------------------------------------------------
        public void DrawGUI()
        {
            
            EditorGUILayout.BeginHorizontal();//水平布局
            {
                _selectedCategory = GUILayout.SelectionGrid(_selectedCategory,_categoryNames,2,GUILayout.Height(40));
            }
            EditorGUILayout.EndHorizontal();
            
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                _number = EditorGUILayout.IntSlider("目标数量",_number, 1, 10);
            }
            EditorGUILayout.EndVertical();
            
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos,EditorStyles.helpBox);
            {
                switch (_selectedCategory)
                {
                    case 0: GradientGUI();
                        ArtToolCommon.Instance.sRGB = true ; break;
                    case 1: CurveGUI();
                        ArtToolCommon.Instance.sRGB = false; break;
                    default: break;
                }
                Rect texRect = GUILayoutUtility.GetRect(1, 200);
                if(previewTexture!=null)EditorGUI.DrawPreviewTexture(texRect,previewTexture);
            }
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.HelpBox(ArtToolCommon.Instance.Message, MessageType.Info);
            
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            {
                ArtToolCommon.Instance.TextureSize = EditorGUILayout.Vector2IntField(
                    $"输出长宽：{ArtToolCommon.Instance.TextureSize.x} X ({ArtToolCommon.Instance.TextureSize.y} * {_number}) --> {ArtToolCommon.Instance.TextureSize.x} X {ArtToolCommon.Instance.TextureSize.y * _number}" ,ArtToolCommon.Instance.TextureSize);
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox,GUILayout.Width(300));
                {
                    ArtToolCommon.Instance.FilterMode = (FilterMode)EditorGUILayout.EnumPopup("图像过滤模式：",ArtToolCommon.Instance.FilterMode);
                    ArtToolCommon.Instance.sRGB = EditorGUILayout.ToggleLeft("sRGB",ArtToolCommon.Instance.sRGB);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);//水平布局
            {
                if (GUILayout.Button("选择路径")) ArtToolCommon.Instance.Path = ArtToolCore.SelectionPath();
                ArtToolCommon.Instance.Path = EditorGUILayout.TextField(ArtToolCommon.Instance.Path);
                EditorGUILayout.LabelField("Name:",EditorStyles.centeredGreyMiniLabel,GUILayout.Width(38));
                _textureName = EditorGUILayout.TextField(_textureName);
                if (GUILayout.Button("Save"))
                {
                    switch (_selectedCategory)
                    {
                        case 0: previewTexture = CreateGradientTexture(ArtToolCommon.Instance.TextureSize,_number,_gradient);break;
                        case 1: previewTexture = CreateAnimationCurveTexture(ArtToolCommon.Instance.TextureSize,_number,_animationCurves); break;
                        default: break;
                    }
                    ArtToolCore.SaveTexture(previewTexture,_textureName,ArtToolCommon.Instance.Path);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        //-------------------------------------------------------------------------------------------------------------
    
    
        //渐变图GUI
        void GradientGUI()
        {
            for (int i = _number-1; i >= 0 ; i--)
            {
                _gradient[i] = EditorGUILayout.GradientField($"渐变:{i+1}", _gradient[i],GUILayout.Height(30),GUILayout.MinWidth(10));
            }
        }
        //曲线图GUI
        void CurveGUI()
        {
            for (int i = _number-1; i >= 0 ; i--)
            {
                _animationCurves[i] = EditorGUILayout.CurveField($"曲线:{i+1}",_animationCurves[i],GUILayout.Height(30));
            }
        }
        //-------------------------------------------------------------------------------------------------------------
        
        
        Texture2D CreateAnimationCurveTexture(Vector2Int size,int index,AnimationCurve[] animationCurves)
        {
            Texture2D texture = new Texture2D(size.x, size.y * index);
            Color[] colors = new Color[size.x * size.y  * animationCurves.Length];
            int num = 0;
            for (int i = 0; i < index; i++)  //1
            for (int j = 0; j < size.y; j++) //1
            for (int k = 0; k < size.x; k++) //256
            {
                float color = animationCurves[i].Evaluate((float)k/size.x);
                colors[num] = new Color(color, color, color, color);
                num++;
            }
            texture.SetPixels(colors);
            texture.Apply();
            return texture;
        }

        Texture2D CreateGradientTexture(Vector2Int size, int index, Gradient[] gradients)
        {
            Texture2D texture = new Texture2D(size.x, size.y * index);
            texture.filterMode = ArtToolCommon.Instance.FilterMode;
            Color[] colors = new Color[size.x * size.y  * gradients.Length];
            int num = 0;
            for (int i = 0; i < index; i++)  //10
            for (int j = 0; j < size.y; j++) //128
            for (int k = 0; k < size.x; k++) //1024
            {
                colors[num] = _gradient[i].Evaluate((float)k / size.x);
                num++;
            }
            texture.SetPixels(colors);
            texture.Apply();
            return texture;
        }
    }
}
