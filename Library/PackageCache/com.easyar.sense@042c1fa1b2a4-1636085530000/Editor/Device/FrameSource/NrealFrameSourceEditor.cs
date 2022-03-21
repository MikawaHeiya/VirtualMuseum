//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using UnityEditor;
using UnityEngine;
#if EASYAR_ENABLE_NREAL
using NRKernal;
#else
using NRHMDPoseTracker = UnityEngine.MonoBehaviour;
#endif

namespace easyar
{
    [CustomEditor(typeof(NrealFrameSource), true)]
    public class NrealFrameSourceEditor : FrameSourceEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var fs = target as NrealFrameSource;
            var worldRoot = fs.WorldRoot;
            fs.WorldRoot = (WorldRootController)EditorGUILayout.ObjectField(new GUIContent("World Root", "If the value is not set, it will pick or create a suitable one automatically at runtime."), fs.WorldRoot, typeof(WorldRootController), true);
            if (worldRoot != fs.WorldRoot)
            {
                EditorUtility.SetDirty(fs);
            }
            var cameraRig = fs.CameraRig;
            fs.CameraRig = (NRHMDPoseTracker)EditorGUILayout.ObjectField(new GUIContent("Nreal Camera Rig", "If the value is not set, it will pick or create a suitable one automatically at runtime."), fs.CameraRig, typeof(NRHMDPoseTracker), true);
            if (cameraRig != fs.CameraRig)
            {
                EditorUtility.SetDirty(fs);
            }
        }
    }
}
