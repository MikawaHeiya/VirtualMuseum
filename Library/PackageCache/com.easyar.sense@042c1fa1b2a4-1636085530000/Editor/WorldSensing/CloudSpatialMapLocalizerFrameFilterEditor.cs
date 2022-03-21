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
    [CustomEditor(typeof(CloudSpatialMapLocalizerFrameFilter), true)]
    public class CloudSpatialMapLocalizerFrameFilterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
#if !EASYAR_ENABLE_CLOUDSPATIALMAP
            EditorGUILayout.HelpBox($"Package com.easyar.spatialmap is required to use {nameof(CloudSpatialMapLocalizerFrameFilter)}", MessageType.Error);
#else
            DrawDefaultInspector();
            if (!((CloudSpatialMapLocalizerFrameFilter)target).UseGlobalServiceConfig)
            {
                var serviceConfig = serializedObject.FindProperty("ServiceConfig");
                serviceConfig.isExpanded = EditorGUILayout.Foldout(serviceConfig.isExpanded, "Service Config");
                EditorGUI.indentLevel += 1;
                if (serviceConfig.isExpanded)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ServiceConfig.ServerAddress"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ServiceConfig.APIKey"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ServiceConfig.APISecret"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ServiceConfig.CloudLocalizerAppID"), true);
                }
                EditorGUI.indentLevel -= 1;
            }
            serializedObject.ApplyModifiedProperties();
#endif
        }
    }
}
