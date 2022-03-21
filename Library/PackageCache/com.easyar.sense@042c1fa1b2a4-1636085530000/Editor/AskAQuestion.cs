//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace easyar
{
    public class AskAQuestion : EditorWindow
    {
        bool opened;
        string lang;
        Vector2 leftScrollPos;
        Vector2 leftScrollPos2;
        Vector2 rightScrollPos;
        string deviceInfo = string.Empty;
        string easyarInfo;
        string hostInfo;
        bool isHostWin = false;
        bool isDoneSample;
        bool isDoneDocument;
        bool isDoneLog;
        bool isDoneLatest;
        bool isDoneAll;
        bool isSampleSimple;
        string sampleName;
        readonly bool[] features = new bool[(int)Feature.None];
        readonly bool[] platforms = new bool[(int)PlatformType.None];
        PlatformType platform = PlatformType.None;
        const string deviceCodeEditor = @"
var deviceInfo = SystemInfo.operatingSystem + Environment.NewLine + $""{easyar.Engine.name()} Version {easyar.Engine.versionString()}"";
Debug.LogWarning(deviceInfo);
";
        const string deviceCode = @"
var deviceModel = string.Empty;
#if UNITY_ANDROID && !UNITY_EDITOR
if (Application.platform == RuntimePlatform.Android)
{
    try
    {
        using (var buildClass = new AndroidJavaClass(""android.os.Build""))
        {
            deviceModel = $""(Device = {buildClass.GetStatic<string>(""DEVICE"")}, Model = {buildClass.GetStatic<string>(""MODEL"")})"";
        }
    }
    catch (Exception e) { deviceModel = e.Message; }
}
#endif
var deviceInfo = $""System: {SystemInfo.operatingSystem}"" + Environment.NewLine + $""Device Model: {SystemInfo.deviceModel} {deviceModel}"" + Environment.NewLine + $""{easyar.Engine.name()} Version {easyar.Engine.versionString()}"";
Debug.LogWarning(deviceInfo);
";

        enum Pipeline
        {
            Builtin,
            URP,
            Unsupported,
        }

        enum PlatformType
        {
            Editor_Win,
            Editor_Mac,
            Windows,
            MacOS,
            Android,
            iOS,
            Nreal,
            Other,
            WebGL,
            Hololens,
            None,
        }

        enum Feature
        {
            ImageTracking,
            ObjectTracking,
            CloudRecognition,
            MotionTracking,
            SurfaceTracking,
            VideoRecording,
            SparseSpatialMap,
            DenseSpatialMap,
            CloudSpatialMap,
            Other,
            None,
        }

        [MenuItem("EasyAR/Sense/Ask a Question", priority = 100)]
        private static void DocumentQaAEn()
        {
            var win = GetWindow<AskAQuestion>(true, "", true);
            if (!win.opened)
            {
                win.minSize = new Vector2(1100, 720);
                win.opened = true;
            }
            win.lang = "en";
            win.titleContent = new GUIContent("Ask a Question");
        }

        [MenuItem("EasyAR/Sense/提问", priority = 100)]
        private static void DocumentQaAZh()
        {
            var win = GetWindow<AskAQuestion>(true, "", true);
            if (!win.opened)
            {
                win.minSize = new Vector2(1100, 720);
                win.opened = true;
            }
            win.lang = "zh";
            win.titleContent = new GUIContent("提问");
        }

        private void OnEnable()
        {
#if UNITY_EDITOR_WIN
            isHostWin = true;
#endif

            var pipeline = Pipeline.Unsupported;
            if (GraphicsSettings.currentRenderPipeline == null)
            {
                pipeline = Pipeline.Builtin;
            }
#if EASYAR_URP_ENABLE
            else if (GraphicsSettings.currentRenderPipeline is UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset)
            {
                pipeline = Pipeline.URP;
            }
#endif
            var packages = string.Empty;
#if EASYAR_ENABLE_CLOUDSPATIALMAP
            packages += "<Cloud SpatialMap>";
#endif
#if EASYAR_ARFOUNDATION_ENABLE
            packages += "<AR Foundation>";
#endif
#if EASYAR_HWARENGINE_ENABLE
            packages += "<Huawei AR Engine>";
#endif
#if EASYAR_ENABLE_NREAL
            packages += "<Nreal>";
#endif

            easyarInfo = $"EasyAR Sense Unity Plugin ({EasyARVersion.FullVersion})" + Environment.NewLine +
                $"EasyAR Sense ({Engine.versionString()})";

            hostInfo = SystemInfo.operatingSystem + Environment.NewLine +
                Engine.name() + Environment.NewLine +
                $"Unity {Application.unityVersion}" + Environment.NewLine +
                $"Pipeline: {pipeline}" + Environment.NewLine +
                $"Extra Packages: {packages}";
        }

        private void OnGUI()
        {
            var essentialInfo = "EasyAR Products:" + Environment.NewLine +
                easyarInfo + Environment.NewLine +
                Environment.NewLine +
                "Development Environment:" + Environment.NewLine +
                hostInfo + Environment.NewLine +
                Environment.NewLine +
                "Runtime Environment:" + Environment.NewLine +
                $"Platform: {platform}" + Environment.NewLine;

            if (platform < PlatformType.WebGL)
            {
                essentialInfo += deviceInfo + Environment.NewLine;
            }

            var feat = Environment.NewLine + "Features: ";
            for (int i = 0; i < features.Length; ++i)
            {
                if (features[i])
                {
                    feat += $"<{(Feature)i}>";
                }
            }
            essentialInfo += feat + Environment.NewLine;

            essentialInfo += Environment.NewLine +
                $"Break down:" + Environment.NewLine +
                $"Tried Latest version: {isDoneLatest}" + Environment.NewLine +
                $"Read Document: {isDoneDocument}" + Environment.NewLine +
                $"Read Log: {isDoneLog}" + Environment.NewLine +
                $"Tried Sample: {isDoneSample} ({sampleName}{(isSampleSimple ? ", but is too simple" : "")})" + Environment.NewLine +
                Environment.NewLine;

            using (_ = new GUILayout.VerticalScope())
            {
                using (_ = new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(10);
                    using (_ = new GUILayout.VerticalScope())
                    {
                        GUILayout.Space(10);
                        GUILayout.Label(lang == "en" ? "EasyAR Products" : "EasyAR 产品");
                        GUILayout.TextArea(easyarInfo);

                        GUILayout.Space(10);
                        GUILayout.Label(lang == "en" ? "Development Environment" : "开发环境");
                        using (var scroll = new GUILayout.ScrollViewScope(leftScrollPos2, false, true, GUILayout.Height(50)))
                        {
                            leftScrollPos2 = scroll.scrollPosition;
                            GUILayout.TextArea(hostInfo, GUILayout.ExpandHeight(true));
                        }

                        GUILayout.Space(10);
                        GUILayout.Label(lang == "en" ? "Runtime Environment (Please choose one of the following options)" : "运行环境（单选）");
                        using (_ = new GUILayout.VerticalScope(GUI.skin.box))
                        {
                            using (_ = new GUILayout.HorizontalScope())
                            {
                                foreach (var type in new PlatformType[] { PlatformType.Editor_Win, PlatformType.Editor_Mac, PlatformType.Windows, PlatformType.MacOS, PlatformType.Android, PlatformType.iOS })
                                {
                                    platforms[(int)type] = GUILayout.Toggle(platforms[(int)type], type.ToString(), GUILayout.Width(80));
                                }
                            }
                            using (_ = new GUILayout.HorizontalScope())
                            {
                                foreach (var type in new PlatformType[] { PlatformType.Nreal, PlatformType.WebGL, PlatformType.Hololens, PlatformType.Other })
                                {
                                    platforms[(int)type] = GUILayout.Toggle(platforms[(int)type], type.ToString(), GUILayout.Width(80));
                                }
                            }
                            SelectPlatform();
                            foreach (var type in new PlatformType[] { PlatformType.WebGL, PlatformType.Hololens })
                            {
                                if (platforms[(int)type])
                                {
                                    using (_ = new ColorScope(Color.red))
                                    {
                                        GUILayout.Label(lang == "en" ? $"({type} is not supported in this version)" : $"（这个版本不支持 {type}）");
                                    }
                                    break;
                                }
                            }

                            if (platform < PlatformType.WebGL)
                            {
                                var code = deviceCode;
                                var label = lang == "en" ? "Device Infomation (Please run code on device and paste result bellow)" : "设备信息（请在设备上运行代码并填写结果）";
                                if (platform < PlatformType.Android)
                                {
                                    code = deviceCodeEditor;
                                    if ((isHostWin && (platform == PlatformType.Editor_Win || platform == PlatformType.Windows)) || (!isHostWin && (platform == PlatformType.Editor_Mac || platform == PlatformType.MacOS)))
                                    {
                                        label = lang == "en" ? "Device Infomation (Please confirm, or run code on device and paste result bellow)" : "设备信息（请确认，或在设备上运行代码并填写结果）";
                                    }
                                }
                                GUILayout.Space(10);
                                using (_ = new GUILayout.HorizontalScope())
                                {
                                    GUILayout.Label(label);
                                    if (GUILayout.Button(new GUIContent(lang == "en" ? "Copy Code" : "复制代码", code), GUILayout.Width(100)))
                                    {
                                        GUIUtility.systemCopyBuffer = code;
                                    }
                                }
                                using (var scroll = new GUILayout.ScrollViewScope(leftScrollPos, false, true, GUILayout.Height(50)))
                                {
                                    leftScrollPos = scroll.scrollPosition;
                                    deviceInfo = GUILayout.TextArea(deviceInfo, GUILayout.ExpandHeight(true));
                                }
                            }
                        }


                        GUILayout.Space(10);
                        GUILayout.Label(lang == "en" ? "Features in use (Please choose one or more of the following options)" : "使用的功能（复选）");
                        using (_ = new GUILayout.VerticalScope(GUI.skin.box))
                        {
                            using (_ = new GUILayout.HorizontalScope())
                            {
                                foreach (var type in new Feature[] { Feature.ImageTracking, Feature.CloudRecognition, Feature.ObjectTracking, Feature.VideoRecording })
                                {
                                    features[(int)type] = GUILayout.Toggle(features[(int)type], type.ToString(), GUILayout.Width(130));
                                }
                            }
                            using (_ = new GUILayout.HorizontalScope())
                            {
                                foreach (var type in new Feature[] { Feature.MotionTracking, Feature.SparseSpatialMap, Feature.DenseSpatialMap, Feature.SurfaceTracking })
                                {
                                    features[(int)type] = GUILayout.Toggle(features[(int)type], type.ToString(), GUILayout.Width(130));
                                }
                            }
                            using (_ = new GUILayout.HorizontalScope())
                            {
                                foreach (var type in new Feature[] { Feature.CloudSpatialMap, Feature.Other })
                                {
                                    features[(int)type] = GUILayout.Toggle(features[(int)type], type.ToString(), GUILayout.Width(130));
                                }
                            }
                        }
                        if (platform < PlatformType.WebGL && features.Contains(true))
                        {
                            GUILayout.Space(10);
                            GUILayout.Label(lang == "en" ? "Breakdown the Problem" : "问题分解");
                            using (_ = new GUILayout.VerticalScope(GUI.skin.box))
                            {
                                isDoneLatest = GUILayout.Toggle(isDoneLatest, lang == "en" ? "I have tried latest EasyAR Sense Unity Plugin release" : "我已试过最新版本的EasyAR Sense Unity插件");
                                if (isDoneLatest)
                                {
                                    using (_ = new ColorScope(Color.yellow))
                                    {
                                        GUILayout.Label(lang == "en" ? $"(There are usually bug fixes and new features in new versions, please consider upgrade first)" : $"（新版本中通常包含bug修复及新功能，建议先升级到最新版本尝试）");
                                    }
                                }

                                GUILayout.Space(10);
                                using (_ = new GUILayout.HorizontalScope())
                                {
                                    isDoneDocument = GUILayout.Toggle(isDoneDocument, lang == "en" ? "I have read documents" : "我已阅读过文档");
                                    if (GUILayout.Button(lang == "en" ? "Documents" : "查看文档", GUILayout.Width(100)))
                                    {
                                        Application.OpenURL($"https://www.easyar.{(lang == "en" ? "com" : "cn")}/view/support.html");
                                    }
                                    if (GUILayout.Button(lang == "en" ? "FAQ" : "常见问题", GUILayout.Width(100)))
                                    {
                                        Application.OpenURL($"https://www.easyar.{(lang == "en" ? "com" : "cn")}/view/question.html");
                                    }
                                }
                                if (isDoneDocument)
                                {
                                    using (_ = new ColorScope(Color.yellow))
                                    {
                                        GUILayout.Label(lang == "en" ? $"(Please read documents and FAQs if you are not familar with EasyAR)" : $"（如果你对EasyAR不了解，建议查看文档及常见问题）");
                                    }
                                }

                                GUILayout.Space(10);
                                using (_ = new GUILayout.HorizontalScope())
                                {
                                    isDoneLog = GUILayout.Toggle(isDoneLog, lang == "en" ? "I have read system and Unity logs" : "我已阅读过系统及Unity日志");
                                    using (_ = new DisabledScope())
                                    {
                                        if (platform == PlatformType.Android || platform == PlatformType.Nreal)
                                        {
                                            GUILayout.Label(lang == "en" ? "Please try: `adb logcat`, do not read Unity tag only" : "请尝试：`adb logcat`，不要只看Unity标签");
                                        }
                                        else if (platform == PlatformType.iOS)
                                        {
                                            GUILayout.Label(lang == "en" ? "Please try: `XCode` or `Console` App" : "请尝试：`XCode` 或 `控制台`应用");
                                        }
                                    }
                                }
                                if (isDoneLog)
                                {
                                    using (_ = new ColorScope(Color.yellow))
                                    {
                                        GUILayout.Label(lang == "en" ? $"(Please attach full log when ask a question)" : $"（建议在提问时提供完整日志）");
                                    }
                                }

                                GUILayout.Space(10);
                                isDoneSample = GUILayout.Toggle(isDoneSample, lang == "en" ? "I have tried to reproduce the problem in samples (inside an empty Unity project)" : "我已尝试过在Sample中复现问题（使用空的Unity工程）");
                                if (isDoneSample)
                                {
                                    using (_ = new GUILayout.HorizontalScope())
                                    {
                                        GUILayout.Space(20);
                                        using (_ = new GUILayout.VerticalScope())
                                        {
                                            using (_ = new GUILayout.HorizontalScope())
                                            {
                                                GUILayout.Label(lang == "en" ? "Sample Name" : "Sample 名称", GUILayout.Width(100));
                                                sampleName = GUILayout.TextField(sampleName);
                                            }
                                            isSampleSimple = GUILayout.Toggle(isSampleSimple, lang == "en" ? "Samples are too simple to reproduce my problem" : "Sample 太简单，无法用于复现问题");
                                            if (!isSampleSimple)
                                            {
                                                using (_ = new ColorScope(Color.yellow))
                                                {
                                                    GUILayout.Label(lang == "en" ? $"(Please describe how to reproduce the problem in samples when ask a question)" : $"（建议在提问时描述如何在Sample中复现问题）");
                                                }
                                            }
                                        }
                                    }
                                }

                                GUILayout.Space(10);
                            }

                            isDoneAll = isDoneSample && isDoneDocument && isDoneLog && isDoneLatest;
                            isDoneAll = GUILayout.Toggle(isDoneAll, lang == "en" ? "I have tried all above methods, but the problem is still there" : "我已尝试上面所有方法，但问题仍然存在");
                            if (isDoneAll)
                            {
                                isDoneSample = true;
                                isDoneDocument = true;
                                isDoneLog = true;
                                isDoneLatest = true;
                            }
                        }
                    }
                    using (_ = new GUILayout.VerticalScope())
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(">", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
                        GUILayout.FlexibleSpace();
                    }
                    using (_ = new GUILayout.VerticalScope())
                    {
                        GUILayout.Space(5);
                        using (_ = new GUILayout.HorizontalScope())
                        {
                            using (_ = new GUILayout.VerticalScope())
                            {
                                GUILayout.Space(5);
                                GUILayout.Label(lang == "en" ? "Essential Infomation" : "基本信息");
                            }
                            if (GUILayout.Button(lang == "en" ? "Copy" : "复制", GUILayout.Width(100), GUILayout.Height(20)))
                            {
                                GUIUtility.systemCopyBuffer = essentialInfo;
                            }
                        }
                        using (var scroll = new GUILayout.ScrollViewScope(rightScrollPos, false, true, GUILayout.Height(620)))
                        using (_ = new DisabledScope())
                        {
                            rightScrollPos = scroll.scrollPosition;
                            GUILayout.TextArea(essentialInfo, GUILayout.ExpandHeight(true));
                        }
                        GUILayout.Label(lang == "en" ? "Please provide above information when ask a question" : "请在提问时提供这些信息");
                    }
                    GUILayout.Space(10);
                }

                GUILayout.FlexibleSpace();

                using (_ = new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("中/EN", GUILayout.Width(60), GUILayout.Height(20)))
                    {
                        lang = lang == "en" ? "zh" : "en";
                        titleContent = new GUIContent(lang == "en" ? "Ask a Question" : "提问");
                    }
                    GUILayout.FlexibleSpace();
                    using (_ = new DisabledScope(platform >= PlatformType.WebGL || !isDoneAll))
                    {
                        if (GUILayout.Button(lang == "en" ? "Goto EasyAR Q&A" : "前往EasyAR问答", GUILayout.Width(400), GUILayout.Height(30)))
                        {
                            GUIUtility.systemCopyBuffer = essentialInfo;
                            Application.OpenURL($"https://answers.easyar.{(lang == "en" ? "com" : "cn")}/");
                        }
                    }
                    GUILayout.FlexibleSpace();
                }
                GUILayout.Space(10);
            }

            void SelectPlatform()
            {
                var hasPlatform = (int)platform < platforms.Length && platforms[(int)platform];
                for (int i = 0; i < platforms.Length; ++i)
                {
                    if (platforms[i] && i != (int)platform)
                    {
                        platform = (PlatformType)i;
                        if (platform < PlatformType.Android)
                        {
                            if ((isHostWin && (platform == PlatformType.Editor_Win || platform == PlatformType.Windows)) || (!isHostWin && (platform == PlatformType.Editor_Mac || platform == PlatformType.MacOS)))
                            {
                                deviceInfo = SystemInfo.operatingSystem + Environment.NewLine +
                                    $"{easyar.Engine.name()} Version {easyar.Engine.versionString()}";
                            }
                            else
                            {
                                deviceInfo = "System: ? (Version ?)" + Environment.NewLine +
                                    $"EasyAR Sense Version in Log: ?";
                            }
                        }
                        else if (platform < PlatformType.WebGL)
                        {
                            deviceInfo = "System: ? (Version ?)" + Environment.NewLine +
                                "Device Model: ?" + Environment.NewLine +
                                $"EasyAR Sense Version in Log: ?";
                        }
                        hasPlatform = true;
                        break;
                    }
                }
                if (!hasPlatform)
                {
                    platform = PlatformType.None;
                }
                for (int i = 0; i < platforms.Length; ++i)
                {
                    platforms[i] = (int)platform == i;
                }
            }
        }

        private class DisabledScope : IDisposable
        {
            private bool enabled;

            public DisabledScope(bool disable = true)
            {
                enabled = GUI.enabled;
                GUI.enabled = !disable;
            }

            public void Dispose()
            {
                GUI.enabled = enabled;
            }
        }

        private class ColorScope : IDisposable
        {
            private Color color;

            public ColorScope(Color c)
            {
                color = GUI.color;
                GUI.color = c;
            }

            public void Dispose()
            {
                GUI.color = color;
            }
        }
    }
}
