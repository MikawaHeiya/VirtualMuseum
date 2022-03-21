//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

#if !EASYAR_ENABLE_SENSE
#error DO NOT DECOMPRESS .TGZ FILE!
#error EasyAR package was not correctly imported by UNITY PACKAGE MANAGER! Please read online documents for how to use.
#warning If you are breaking package on purpose, just comment out these errors. But NO official support will be provided for possible building or runtime errors caused by this kind of usage.
#endif

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace easyar
{
    public static class PackageChecker
    {
#if UNITY_EDITOR
        static readonly string[] oldScripts = new string[] {
            "EasyAR/Scripts/csapi.cs",
            "EasyAR/Scripts/EasyAR.Unity.dll",
        };
        static readonly string[] oldNativePluginFiles = new string[] {
            "Plugins/Android/libs/arm64-v8a/libEasyAR.so",
            "Plugins/Android/libs/arm64-v8a/libEasyARUnity.so",
            "Plugins/Android/libs/armeabi-v7a/libEasyAR.so",
            "Plugins/Android/libs/armeabi-v7a/libEasyARUnity.so",
            "Plugins/Android/libs/EasyAR.jar",
            "Plugins/iOS/EasyARAppController.mm",
            "Plugins/iOS/libEasyARUnity.a",
            "Plugins/x64/bin/EasyAR.dll",
            "Plugins/x86/bin/EasyAR.dll",
            "Plugins/x86/EasyARUnity.dll",
            "Plugins/x86_64/EasyARUnity.dll",
        };
        static readonly string[] oldNativePluginFolders = new string[] {
            "Plugins/EasyAR.bundle",
            "Plugins/EasyARUnity.bundle",
            "Plugins/iOS/easyar.framework",
        };
        static readonly string[] oldNativePluginFilesWarn = new string[] {
            "Plugins/Android/libs/arcore-classes.jar",
            "Plugins/Android/libs/arm64-v8a/libarcore_sdk_c.so",
            "Plugins/Android/libs/armeabi-v7a/libarcore_sdk_c.so",
        };

        [InitializeOnLoadMethod]
        static void OnLoadCheck()
        {
            CheckPath();
            CheckOldAssets();
        }

        public static void CheckPath([System.Runtime.CompilerServices.CallerFilePath] string filepath = "")
        {
            if (!AssetDatabase.GUIDToAssetPath("977ba8ebe7b21294b84bb418b677fd63").StartsWith("Packages") || filepath.Replace("\\", "/").Contains(Application.dataPath.Replace("\\", "/")))
            {
                Debug.LogError("DO NOT DECOMPRESS .TGZ FILE!");
                throw new Exception("EasyAR package was not correctly imported by UNITY PACKAGE MANAGER! Please read online documents for how to use.");
            }
        }

        public static void CheckOldAssets()
        {
            var message = "Old EasyAR package files detected at Assets/{0}. These files are not compatible with current package. Please remove all files extracted from old packages!";
            foreach (var _ in oldScripts.Where(file => File.Exists(Path.Combine(Application.dataPath, file))))
            {
                throw new Exception(string.Format(message, "EasyAR"));
            }
            foreach (var file in oldNativePluginFiles.Where(file => File.Exists(Path.Combine(Application.dataPath, file))))
            {
                throw new Exception(string.Format(message, file));
            }
            foreach (var file in oldNativePluginFolders.Where(file => Directory.Exists(Path.Combine(Application.dataPath, file))))
            {
                throw new Exception(string.Format(message, file));
            }
            foreach (var file in oldNativePluginFilesWarn.Where(file => File.Exists(Path.Combine(Application.dataPath, file))))
            {
                Debug.LogWarning($"Possible old EasyAR package files detected at Assets/{file}. Please remove this file if it was from EasyAR packages, except you are doing this on purpose.");
                break;
            }
        }
#endif
    }
}
