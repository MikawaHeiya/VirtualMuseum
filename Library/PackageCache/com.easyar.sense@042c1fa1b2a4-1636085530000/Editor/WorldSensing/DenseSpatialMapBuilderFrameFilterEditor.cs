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
    [CustomEditor(typeof(DenseSpatialMapBuilderFrameFilter), true)]
    public class DenseSpatialMapBuilderFrameFilterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var dense = target as DenseSpatialMapBuilderFrameFilter;
            var renderMesh = dense.RenderMesh;
            dense.RenderMesh = EditorGUILayout.Toggle("Render Mesh", dense.RenderMesh);
            if (renderMesh != dense.RenderMesh)
            {
                EditorUtility.SetDirty(dense);
            }
        }
    }
}
