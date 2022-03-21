//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace easyar
{
    /// <summary>
    /// <para xml:lang="en">Display device interface.</para>
    /// <para xml:lang="zh">显示设备接口。</para>
    /// </summary>
    public interface IDisplay
    {
        /// <summary>
        /// <para xml:lang="en">Device rotation.</para>
        /// <para xml:lang="zh">设备旋转信息。</para>
        /// </summary>
        int Rotation { get; }
    }

    internal class Display : IDisplay, IDisposable
    {
        private Dictionary<int, int> rotations = new Dictionary<int, int>();
#if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaObject defaultDisplay;
#endif

        public Display()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                InitializeAndroid();
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                InitializeIOS();
            }
        }

        ~Display()
        {
            DeleteAndroidJavaObjects();
        }

        public int Rotation
        {
            get
            {
                if (Application.platform == RuntimePlatform.Android)
                {
#if UNITY_ANDROID && !UNITY_EDITOR
                    var rotation = defaultDisplay?.Call<int>("getRotation") ?? 0;
                    return rotations[rotation];
#endif
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    return rotations[(int)Screen.orientation];
                }
                return 0;
            }
        }

        public void Dispose()
        {
            DeleteAndroidJavaObjects();
            GC.SuppressFinalize(this);
        }

        private void InitializeIOS()
        {
            rotations[(int)ScreenOrientation.Portrait] = 0;
            rotations[(int)ScreenOrientation.LandscapeLeft] = 90;
            rotations[(int)ScreenOrientation.PortraitUpsideDown] = 180;
            rotations[(int)ScreenOrientation.LandscapeRight] = 270;
        }

        private void InitializeAndroid()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using (var surfaceClass = new AndroidJavaClass("android.view.Surface"))
            using (var contextClass = new AndroidJavaClass("android.content.Context"))
            using (var windowService = contextClass.GetStatic<AndroidJavaObject>("WINDOW_SERVICE"))
            using (var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var systemService = currentActivity.Call<AndroidJavaObject>("getSystemService", windowService))
            {
                defaultDisplay = systemService.Call<AndroidJavaObject>("getDefaultDisplay");
                rotations[surfaceClass.GetStatic<int>("ROTATION_0")] = 0;
                rotations[surfaceClass.GetStatic<int>("ROTATION_90")] = 90;
                rotations[surfaceClass.GetStatic<int>("ROTATION_180")] = 180;
                rotations[surfaceClass.GetStatic<int>("ROTATION_270")] = 270;
            }
#endif
        }

        private void DeleteAndroidJavaObjects()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (defaultDisplay != null)
            {
                defaultDisplay.Dispose();
                defaultDisplay = null;
            }
#endif
        }
    }

    internal class DisplayEmulator : IDisplay
    {
        public int Rotation { get; private set; }

        internal void EmulateRotation(int value)
        {
            Rotation = value;
        }
    }
}
