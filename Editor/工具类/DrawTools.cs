using System.Collections.Generic;
using System.Text;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;


namespace Unity.FantasyKingdom
{
    public class DrawTools : EditorWindow
    {
        [MenuItem("LiMi/工具/绘制查看器 v0.1")]
        static void Open()
        {
            GetWindow<DrawTools>("绘制查看器").Show();
        }
        
        const int sampleCount = 30;
        const float renderScale = 0.5f;
        private StringBuilder infoBuilder = new StringBuilder();
        
        //渲染绘制数和定点数
        long vertexCount;
        List<long> vertexCountSamples = new List<long>();
        long drawCallCount;
        List<long> drawCallCountSamples = new List<long>();
        
        //分析器记录器
        
     
        
        ProfilerRecorder drawCallsRecorder;
        ProfilerRecorder verticesRecorder;
        ProfilerRecorder cpuMainThreaTimeRecorder;
        ProfilerRecorder gpuFrameTimeRecorder;
        ProfilerRecorder mainThreadTimeRecorder;
        
        ProfilerRecorder systemMemoryRecorder;
        ProfilerRecorder gcMemoryRecorder;
        ProfilerRecorder mainThreadTimeRecorder1;
        
        private void OnEnable()
        {
            
            
           
            drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
            verticesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");
            cpuMainThreaTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render,"CPU Main Thread Frame Time", sampleCount);
            gpuFrameTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "GPU Frame Time", sampleCount);
            mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", sampleCount);
            
            systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
            gcMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
            mainThreadTimeRecorder1 = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);

            
          
        }

        private void OnDisable()
        {
           
            
            
            drawCallsRecorder.Dispose();
            verticesRecorder.Dispose();
            cpuMainThreaTimeRecorder.Dispose();
            gpuFrameTimeRecorder.Dispose();
            mainThreadTimeRecorder.Dispose();
            
            systemMemoryRecorder.Dispose();
            gcMemoryRecorder.Dispose();
            mainThreadTimeRecorder1.Dispose();
            
         
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }
//----------------------------------------------------------------------------------------------------------------------
        void OnGUI()
        {
            UpdateStats();
            EditorGUILayout.LabelField($"Draws：{drawCallCount}");//Draws
            EditorGUILayout.LabelField($"Verts：{(vertexCount / 1000000f):F1} m");//Verts
            EditorGUILayout.LabelField($"FPS：{1/(GetRecorderFrameAverage(mainThreadTimeRecorder) * (1e-9f)):F1}");//FPS
            EditorGUILayout.LabelField($"CPU：{GetRecorderFrameAverage(cpuMainThreaTimeRecorder)*(1e-6f):F1}");//CPU
            EditorGUILayout.LabelField($"GPU：{GetRecorderFrameAverage(gpuFrameTimeRecorder) * (1e-6f):F1}");//GPU
            
            EditorGUILayout.LabelField($"Frame Time: {GetRecorderFrameAverage(mainThreadTimeRecorder1) * (1e-6f):F1} ms");//Frame Time
            EditorGUILayout.LabelField($"GC Memory: {gcMemoryRecorder.LastValue / (1024 * 1024)} MB");//GC Memory
            EditorGUILayout.LabelField($"System Memory: {systemMemoryRecorder.LastValue / (1024 * 1024)} MB");//System Memory
        }
        
        void UpdateStats()
        {
            if (verticesRecorder.Valid == true)
            {
                vertexCountSamples.Add(verticesRecorder.LastValue);
            }

            if (vertexCountSamples.Count > sampleCount)
            {
                vertexCountSamples.RemoveAt(0);
            }


            vertexCount = 0;
            for (int i = 0; i < vertexCountSamples.Count; i++)
            {
                if (vertexCountSamples[i] > vertexCount)
                {
                    vertexCount = vertexCountSamples[i];
                }
            }

            if (drawCallsRecorder.Valid == true)
            {
                drawCallCountSamples.Add(drawCallsRecorder.LastValue);
            }

            if (drawCallCountSamples.Count > sampleCount)
            {
                drawCallCountSamples.RemoveAt(0);
            }

            drawCallCount = 0;
            for (int i = 0; i < drawCallCountSamples.Count; i++)
            {
                if (drawCallCountSamples[i] > drawCallCount)
                {
                    drawCallCount = drawCallCountSamples[i];
                }
            }
        }
        
        static double GetRecorderFrameAverage(ProfilerRecorder recorder)
        {
            var samplesCount = recorder.Capacity;
            if (samplesCount == 0)
                return 0;

            double r = 0;
            var samples = new List<ProfilerRecorderSample>(samplesCount);
            recorder.CopyTo(samples);
            for (var i = 0; i < samples.Count; ++i)
                r += samples[i].Value;
            r /= samplesCount;

            return r;
        }
    }
}
