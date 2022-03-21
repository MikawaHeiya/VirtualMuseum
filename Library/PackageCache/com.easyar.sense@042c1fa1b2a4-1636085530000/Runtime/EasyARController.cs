//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using System;
using UnityEditor;
using UnityEngine;

namespace easyar
{
    /// <summary>
    /// <para xml:lang="en"><see cref="MonoBehaviour"/> which controls EasyAR Sense initialization and some global settings.</para>
    /// <para xml:lang="zh">在场景中控制EasyAR Sense初始化以及一些全局设置的<see cref="MonoBehaviour"/>。</para>
    /// </summary>
    public class EasyARController : MonoBehaviour
    {
        /// <summary>
        /// <para xml:lang="en">If popup message will be displayed. All popup message from EasyAR Sense Unity Plugin is controlled by this flag.</para>
        /// <para xml:lang="zh">是否显示弹出消息。所有EasyAR Sense Unity Plugin的弹出消息都由这个flag控制。</para>
        /// </summary>
        public bool ShowPopupMessage = true;

        private static EasyARSettings settings;
        private static string exceptionMessage = string.Empty;
        private static bool initializeCalled;
        private bool hasError;

        internal event Action PostUpdate;

        /// <summary>
        /// <para xml:lang="en">Global <see cref="EasyARController"/>.</para>
        /// <para xml:lang="zh">全局<see cref="EasyARController"/>。</para>
        /// </summary>
        public static EasyARController Instance { get; private set; }

        /// <summary>
        /// <para xml:lang="en">EasyAR Sense initialize result, false if license key validation fails.</para>
        /// <para xml:lang="zh">EasyAR Sense初始化结果。如果license key验证失败会是false。</para>
        /// </summary>
        public static bool Initialized { get; private set; }

        /// <summary>
        /// <para xml:lang="en">Global Scheduler. Accessible after initialized.</para>
        /// <para xml:lang="zh">全局回调调度器。可以在初始化后访问。</para>
        /// </summary>
        /// <senseapi/>
        public static DelayedCallbackScheduler Scheduler { get; private set; }

        /// <summary>
        /// <para xml:lang="en">Global <see cref="EasyARSettings"/>.</para>
        /// <para xml:lang="zh">全局<see cref="EasyARSettings"/>。</para>
        /// </summary>
        public static EasyARSettings Settings
        {
            get
            {
                if (!settings)
                {
                    settings = Resources.Load<EasyARSettings>(settingsPath);
#if UNITY_EDITOR
                    if (!settings)
                    {
                        var settingsAsset = ScriptableObject.CreateInstance<EasyARSettings>();
                        if (settingsAsset != null)
                        {
                            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                            {
                                AssetDatabase.CreateFolder("Assets", "Resources");
                            }
                            if (!AssetDatabase.IsValidFolder("Assets/Resources/EasyAR"))
                            {
                                AssetDatabase.CreateFolder("Assets/Resources", "EasyAR");
                            }
                            AssetDatabase.CreateAsset(settingsAsset, "Assets/Resources/EasyAR/Settings.asset");
                            AssetDatabase.SaveAssets();
                        }
                        settings = Resources.Load<EasyARSettings>(settingsPath);
                    }
#endif
                    if (!settings)
                    {
                        if (Application.isPlaying)
                        {
                            Debug.LogError("Fail to load EasyAR settings resource");
                        }
                    }
                }
                return settings;
            }
        }
        private static string settingsPath { get { return "EasyAR/Settings"; } }

        /// <summary>
        /// <para xml:lang="en">Thread worker. Accessible after Awake.</para>
        /// <para xml:lang="zh">线程工作器。可以在Awake之后访问。</para>
        /// </summary>
        public ThreadWorker Worker { get; private set; }

        internal Display Display { get; private set; }

        /// <summary>
        /// <para xml:lang="en">EasyAR Sense initialization.</para>
        /// <para xml:lang="zh">初始化EasyAR Sense。</para>
        /// </summary>
        public static bool Initialize() => Initialize(Settings != null ? Settings.LicenseKey : string.Empty);

        /// <summary>
        /// <para xml:lang="en">EasyAR Sense initialization.</para>
        /// <para xml:lang="zh">初始化EasyAR Sense。</para>
        /// </summary>
        public static bool Initialize(string licenseKey)
        {
            try
            {
                Debug.Log("EasyAR Sense Unity Plugin Version " + EasyARVersion.FullVersion);
                initializeCalled = true;
                Initialized = false;
                exceptionMessage = string.Empty;
#if UNITY_EDITOR
                PackageChecker.CheckPath();
                PackageChecker.CheckOldAssets();
#endif
                Scheduler?.Dispose();
                Scheduler = new DelayedCallbackScheduler();
#if UNITY_EDITOR
                Log.setLogFuncWithScheduler(Scheduler, (LogLevel, msg) =>
                {
                    switch (LogLevel)
                    {
                        case LogLevel.Error:
                            Debug.LogError(msg);
                            break;
                        case LogLevel.Warning:
                            Debug.LogWarning(msg);
                            break;
                        case LogLevel.Info:
                            Debug.Log(msg);
                            break;
                        default:
                            break;
                    }
                });
#endif
                AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;
#if UNITY_ANDROID && !UNITY_EDITOR
                using (var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var easyarEngineClass = new AndroidJavaClass("cn.easyar.Engine"))
                {
                    var activityclassloader = currentActivity.Call<AndroidJavaObject>("getClass").Call<AndroidJavaObject>("getClassLoader");
                    if (activityclassloader == null)
                    {
                        Debug.Log("ActivityClassLoader is null");
                    }
                    easyarEngineClass.CallStatic("loadLibraries");
                    if (!easyarEngineClass.CallStatic<bool>("setupActivity", currentActivity))
                    {
                        return Initialized;
                    }
                }
#endif
                Initialized = Engine.initialize(licenseKey.Trim());
            }
            catch (Exception e)
            {
                Initialized = false;

                if (e is DllNotFoundException
#if UNITY_ANDROID && !UNITY_EDITOR
                    || e is AndroidJavaException
#endif
                    )
                {
                    exceptionMessage += "Fail to load EasyAR library." + Environment.NewLine;
                }
                if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
                {
                    Version version;
                    if (Version.TryParse(SystemInfo.operatingSystem.ToLower().Replace("mac", "").Replace("os", "").Replace("x", "").Trim(), out version))
                    {
                        if (version.Major < 10 || (version.Major == 10 && version.Minor < 15))
                        {
                            exceptionMessage += $"EasyAR Sense does not run on {SystemInfo.operatingSystem} (require 10.15 or later)." + Environment.NewLine;
                        }
                    }
                }
                exceptionMessage += "Exception caught in Initialize:" + Environment.NewLine;
                exceptionMessage += $"{e.GetType()}: {e.Message}";
                throw e;
            }
            return Initialized;
        }

        /// <summary>
        /// <para xml:lang="en">EasyAR Sense deinitialize.</para>
        /// <para xml:lang="en">This method has nothihng to do with resource dispose. Usually do not require to call manually. Use it if you want to initialize and deinitialize EasyAR multiple times.</para>
        /// <para xml:lang="zh">反初始化EasyAR Sense。</para>
        /// <para xml:lang="zh">这个方法与资源释放无关。通常不需要手动调用。只有在需要初始化与反初始化多次的时候调用。</para>
        /// </summary>
        public static void Deinitialize()
        {
#if UNITY_EDITOR
            Log.resetLogFunc();
#endif
            Scheduler?.Dispose();
            Scheduler = null;
            Initialized = false;
            initializeCalled = false;
            AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void AttemptInitializeOnLoad()
        {
            if (!Settings || !Settings.InitializeOnStartup) { return; }
            Initialize();
        }

        private void Awake()
        {
            Instance = this;
            Display = new Display();
            Worker = new ThreadWorker();
            hasError = !Initialized;
            if (hasError)
            {
                ShowErrorMessage();
            }
            if (Scheduler != null)
            {
                while (Scheduler.runOne())
                {
                }
            }
        }

        private void Update()
        {
            if (!Initialized)
            {
                if (!hasError)
                {
                    hasError = true;
                    ShowErrorMessage();
                }
                return;
            }
            if (!string.IsNullOrEmpty(Engine.errorMessage()))
            {
                hasError = true;
                ShowErrorMessage();
                Initialized = false;
            }

            if (Scheduler != null)
            {
                while (Scheduler.runOne())
                {
                }
            }
            PostUpdate?.Invoke();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                Engine.onPause();
            }
            else
            {
                Engine.onResume();
            }
        }

        private void OnDestroy()
        {
            Worker.Dispose();
            Display.Dispose();
        }

        private static void OnDomainUnload(object sender, EventArgs args)
        {
            try
            {
                Deinitialize();
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        private void ShowErrorMessage()
        {
            var message = "";
            if (!initializeCalled && Settings)
            {
                message = (Settings.InitializeOnStartup ? "" : $"InitializeOnStartup is FALSE in EasyAR Settings. ") +
                    $"{nameof(EasyARController)}.Initialize must be called before any EasyAR component is running.";
            }
            else if (!string.IsNullOrEmpty(exceptionMessage))
            {
                message = exceptionMessage;
            }
            else
            {
                message = Engine.errorMessage() + Environment.NewLine;
                if (!Settings || string.IsNullOrEmpty(Settings.LicenseKey))
                {
                    message += "License Key is empty" + Environment.NewLine +
                        "Get from EasyAR Develop Center (www.easyar.com) -> SDK Authorization" +
                        (Application.isEditor ? " and fill it into asset using menu: EasyAR -> Sense -> Configuration." : "");
                }
                else
                {
                    var key = Settings.LicenseKey;
                    if (key.Length > 10)
                    {
                        key = key.Substring(0, 5) + "..." + key.Substring(key.Length - 5, 5);
                    }
                    message += $"License key in use: {key}";
                }
            }
            GUIPopup.EnqueueMessage(message, 10000, true);
        }
    }
}
