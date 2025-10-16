using UnityEditor;
using UnityEngine;

public class SystemInfoCode : EditorWindow
{
    [MenuItem("SystemInfoCode/SystemInfoCode")]
    static void Init()
    {
        SystemInfoCode window = (SystemInfoCode)EditorWindow.GetWindow(typeof(SystemInfoCode), 
            true, 
            "系统信息");
        window.Show();
    }
    Vector2 scrollPos;
    Vector2 scrollPos2;
    void OnGUI()
    {
        var productName = Application.productName;//返回应用程序产品名称
        var platform = Application.platform;//返回游戏运行平台（只读）。
        var sandboxType = Application.sandboxType;//返回在沙盒中运行的应用程序（只读）。
        var systemLanguage = Application.systemLanguage;//用户操作系统运行时所使用的语言。
        var temporaryCachePath = Application.temporaryCachePath;//包含临时数据/缓存目录的路径（只读）。
        
        var version = Application.version;//返回应用程序版本号（只读）。
        var HasProLicense = Application.HasProLicense();//Unity 是否是使用 Pro 许可证激活？
        var installMode = Application.installMode;//返回应用程序安装模式（只读）
        var isConsolePlatform = Application.isConsolePlatform;//当前的运行时平台是否为已知的控制台平台？
        var genuineCheckAvailable = Application.temporaryCachePath;//如果可以确认应用程序完整性，则返回 true。
        
        var genuine = Application.genuine;//如果应用程序在构建后以任何方式进行了更改，则返回 false。
        var dataPath = Application.dataPath;//包含目标设备上的游戏数据文件夹路径（只读）。
        var consoleLogPath = Application.consoleLogPath;//返回控制台日志文件的路径，如果当前平台不支持日志文件，则返回空字符串。
        var buildGUID = Application.buildGUID;//返回此构建的 GUID（只读）。

        var graphicsDeviceName = SystemInfo.graphicsDeviceName;//显卡名称（仅支持部分平台）
        var graphicsMemorySize =  SystemInfo.graphicsMemorySize;//显存大小（MB）
        var supportsComputeShaders =  SystemInfo.supportsComputeShaders;//是否支持图形特性
        var processorType =  SystemInfo.processorType;//处理器名称
        var processorCount =  SystemInfo.processorCount;//核心数
        var systemMemorySize =  SystemInfo.systemMemorySize;//内存信息
        var operatingSystem =  SystemInfo.operatingSystem;//操作系统信息

        var batteryLevel = SystemInfo.batteryLevel;//当前的电池电量（只读）。
        var batteryStatus = SystemInfo.batteryStatus;//返回设备电池的当前状态（只读）。
        var computeSubGroupSize = SystemInfo.computeSubGroupSize;//支持GPU上高效内存共享的计算线程组的大小（只读）。
        var constantBufferOffsetAlignment = SystemInfo.constantBufferOffsetAlignment;//使用 Shader.SetConstantBuffer 或 Material.SetConstantBuffer 绑定常量缓冲区时的最小缓冲区偏移（以字节为单位）。
        var copyTextureSupport = SystemInfo.copyTextureSupport;//对各种 Graphics.CopyTexture 情况的支持（只读）。
        
        var deviceName = SystemInfo.deviceName;//设备的型号（只读）。
        var deviceType = SystemInfo.deviceType;//返回运行该应用程序的设备的类型（只读）。
        var deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier;//唯一设备标识符。保证对于每个设备都是唯一的（只读）。
        var foveatedRenderingCaps = SystemInfo.foveatedRenderingCaps;//该平台支持的中心凹渲染技术。
        var graphicsDeviceID = SystemInfo.graphicsDeviceID;//图形设备的标识符代码（只读）。
        
        var graphicsDeviceName2 = SystemInfo.graphicsDeviceName;//图形设备的名称（只读）。
        var graphicsDeviceType = SystemInfo.graphicsDeviceType;//图形设备使用的图形 API 类型（只读）。
        var graphicsDeviceVendor = SystemInfo.graphicsDeviceVendor;//图形设备的供应商（只读）。
        var graphicsDeviceVendorID = SystemInfo.graphicsDeviceVendorID;//图形设备供应商的标识符代码（只读）。
        var graphicsDeviceVersion = SystemInfo.graphicsDeviceVersion;//图形设备使用的图形 API 类型和驱动程序版本（只读）。
        
        var graphicsMemorySize2 = SystemInfo.graphicsMemorySize;//具有的显存容量（只读）。
        var graphicsMultiThreaded = SystemInfo.graphicsMultiThreaded;//图形设备是否使用多线程渲染？（只读）
        var graphicsShaderLevel = SystemInfo.graphicsShaderLevel;//图形设备着色器功能级别（只读）。
        var graphicsUVStartsAtTop = SystemInfo.graphicsUVStartsAtTop;//如果该平台的纹理 UV 坐标约定的 Y 坐标从图像顶部开始，则返回 true。
        var hasDynamicUniformArrayIndexingInFragmentShaders = SystemInfo.hasDynamicUniformArrayIndexingInFragmentShaders;//如果 GPU 原生支持对片元着色器中的 uniform 数组编制索引，并且无任何限制，则返回 true。
        var hasHiddenSurfaceRemovalOnGPU = SystemInfo.hasHiddenSurfaceRemovalOnGPU;//如果 GPU 支持删除隐藏的表面，则为 true。

        #region MyRegion

        

        
        EditorGUILayout.BeginHorizontal();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos,GUILayout.Width(position.width/2), GUILayout.Height(position.height));
        
        EditorGUILayout.LabelField("应用程序产品名称：" + productName);
        EditorGUILayout.LabelField("游戏运行平台:" + platform);
        EditorGUILayout.LabelField("沙盒中运行的应用程序:" + sandboxType);
        EditorGUILayout.LabelField("系统运行时所使用的语言:" + systemLanguage);
        EditorGUILayout.LabelField("包含临时数据/缓存目录的路径:" + temporaryCachePath);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("序版本号:" + version);
        EditorGUILayout.LabelField("是否是使用 Pro 许可证激活:" + HasProLicense);
        EditorGUILayout.LabelField("应用程序安装模式:" + installMode);
        EditorGUILayout.LabelField("已知的控制台平台:" + isConsolePlatform);
        EditorGUILayout.LabelField("确认应用程序完整性:" + genuineCheckAvailable);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("构建后以任何方式进行了更改:" + genuine);
        EditorGUILayout.LabelField("游戏数据文件夹路径:" + dataPath);
        EditorGUILayout.LabelField("控制台日志文件的路径:" + consoleLogPath);
        EditorGUILayout.LabelField("构建的 GUID:" + buildGUID);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("显卡名称:" + graphicsDeviceName);
        EditorGUILayout.LabelField("显存大小:" + graphicsMemorySize);
        EditorGUILayout.LabelField("是否支持图形特性:" + supportsComputeShaders);
        EditorGUILayout.LabelField("处理器名称:" + processorType);
        EditorGUILayout.LabelField("核心数:" + processorCount);
        EditorGUILayout.LabelField("内存信息:" + systemMemorySize);
        EditorGUILayout.LabelField("操作系统信息:" + operatingSystem);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("当前的电池电量:" + batteryLevel);
        EditorGUILayout.LabelField("电池的当前状态:" + batteryStatus);
        EditorGUILayout.LabelField("GPU上高效内存共享的计算线程组的大小:" + computeSubGroupSize);
        EditorGUILayout.LabelField("绑定常量缓冲区时的最小缓冲区偏移:" + constantBufferOffsetAlignment);
        EditorGUILayout.LabelField("各种 Graphics.CopyTexture 情况的支持:" + copyTextureSupport);
        
        EditorGUILayout.LabelField("设备的型号:" + deviceName);
        EditorGUILayout.LabelField("运行该应用程序的设备的类型:" + deviceType);
        EditorGUILayout.LabelField("唯一设备标识符:" + deviceUniqueIdentifier);
        EditorGUILayout.LabelField("操作系统信息:" + foveatedRenderingCaps);
        EditorGUILayout.LabelField("操作系统信息:" + graphicsDeviceID);
        
        EditorGUILayout.LabelField("图形设备的名称:" + graphicsDeviceName2);
        EditorGUILayout.LabelField("图形设备使用的图形 API 类型:" + graphicsDeviceType);
        EditorGUILayout.LabelField("图形设备的供应商:" + graphicsDeviceVendor);
        EditorGUILayout.LabelField("图形设备供应商的标识符代码:" + graphicsDeviceVendorID);
        EditorGUILayout.LabelField("图形设备使用的图形 API 类型和驱动程序版本:" + graphicsDeviceVersion);
        
        EditorGUILayout.LabelField("具有的显存容量:" + graphicsMemorySize2);
        EditorGUILayout.LabelField("图形设备是否使用多线程渲染:" + graphicsMultiThreaded);
        EditorGUILayout.LabelField("图形设备着色器功能级别:" + graphicsShaderLevel);
        EditorGUILayout.LabelField("如果该平台的纹理 UV 坐标约定的 Y:" + graphicsUVStartsAtTop);
        EditorGUILayout.LabelField("GPU 原生支持对片元着色器中的 uniform 数组编制索引，并且无任何限制:" + hasDynamicUniformArrayIndexingInFragmentShaders);
        EditorGUILayout.LabelField("GPU 支持删除隐藏的表面:" + hasHiddenSurfaceRemovalOnGPU);
        EditorGUILayout.LabelField("GPU 支持部分 Mipmap 链:" + SystemInfo.hasMipMaxLevel);
        EditorGUILayout.LabelField("用于描述系统上的 HDR 显示支持:" + SystemInfo.hdrDisplaySupportFlags);
        EditorGUILayout.LabelField("各向异性过滤的最大各向异性级别:" + SystemInfo.maxAnisotropyLevel);
        EditorGUILayout.LabelField("确定 Unity 在计算着色器中同时支持进行读取的计算缓冲区数:" + SystemInfo.maxComputeBufferInputsCompute);
        EditorGUILayout.LabelField("确定 Unity 在域着色器中同时支持进行读取的计算缓冲区数:" + SystemInfo.maxComputeBufferInputsDomain);
        EditorGUILayout.LabelField("确定 Unity 在片元着色器中同时支持进行读取的计算缓冲区数:" + SystemInfo.maxComputeBufferInputsFragment);
        EditorGUILayout.LabelField("确定 Unity 在几何着色器中同时支持进行读取的计算缓冲区数:" + SystemInfo.maxComputeBufferInputsGeometry);
        EditorGUILayout.LabelField("确定 Unity 在外壳着色器中同时支持进行读取的计算缓冲区数:" + SystemInfo.maxComputeBufferInputsHull);
        EditorGUILayout.LabelField("确定 Unity 在顶点着色器中同时支持进行读取的计算缓冲区数:" + SystemInfo.maxComputeBufferInputsVertex);
        EditorGUILayout.LabelField("单个本地工作组中可以分发到计算着色器的最大调用总数:" + SystemInfo.maxComputeWorkGroupSize);
        EditorGUILayout.LabelField("计算着色器可以在 X 维度使用的最大工作组数:" + SystemInfo.maxComputeWorkGroupSizeX);
        EditorGUILayout.LabelField("计算着色器可以在 Y 维度使用的最大工作组数:" + SystemInfo.maxComputeWorkGroupSizeY);
        EditorGUILayout.LabelField("计算着色器可以在 Z 维度使用的最大工作组数:" + SystemInfo.maxComputeWorkGroupSizeZ);
        EditorGUILayout.LabelField("常量缓冲区绑定的最大大小:" + SystemInfo.maxConstantBufferSize);
        EditorGUILayout.LabelField("最大立方体贴图纹理大小:" + SystemInfo.maxCubemapSize);
        EditorGUILayout.LabelField("图形缓冲区的最大大小:" + SystemInfo.maxGraphicsBufferSize);
        EditorGUILayout.LabelField("最大3D纹理大小:" + SystemInfo.maxTexture3DSize);
        EditorGUILayout.LabelField("纹理数组中的最大切片数:" + SystemInfo.maxTextureArraySlices);
        EditorGUILayout.LabelField("最大纹理大小:" + SystemInfo.maxTextureSize);
        EditorGUILayout.LabelField("GPU 提供何种 NPOT（大小并非 2 的幂）纹理支持:" + SystemInfo.npotSupport);
        EditorGUILayout.LabelField("包含版本的操作系统名称:" + SystemInfo.operatingSystem);
        EditorGUILayout.LabelField("返回运行该游戏的操作系统系列:" + SystemInfo.operatingSystemFamily);
        //--------------------------------------------------------------------------------------------------------------

        //Rect r1 = new Rect(position.x, position.y, position.width, position.height);
        //GUI.BeginScrollView(r1)
        
        EditorGUILayout.EndScrollView();
        
        scrollPos2 = EditorGUILayout.BeginScrollView(scrollPos2,GUILayout.Width(position.width/2), GUILayout.Height(position.height));
        
        EditorGUILayout.LabelField("处理器频率，单位为 MHz:" + SystemInfo.processorFrequency);
        EditorGUILayout.LabelField("处理器名称:" + SystemInfo.processorType);
        EditorGUILayout.LabelField("应用程序实际渲染线程模式:" + SystemInfo.renderingThreadingMode);
        EditorGUILayout.LabelField("Unity 同时支持的随机写入目标 (UAV) 的最大数量:" + SystemInfo.supportedRandomWriteTargetCount);
        EditorGUILayout.LabelField("支持多少个同时渲染目标 (MRT):" + SystemInfo.supportedRenderTargetCount);
        EditorGUILayout.LabelField("是否支持 2D 数组纹理:" + SystemInfo.supports2DArrayTextures);
        EditorGUILayout.LabelField("是否支持 32 位索引缓冲区:" + SystemInfo.supports32bitsIndexBuffer);
        EditorGUILayout.LabelField("是否支持 3D（体积）RenderTextures:" + SystemInfo.supports3DRenderTextures);
        EditorGUILayout.LabelField("是否支持 3D（体积）纹理:" + SystemInfo.supports3DTextures);
        EditorGUILayout.LabelField("设备上是否有加速度计:" + SystemInfo.supportsAccelerometer);
        EditorGUILayout.LabelField("设备支持各向异性过滤:" + SystemInfo.supportsAnisotropicFilter);
        EditorGUILayout.LabelField("当平台支持异步计算队列时:" + SystemInfo.supportsAsyncCompute);
        EditorGUILayout.LabelField("如果 GPU 数据的异步回读可用于该设备:" + SystemInfo.supportsAsyncGPUReadback);
        EditorGUILayout.LabelField("是否有可用于播放的音频设备:" + SystemInfo.supportsAudio);
        EditorGUILayout.LabelField("是否支持3D（体积）纹理的压缩格式:" + SystemInfo.supportsCompressed3DTextures);
        EditorGUILayout.LabelField("是否支持计算着色器:" + SystemInfo.supportsComputeShaders);
        EditorGUILayout.LabelField("是否支持保守光栅化:" + SystemInfo.supportsConservativeRaster);
        EditorGUILayout.LabelField("是否支持立方体贴图数组纹理:" + SystemInfo.supportsCubemapArrayTextures);
        EditorGUILayout.LabelField("是否支持几何着色器:" + SystemInfo.supportsGeometryShaders);
        EditorGUILayout.LabelField("指定当前平台是否支持 GPU 录制器:" + SystemInfo.supportsGpuRecorder);
        EditorGUILayout.LabelField("如果平台支持GraphicsFences:" + SystemInfo.supportsGraphicsFence);
        EditorGUILayout.LabelField("设备上是否有陀螺仪:" + SystemInfo.supportsGyroscope);
        EditorGUILayout.LabelField("硬件是否支持四边形拓扑:" + SystemInfo.supportsHardwareQuadTopology);
        EditorGUILayout.LabelField("图形系统支持使用间接参数缓冲区的GPU绘图调用:" + SystemInfo.supportsIndirectArgumentsBuffer);
        EditorGUILayout.LabelField("是否支持内联光线追踪（光线查询）:" + SystemInfo.supportsInlineRayTracing);
        EditorGUILayout.LabelField("是否支持 GPU 绘制调用实例化:" + SystemInfo.supportsInstancing);
        EditorGUILayout.LabelField("设备是否能够报告其位置:" + SystemInfo.supportsLocationService);
        EditorGUILayout.LabelField("是否支持纹理 Mipmap 串流:" + SystemInfo.supportsMipStreaming);
        EditorGUILayout.LabelField("该平台是否支持运动矢量:" + SystemInfo.supportsMotionVectors);
        EditorGUILayout.LabelField("自动解析了多重采样纹理:" + SystemInfo.supportsMultisampleAutoResolve);
        EditorGUILayout.LabelField("是否支持多采样纹理数组:" + SystemInfo.supportsMultisampled2DArrayTextures);
        EditorGUILayout.LabelField("是否支持多重采样纹理:" + SystemInfo.supportsMultisampledTextures);
        EditorGUILayout.LabelField("平台支持深度纹理的多采样解析:" + SystemInfo.supportsMultisampleResolveDepth);
        EditorGUILayout.LabelField("平台支持模板纹理的多样本解析:" + SystemInfo.supportsMultisampleResolveStencil);
        EditorGUILayout.LabelField("指示是否支持多视图:" + SystemInfo.supportsMultiview);
        EditorGUILayout.LabelField("是否支持从阴影贴图进行原始深度采样:" + SystemInfo.supportsRawShadowDepthSampling);
        EditorGUILayout.LabelField("是否支持任何光线跟踪功能:" + SystemInfo.supportsRayTracing);
        EditorGUILayout.LabelField("否支持光线跟踪着色器:" + SystemInfo.supportsRayTracingShaders);
        EditorGUILayout.LabelField("SV_RenderTargetArrayIndex是否可以在顶点着色器中使用:" + SystemInfo.supportsRenderTargetArrayIndexFromVertexShader);
        EditorGUILayout.LabelField("如果在渲染到多个渲染目标时，平台支持不同的混合模式:" + SystemInfo.supportsSeparatedRenderTargetsBlend);
        EditorGUILayout.LabelField("当前渲染器是否支持直接绑定常量缓冲区:" + SystemInfo.supportsSetConstantBuffer);
        EditorGUILayout.LabelField("是否支持内置阴影:" + SystemInfo.supportsShadows);
        EditorGUILayout.LabelField("是否支持稀疏纹理:" + SystemInfo.supportsSparseTextures);
        EditorGUILayout.LabelField("目标生成平台的图形API采用RenderBufferStoreAction:" + SystemInfo.supportsStoreAndResolveAction);
        EditorGUILayout.LabelField("是否支持曲面细分着色器:" + SystemInfo.supportsTessellationShaders);
        EditorGUILayout.LabelField("如果支持“Mirror Once”纹理包裹模式:" + SystemInfo.supportsTextureWrapMirrorOnce);
        EditorGUILayout.LabelField("设备是否能够通过振动为用户提供触觉反馈:" + SystemInfo.supportsVibration);
        EditorGUILayout.LabelField("具有的系统内存容量:" + SystemInfo.systemMemorySize);
        EditorGUILayout.LabelField("SystemInfo 字符串属性返回的当前平台不支持的值:" + SystemInfo.unsupportedIdentifier);
        EditorGUILayout.LabelField("如果图形 API 考虑 RenderBufferLoadAction 和 RenderBufferStoreAction:" + SystemInfo.usesLoadStoreActions);
        EditorGUILayout.LabelField("如果当前平台使用反转深度缓冲区:" + SystemInfo.usesReversedZBuffer);
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();
        this.Repaint();
        #endregion
    }

    void OnEnable1()
    {
        
    }

}
