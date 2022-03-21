//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using UnityEngine;
#if EASYAR_ENABLE_NREAL
using NRKernal;
using System;
using System.Collections;
using System.Collections.Generic;
#else
using NRHMDPoseTracker = UnityEngine.MonoBehaviour;
#endif

namespace easyar
{
    /// <summary>
    /// <para xml:lang="en">A custom frame source which connects Nreal output to EasyAR input in the scene, providing Nreal support using custom camera feature of EasyAR Sense.</para>
    /// <para xml:lang="en">This frame source is one type of motion tracking device, and will output motion data in a <see cref="ARSession"/>.</para>
    /// <para xml:lang="en">``Nreal SDK For Unity`` is required to use this frame source, you need to setup Nreal SDK For Unity according to official documents.</para>
    /// <para xml:lang="en"><see cref="CameraRig"/> and <see cref="WorldRoot"/> are required for availability check, they will be automatically picked from scene objects if not setup. <see cref="FrameSource.Camera"/> will be created at runtime and attached to <see cref="CameraRig"/>.</para>
    /// <para xml:lang="zh">在场景中将Nreal的输出连接到EasyAR输入的自定义frame source。通过EasyAR Sense的自定义相机功能提供Nreal支持。</para>
    /// <para xml:lang="zh">这个frame source是一种运动跟踪设备，在<see cref="ARSession"/>中会输出运动数据。</para>
    /// <para xml:lang="zh">为了使用这个frame source， ``Nreal SDK For Unity`` 是必需的。你需要根据官方文档配置Nreal SDK For Unity。</para>
    /// <para xml:lang="zh">在可用性检查中，<see cref="CameraRig"/> 和<see cref="WorldRoot"/> 是需要的，如果没有事先设置，会自动从场景物体中选择。<see cref="FrameSource.Camera"/>会在运行时创建并被附加在<see cref="CameraRig"/>上</para>
    /// </summary>
    public class NrealFrameSource : FrameSource
    {
        /// <summary>
        /// <para xml:lang="en">The distance of the near clipping plane from the the RGB Camera.</para>
        /// <para xml:lang="zh">RGB相机的近裁平面距离。</para>
        /// </summary>
        public float CameraNearClipPlane = 0.3f;
        /// <summary>
        /// <para xml:lang="en">The distance of the far clipping plane from the the RGB Camera.</para>
        /// <para xml:lang="zh">RGB相机的远裁平面距离。</para>
        /// </summary>
        public float CameraFarClipPlane = 1000f;

        [SerializeField, HideInInspector]
        private WorldRootController worldRoot;
        [SerializeField, HideInInspector]
        private NRHMDPoseTracker cameraRig;
        private bool assembled = false;

        /// <summary>
        /// <para xml:lang="en">Nreal CameraRig.</para>
        /// <para xml:lang="zh">Nreal CameraRig。</para>
        /// </summary>
        /// <mutabletype disabled="UnityEngine.MonoBehaviour" enabled="NRKernal.NRHMDPoseTracker"/>
        public NRHMDPoseTracker CameraRig
        {
            get => cameraRig;
            set
            {
                if (assembled) { return; }
                cameraRig = value;
            }
        }

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

        /// <summary>
        /// <para xml:lang="en">Received frame count from Nreal. Usually used for debug. There are usually hardware issues if this value stop to increase, and a device re-plug may help.</para>
        /// <para xml:lang="zh">从Nreal获取到的帧计数。通常在debug中使用。如果这个数值停止增长，通常是硬件问题，重新插拔设备可能能解决。</para>
        /// </summary>
        public int ReceivedFrameCount { get; private set; }
#if EASYAR_ENABLE_NREAL
        private static IReadOnlyList<ARSession.ARCenterMode> availableCenterMode = new List<ARSession.ARCenterMode> { ARSession.ARCenterMode.SessionOrigin };
        private NRCenterCameraDevice nrCamera;
        private CameraParameters cameraParameters;
        private ulong curTimestamp;
        private BufferPool bufferPool;
        private int bufferSize;
        private Vec2I size;
        private Optional<Pose> RGBCameraPose;
        private Action<Pose, MotionTrackingStatus> newFrame;
        private WorldRootController worldRootCache;
        private GameObject worldRootObject;
        private NRHMDPoseTracker cameraRigCache;
        private GameObject cameraObject;
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

        public override bool IsCameraUnderControl { get => false; }

        public override IReadOnlyList<ARSession.ARCenterMode> AvailableCenterMode { get => availableCenterMode; }

        public override bool IsHMD { get => true; }

        protected override void Awake()
        {
            base.Awake();
            if (!worldRoot) { worldRootCache = FindObjectOfType<WorldRootController>(); }
            if (!cameraRig) { cameraRigCache = FindObjectOfType<NRHMDPoseTracker>(); }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            NRKernalUpdater.OnPostUpdate += OnNrealPostUpdate;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            NRKernalUpdater.OnPostUpdate -= OnNrealPostUpdate;
        }

        protected virtual void OnDestroy()
        {
            if (nrCamera != null)
            {
                nrCamera.FrameUpdate -= OnCameraFrameReceived;
                nrCamera.Stop();
            }
            cameraParameters?.Dispose();
            if (worldRootObject) Destroy(worldRootObject);
            if (cameraObject) Destroy(cameraObject);
            bufferPool?.Dispose();
        }

        public override void OnAssemble(ARSession session)
        {
            base.OnAssemble(session);

            nrCamera = new NRCenterCameraDevice();
            nrCamera.Play();
            nrCamera.FrameUpdate += OnCameraFrameReceived;

            var intrinsic = NRFrame.GetRGBCameraIntrinsicMatrix();
            size = new Vec2I(nrCamera.Width, nrCamera.Height);
            cameraParameters = new CameraParameters(size, new Vec2F(intrinsic[0, 0], intrinsic[1, 1]), new Vec2F(intrinsic[0, 2], intrinsic[1, 2]), CameraDeviceType.Back, 0);

            assembled = true;
            SetupOriginUsingWorldRoot();
            StartCoroutine(InitializeCamera(Camera));
        }

        public override IEnumerator CheckAvailability()
        {
            SetupCameraRig();
            isAvailable = cameraRig && cameraRig.gameObject.activeInHierarchy;
            return null;
        }

        public override Camera PickCamera()
        {
            if (!cameraRig) { return null; }
            Debug.Log($"RGB Camera not found, create from {typeof(NrealFrameSource)}");
            cameraObject = new GameObject("RGB Camera", typeof(Camera));
            cameraObject.transform.SetParent(CameraRig.transform, false);
            var camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.enabled = false;
            return camera;
        }

        protected override bool IsValidCamera(Camera cam)
        {
            var rig = CameraRig;
            if (!rig) { rig = FindObjectOfType<NRHMDPoseTracker>(); }
            if (!rig) { return false; }
            return cam != rig.centerCamera && cam != rig.leftCamera && cam != rig.rightCamera && cam.transform.parent == rig.transform;
        }

        private void OnCameraFrameReceived(FrameRawData nrFrame)
        {
            if (nrFrame.data == null || nrFrame.data.Length == 0)
            {
                return;
            }
            var timestamp = nrFrame.timeStamp;
            if (timestamp == curTimestamp) { return; }

            curTimestamp = timestamp;

            if (bufferSize != size.data_0 * size.data_1)
            {
                bufferSize = size.data_0 * size.data_1;
                bufferPool?.Dispose();
                bufferPool = new BufferPool(bufferSize, bufferCapacity);
            }
            var bufferO = bufferPool.tryAcquire();
            if (bufferO.OnNone) { return; }

            var buffer = bufferO.Value;
            buffer.copyFromByteArray(nrFrame.data, 0, 0, bufferSize);
            ReceivedFrameCount++;

            newFrame = (pose, trackingStatus) =>
            {
                using (buffer)
                using (var image = new Image(buffer, PixelFormat.Gray, size.data_0, size.data_1))
                using (var frame = InputFrame.create(image, cameraParameters, timestamp * 1e-9, pose.ToEasyARPose(), trackingStatus))
                {
                    sink.handle(frame);
                }
            };
        }

        private void OnNrealPostUpdate()
        {
            if (RGBCameraPose.OnNone || !NRFrame.isHeadPoseReady) { return; }
            newFrame?.Invoke(RGBCameraPose.Value.GetTransformedBy(NRFrame.HeadPose), NRFrame.SessionStatus == SessionState.Running ? MotionTrackingStatus.Tracking : MotionTrackingStatus.NotTracking);
            newFrame = null;
        }

        private IEnumerator InitializeCamera(Camera rgbCamera)
        {
            rgbCamera.nearClipPlane = CameraNearClipPlane;
            rgbCamera.farClipPlane = CameraFarClipPlane;
            var projection = NRFrame.GetEyeProjectMatrix(out var result, CameraNearClipPlane, CameraFarClipPlane);
            while (!result)
            {
                yield return null;
                projection = NRFrame.GetEyeProjectMatrix(out result, CameraNearClipPlane, CameraFarClipPlane);
            }
            while (NRFrame.SessionStatus != SessionState.Running)
            {
                yield return null;
            }

            var eyeposFromHead = NRFrame.EyePoseFromHead;
            rgbCamera.projectionMatrix = projection.RGBEyeMatrix;
#if EASYAR_ENABLE_NREAL_1_5
            rgbCamera.transform.localPosition = eyeposFromHead.RGBEyePos.position;
            rgbCamera.transform.localRotation = eyeposFromHead.RGBEyePos.rotation;
            RGBCameraPose = eyeposFromHead.RGBEyePos;
#else
            rgbCamera.transform.localPosition = eyeposFromHead.RGBEyePose.position;
            rgbCamera.transform.localRotation = eyeposFromHead.RGBEyePose.rotation;
            RGBCameraPose = eyeposFromHead.RGBEyePose;
#endif
        }

        private void SetupOriginUsingWorldRoot()
        {
            if (worldRoot) { return; }
            worldRoot = worldRootCache;
            if (worldRoot) { return; }
            worldRoot = FindObjectOfType<WorldRootController>();
            if (worldRoot) { return; }
            Debug.Log($"WorldRoot not found, create from {typeof(NrealFrameSource)}");
            worldRootObject = new GameObject("WorldRoot");
            worldRoot = worldRootObject.AddComponent<WorldRootController>();
        }

        private void SetupCameraRig()
        {
            if (cameraRig) { return; }
            cameraRig = cameraRigCache;
            if (cameraRig) { return; }
            cameraRig = FindObjectOfType<NRHMDPoseTracker>();
        }

        class NRCenterCameraDevice : CameraModelView
        {
            public event Action<FrameRawData> FrameUpdate;
            public NRCenterCameraDevice() : base(CameraImageFormat.YUV_420_888) { }
            // from NRKernalUpdater.OnUpdate
            protected override void OnRawDataUpdate(FrameRawData frame) => FrameUpdate?.Invoke(frame);
        }
#else
        public override Optional<bool> IsAvailable { get => false; }
#endif
    }
}
