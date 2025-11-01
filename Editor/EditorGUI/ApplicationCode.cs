using System;
using UnityEditor;
using UnityEngine;

public class ApplicationCode : EditorWindow
{
    [MenuItem("LiMi/测试/ApplicationCode")]
    static void Init()
    {
        ApplicationCode window = (ApplicationCode)EditorWindow.GetWindow(typeof(ApplicationCode), 
            true, 
            "游戏运行数据");
        window.Show();
    }
    
    // 区域a: 所有Application属性转换为string类型变量
    string absoluteURL;
    string buildGUID;
    string cloudProjectId;
    string companyName;
    string consoleLogPath;
    string dataPath;
    string genuine;
    string genuineCheckAvailable;
    string identifier;
    string installerName;
    string installMode;
    string internetReachability;
    string isBatchMode;
    string isConsolePlatform;
    string isEditor;
    string isFocused;
    string isMobilePlatform;
    string isPlaying;
    string persistentDataPath;
    string platform;
    string productName;
    string runInBackground;
    string sandboxType;
    string streamingAssetsPath;
    string systemLanguage;
    string targetFrameRate;
    string temporaryCachePath;
    string unityVersion;
    string version;
    string hasUserAuthorization;
    string requestUserAuthorization;
    string advertisingIdentifierResult;
    
    // 用于存储广告标识符请求的状态
    private bool isAdvertisingIdRequested = false;
    private string advertisingId = "请求中...";
    
    private void OnEnable()
    {
        // 在窗口启用时初始化所有变量
        UpdateApplicationInfo();
    }
    
    private void UpdateApplicationInfo()
    {
        absoluteURL = Application.absoluteURL;
        buildGUID = Application.buildGUID;
        cloudProjectId = Application.cloudProjectId;
        companyName = Application.companyName;
        consoleLogPath = Application.consoleLogPath;
        dataPath = Application.dataPath;
        genuine = Application.genuine.ToString();
        genuineCheckAvailable = Application.genuineCheckAvailable.ToString();
        identifier = Application.identifier;
        installerName = Application.installerName;
        installMode = Application.installMode.ToString();
        internetReachability = Application.internetReachability.ToString();
        isBatchMode = Application.isBatchMode.ToString();
        isConsolePlatform = Application.isConsolePlatform.ToString();
        isEditor = Application.isEditor.ToString();
        isFocused = Application.isFocused.ToString();
        isMobilePlatform = Application.isMobilePlatform.ToString();
        isPlaying = Application.isPlaying.ToString();
        persistentDataPath = Application.persistentDataPath;
        platform = Application.platform.ToString();
        productName = Application.productName;
        runInBackground = Application.runInBackground.ToString();
        sandboxType = Application.sandboxType.ToString();
        streamingAssetsPath = Application.streamingAssetsPath;
        systemLanguage = Application.systemLanguage.ToString();
        targetFrameRate = Application.targetFrameRate.ToString();
        temporaryCachePath = Application.temporaryCachePath;
        unityVersion = Application.unityVersion;
        version = Application.version;
        hasUserAuthorization = Application.HasUserAuthorization(UserAuthorization.Microphone).ToString();
        
        // 特殊处理需要回调的异步方法
        advertisingIdentifierResult = "未请求";
        isAdvertisingIdRequested = false;
        
        // 处理需要立即执行的方法
        try
        {
            // RequestUserAuthorization是一个异步操作，这里只记录请求动作
            requestUserAuthorization = "调用中...";
            Application.RequestUserAuthorization(UserAuthorization.Microphone);
        }
        catch (Exception e)
        {
            requestUserAuthorization = "错误: " + e.Message;
        }
    }
    
    private void OnGUI()
    {
        if (GUILayout.Button("刷新数据"))
        {
            UpdateApplicationInfo();
        }
        
        if (GUILayout.Button("请求广告标识符"))
        {
            RequestAdvertisingIdentifier();
        }
        
        EditorGUILayout.LabelField("文档的URL:" + absoluteURL);
        EditorGUILayout.LabelField("构建的 GUID:" + buildGUID);
        EditorGUILayout.LabelField("唯一的云端项目标识符:" + cloudProjectId);
        EditorGUILayout.LabelField("应用程序公司名称:" + companyName);
        EditorGUILayout.LabelField("控制台日志文件的路径:" + consoleLogPath);
        EditorGUILayout.LabelField("目标设备上的游戏数据文件夹路径:" + dataPath);
        EditorGUILayout.LabelField("如果应用程序在构建后未进行了更改:" + genuine);
        EditorGUILayout.LabelField("确认应用程序完整性:" + genuineCheckAvailable);
        EditorGUILayout.LabelField("在运行时返回应用程序标识符:" + identifier);
        EditorGUILayout.LabelField("返回安装应用程序的商店或包的名称:" + installerName);
        EditorGUILayout.LabelField("返回应用程序安装模式:" + installMode);
        EditorGUILayout.LabelField("区分WiFi连接与运营商网络:" + internetReachability);
        EditorGUILayout.LabelField("从命令行标志启动了 Unity:" + isBatchMode);
        EditorGUILayout.LabelField("当前的运行时平台是否为已知的控制台平台:" + isConsolePlatform);
        EditorGUILayout.LabelField("游戏是否在Unity编辑器中运行:" + isEditor);
        EditorGUILayout.LabelField("玩家当前是否具有焦点:" + isFocused);
        EditorGUILayout.LabelField("标识当前运行时平台是否是已知的移动平台:" + isMobilePlatform);
        EditorGUILayout.LabelField("在任何类型的内置播放器中调用时，或在播放模式:" + isPlaying);
        EditorGUILayout.LabelField("包含持久数据目录（只读）的路径:" + persistentDataPath);
        EditorGUILayout.LabelField("返回游戏运行平台:" + platform);
        EditorGUILayout.LabelField("返回应用程序产品名称:" + productName);
        EditorGUILayout.LabelField("当应用程序在后台时，播放器是否应该运行:" + runInBackground);
        EditorGUILayout.LabelField("返回在沙盒中运行的应用程序:" + sandboxType);
        EditorGUILayout.LabelField("StreamingAssets文件夹的路径:" + streamingAssetsPath);
        EditorGUILayout.LabelField("用户操作系统运行时所使用的语言:" + systemLanguage);
        EditorGUILayout.LabelField("指定Unity尝试渲染游戏的帧速率:" + targetFrameRate);
        EditorGUILayout.LabelField("包含临时数据/缓存目录的路径:" + temporaryCachePath);
        EditorGUILayout.LabelField("用于播放内容的 Unity 运行时版本:" + unityVersion);
        EditorGUILayout.LabelField("返回应用程序版本号:" + version);
        EditorGUILayout.LabelField("检查iOS和WebGL平台的摄像头/麦克风授权状态:" + hasUserAuthorization);
        EditorGUILayout.LabelField("请求用户授权访问设备敏感功能的重要方法:" + requestUserAuthorization);
        EditorGUILayout.LabelField("广告标识符:" + advertisingIdentifierResult);
        
        // 显示广告标识符请求结果
        if (isAdvertisingIdRequested)
        {
            EditorGUILayout.LabelField("广告标识符结果: " + advertisingId);
        }
        
        this.Repaint();
    }
    
    private void RequestAdvertisingIdentifier()
    {
        if (isAdvertisingIdRequested)
            return;
            
        isAdvertisingIdRequested = true;
        advertisingIdentifierResult = "请求中...";
        
        Application.RequestAdvertisingIdentifierAsync((string adId, bool trackingEnabled, string error) =>
        {
            advertisingId = adId;
            advertisingIdentifierResult = $"ID: {adId}, 跟踪启用: {trackingEnabled}, 错误: {error}";
            this.Repaint(); // 更新UI显示
        });
    }
}