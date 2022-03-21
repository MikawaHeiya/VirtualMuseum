//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using UnityEngine;
using UnityEditor;

namespace easyar
{
    [CustomEditor(typeof (CameraDeviceFrameSource), true)]
    public class CameraDeviceFrameSourceEditor : FrameSourceEditor
    {
        CameraDevicePreference preference;

        public void OnEnable()
        {
            preference = ((CameraDeviceFrameSource)target).CameraPreference;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (((CameraDeviceFrameSource)target).CameraOpenMethod == CameraDeviceFrameSource.CameraDeviceOpenMethod.DeviceType)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraType"), true);
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraIndex"), true);
            }
            var cameraPreference = serializedObject.FindProperty("cameraPreference");
            EditorGUILayout.PropertyField(cameraPreference, new GUIContent("Camera Preference"), true);
            serializedObject.ApplyModifiedProperties();
            if(preference != (CameraDevicePreference)cameraPreference.enumValueIndex)
            {
                ((CameraDeviceFrameSource)target).CameraPreference = (CameraDevicePreference)cameraPreference.enumValueIndex;
                preference = (CameraDevicePreference)cameraPreference.enumValueIndex;
            }
        }
    }
}
