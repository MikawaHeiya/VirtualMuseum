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
    /// <para xml:lang="en"><see cref="MonoBehaviour"/> which controls <see cref="Camera"/> in the scene. The <see cref="Camera"/> projection is set to fit real world <see cref="CameraDevice"/> or other optical device.</para>
    /// <para xml:lang="zh">在场景中控制<see cref="Camera"/>的<see cref="MonoBehaviour"/>，<see cref="Camera"/> 投影矩阵会反映现实世界中的<see cref="CameraDevice"/>或其它光学设备。</para>
    /// </summary>
    public class RenderCameraController : MonoBehaviour
    {
        private Camera targetCamera;
        private CameraImageRenderer cameraRenderer;
        private Matrix4x4 currentDisplayCompensation = Matrix4x4.identity;
        private CameraParameters cameraParameters;
        private bool projectHFilp;
        private ARSession arSession;
        Optional<float> targetCameraFOV;

        protected virtual void OnEnable()
        {
            if (arSession)
            {
                arSession.FrameChange += OnFrameChange;
                arSession.FrameUpdate += OnFrameUpdate;
            }
        }

        protected virtual void OnDisable()
        {
            if (arSession)
            {
                arSession.FrameChange -= OnFrameChange;
                arSession.FrameUpdate -= OnFrameUpdate;
                if (targetCamera && targetCameraFOV.OnSome)
                {
                    targetCamera.fieldOfView = targetCameraFOV.Value;
                    targetCameraFOV = Optional<float>.Empty;
                }
            }
        }

        protected virtual void OnDestroy()
        {
            cameraParameters?.Dispose();
        }

        internal void OnAssemble(ARSession session)
        {
            arSession = session;
            targetCamera = session.Assembly.Camera;
            targetCameraFOV = targetCamera.fieldOfView;
            if (enabled)
            {
                arSession.FrameChange += OnFrameChange;
                arSession.FrameUpdate += OnFrameUpdate;
            }
            cameraRenderer = GetComponent<CameraImageRenderer>();
            if (cameraRenderer)
            {
                cameraRenderer.OnAssemble(session);
            }
        }

        internal void SetProjectHFlip(bool hFlip)
        {
            projectHFilp = hFlip;
            if (cameraRenderer)
            {
                cameraRenderer.InvertCulling = hFlip;
            }
        }

        internal void SetRenderImageHFilp(bool hFlip)
        {
            if (cameraRenderer)
            {
                cameraRenderer.SetHFilp(hFlip);
            }
        }

        private void OnFrameChange(OutputFrame outputFrame, Quaternion displayCompensation)
        {
            if (outputFrame == null)
            {
                return;
            }
            currentDisplayCompensation = Matrix4x4.Rotate(Quaternion.Inverse(displayCompensation));

            using (var frame = outputFrame.inputFrame())
            {
                if (cameraParameters != null)
                {
                    cameraParameters.Dispose();
                }
                cameraParameters = frame.cameraParameters();
            }
        }

        private void OnFrameUpdate(OutputFrame outputFrame)
        {
            var projection = cameraParameters.projection(targetCamera.nearClipPlane, targetCamera.farClipPlane, targetCamera.aspect, arSession.Assembly.Display.Rotation, false, false).ToUnityMatrix();
            projection *= currentDisplayCompensation;
            if (projectHFilp)
            {
                var translateMatrix = Matrix4x4.identity;
                translateMatrix.m00 = -1;
                projection = translateMatrix * projection;
            }
            targetCamera.projectionMatrix = projection;
            targetCamera.fieldOfView = Mathf.Atan(1 / projection.m11) * 2 * Mathf.Rad2Deg;
        }
    }
}
