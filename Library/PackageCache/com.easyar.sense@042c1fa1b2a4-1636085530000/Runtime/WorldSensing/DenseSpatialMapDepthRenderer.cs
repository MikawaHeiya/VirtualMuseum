//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using UnityEngine;

namespace easyar
{
    /// <summary>
    /// <para xml:lang="en"><see cref="MonoBehaviour"/> which controls depth data render <see cref="DenseSpatialMap"/> in the scene. It does not work in URP for now.</para>
    /// <para xml:lang="zh">在场景中控制<see cref="DenseSpatialMap"/>深度数据渲染的<see cref="MonoBehaviour"/>。在当前版本中URP下不工作。</para>
    /// </summary>
    public class DenseSpatialMapDepthRenderer : MonoBehaviour
    {
        /// <summary>
        /// <para xml:lang="en"><see cref="UnityEngine.Shader"/> to render depth texture.</para>
        /// <para xml:lang="zh">用于深度渲染的<see cref="UnityEngine.Shader"/>。</para>
        /// </summary>
        public Shader Shader;

        private RenderTexture depthTexture;

        /// <summary>
        /// <para xml:lang="en"><see cref="Camera"/> used to render depth texture.</para>
        /// <para xml:lang="zh">用于深度渲染的<see cref="Camera"/>。</para>
        /// </summary>
        public Camera RenderDepthCamera { get; set; }
        /// <summary>
        /// <para xml:lang="en"><see cref="Material"/> used to render mesh.</para>
        /// <para xml:lang="zh">用于渲染网格的<see cref="Material"/>。</para>
        /// </summary>
        public Material MapMeshMaterial { get; set; }

        private void OnEnable()
        {
            Application.onBeforeRender += OnBeforeRender;
        }

        private void OnDisable()
        {
            Application.onBeforeRender -= OnBeforeRender;
        }

        [BeforeRenderOrder(100)]
        private void OnBeforeRender()
        {
            if (!RenderDepthCamera || !MapMeshMaterial)
            {
                return;
            }
            if (depthTexture && (depthTexture.width != Screen.width || depthTexture.height != Screen.height))
            {
                Destroy(depthTexture);
            }
            if (!depthTexture)
            {
                depthTexture = new RenderTexture(Screen.width, Screen.height, 24);
                MapMeshMaterial.SetTexture("_DepthTexture", depthTexture);
            }
            RenderDepthCamera.targetTexture = depthTexture;
            RenderDepthCamera.RenderWithShader(Shader, "Tag");
            RenderDepthCamera.targetTexture = null;
        }

        private void OnDestroy()
        {
            if (depthTexture)
            {
                Destroy(depthTexture);
            }
        }
    }
}
