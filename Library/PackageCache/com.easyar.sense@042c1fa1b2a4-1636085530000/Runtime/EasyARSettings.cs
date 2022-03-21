//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using System;
using UnityEngine;

namespace easyar
{
    /// <summary>
    /// <para xml:lang="en">EasyAR Sense Unity Plugin settings.</para>
    /// <para xml:lang="zh">EasyAR Sense Unity Plugin的配置信息。</para>
    /// </summary>
    public class EasyARSettings : ScriptableObject
    {
        /// <summary>
        /// <para xml:lang="en">EasyAR Sense License Key. Used for validation of EasyAR Sense functions. Please visit https://www.easyar.com for more details.</para>
        /// <para xml:lang="zh">EasyAR Sense License Key。用于验证EasyAR Sense内部各种功能是否可用。详见 https://www.easyar.cn 。</para>
        /// </summary>
        [Tooltip("EasyAR Sense License Key. Used for validation of EasyAR Sense functions. Please visit https://www.easyar.com for more details.")]
        [HideInInspector, SerializeField]
        [TextArea(1, 10)]
        public string LicenseKey = string.Empty;
        /// <summary>
        /// <para xml:lang="en">Initialize EasyAR Sense on startup. EasyAR initialize does not result extra resource usages, so usually you can keep this option on.</para>
        /// <para xml:lang="zh">在启动时初始化EasyAR。EasyAR的初始化不会造成额外的资源消耗，因此通常可以保持这个选项打开。</para>
        /// </summary>>
        [Tooltip("Initialize EasyAR Sense on startup. EasyAR initialize does not result extra resource usages, so usually you can keep this option on.")]
        public bool InitializeOnStartup = true;
        /// <summary>
        /// <para xml:lang="en">Configuration for application permissions. Permissions must be turned on for the feature in use.</para>
        /// <para xml:lang="zh">应用权限配置。对应功能的权限必须打开。</para>
        /// </summary>
        [Tooltip("Configuration for application permissions. Permissions must be turned on for the feature in use.")]
        public Permission Permissions = new Permission();
        /// <summary>
        /// <para xml:lang="en"><see cref="Gizmos"/> configuration for <see cref="ImageTarget"/> and <see cref="ObjectTarget"/>.</para>
        /// <para xml:lang="zh"><see cref="ImageTarget"/> 和 <see cref="ObjectTarget"/>的<see cref="Gizmos"/>配置。</para>
        /// </summary>
        [Tooltip("Gizmos configuration for ImageTarget and ObjectTarget.")]
        public TargetGizmoConfig GizmoConfig = new TargetGizmoConfig();
        /// <summary>
        /// <para xml:lang="en">Global spatial map service config.</para>
        /// <para xml:lang="zh">全局稀疏地图服务器配置。</para>
        /// </summary>
        [Tooltip("Global spatial map service config.")]
        public SparseSpatialMapWorkerFrameFilter.SpatialMapServiceConfig GlobalSpatialMapServiceConfig = new SparseSpatialMapWorkerFrameFilter.SpatialMapServiceConfig();
        /// <summary>
        /// <para xml:lang="en">Global cloud recognizer service config.</para>
        /// <para xml:lang="zh">全局云识别服务器配置。</para>
        /// </summary>
        [Tooltip("Global cloud recognizer service config.")]
        public CloudRecognizerFrameFilter.CloudRecognizerServiceConfig GlobalCloudRecognizerServiceConfig = new CloudRecognizerFrameFilter.CloudRecognizerServiceConfig();
        /// <summary>
        /// <para xml:lang="en">Global cloud localizer serveice config (used in Cloud SpatialMap).</para>
        /// <para xml:lang="zh">全局云定位服务器配置（Cloud SpatialMap功能使用）。</para>
        /// </summary>
        [Tooltip("Global cloud localizer serveice config (used in Cloud SpatialMap).")]
        public CloudSpatialMapLocalizerFrameFilter.CloudLocalizerServiceConfig GlobalCloudLocalizerServeiceConfig = new CloudSpatialMapLocalizerFrameFilter.CloudLocalizerServiceConfig();
        /// <summary>
        /// <para xml:lang="en">ARCore SDK configuration. If you are using AR Foundation, use <see cref="ARCoreType.ARFoundationOrOptional"/> to let the plugin decide which one to use, or use <see cref="ARCoreType.External"/>. If other ARCore SDK distributions is desired, use <see cref="ARCoreType.External"/>.</para>
        /// <para xml:lang="zh">ARCore SDK配置。如果你在使用AR Foundation，可以使用 <see cref="ARCoreType.ARFoundationOrOptional"/> 来让插件自动决定使用的ARCore分发，也可以使用<see cref="ARCoreType.External"/>。如果期望使用其它ARCore SDK分发，需要设置为<see cref="ARCoreType.External"/>。</para>
        /// </summary>
        [Tooltip("ARCore SDK configuration. If you are using AR Foundation, use ARFoundationOrOptional to let the plugin decide which one to use, or use External. If other ARCore SDK distributions is desired, use External.")]
        public ARCoreType ARCoreSDK;

#if !UNITY_2020_1_OR_NEWER
        /// <summary>
        /// <para xml:lang="en">Turn on this option if you are building with Android SDK Platform >= 30 and need ARCore to work. Projects built with Unity 2019.4 must be updated to use Gradle 5.6.4 or later. Please refer to https://developers.google.com/ar/develop/unity/android-11-build#unity_20193_20194_and_20201 for updating your project's Gradle version.</para>
        /// <para xml:lang="zh">如果你在使用Android SDK Platform >= 30构建工程并希望ARCore可以工作，需要打开这个选项。使用Unity 2019.4构建的项目必须使用Gradle 5.6.4或更新版本。可以参考 https://developers.google.com/ar/develop/unity/android-11-build#unity_20193_20194_and_20201 来更新工程使用的Gradle版本。</para>
        /// </summary>
        [Tooltip("Turn on this option if you are building with Android SDK Platform >= 30 and need ARCore to work. Projects built with Unity 2019.4 must be updated to use Gradle 5.6.4 or later. Please refer to https://developers.google.com/ar/develop/unity/android-11-build#unity_20193_20194_and_20201 for updating your project's Gradle version.")]
        public bool ARCoreForAndroid11 = false;
#endif

        /// <summary>
        /// <para xml:lang="en">Generate XML document when script reload to make intelliSense for API document work.</para>
        /// <para xml:lang="zh">在脚本重新加载时生成XML文档，以使API文档的intelliSense可以工作。</para>
        /// </summary>
        [Tooltip("Generate XML document when script reload to make intelliSense for API document work.")]
        public bool GenerateXMLDoc = true;

        /// <summary>
        /// <para xml:lang="en">ARCore SDK configuration.</para>
        /// <para xml:lang="zh">ARCore SDK配置。</para>
        /// </summary>
        public enum ARCoreType
        {
            /// <summary>
            /// <para xml:lang="en">Either ARCore SDK distributed with EasyAR or AR Foundation will be included in the build according to the settings of ARCore XR Plugin.</para>
            /// <para xml:lang="en">If ARCore SDK distributed with EasyAR is selected, ARCore features will be activated only on ARCore supported devices that have Google Play Services for AR installed.</para>
            /// <para xml:lang="en">Please visit https://developers.google.com/ar/develop/java/enable-arcore for more details and configurations required for your app.</para>
            /// <para xml:lang="zh">随EasyAR或AR Foundation一起分发的ARCore SDK将会被包含在应用中，根据ARCore XR Plugin的设置决定。</para>
            /// <para xml:lang="zh">如果随EasyAR一起分发的ARCore SDK被选中，ARCore 功能只在支持ARCore并安装了Google Play Services for AR的设备上可以使用。</para>
            /// <para xml:lang="zh">更多细节及应用所需要的配置请访问 https://developers.google.com/ar/develop/java/enable-arcore 。</para>
            /// </summary>
            [Tooltip("Either ARCore SDK distributed with EasyAR or AR Foundation will be included in the build according to the settings of ARCore XR Plugin. If ARCore SDK distributed with EasyAR is selected, ARCore features will be activated only on ARCore supported devices that have Google Play Services for AR installed.")]
            ARFoundationOrOptional,
            /// <summary>
            /// <para xml:lang="en">ARCore SDK distributed with EasyAR will be included in the build.</para>
            /// <para xml:lang="en">ARCore features are activated only on ARCore supported devices that have Google Play Services for AR installed.</para>
            /// <para xml:lang="en">Please visit https://developers.google.com/ar/develop/java/enable-arcore for more details and configurations required for your app.</para>
            /// <para xml:lang="zh">随EasyAR一起分发的ARCore SDK将会被包含在应用中。</para>
            /// <para xml:lang="zh">ARCore 功能只在支持ARCore并安装了Google Play Services for AR的设备上可以使用。</para>
            /// <para xml:lang="zh">更多细节及应用所需要的配置请访问 https://developers.google.com/ar/develop/java/enable-arcore 。</para>
            /// </summary>
            [Tooltip("ARCore SDK distributed with EasyAR will be included in the build. ARCore features are activated only on ARCore supported devices that have Google Play Services for AR installed.")]
            Optional,
            /// <summary>
            /// <para xml:lang="en">ARCore SDK distributed with EasyAR will be included in the build.</para>
            /// <para xml:lang="en">Your app will require an ARCore Supported Device that has Google Play Services for AR installed on it.</para>
            /// <para xml:lang="en">Please visit https://developers.google.com/ar/develop/java/enable-arcore for more details and configurations required for your app.</para>
            /// <para xml:lang="zh">随EasyAR一起分发的ARCore SDK将会被包含在应用中。</para>
            /// <para xml:lang="zh">应用将只能在支持ARCore并安装了Google Play Services for AR的设备上可以运行。</para>
            /// <para xml:lang="zh">更多细节及应用所需要的配置请访问 https://developers.google.com/ar/develop/java/enable-arcore 。</para>
            /// </summary>
            [Tooltip("ARCore SDK distributed with EasyAR will be included in the build. Your app will require an ARCore Supported Device that has Google Play Services for AR installed on it.")]
            Required,
            /// <summary>
            /// <para xml:lang="en">ARCore SDK distributed with EasyAR will not be used.</para>
            /// <para xml:lang="zh">随EasyAR一起分发的ARCore SDK将不会使用。</para>
            /// </summary>
            [Tooltip("ARCore SDK distributed with EasyAR will not be used.")]
            External,
        }

        /// <summary>
        /// <para xml:lang="en"><see cref="Gizmos"/> configuration for target.</para>
        /// <para xml:lang="zh">Target的<see cref="Gizmos"/>配置。</para>
        /// </summary>
        [Serializable]
        public class TargetGizmoConfig
        {
            /// <summary>
            /// <para xml:lang="en"><see cref="Gizmos"/> configuration for <see cref="easyar.ImageTarget"/>.</para>
            /// <para xml:lang="zh"><see cref="easyar.ImageTarget"/>的<see cref="Gizmos"/>配置。</para>
            /// </summary>
            [Tooltip("Gizmos configuration for ImageTarget.")]
            public ImageTargetConfig ImageTarget = new ImageTargetConfig();
            /// <summary>
            /// <para xml:lang="en"><see cref="Gizmos"/> configuration for <see cref="easyar.ObjectTarget"/>.</para>
            /// <para xml:lang="zh"><see cref="easyar.ObjectTarget"/>的<see cref="Gizmos"/>配置。</para>
            /// </summary>
            [Tooltip("Gizmos configuration for ObjectTarget.")]
            public ObjectTargetConfig ObjectTarget = new ObjectTargetConfig();

            /// <summary>
            /// <para xml:lang="en"><see cref="Gizmos"/> configuration for <see cref="easyar.ImageTarget"/>.</para>
            /// <para xml:lang="zh"><see cref="easyar.ImageTarget"/>的<see cref="Gizmos"/>配置。</para>
            /// </summary>
            [Serializable]
            public class ImageTargetConfig
            {
                /// <summary>
                /// <para xml:lang="en">Enable <see cref="Gizmos"/> of target which <see cref="ImageTargetController.SourceType"/> equals to <see cref="ImageTargetController.DataSource.ImageFile"/>. Enable this option will load image file and display gizmo in Unity Editor, the startup performance of the Editor will be affected if there are too much target of this kind in the scene, but the Unity runtime will not be affected when running on devices.</para>
                /// <para xml:lang="zh">开启<see cref="ImageTargetController.SourceType"/>类型为<see cref="ImageTargetController.DataSource.ImageFile"/>的target的<see cref="Gizmos"/>。打开这个将会在Unity Editor中加载图像文件并显示对应gizmo，如果场景中该类target过多，可能会影响编辑器中的启动性能。在设备上运行时，Unity运行时的性能不会受到影响。</para>
                /// </summary>
                [Tooltip("Enable Gizmos of target which ImageTargetController.SourceType equals to ImageTargetController.DataSource.ImageFile. Enable this option will load image file and display gizmo in Unity Editor, the startup performance of the Editor will be affected if there are too much target of this kind in the scene, but the Unity runtime will not be affected when running on devices.")]
                public bool EnableImageFile = true;
                /// <summary>
                /// <para xml:lang="en">Enable <see cref="Gizmos"/> of target which <see cref="ImageTargetController.SourceType"/> equals to <see cref="ImageTargetController.DataSource.TargetDataFile"/>. Enable this option will target data file and display gizmo in Unity Editor, the startup performance of the Editor will be affected if there are too much target of this kind in the scene, but the Unity runtime will not be affected when running on devices.</para>
                /// <para xml:lang="zh">开启<see cref="ImageTargetController.SourceType"/>类型为<see cref="ImageTargetController.DataSource.TargetDataFile"/>的target的<see cref="Gizmos"/>。打开这个将会在Unity Editor中加载target数据文件并显示显示对应gizmo，如果场景中该类target过多，可能会影响编辑器中的启动性能。在设备上运行时，Unity运行时的性能不会受到影响。</para>
                /// </summary>
                [Tooltip("Enable Gizmos of target which ImageTargetController.SourceType equals to ImageTargetController.DataSource.TargetDataFile. Enable this option will target data file and display gizmo in Unity Editor, the startup performance of the Editor will be affected if there are too much target of this kind in the scene, but the Unity runtime will not be affected when running on devices.")]
                public bool EnableTargetDataFile = true;
                /// <summary>
                /// <para xml:lang="en">Enable <see cref="Gizmos"/> of target which <see cref="ImageTargetController.SourceType"/> equals to <see cref="ImageTargetController.DataSource.Target"/>. Enable this option will display gizmo in Unity Editor, the startup performance of the Editor will be affected if there are too much target of this kind in the scene, but the Unity runtime will not be affected when running on devices.</para>
                /// <para xml:lang="zh">开启<see cref="ImageTargetController.SourceType"/>类型为<see cref="ImageTargetController.DataSource.Target"/>的target的<see cref="Gizmos"/>。打开这个将会在Unity Editor中显示对应gizmo，如果场景中该类target过多，可能会影响编辑器中的启动性能。在设备上运行时，Unity运行时的性能不会受到影响。</para>
                /// </summary>
                [Tooltip("Enable Gizmos of target which ImageTargetController.SourceType equals to  ImageTargetController.DataSource.Target. Enable this option will display gizmo in Unity Editor, the startup performance of the Editor will be affected if there are too much target of this kind in the scene, but the Unity runtime will not be affected when running on devices.")]
                public bool EnableTarget = true;
            }

            /// <summary>
            /// <para xml:lang="en"><see cref="Gizmos"/> configuration for <see cref="easyar.ObjectTarget"/>.</para>
            /// <para xml:lang="zh"><see cref="easyar.ObjectTarget"/>的<see cref="Gizmos"/>配置。</para>
            /// </summary>
            [Serializable]
            public class ObjectTargetConfig
            {
                /// <summary>
                /// <para xml:lang="en">Enable <see cref="Gizmos"/>.</para>
                /// <para xml:lang="zh">开启<see cref="Gizmos"/>。</para>
                /// </summary>
                public bool Enable = true;
            }
        }

        /// <summary>
        /// <para xml:lang="en">Configuration for AndroidManifest.</para>
        /// <para xml:lang="zh">AndroidManifest权限配置。</para>
        /// </summary>
        [Serializable]
        public class Permission
        {
            /// <summary>
            /// <para xml:lang="en">Permission required for <see cref="easyar.CameraDevice"/> and other frame sources which require camera device usages.</para>
            /// <para xml:lang="en">Turn on this option will use Camera permission on device.</para>
            /// <para xml:lang="zh">使用<see cref="easyar.CameraDevice"/>及其它需要使用相机设备的frame source需要的权限。</para>
            /// <para xml:lang="zh">开启这个选项将会使用设备的相机权限。</para>
            /// </summary>
            [Tooltip("Permission required for easyar.CameraDevice and other frame sources which require camera device usages. Turn on this option will use Camera permission on device.")]
            public bool CameraDevice = true;
            /// <summary>
            /// <para xml:lang="en">Permission required for <see cref="easyar.VideoRecorder"/>.</para>
            /// <para xml:lang="en">Turn on this option will use Microphone permission on device.</para>
            /// <para xml:lang="en">Microphone permission is not forced in iOS build. Don't forget to add MicrophoneUsageDescription to your player settings.</para>
            /// <para xml:lang="zh">使用<see cref="easyar.VideoRecorder"/>需要的权限。</para>
            /// <para xml:lang="zh">开启这个选项将会使用设备的麦克风权限。</para>
            /// <para xml:lang="zh">在iOS构建中，麦克风权限没有强制开启。需要注意在player设置中添加MicrophoneUsageDescription。</para>
            /// </summary>
            [Tooltip("Permission required for easyar.VideoRecorder. Turn on this option will use Microphone permission on device. Microphone permission is not forced in iOS build. Don't forget to add MicrophoneUsageDescription to your player settings.")]
            public bool VideoRecording = true;
            /// <summary>
            /// <para xml:lang="en">Permission suggested for <see cref="easyar.CloudSpatialMapLocalizerFrameFilter"/>.</para>
            /// <para xml:lang="en">Turn on this option will use (fine) Location permission on device (ONLY when com.easyar.spatialmap package exist).</para>
            /// <para xml:lang="zh">使用<see cref="easyar.CloudSpatialMapLocalizerFrameFilter"/>建议开启的权限。</para>
            /// <para xml:lang="zh">开启这个选项将会使用设备的（fine）定位权限（只有当com.easyar.spatialmap package存在时才起作用）。</para>
            /// </summary>
            [Tooltip("Permission required for easyar.CloudSpatialMapLocalizerFrameFilter. Turn on this option will use (fine) Location permission on device (ONLY when com.easyar.spatialmap package exist).")]
            public bool CloudSpatialMap = true;
        }
    }
}
