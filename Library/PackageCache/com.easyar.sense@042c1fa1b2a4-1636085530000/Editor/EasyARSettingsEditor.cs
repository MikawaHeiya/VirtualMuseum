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
    [CustomEditor(typeof(EasyARSettings), true)]
    public class EasyARSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("LicenseKey"), new GUIContent("EasyAR SDK License Key"), true);
            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("EasyAR/Sense/Configuration", priority = 0)]
        private static void ConfigurationMenu()
        {
            Selection.SetActiveObjectWithContext(EasyARController.Settings, null);
        }

        [MenuItem("EasyAR/Sense/Document", priority = 100)]
        private static void DocumentEn()
        {
            Application.OpenURL("https://www.easyar.com/view/support.html");
        }

        [MenuItem("EasyAR/Sense/文档", priority = 100)]
        private static void DocumentZh()
        {
            Application.OpenURL("https://www.easyar.cn/view/support.html");
        }
    }
}
