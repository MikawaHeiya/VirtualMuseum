//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace easyar
{
    /// <summary>
    /// <para xml:lang="en"><see cref="MonoBehaviour"/> which controls <see cref="MotionTrackerCameraDevice"/> in the scene, providing a few extensions in the Unity environment. Use <see cref="Device"/> directly when necessary.</para>
    /// <para xml:lang="en">This frame source is one type of motion tracking device, and will output motion data in a <see cref="ARSession"/>.</para>
    /// <para xml:lang="en">This frame source will download device list from server when checking availability, timeout in <see cref="CalibrationDownloaderTimeout"/> seconds. Device list is distributed more frequently than SDK, so some unsupported devices may happen to be supported later without repacking your application. You can disable device list update by setting <see cref="CalibrationDownloaderTimeout"/> to a value less than or equal to 0.</para>
    /// <para xml:lang="en">To choose frame source in runtime, you can deactive Camera GameObject and set all required values of all frame sources for availability check, and active Camera GameObject when this frame source is chosen.</para>
    /// <para xml:lang="zh">在场景中控制<see cref="MotionTrackerCameraDevice"/>的<see cref="MonoBehaviour"/>，在Unity环境下提供功能扩展。如有需要可以直接使用<see cref="Device"/>。</para>
    /// <para xml:lang="zh">这个frame source是一种运动跟踪设备，在<see cref="ARSession"/>中会输出运动数据。</para>
    /// <para xml:lang="zh">这个frame source 会在检查可用性时从服务器下载设备列表，超时时间为<see cref="CalibrationDownloaderTimeout"/> 秒。设备列表比SDK的发布更加频繁，因此一些一开始不被支持的设备可能在之后被支持，而无需重新打包应用。设置<see cref="CalibrationDownloaderTimeout"/>数值小于等于 0可以关闭设备列表更新。</para>
    /// <para xml:lang="zh">如果要在运行时选择 frame source，可以deactive Camera GameObject，并设置所有frame source可用性检查所需要的数值，然后在这个frame source被选择后active Camera GameObject。</para>
    /// </summary>
    public class MotionTrackerFrameSource : FrameSource
    {
        /// <summary>
        /// <para xml:lang="en">EasyAR Sense API. Accessible between <see cref="DeviceCreated"/> and <see cref="DeviceClosed"/> event if available.</para>
        /// <para xml:lang="zh">EasyAR Sense API，如果功能可以使用，可以在<see cref="DeviceCreated"/>和<see cref="DeviceClosed"/>事件之间访问。</para>
        /// </summary>
        /// <senseapi/>
        public MotionTrackerCameraDevice Device { get; private set; }

        /// <summary>
        /// <para xml:lang="en">Desired motion tracker parameters, used only when <see cref="Device"/> start.</para>
        /// <para xml:lang="zh">期望的运动跟踪参数，只在<see cref="Device"/>启动时使用。</para>
        /// </summary>
        public MotionTrackerCameraDeviceParameters DesiredMotionTrackerParameters = new MotionTrackerCameraDeviceParameters();

        /// <summary>
        /// <para xml:lang="en">Timeout in seconds to update device list from server when checking availability using <see cref="CheckAvailability"/>. Set value less than or equal to 0 to skip update from server.</para>
        /// <para xml:lang="zh">使用<see cref="CheckAvailability"/>检查设备是否支持时从服务器更新设备列表的超时时间（秒）。设置数值小于等于 0 可以跳过从服务器获取数据。</para>
        /// </summary>
        public float CalibrationDownloaderTimeout = 2;

        private bool willOpen;
        private bool disableAutoOpen;
        private bool assembled;
        [SerializeField, HideInInspector]
        private WorldRootController worldRoot;
        private WorldRootController worldRootCache;
        private GameObject worldRootObject;
        private Optional<bool> isAvailable;

        /// <summary>
        /// <para xml:lang="en">Event when <see cref="Device"/> created.</para>
        /// <para xml:lang="zh"><see cref="Device"/> 创建的事件。</para>
        /// </summary>
        public event Action DeviceCreated;
        /// <summary>
        /// <para xml:lang="en">Event when <see cref="Device"/> opened.</para>
        /// <para xml:lang="zh"><see cref="Device"/> 打开的事件。</para>
        /// </summary>
        public event Action DeviceOpened;
        /// <summary>
        /// <para xml:lang="en">Event when <see cref="Device"/> closed.</para>
        /// <para xml:lang="zh"><see cref="Device"/> 关闭的事件。</para>
        /// </summary>
        public event Action DeviceClosed;

        public override Optional<bool> IsAvailable { get { return isAvailable; } }

        public override int BufferCapacity
        {
            get
            {
                if (Device != null)
                {
                    return Device.bufferCapacity();
                }
                return bufferCapacity;
            }
            set
            {
                bufferCapacity = value;
                if (Device != null)
                {
                    Device.setBufferCapacity(value);
                }
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

        public override GameObject Origin { get => worldRoot ? worldRoot.gameObject : null; }

        protected override void Awake()
        {
            base.Awake();
            if (worldRoot) { return; }
            worldRootCache = FindObjectOfType<WorldRootController>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Device != null)
            {
                if (DesiredMotionTrackerParameters != null)
                {
                    Device.setFrameRateType(DesiredMotionTrackerParameters.FPS);
                    Device.setFocusMode(DesiredMotionTrackerParameters.FocusMode);
                    Device.setFrameResolutionType(DesiredMotionTrackerParameters.Resolution);
                    Device.setTrackingMode(DesiredMotionTrackerParameters.TrackingMode);
                }
                Device.start();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (Device != null)
            {
                Device.stop();
            }
        }

        protected virtual void OnDestroy()
        {
            Close();
            if (worldRootObject) Destroy(worldRootObject);
        }

        public override void OnAssemble(ARSession session)
        {
            base.OnAssemble(session);
            SetupOriginUsingWorldRoot();
            StartCoroutine(AutoOpen());
            assembled = true;
        }

        public override IEnumerator CheckAvailability()
        {
            if (isAvailable.OnNone && CalibrationDownloaderTimeout > 0 && Application.platform == RuntimePlatform.Android)
            {
                var available = MotionTrackerCameraDevice.isAvailable();
                var tStart = Time.time;
                var finish = false;
                var downloader = new CalibrationDownloader();
                downloader.download(EasyARController.Scheduler, (status, error) =>
                {
                    finish = true;
                    if (status != CalibrationDownloadStatus.Successful && status != CalibrationDownloadStatus.NotModified)
                    {
                        Debug.LogWarning($"calibration update {status}: {error}");
                    }
                    downloader.Dispose();
                });
                while (!available && !finish && Time.time - tStart < CalibrationDownloaderTimeout) { yield return null; }
            }

            isAvailable = MotionTrackerCameraDevice.isAvailable() && MotionTrackerCameraDevice.getQualityLevel() >= DesiredMotionTrackerParameters.MinQualityLevel;
        }

        /// <summary>
        /// <para xml:lang="en">Open device.</para>
        /// <para xml:lang="zh">打开设备。</para>
        /// </summary>
        public void Open()
        {
            disableAutoOpen = true;
            willOpen = true;
            CameraDevice.requestPermissions(EasyARController.Scheduler, (Action<PermissionStatus, string>)((status, msg) =>
            {
                if (!willOpen)
                {
                    return;
                }
                if (status != PermissionStatus.Granted)
                {
                    throw new UIPopupException("Camera permission not granted");
                }

                Close();

                Device = new MotionTrackerCameraDevice();
                DeviceCreated?.Invoke();

                if (bufferCapacity != 0)
                {
                    Device.setBufferCapacity(bufferCapacity);
                }

                if (sink != null)
                {
                    Device.inputFrameSource().connect(sink);
                }

                if (enabled)
                {
                    OnEnable();
                }

                DeviceOpened?.Invoke();
            }));
        }

        /// <summary>
        /// <para xml:lang="en">Close device.</para>
        /// <para xml:lang="zh">关闭设备。</para>
        /// </summary>
        public void Close()
        {
            disableAutoOpen = true;
            willOpen = false;
            if (Device != null)
            {
                OnDisable();
                Device.close();
                Device.Dispose();
                DeviceClosed?.Invoke();

                Device = null;
            }
        }

        public override void Connect(InputFrameSink val)
        {
            base.Connect(val);
            if (Device != null)
            {
                Device.inputFrameSource().connect(val);
            }
        }

        /// <summary>
        /// <para xml:lang="en">Performs ray cast from the user's device in the direction of given screen point. Intersections with horizontal plane is detected in real time in the current field of view,and return the 3D point nearest to ray on horizontal plane. <paramref name="pointInView"/> should be normalized to [0, 1]^2.</para>
        /// <para xml:lang="zh">在当前视野内实时检测到的水平面上进行Hit Test,点击到某个水平面后返回该平面上距离Hit Test射线最近的3D点的位置坐标。<paramref name="pointInView"/> 需要被归一化到[0, 1]^2。</para>
        /// </summary>
        public List<Vector3> HitTestAgainstHorizontalPlane(Vector2 pointInView)
        {
            var points = new List<Vector3>();
            if (Device == null || !arSession)
            {
                return points;
            }

            var coord = arSession.ImageCoordinatesFromScreenCoordinates(pointInView);
            if (coord.OnNone)
            {
                return points;
            }
            var hitPoints = Device.hitTestAgainstHorizontalPlane(coord.Value.ToEasyARVector());

            foreach (var p in hitPoints)
            {
                points.Add(new Vector3(p.data_0, p.data_1, -p.data_2));
            }

            return points;
        }

        /// <summary>
        /// <para xml:lang="en">Perform hit test against the point cloud and return the nearest 3D point. <paramref name="pointInView"/> should be normalized to [0, 1]^2.</para>
        /// <para xml:lang="zh">在当前点云中进行Hit Test,得到距离相机从近到远一条射线上的最近的一个3D点位置坐标。<paramref name="pointInView"/> 需要被归一化到[0, 1]^2。</para>
        /// </summary>
        public List<Vector3> HitTestAgainstPointCloud(Vector2 pointInView)
        {
            var points = new List<Vector3>();
            if (Device == null || !arSession)
            {
                return points;
            }

            var coord = arSession.ImageCoordinatesFromScreenCoordinates(pointInView);
            if (coord.OnNone)
            {
                return points;
            }
            var hitPoints = Device.hitTestAgainstPointCloud(coord.Value.ToEasyARVector());

            foreach (var p in hitPoints)
            {
                points.Add(new Vector3(p.data_0, p.data_1, -p.data_2));
            }

            return points;
        }

        private IEnumerator AutoOpen()
        {
            while (!enabled)
            {
                if (disableAutoOpen) { yield break; }
                yield return null;
            }
            if (disableAutoOpen) { yield break; }
            if (IsAvailable.OnNone || !IsAvailable.Value) { throw new UIPopupException(typeof(MotionTrackerCameraDevice) + $" not available for quality level {DesiredMotionTrackerParameters.MinQualityLevel}"); }
            Open();
        }

        private void SetupOriginUsingWorldRoot()
        {
            if (worldRoot) { return; }
            worldRoot = worldRootCache;
            if (worldRoot) { return; }
            worldRoot = FindObjectOfType<WorldRootController>();
            if (worldRoot) { return; }
            Debug.Log($"WorldRoot not found, create from {typeof(MotionTrackerFrameSource)}");
            worldRootObject = new GameObject("WorldRoot");
            worldRoot = worldRootObject.AddComponent<WorldRootController>();
        }

        /// <summary>
        /// <para xml:lang="en">Motion tracker parameters.</para>
        /// <para xml:lang="zh">运动跟踪参数。</para>
        /// </summary>
        [Serializable]
        public class MotionTrackerCameraDeviceParameters
        {
            /// <summary>
            /// <para xml:lang="en">Device frame rate.</para>
            /// <para xml:lang="zh">设备帧率。</para>
            /// </summary>
            public MotionTrackerCameraDeviceFPS FPS;
            /// <summary>
            /// <para xml:lang="en">Focus mode.</para>
            /// <para xml:lang="zh">对焦模式。</para>
            /// </summary>
            public MotionTrackerCameraDeviceFocusMode FocusMode;
            /// <summary>
            /// <para xml:lang="en">Frame resolution.</para>
            /// <para xml:lang="zh">分辨率。</para>
            /// </summary>
            public MotionTrackerCameraDeviceResolution Resolution;
            /// <summary>
            /// <para xml:lang="en">Tracking mode.</para>
            /// <para xml:lang="zh">跟踪模式。</para>
            /// </summary>
            public MotionTrackerCameraDeviceTrackingMode TrackingMode = MotionTrackerCameraDeviceTrackingMode.Anchor;
            /// <summary>
            /// <para xml:lang="en">Minimum allowed quality level on the device.</para>
            /// <para xml:lang="zh">最低允许的质量级别。</para>
            /// </summary>
            public MotionTrackerCameraDeviceQualityLevel MinQualityLevel;
        }
    }
}
