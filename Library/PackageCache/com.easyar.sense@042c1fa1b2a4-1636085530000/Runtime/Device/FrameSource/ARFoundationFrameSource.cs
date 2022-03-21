//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using UnityEngine;
#if EASYAR_ARFOUNDATION_ENABLE
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using ARFoundation = UnityEngine.XR.ARFoundation;
using ARSessionOrigin = UnityEngine.XR.ARFoundation.ARSessionOrigin;
#else
using ARSessionOrigin = UnityEngine.MonoBehaviour;
#endif

namespace easyar
{
    /// <summary>
    /// <para xml:lang="en">A custom frame source which connects AR Foundation output to EasyAR input in the scene, providing AR Foundation support using custom camera feature of EasyAR Sense.</para>
    /// <para xml:lang="en">This frame source is one type of motion tracking device, and will output motion data in a <see cref="ARSession"/>.</para>
    /// <para xml:lang="en">``AR Foundation`` is required to use this frame source, you need to setup AR Foundation according to official documents.</para>
    /// <para xml:lang="en">This frame source will use ``ARFoundation.ARSession.CheckAvailability`` to check availability. <see cref="FrameSource.Camera"/> and <see cref="ARSessionOrigin"/> are also required for availability check, they will be automatically picked from scene objects if not setup. To choose frame source in runtime, you can deactive AR Foundation GameObjects and set all required values of all frame sources for availability check, and active AR Foundation GameObjects when this frame source is chosen.</para>
    /// <para xml:lang="zh">在场景中将AR Foundation 的输出连接到EasyAR输入的自定义frame source。通过EasyAR Sense的自定义相机功能提供AR Foundation支持。</para>
    /// <para xml:lang="zh">这个frame source是一种运动跟踪设备，在<see cref="ARSession"/>中会输出运动数据。</para>
    /// <para xml:lang="zh">为了使用这个frame source， ``AR Foundation`` 是必需的。你需要根据官方文档配置AR Foundation。</para>
    /// <para xml:lang="zh">这个frame source会使用 ``ARFoundation.ARSession.CheckAvailability`` 来检查可用性。在可用性检查中，<see cref="FrameSource.Camera"/> 和<see cref="ARSessionOrigin"/> 也是需要的，如果没有事先设置，会自动从场景物体中选择。如果要在运行时选择 frame source，可以deactive AR Foundation使用的所有GameObject，并设置所有frame source可用性检查所需要的数值，然后在这个frame source被选择后active AR Foundation 的GameObject。</para>
    /// </summary>
    public class ARFoundationFrameSource : FrameSource
    {
        /// <summary>
        /// <para xml:lang="en">If the device supports AR Foundation but does not have the necessary software, some platforms allow prompting the user to install or update the software. If this field is true, a software update will be attempted. If the appropriate software is not installed or out of date, and this field is false, then this frame source will not be available.</para>
        /// <para xml:lang="zh">如果设备支持AR Foundation但没有必要的软件，一些平台允许提示用户安装或更新软件。如果变量值为true，会尝试软件更新。如果系统中没有安装软件或软件过期，且变量值为false，这个frame source将是不可用的。</para>
        /// </summary>
        public bool AttemptUpdate = true;
        [SerializeField, HideInInspector]
        private ARSessionOrigin sessionOrigin;
        private bool assembled = false;

        /// <summary>
        /// <para xml:lang="en">The object Camera move against, will be automatically get from the scene.</para>
        /// <para xml:lang="zh">相机运动的相对物体，如果没设置，将会自动从场景中获取。</para>
        /// </summary>
        /// <mutabletype disabled="UnityEngine.MonoBehaviour" enabled="UnityEngine.XR.ARFoundation.ARSessionOrigin"/>
        public ARSessionOrigin ARSessionOrigin
        {
            get => sessionOrigin;
            set
            {
                if (assembled) { return; }
                sessionOrigin = value;
            }
        }

#if EASYAR_ARFOUNDATION_ENABLE
        private static IReadOnlyList<ARSession.ARCenterMode> availableCenterMode = new List<ARSession.ARCenterMode> { ARSession.ARCenterMode.SessionOrigin, ARSession.ARCenterMode.FirstTarget, ARSession.ARCenterMode.SpecificTarget };
        private double curTimestamp;
        private int cameraOrientation;
        private BufferPool bufferPool;
        private int bufferSize;
        private ARFoundation.ARCameraManager cameraManager;
        private Action<Pose> newFrame;
        private ARFoundation.CameraFacingDirection currentFacingDirection;
        private ARFoundation.ARSessionOrigin sessionOriginCache;
        private Optional<bool> isAvailable;

        public override Optional<bool> IsAvailable { get { return isAvailable; } }

        public override int BufferCapacity
        {
            get => bufferCapacity;
            set
            {
                bufferCapacity = value;
                if (bufferPool == null || bufferPool.capacity() == bufferCapacity) { return; }
                bufferPool.Dispose();
                bufferPool = new BufferPool(bufferSize, bufferCapacity);
            }
        }

        public override GameObject Origin { get => sessionOrigin ? sessionOrigin.gameObject : null; }

        public override bool IsCameraUnderControl { get { return false; } }

        public override IReadOnlyList<ARSession.ARCenterMode> AvailableCenterMode { get => availableCenterMode; }

        protected override void Awake()
        {
            base.Awake();
            if (sessionOrigin) { return; }
            sessionOriginCache = FindObjectOfType<ARFoundation.ARSessionOrigin>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (cameraManager)
            {
                cameraManager.frameReceived += OnCameraFrameReceived;
            }
            Application.onBeforeRender += OnBeforeRender;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (cameraManager)
            {
                cameraManager.frameReceived -= OnCameraFrameReceived;
            }
            Application.onBeforeRender -= OnBeforeRender;
        }

        private void OnDestroy()
        {
            bufferPool?.Dispose();
        }

        public override void OnAssemble(ARSession session)
        {
            base.OnAssemble(session);
            cameraManager = Camera.GetComponent<ARFoundation.ARCameraManager>();
            if (enabled)
            {
                cameraManager.frameReceived += OnCameraFrameReceived;
            }
            cameraOrientation = CameraOrientation();
            assembled = true;
            SetupOrigin();
        }

        public override IEnumerator CheckAvailability()
        {
            if (!Camera && !PickCamera())
            {
                isAvailable = false;
                yield break;
            }
            if (ARFoundation.ARSession.state <= ARFoundation.ARSessionState.CheckingAvailability)
            {
                yield return ARFoundation.ARSession.CheckAvailability();
            }
            if (ARFoundation.ARSession.state == ARFoundation.ARSessionState.NeedsInstall && AttemptUpdate)
            {
                yield return ARFoundation.ARSession.Install();
            }
            while (ARFoundation.ARSession.state == ARFoundation.ARSessionState.Installing)
            {
                yield return null;
            }

            isAvailable = ARFoundation.ARSession.state >= ARFoundation.ARSessionState.Ready;
        }

        public override Camera PickCamera()
        {
            if (IsValidCamera(Camera.main))
            {
                return Camera.main;
            }
            var component = FindObjectOfType<ARFoundation.ARCameraManager>();
            if (!component)
            {
                return null;
            }
            return component.GetComponent<Camera>();
        }

        protected override bool IsValidCamera(Camera cam)
        {
            return cam && cam.GetComponent<ARFoundation.ARCameraManager>();
        }

        unsafe void OnCameraFrameReceived(ARFoundation.ARCameraFrameEventArgs eventArgs)
        {
            if (!arSession || !cameraManager || bufferCapacity <= 0) { return; }
            if (ARFoundation.ARSession.state <= ARFoundation.ARSessionState.Ready) { return; }

            if (!cameraManager.TryGetIntrinsics(out var intrinsics)) { return; }
            if (!cameraManager.TryAcquireLatestCpuImage(out var cameraImage)) { return; }

            Buffer buffer;
            Vec2I size;
            var timestamp = eventArgs.timestampNs ?? (long)(cameraImage.timestamp * 1e9);

            using (cameraImage)
            {
                if (timestamp == curTimestamp) { return; }

                curTimestamp = timestamp;
                size = new Vec2I(cameraImage.width, cameraImage.height);

                if (bufferSize != cameraImage.width * cameraImage.height)
                {
                    bufferSize = cameraImage.width * cameraImage.height;
                    bufferPool?.Dispose();
                    bufferPool = new BufferPool(bufferSize, bufferCapacity);
                }
                var bufferO = bufferPool.tryAcquire();
                if (bufferO.OnNone) { return; }

                buffer = bufferO.Value;
                var plane0 = cameraImage.GetPlane(0);
                var src = new IntPtr(plane0.data.GetUnsafePtr());
                if (plane0.rowStride != cameraImage.width)
                {
                    for (int i = 0; i < cameraImage.height; i++)
                    {
                        buffer.tryCopyFrom(src, plane0.rowStride * i, cameraImage.width * i, cameraImage.width);
                    }
                }
                else
                {
                    buffer.tryCopyFrom(src, 0, 0, bufferSize);
                }
            }

            var screenRotation = arSession.Assembly.Display.Rotation;
            var trackingStatus = ARFoundation.ARSession.state == ARFoundation.ARSessionState.SessionTracking ? MotionTrackingStatus.Tracking : MotionTrackingStatus.NotTracking;
            if (currentFacingDirection != cameraManager.currentFacingDirection)
            {
                cameraOrientation = CameraOrientation();
                currentFacingDirection = cameraManager.currentFacingDirection;
            }

            newFrame = (pose) =>
            {
                using (var cameraParameters = new CameraParameters(size, new Vec2F(intrinsics.focalLength.x, intrinsics.focalLength.y), new Vec2F(intrinsics.principalPoint.x, intrinsics.principalPoint.y), CameraDeviceType.Back, cameraOrientation))
                using (buffer)
                using (var image = new Image(buffer, PixelFormat.Gray, size.data_0, size.data_1))
                {
                    var displayCompensation = Quaternion.Euler(0, 0, -cameraParameters.imageOrientation(screenRotation));
                    var pe = new Pose(Vector3.zero, displayCompensation).GetTransformedBy(pose).ToEasyARPose();
                    using (var frame = InputFrame.create(image, cameraParameters, timestamp * 1e-9, pe, trackingStatus))
                    {
                        sink.handle(frame);
                    }
                }
            };
        }

        [BeforeRenderOrder(100)]
        void OnBeforeRender()
        {
            if (!Camera) { return; }
            newFrame?.Invoke(new Pose(Camera.transform.localPosition, Camera.transform.localRotation));
            newFrame = null;
        }

        private int CameraOrientation()
        {
            var orientation = 0;
#if UNITY_ANDROID && !UNITY_EDITOR
            var index = cameraManager.currentFacingDirection != ARFoundation.CameraFacingDirection.User ? 0 : 1;
            if (Application.platform == RuntimePlatform.Android)
            {
                using (var cameraInfo = new AndroidJavaObject("android.hardware.Camera$CameraInfo"))
                using (var cameraClass = new AndroidJavaClass("android.hardware.Camera"))
                {
                    cameraClass.CallStatic("getCameraInfo", index, cameraInfo);
                    orientation = cameraInfo.Get<int>("orientation");
                }
            }
#else
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                orientation = 90;
            }
            else
            {
                orientation = 0;
            }
#endif
            return orientation;
        }

        private void SetupOrigin()
        {
            if (sessionOrigin) { return; }
            sessionOrigin = sessionOriginCache;
            if (sessionOrigin) { return; }
            sessionOrigin = FindObjectOfType<ARFoundation.ARSessionOrigin>();
        }
#else
        public override Optional<bool> IsAvailable { get => false; }
#endif
    }
}
