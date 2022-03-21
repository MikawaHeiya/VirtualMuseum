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
    [CustomEditor(typeof(ARComponentPicker), true)]
    public class ARComponentPickerEditor : Editor
    {
        private static readonly string previewTooltip = "This is a preview, runtime value may be different.";
        private static readonly string previewFrameSourceTooltip = previewTooltip + " The first available one in transfrom order will be used at runtime. You can change the list by move the GameObject directly in the hierarchy window.";

        public override void OnInspectorGUI()
        {
            var picker = target as ARComponentPicker;

            var guiEnabled = GUI.enabled;
            var guiColor = GUI.color;
            if (Application.isPlaying)
            {
                GUI.enabled = false;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(picker.FramePlayer)));
            if (!Application.isPlaying)
            {
                serializedObject.ApplyModifiedProperties();
                if (picker.FramePlayer == ARComponentPicker.SingleSelection.Specify)
                {
                    ++EditorGUI.indentLevel;
                    GUILayout.BeginVertical(GUI.skin.box);
                    picker.SpecifiedComponents.FramePlayer = (FramePlayer)EditorGUILayout.ObjectField(picker.SpecifiedComponents.FramePlayer, typeof(FramePlayer), true);
                    GUILayout.EndVertical();
                    --EditorGUI.indentLevel;
                }
            }

            if (picker.FramePlayer != ARComponentPicker.SingleSelection.Disable)
            {
                var curGuiEnabled = GUI.enabled;
                GUI.enabled = false;
                EditorGUILayout.Popup("Frame Source", 0, new string[] { "Frame Player" });
                GUI.enabled = curGuiEnabled;
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(picker.FrameSource)));
                if (!Application.isPlaying)
                {
                    serializedObject.ApplyModifiedProperties();
                    if (picker.FrameSource == ARComponentPicker.SingleSelection.Specify)
                    {
                        ++EditorGUI.indentLevel;
                        GUILayout.BeginVertical(GUI.skin.box);
                        picker.SpecifiedComponents.FrameSource = (FrameSource)EditorGUILayout.ObjectField(picker.SpecifiedComponents.FrameSource, typeof(FrameSource), true);
                        GUILayout.EndVertical();
                        --EditorGUI.indentLevel;
                    }
                }
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(picker.FrameFilter)));
            if (!Application.isPlaying)
            {
                serializedObject.ApplyModifiedProperties();
                if (picker.FrameFilter == ARComponentPicker.MultipleSelection.Specify)
                {
                    ++EditorGUI.indentLevel;
                    GUILayout.BeginVertical(GUI.skin.box);
                    ShowListPropertyField("List", $"{nameof(picker.SpecifiedComponents)}.{nameof(picker.SpecifiedComponents.FrameFilters)}");
                    GUILayout.EndVertical();
                    --EditorGUI.indentLevel;
                }
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(picker.FrameRecorder)));
            if (!Application.isPlaying)
            {
                serializedObject.ApplyModifiedProperties();
                if (picker.FrameRecorder == ARComponentPicker.SingleSelection.Specify)
                {
                    ++EditorGUI.indentLevel;
                    GUILayout.BeginVertical(GUI.skin.box);
                    picker.SpecifiedComponents.FrameRecorder = (FrameRecorder)EditorGUILayout.ObjectField(picker.SpecifiedComponents.FrameRecorder, typeof(FrameRecorder), true);
                    GUILayout.EndVertical();
                    --EditorGUI.indentLevel;
                }
            }

            if (!Application.isPlaying)
            {
                serializedObject.ApplyModifiedProperties();
                var picked = picker.Preview();
                var list = serializedObject.FindProperty(nameof(picker.SpecifiedComponents));
                list.isExpanded = EditorGUILayout.Foldout(list.isExpanded, new GUIContent("Preview (Will Recalculate at Runtime)", previewTooltip));

                ++EditorGUI.indentLevel;
                if (list.isExpanded)
                {
                    GUI.enabled = false;
                    GUILayout.BeginVertical(GUI.skin.box);

                    if (picker.FrameSource == ARComponentPicker.SingleSelection.FirstAvailableActiveChild && picker.FramePlayer == ARComponentPicker.SingleSelection.Disable)
                    {
                        var frameSources = picker.GetComponentsInChildrenTransformOrder<FrameSource>();
                        EditorGUILayout.LabelField(new GUIContent("FrameSource", previewFrameSourceTooltip));
                        ++EditorGUI.indentLevel;
                        GUI.color = Color.yellow;
                        EditorGUILayout.LabelField(new GUIContent("Transfrom order, first one available will be used", previewFrameSourceTooltip));
                        GUI.color = guiColor;
                        for (int i = 0; i < frameSources.Count; i++)
                        {
                            if (frameSources[i] is FramePlayer) { continue; }
                            EditorGUILayout.ObjectField(new GUIContent($"Order {i}", previewFrameSourceTooltip), frameSources[i], typeof(FrameSource), true);
                        }
                        --EditorGUI.indentLevel;
                    }
                    else
                    {
                        EditorGUILayout.ObjectField(new GUIContent("FrameSource", previewTooltip), picked.FrameSource, typeof(FrameSource), true);
                    }

                    EditorGUILayout.LabelField(new GUIContent($"FrameFilter List ({picked.FrameFilters.Count})", previewTooltip));
                    ++EditorGUI.indentLevel;
                    for (int i = 0; i < picked.FrameFilters.Count; i++) { EditorGUILayout.ObjectField(new GUIContent($"Element {i}", previewTooltip), picked.FrameFilters[i], typeof(FrameFilter), true); }
                    --EditorGUI.indentLevel;

                    EditorGUILayout.ObjectField(new GUIContent("FrameRecorder", previewTooltip), picked.FrameRecorder, typeof(FrameRecorder), true);

                    GUILayout.EndVertical();
                    GUI.enabled = guiEnabled;
                }
                --EditorGUI.indentLevel;
            }

            GUI.enabled = guiEnabled;
        }

        private void ShowListPropertyField(string label, string propertyPath)
        {
            var list = serializedObject.FindProperty(propertyPath);
            list.isExpanded = EditorGUILayout.Foldout(list.isExpanded, label);
            ++EditorGUI.indentLevel;
            if (list.isExpanded)
            {
                int count = Mathf.Max(0, EditorGUILayout.IntField("Size", list.arraySize));
                while (count < list.arraySize) { list.DeleteArrayElementAtIndex(list.arraySize - 1); }
                while (count > list.arraySize) { list.InsertArrayElementAtIndex(list.arraySize); }
                for (int i = 0; i < list.arraySize; i++) { EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i)); }
            }
            --EditorGUI.indentLevel;
        }
    }
}
