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
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace easyar
{
    class BuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 1;

        static string[] shaderNames = new string[] {
            "EasyAR/CameraImage_RGB",
            "EasyAR/CameraImage_BGR",
            "EasyAR/CameraImage_Gray",
            "EasyAR/CameraImage_YUV_I420_YV12",
            "EasyAR/CameraImage_YUV_NV12",
            "EasyAR/CameraImage_YUV_NV21",
        };

        public void OnPreprocessBuild(BuildReport report)
        {
            try
            {
                PackageChecker.CheckPath();
                PackageChecker.CheckOldAssets();
                if (AssetDatabase.IsValidFolder("Assets/HiddenEasyAR"))
                {
                    AssetDatabase.DeleteAsset("Assets/HiddenEasyAR");
                }

                var permissions = new EasyARSettings.Permission();
                if (EasyARController.Settings)
                {
                    permissions = EasyARController.Settings.Permissions;
                }

                if (report.summary.platform == BuildTarget.iOS)
                {
#if UNITY_2020_2_OR_NEWER
                    var videoRecording = false;
#else
                    var videoRecording = permissions.VideoRecording;
#endif
                    var hasNR = true;
#if EASYAR_USE_SEPERATE_BINDING
#error seperate binding cannot be used with this package
#else
                    PrepareOptionalNativePlugin($"Packages/com.easyar.sense/Runtime/Binding/Apple/iOS/ios-arm64_armv7/easyar.framework", videoRecording || !hasNR);
                    if (hasNR)
                    {
                        PrepareOptionalNativePlugin($"Packages/com.easyar.sense/Runtime/BindingNR/Apple/iOS/ios-arm64_armv7/easyar.framework", !videoRecording);
                    }
#endif

                    if (permissions.CameraDevice && string.IsNullOrEmpty(PlayerSettings.iOS.cameraUsageDescription))
                    {
                        throw new BuildFailedException("EasyAR Camera Device requires a Camera Usage Description (Player Settings > iOS > Other Settings > Camera Usage Description). You can turn this feature off if not used (EasyAR > Sense > Configuration > Permissions > Camera Device).");
                    }
                    if (videoRecording && string.IsNullOrEmpty(PlayerSettings.iOS.microphoneUsageDescription))
                    {
                        Debug.LogWarning("EasyAR Video Recording requires a Microphone Usage Description (Player Settings > iOS > Other Settings > Microphone Usage Description). You can turn this feature off if not used (EasyAR > Sense > Configuration > Permissions > Video Recording).");
                    }
#if EASYAR_ENABLE_CLOUDSPATIALMAP
                    if (permissions.CloudSpatialMap && string.IsNullOrEmpty(PlayerSettings.iOS.locationUsageDescription))
                    {
                        throw new BuildFailedException("EasyAR Cloud SpatialMap requires a Location Usage Description (Player Settings > iOS > Other Settings > Location Usage Description). You can turn this feature off if not used (EasyAR > Sense > Configuration > Permissions > Cloud Spatial Map).");
                    }
#endif
                }
                if (report.summary.platform == BuildTarget.Android)
                {
                    PrepareOptionalNativePlugin("Packages/com.easyar.sense/Runtime/Android/permission.CAMERA.aar", permissions.CameraDevice);
                    PrepareOptionalNativePlugin("Packages/com.easyar.sense/Runtime/Android/permission.RECORD_AUDIO.aar", permissions.VideoRecording);
#if EASYAR_ENABLE_CLOUDSPATIALMAP
                    PrepareOptionalNativePlugin("Packages/com.easyar.spatialmap/Runtime/Android/permission.ACCESS_FINE_LOCATION.aar", permissions.CloudSpatialMap);
#endif
                    var arcore = EasyARSettings.ARCoreType.ARFoundationOrOptional;
#if !UNITY_2020_1_OR_NEWER
                    var arcoreForAndroid11 = false;
#else
                    var arcoreForAndroid11 = true;
#endif
                    if (EasyARController.Settings)
                    {
                        arcore = EasyARController.Settings.ARCoreSDK;
#if !UNITY_2020_1_OR_NEWER
                        arcoreForAndroid11 = EasyARController.Settings.ARCoreForAndroid11;
#endif
                    }
#if EASYAR_ENABLE_XRMANAGEMENT && EASYAR_ENABLE_UNITYARCORE
                    var generalSettings = UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
                    if (generalSettings)
                    {
#if EASYAR_ENABLE_XRMANAGEMENT_3
                        foreach (var loader in generalSettings.Manager.loaders)
#else
                        foreach (var loader in generalSettings.Manager.activeLoaders)
#endif
                        {
                            if (loader is UnityEngine.XR.ARCore.ARCoreLoader)
                            {
                                if (arcore == EasyARSettings.ARCoreType.Optional || arcore == EasyARSettings.ARCoreType.Required)
                                {
                                    Debug.LogWarning($"ARCoreLoader is active, ARCoreSDK (current = {arcore}) is forced to {EasyARSettings.ARCoreType.External}");
                                }
                                arcore = EasyARSettings.ARCoreType.External;
                                break;
                            }
                        }
                    }
#endif
                    if (arcore == EasyARSettings.ARCoreType.ARFoundationOrOptional)
                    {
                        arcore = EasyARSettings.ARCoreType.Optional;
                    }
                    PrepareOptionalNativePlugin("Packages/com.easyar.sense/Runtime/Android/com.google.ar.core-1.6.0.aar", arcore != EasyARSettings.ARCoreType.External);
                    PrepareOptionalNativePlugin("Packages/com.easyar.sense/Runtime/Android/com.google.ar.core-queries.aar", arcore != EasyARSettings.ARCoreType.External && arcoreForAndroid11);
                    PrepareOptionalNativePlugin("Packages/com.easyar.sense/Runtime/Android/com.google.ar.core-required.aar", arcore == EasyARSettings.ARCoreType.Required);
                    PrepareOptionalNativePlugin("Packages/com.easyar.sense/Runtime/Android/com.google.ar.core-optional.aar", arcore == EasyARSettings.ARCoreType.Optional);
                }
                AddShader();
            }
            catch (Exception e)
            {
                throw new BuildFailedException(e);
            }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            try
            {
                RemoveShader();
                if (AssetDatabase.IsValidFolder("Assets/HiddenEasyAR"))
                {
                    AssetDatabase.DeleteAsset("Assets/HiddenEasyAR");
                }

                if (report.summary.platform == BuildTarget.iOS)
                {
#if UNITY_IOS
                    var proj = new PBXProject();
                    var projPath = PBXProject.GetPBXProjectPath(report.summary.outputPath);
                    proj.ReadFromFile(projPath);
                    proj.SetBuildProperty(proj.GetUnityFrameworkTargetGuid(), "ENABLE_BITCODE", "NO");
                    proj.SetBuildProperty(proj.GetUnityMainTargetGuid(), "ENABLE_BITCODE", "NO");
                    proj.WriteToFile(projPath);
#endif
                }
            }
            catch (Exception e)
            {
                throw new BuildFailedException(e);
            }
        }

        private void PrepareOptionalNativePlugin(string asset, bool enable)
        {
            var plugin = AssetImporter.GetAtPath(asset) as PluginImporter;
            plugin.SetIncludeInBuildDelegate(path => enable);
        }

        private static void AddShader()
        {
            var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();
            var changed = false;

            foreach (var shaderName in shaderNames)
            {
                var shader = Shader.Find(shaderName);
                if (!shader)
                {
                    throw new Exception($"Cannot find shader '{shaderName}'");
                }
                if (!preloadedAssets.Where(a => shader.Equals(a)).Any())
                {
                    preloadedAssets.Add(shader);
                    changed = true;
                }
            }
            if (changed)
            {
                PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
            }
        }

        private static void RemoveShader()
        {
            var shaders = shaderNames.Select(s => Shader.Find(s));
            PlayerSettings.SetPreloadedAssets(PlayerSettings.GetPreloadedAssets().Where(a => !shaders.Contains(a)).ToArray());
        }
    }
}
