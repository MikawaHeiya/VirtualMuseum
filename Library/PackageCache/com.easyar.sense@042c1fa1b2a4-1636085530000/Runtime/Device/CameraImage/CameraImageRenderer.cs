//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace easyar
{
    /// <summary>
    /// <para xml:lang="en"><see cref="MonoBehaviour"/> which controls camera image rendering in the scene.</para>
    /// <para xml:lang="zh">在场景中控制camera图像渲染的<see cref="MonoBehaviour"/>。</para>
    /// </summary>
    [RequireComponent(typeof(RenderCameraController))]
    public class CameraImageRenderer : MonoBehaviour
    {
        static readonly CameraEvent[] cameraEvents = new[] { CameraEvent.BeforeForwardOpaque, CameraEvent.BeforeGBuffer };
        private Camera targetCamera;
        private CommandBuffer commandBuffer;
        private CameraImageMaterial arMaterial;
        private Material material;
        private CameraParameters cameraParameters;
        private bool renderImageHFlip;
        private UserRequest request;
        private ARSession arSession;
        private bool invertCulling;
        private bool invertCullingChanged;
        private static Dictionary<Camera, CameraImageRenderer> allRenderers = new Dictionary<Camera, CameraImageRenderer>();

        /// <summary>
        /// <para xml:lang="en">Camera image rendering update event. This event will pass out the Material and texture size of current camera image rendering. This event only indicates a new render happens, while the camera image itself may not change.</para>
        /// <para xml:lang="zh">camera图像渲染更新的事件。这个事件会传出当前用于camera图像渲染的材质和贴图大小。当这个事件发生时，camera图像本身不一定有改变，它只表示一次渲染的发生。</para>
        /// </summary>
        public event Action<Material, Vector2> OnFrameRenderUpdate;
        private event Action<Camera, RenderTexture> TargetTextureChange;

        internal Material Material { get => material; }
        internal Optional<RenderTexture> UserTexture { get; private set; }

        internal bool InvertCulling
        {
            get => invertCulling;
            set
            {
                invertCulling = value;
                invertCullingChanged = true;
            }
        }

        protected virtual void Awake()
        {
            arMaterial = new CameraImageMaterial();
        }

        protected virtual void OnEnable()
        {
            UpdateCommandBuffer(targetCamera, material, invertCulling);
        }

        protected virtual void OnDisable()
        {
            RemoveCommandBuffer(targetCamera);
        }

        protected virtual void OnDestroy()
        {
            arMaterial.Dispose();
            if (request != null) { request.Dispose(); }
            if (cameraParameters != null) { cameraParameters.Dispose(); }

            var renderers = new Dictionary<Camera, CameraImageRenderer>();
            foreach(var item in allRenderers)
            {
                if (item.Key && item.Value != this)
                {
                    renderers.Add(item.Key, item.Value);
                }
            }
            allRenderers = renderers;
        }

        /// <summary>
        /// <para xml:lang="en">Get the <see cref="RenderTexture"/> of camera image.</para>
        /// <para xml:lang="en">The texture is a full sized image from <see cref="OutputFrame"/>, not cropped by the screen. The action <paramref name="targetTextureEventHandler"/> will pass out the <see cref="RenderTexture"/> and the <see cref="Camera"/> drawing the texture when the texture created or changed, will not call every frame or when the camera image data change. Calling this method will create external resources, and will trigger render when necessary, so make sure to release the resource using <see cref="DropTargetTexture"/> when not use.</para>
        /// <para xml:lang="zh">获取camera图像的<see cref="RenderTexture"/>。</para>
        /// <para xml:lang="zh">通过这个接口获取的texture是从<see cref="OutputFrame"/>获取的完整大小的图像，未经屏幕裁剪。<paramref name="targetTextureEventHandler"/> action会传出<see cref="RenderTexture"/>以及用于绘制texture的<see cref="Camera"/>。这个action不会每帧调用，也不会在camera图像数据发生变化的时候调用，它只会发生在texture本身创建或改变的时候。调用这个方法会创建额外的资源且会在必要时触发渲染，因此在不使用的时候需要调用<see cref="DropTargetTexture"/>释放资源。</para>
        /// </summary>
        public void RequestTargetTexture(Action<Camera, RenderTexture> targetTextureEventHandler)
        {
            if (request == null)
            {
                request = new UserRequest();
            }
            TargetTextureChange += targetTextureEventHandler;
            RenderTexture texture;
            request.UpdateTexture(targetCamera, material, invertCulling, out texture);
            UserTexture = texture;
            if (TargetTextureChange != null && texture)
            {
                TargetTextureChange(targetCamera, texture);
            }
        }

        /// <summary>
        /// <para xml:lang="en">Release the <see cref="RenderTexture"/> of camera image. Internal resources will be released when all holders release.</para>
        /// <para xml:lang="zh">释放绘制camera图像的<see cref="RenderTexture"/>。内部资源将在所有持有者都释放后释放。</para>
        /// </summary>
        public void DropTargetTexture(Action<Camera, RenderTexture> targetTextureEventHandler)
        {
            targetTextureEventHandler(targetCamera, null);
            UserTexture = null;
            TargetTextureChange -= targetTextureEventHandler;
            if (TargetTextureChange == null && request != null)
            {
                request.RemoveCommandBuffer(targetCamera);
                request.Dispose();
                request = null;
            }
        }

        /// <summary>
        /// <para xml:lang="en">Usually only for internal assemble use. Assemble response.</para>
        /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh">通常只在内部组装时使用。组装响应方法。</para>
        /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public void OnAssemble(ARSession session)
        {
            arSession = session;
            targetCamera = session.Assembly.Camera;
            if (targetCamera)
            {
                allRenderers[targetCamera] = this;
            }
            session.FrameChange += OnFrameChange;
            session.FrameUpdate += OnFrameUpdate;
        }

        /// <summary>
        /// <para xml:lang="en">Set render image horizontal flip.</para>
        /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh">设置渲染的图像的镜像翻转。</para>
        /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public void SetHFilp(bool hFlip)
        {
            renderImageHFlip = hFlip;
        }

        private void OnFrameChange(OutputFrame outputFrame, Quaternion displayCompensation)
        {
            if (outputFrame == null)
            {
                material = null;
                UpdateCommandBuffer(targetCamera, material, invertCulling);
                if (request != null)
                {
                    request.UpdateCommandBuffer(targetCamera, material, invertCulling);
                    RenderTexture texture = null;
                    if (TargetTextureChange != null && request.UpdateTexture(targetCamera, material, invertCulling, out texture))
                    {
                        TargetTextureChange(targetCamera, texture);
                    }
                    UserTexture = texture;
                }
                return;
            }
            if (!enabled && request == null && OnFrameRenderUpdate == null)
            {
                return;
            }
            using (var frame = outputFrame.inputFrame())
            {
                using (var image = frame.image())
                {
                    var materialUpdated = arMaterial.UpdateByImage(image);
                    if (material != materialUpdated)
                    {
                        material = materialUpdated;
                        UpdateCommandBuffer(targetCamera, material, invertCulling);
                        if (request != null) { request.UpdateCommandBuffer(targetCamera, material, invertCulling); }
                    }
                }
                if (cameraParameters != null)
                {
                    cameraParameters.Dispose();
                }
                cameraParameters = frame.cameraParameters();
            }
        }

        internal static CameraImageRenderer TryGetRenderer(Camera camera)
        {
            if (!camera) { return null; }
            if (allRenderers.TryGetValue(camera, out CameraImageRenderer renderer))
            {
                return renderer;
            }
            return null;
        }

        private void OnFrameUpdate(OutputFrame outputFrame)
        {
            if (!enabled && request == null && OnFrameRenderUpdate == null)
            {
                return;
            }

            if (request != null)
            {
                RenderTexture texture = null;
                if (TargetTextureChange != null && request.UpdateTexture(targetCamera, material, invertCulling, out texture))
                {
                    TargetTextureChange(targetCamera, texture);
                }
                UserTexture = texture;
            }

            if (!material || !targetCamera)
            {
                return;
            }
            if (invertCullingChanged)
            {
                UpdateCommandBuffer(targetCamera, material, invertCulling);
                if (request != null) { request.UpdateCommandBuffer(targetCamera, material, invertCulling); }
            }

            var displayTransform = DisplayTransform(cameraParameters, targetCamera.aspect, arSession.Assembly.Display.Rotation, renderImageHFlip);
            material.SetMatrix("_DisplayTransform", displayTransform);
            if (OnFrameRenderUpdate != null)
            {
                OnFrameRenderUpdate(material, new Vector2(Screen.width * targetCamera.rect.width, Screen.height * targetCamera.rect.height));
            }
        }

        private static Matrix4x4 DisplayTransform(CameraParameters parameters, float viewportAspectRatio, int screenRotation, bool hFlip)
        {
            var imageRotation = parameters.imageOrientation(screenRotation);
            parameters.imageProjection(viewportAspectRatio, screenRotation, true, parameters.cameraDeviceType() == CameraDeviceType.Front ? !hFlip : hFlip).ToUnityMatrix();

            var size = parameters.size();
            float imageAspectRatio = (float)size.data_0 / size.data_1;
            var requiredImageAspectRatio = (imageRotation % 180) == 0 ? viewportAspectRatio : 1.0f / viewportAspectRatio;
            var imageScale = imageAspectRatio < requiredImageAspectRatio ? new Vector3(1, imageAspectRatio / requiredImageAspectRatio, 1) : new Vector3(requiredImageAspectRatio / imageAspectRatio, 1, 1);

            var flip = Matrix4x4.identity;
            flip.m00 = hFlip ? -1 : 1;

            var displayTransform = Matrix4x4.Scale(imageScale) * Matrix4x4.Rotate(Quaternion.Euler(0, 0, -imageRotation)) * flip;
            displayTransform.m02 = (1 - displayTransform.m00 - displayTransform.m01) / 2;
            displayTransform.m12 = (1 - displayTransform.m10 - displayTransform.m11) / 2;
            return displayTransform;
        }

        private void UpdateCommandBuffer(Camera cam, Material material, bool invertCulling)
        {
            RemoveCommandBuffer(cam);
            if (!cam || !material)
            {
                return;
            }
            if (enabled)
            {
                var t = material.HasProperty("_MainTex") ? material.GetTexture("_MainTex") : null;
                commandBuffer = new CommandBuffer();
                commandBuffer.SetInvertCulling(invertCulling);
                commandBuffer.ClearRenderTarget(true, false, Color.clear);
                commandBuffer.Blit(t, BuiltinRenderTextureType.CameraTarget, material);
                foreach (var cameraEvent in cameraEvents)
                {
                    cam.AddCommandBuffer(cameraEvent, commandBuffer);
                }
                invertCullingChanged = false;
            }
        }

        private void RemoveCommandBuffer(Camera cam)
        {
            if (commandBuffer != null)
            {
                if (cam)
                {
                    foreach (var cameraEvent in cameraEvents)
                    {
                        cam.RemoveCommandBuffer(cameraEvent, commandBuffer);
                    }
                }
                commandBuffer.Dispose();
                commandBuffer = null;
            }
        }

        private class UserRequest : IDisposable
        {
            private RenderTexture texture;
            private CommandBuffer commandBuffer;

            ~UserRequest()
            {
                if (commandBuffer != null) { commandBuffer.Dispose(); }
                if (texture) { Destroy(texture); }
            }

            public void Dispose()
            {
                if (commandBuffer != null) { commandBuffer.Dispose(); }
                if (texture) { Destroy(texture); }
                GC.SuppressFinalize(this);
            }

            public bool UpdateTexture(Camera cam, Material material, bool invertCulling, out RenderTexture tex)
            {
                tex = texture;
                if (!cam || !material)
                {
                    if (texture)
                    {
                        Destroy(texture);
                        tex = texture = null;
                        return true;
                    }
                    return false;
                }
                int w = (int)(Screen.width * cam.rect.width);
                int h = (int)(Screen.height * cam.rect.height);
                if (texture && (texture.width != w || texture.height != h))
                {
                    Destroy(texture);
                }

                if (texture)
                {
                    return false;
                }
                else
                {
                    texture = new RenderTexture(w, h, 0);
                    UpdateCommandBuffer(cam, material, invertCulling);
                    tex = texture;
                    return true;
                }
            }

            public void UpdateCommandBuffer(Camera cam, Material material, bool invertCulling)
            {
                RemoveCommandBuffer(cam);
                if (!cam || !material)
                {
                    return;
                }
                if (texture)
                {
                    var t = material.HasProperty("_MainTex") ? material.GetTexture("_MainTex") : null;
                    commandBuffer = new CommandBuffer();
                    commandBuffer.SetInvertCulling(invertCulling);
                    commandBuffer.ClearRenderTarget(true, false, Color.clear);
                    commandBuffer.Blit(t, texture, material);
                    foreach (var cameraEvent in cameraEvents)
                    {
                        cam.AddCommandBuffer(cameraEvent, commandBuffer);
                    }
                }
            }

            public void RemoveCommandBuffer(Camera cam)
            {
                if (commandBuffer != null)
                {
                    if (cam)
                    {
                        foreach (var cameraEvent in cameraEvents)
                        {
                            cam.RemoveCommandBuffer(cameraEvent, commandBuffer);
                        }
                    }
                    commandBuffer.Dispose();
                    commandBuffer = null;
                }
            }
        }
    }
}
