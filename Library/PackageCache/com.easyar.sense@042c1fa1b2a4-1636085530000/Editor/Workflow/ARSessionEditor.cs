//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace easyar
{
    [CustomEditor(typeof(ARSession), true)]
    public class ARSessionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var session = target as ARSession;

            if (session.AvailableCenterMode.Count <= 0)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CenterMode"), new GUIContent("Center"));
                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                session.CenterMode = (ARSession.ARCenterMode)EditorGUILayout.Popup("Center", (int)session.CenterMode, session.AvailableCenterMode.Select(m => m.ToString()).ToArray());
            }

            ++EditorGUI.indentLevel;
            if (session.CenterMode == ARSession.ARCenterMode.SpecificTarget)
            {
                var center = session.SpecificTargetCenter;
                session.SpecificTargetCenter = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Target",
                    "The Target must have one of these components: " +
                    $"{nameof(TargetController)}, " +
#if EASYAR_ENABLE_CLOUDSPATIALMAP
                    $"{nameof(SpatialMapRootController)}, " +
#endif
                    $"{nameof(SparseSpatialMapRootController)}."
                    ),
                    session.SpecificTargetCenter, typeof(GameObject), true);
                if (center != session.SpecificTargetCenter)
                {
                    EditorUtility.SetDirty(session);
                }
            }
            if (Application.isPlaying)
            {
                var guiEnabled = GUI.enabled;
                GUI.enabled = false;
                EditorGUILayout.ObjectField("Current", session.CenterObject, typeof(GameObject), true);
                GUI.enabled = guiEnabled;
            }
            --EditorGUI.indentLevel;
        }
    }
}
