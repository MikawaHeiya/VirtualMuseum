//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using UnityEditor;
using UnityEngine;

namespace easyar
{
    [CustomEditor(typeof(FrameSource), true)]
    public class FrameSourceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var fs = target as FrameSource;
            var camera = fs.Camera;
            fs.Camera = (Camera)EditorGUILayout.ObjectField(new GUIContent("Camera", "If the value is not set, it will pick a suitable Camera automatically at runtime."), fs.Camera, typeof(Camera), true);
            if (camera != fs.Camera)
            {
                EditorUtility.SetDirty(fs);
            }
        }
    }
}
