//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using UnityEditor;
using UnityEngine;
#if EASYAR_ARFOUNDATION_ENABLE
using ARSessionOrigin = UnityEngine.XR.ARFoundation.ARSessionOrigin;
#else
using ARSessionOrigin = UnityEngine.MonoBehaviour;
#endif

namespace easyar
{
    [CustomEditor(typeof(ARFoundationFrameSource), true)]
    public class ARFoundationFrameSourceEditor : FrameSourceEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var fs = target as ARFoundationFrameSource;
            var sessionOrigin = fs.ARSessionOrigin;
            fs.ARSessionOrigin = (ARSessionOrigin)EditorGUILayout.ObjectField(new GUIContent("AR Session Origin", "If the value is not set, it will pick a suitable one automatically at runtime. There must be a valid ARSessionOrigin in the scene to make the frame source work."), fs.ARSessionOrigin, typeof(ARSessionOrigin), true);
            if (sessionOrigin != fs.ARSessionOrigin)
            {
                EditorUtility.SetDirty(fs);
            }
        }
    }
}
