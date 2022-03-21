//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using UnityEditor;

namespace easyar
{
    [CustomEditor(typeof(SurfaceTrackerFrameFilter), true)]
    public class SurfaceTrackerFrameFilterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var tracker = target as SurfaceTrackerFrameFilter;
            var t = tracker.Target;
            tracker.Target = (SurfaceTargetController)EditorGUILayout.ObjectField("Target", tracker.Target, typeof(SurfaceTargetController), true);
            if (t != tracker.Target)
            {
                EditorUtility.SetDirty(tracker);
            }
        }
    }
}
