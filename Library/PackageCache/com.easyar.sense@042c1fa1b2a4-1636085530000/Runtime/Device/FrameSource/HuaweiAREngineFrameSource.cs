//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using UnityEngine;
#if EASYAR_HWARENGINE_ENABLE && UNITY_ANDROID
using System.Collections;
using System.Collections.Generic;
using HW = HuaweiARUnitySDK;
#endif

namespace easyar
{
    /// <summary>
    /// <para xml:lang="en">A custom frame source which connects Huawei AR Engine output to EasyAR input in the scene, providing Huawei AR Engine support using custom camera feature of EasyAR Sense.</para>
    /// <para xml:lang="en">This frame source is one type of motion tracking device, and will output motion data in a <see cref="ARSession"/>.</para>
    /// <para xml:lang="en">``Huawei AR Engine Unity SDK`` is required to use this frame source, you need to setup Huawei AR Engine Unity SDK according to official documents.</para>
    /// <para xml:lang="en">This frame source will use ``AREnginesApk.Instance.IsAREngineApkReady`` to check availability. <see cref="FrameSource.Camera"/> and <see cref="WorldRoot"/> are also required for availability check, they will be automatically picked from scene objects if not setup. To choose frame source in runtime, you can deactive Huawei AR Engine GameObjects and set all required values of all frame sources for availability check, and active Huawei AR Engine GameObjects when this frame source is chosen.</para>
    /// <para xml:lang="zh">在场景中将华为AR Engine的输出连接到EasyAR输入的自定义frame source。通过EasyAR Sense的自定义相机功能提供华为AR Engine支持。</para>
    /// <para xml:lang="zh">这个frame source是一种运动跟踪设备，在<see cref="ARSession"/>中会输出运动数据。</para>
    /// <para xml:lang="zh">为了使用这个frame source， ``华为 AR Engine Unity SDK`` 是必需的。你需要根据官方文档配置华为 AR Engine Unity SDK。</para>
    /// <para xml:lang="zh">这个frame source会使用 ``AREnginesApk.Instance.IsAREngineApkReady`` 来检查可用性。在可用性检查中，<see cref="FrameSource.Camera"/> 和<see cref="WorldRoot"/> 也是需要的，如果没有事先设置，会自动从场景物体中选择。如果要在运行时选择 frame source，可以deactive 华为AR Engine使用的所有GameObject，并设置所有frame source可用性检查所需要的数值，然后在这个frame source被选择后active 华为AR Engine的GameObject。</para>
    /// </summary>
    public class HuaweiAREngineFrameSource : FrameSource
    {
        /// <summary>
        /// <para xml:lang="en">If high resolution image is used as frame input. High resolution image is currently useful for Cloud SpatialMap only. There will be no benefit turn on this flag in other situation except customized algorithm filter is used.</para>
        /// <para xml:lang="zh">是否使用高分辨率图像作为frame输入。高分辨率图像目前只有在使用Cloud SpatialMap的时候有用，其它情况下开启并不会获得任何益处。</para>
        /// </summary>
        public bool UseHighResolutionImage;
        [SerializeField, HideInInspector]
        private WorldRootController worldRoot;
        private bool assembled = false;

        /// <summary>
        /// <para xml:lang="en">The object Camera move against, will be automatically get from the scene or generate if not set.</para>
        /// <para xml:lang="zh">相机运动的相对物体，如果没设置，将会自动从场景中获取或生成。</para>
        /// </summary>
        public WorldRootController WorldRoot
        {
            get => worldRoot;
            set
            {
                if (assembled) { return; }
                worldRoot = value;
            }
        }

#if EASYAR_HWARENGINE_ENABLE && UNITY_ANDROID
        private static IReadOnlyList<ARSession.ARCenterMode> availableCenterMode = new List<ARSession.ARCenterMode> { ARSession.ARCenterMode.SessionOrigin };
        private double curTimestamp;
        private int cameraOrientation;
        private BufferPool bufferPool;
        private int bufferSize;
        private HW.ARConfigCameraLensFacing currentFacingDirection;
        private WorldRootController worldRootCache;
        private GameObject worldRootObject;
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

        public override GameObject Origin { get => worldRoot ? worldRoot.gameObject : null; }

        public override bool IsCameraUnderControl { get { return false; } }

        public override IReadOnlyList<ARSession.ARCenterMode> AvailableCenterMode { get => availableCenterMode; }

        protected override void Awake()
        {
            base.Awake();
            if (worldRoot) { return; }
            worldRootCache = FindObjectOfType<WorldRootController>();
        }

        void Update()
        {
            if (!arSession || bufferCapacity <= 0) { return; }
            if (HW.ARFrame.GetTrackingState() != HW.ARTrackable.TrackingState.TRACKING) { return; }

            var timestamp = HW.ARFrame.GetTimestampNs();
            if (timestamp == curTimestamp) { return; }
            curTimestamp = timestamp;

            var cameraImage = UseHighResolutionImage ? HW.ARFrame.AcquirPreviewImageBytes() : HW.ARFrame.AcquireCameraImageBytes();
            var size = new Vec2I(cameraImage.Width, cameraImage.Height);

            if (bufferSize != cameraImage.Width * cameraImage.Height)
            {
                bufferSize = cameraImage.Width * cameraImage.Height;
                bufferPool?.Dispose();
                bufferPool = new BufferPool(bufferSize, bufferCapacity);
            }
            var bufferO = bufferPool.tryAcquire();
            if (bufferO.OnNone)
            {
                cameraImage.Dispose();
                return;
            }

            var buffer = bufferO.Value;
            if (cameraImage.Width != cameraImage.YRowStride)
            {
                for (int i = 0; i < cameraImage.Height; i++)
                {
                    buffer.tryCopyFrom(cameraImage.Y, cameraImage.YRowStride * i, cameraImage.Width * i, cameraImage.Width);
                }
            }
            else
            {
                buffer.tryCopyFrom(cameraImage.Y, 0, 0, bufferSize);
            }
            cameraImage.Dispose();

            var screenRotation = arSession.Assembly.Display.Rotation;
            var pose = HW.ARFrame.GetPose();
            var trackingStatus = HW.ARFrame.GetTrackingState() == HW.ARTrackable.TrackingState.TRACKING ? MotionTrackingStatus.Tracking : MotionTrackingStatus.NotTracking;
            var intrinsics = HW.ARFrame.ImageIntrinsics;

            CameraParameters cameraParameters;
            if (size.data_0 == intrinsics.ARImageDimensions[0] && size.data_1 == intrinsics.ARImageDimensions[1])
            {
                cameraParameters = new CameraParameters(size,
                    new Vec2F(intrinsics.ARFocalLength[0], intrinsics.ARFocalLength[1]),
                    new Vec2F(intrinsics.ARPrincipalPoint[0], intrinsics.ARPrincipalPoint[1]),
                    CameraDeviceType.Back, cameraOrientation);
            }
            else
            {
                var flip = (size.data_0 > size.data_1 && intrinsics.ARImageDimensions[0] < intrinsics.ARImageDimensions[1]) ||
                (size.data_0 < size.data_1 && intrinsics.ARImageDimensions[0] > intrinsics.ARImageDimensions[1]);

                using (var cameraParametersRaw = new CameraParameters(
                    flip ? new Vec2I(intrinsics.ARImageDimensions[1], intrinsics.ARImageDimensions[0]) : new Vec2I(intrinsics.ARImageDimensions[0], intrinsics.ARImageDimensions[1]),
                    flip ? new Vec2F(intrinsics.ARFocalLength[1], intrinsics.ARFocalLength[0]) : new Vec2F(intrinsics.ARFocalLength[0], intrinsics.ARFocalLength[1]),
                    flip ? new Vec2F(intrinsics.ARPrincipalPoint[1], intrinsics.ARPrincipalPoint[0]) : new Vec2F(intrinsics.ARPrincipalPoint[0], intrinsics.ARPrincipalPoint[1]),
                    CameraDeviceType.Back, cameraOrientation))
                {
                    cameraParameters = cameraParametersRaw.getResized(size);
                }
            }

            using (cameraParameters)
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
        }

        private void OnDestroy()
        {
            if (worldRootObject) Destroy(worldRootObject);
            bufferPool?.Dispose();
        }

        public override void OnAssemble(ARSession session)
        {
            base.OnAssemble(session);
            cameraOrientation = CameraOrientation();
            assembled = true;
            SetupOriginUsingWorldRoot();
        }

        public override IEnumerator CheckAvailability()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                isAvailable = false;
                return null;
            }
            if (!Camera && !PickCamera())
            {
                isAvailable = false;
                return null;
            }
            isAvailable = HW.AREnginesApk.Instance.IsAREngineApkReady();
            return null;
        }

        // There is no API to aquire ARConfigBase, but a reference is held by user.
        public void UpdateCameraFacingDirection(HW.ARConfigCameraLensFacing facing)
        {
            if (currentFacingDirection != facing)
            {
                currentFacingDirection = facing;
                cameraOrientation = CameraOrientation();
            }
        }

        public override Camera PickCamera()
        {
            if (IsValidCamera(Camera.main))
            {
                return Camera.main;
            }
            var component = FindObjectOfType<HW.BackGroundRenderer>();
            if (!component)
            {
                return null;
            }
            return component.GetComponent<Camera>();
        }

        protected override bool IsValidCamera(Camera cam)
        {
            return cam && cam.GetComponent<HW.BackGroundRenderer>();
        }

        private int CameraOrientation()
        {
            var orientation = 0;
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Application.platform == RuntimePlatform.Android)
            {
                using (var cameraInfo = new AndroidJavaObject("android.hardware.Camera$CameraInfo"))
                using (var cameraClass = new AndroidJavaClass("android.hardware.Camera"))
                {
                    cameraClass.CallStatic("getCameraInfo", (int)currentFacingDirection, cameraInfo);
                    orientation = cameraInfo.Get<int>("orientation");
                }
            }
#endif
            return orientation;
        }

        private void SetupOriginUsingWorldRoot()
        {
            if (worldRoot) { return; }
            worldRoot = worldRootCache;
            if (worldRoot) { return; }
            worldRoot = FindObjectOfType<WorldRootController>();
            if (worldRoot) { return; }
            Debug.Log($"WorldRoot not found, create from {typeof(HuaweiAREngineFrameSource)}");
            worldRootObject = new GameObject("WorldRoot");
            worldRoot = worldRootObject.AddComponent<WorldRootController>();
        }
#else
        public override Optional<bool> IsAvailable { get => false; }
#endif
    }
}
