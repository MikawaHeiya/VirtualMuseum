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
    /// <para xml:lang="en"><see cref="MonoBehaviour"/> which controls VIO camera device (<see cref="MotionTrackerCameraDevice"/>, <see cref="ARKitCameraDevice"/> or <see cref="ARCoreCameraDevice"/>) in the scene, providing a few extensions in the Unity environment. Use <see cref="Device"/> directly when necessary.</para>
    /// <para xml:lang="zh">在场景中控制VIO相机设备（<see cref="MotionTrackerCameraDevice"/>、<see cref="ARKitCameraDevice"/>、<see cref="ARCoreCameraDevice"/>）的<see cref="MonoBehaviour"/>，在Unity环境下提供功能扩展。如有需要可以直接使用<see cref="Device"/>。</para>
    /// </summary>
    [Obsolete("VIOCameraDeviceUnion is obsolete and will be removed in the future. " +
        "Please re-create your AR Session from 'GameObject' menu or context menu in 'Hierarchy' window. " +
        "You can find replacement for VIOCameraDeviceUnion from 'EasyAR Sense -> Motion Tracking -> AR Session (Motion Tracking Preset)'."
        )]
    public class VIOCameraDeviceUnion : FrameSource
    {
        /// <summary>
        /// <para xml:lang="en">EasyAR Sense API (Union). Accessible between <see cref="DeviceCreated"/> and <see cref="DeviceClosed"/> event if available.</para>
        /// <para xml:lang="zh">EasyAR Sense API (Union)，如果功能可以使用，可以在<see cref="DeviceCreated"/>和<see cref="DeviceClosed"/>事件之间访问。</para>
        /// </summary>
        public DeviceUnion Device { get; private set; }

        /// <summary>
        /// <para xml:lang="en">Strategy of choosing VIO device.</para>
        /// <para xml:lang="zh">选择VIO设备的策略。</para>
        /// </summary>
        public DeviceChooseStrategy DeviceStrategy;

        /// <summary>
        /// <para xml:lang="en">Desired motion tracker parameters, used only when <see cref="Device"/> start. Effective only when <see cref="MotionTrackerCameraDevice"/> is used.</para>
        /// <para xml:lang="zh">期望的运动跟踪参数，只在<see cref="Device"/>启动时使用。只有在使用<see cref="MotionTrackerCameraDevice"/>时有效。</para>
        /// </summary>
        public MotionTrackerCameraDeviceParameters DesiredMotionTrackerParameters = new MotionTrackerCameraDeviceParameters();

        /// <summary>
        /// <para xml:lang="en">Timeout in seconds to update device list from server when checking availability using <see cref="CheckAvailability"/>. Set value less than or equal to 0 to skip update from server.</para>
        /// <para xml:lang="zh">使用<see cref="CheckAvailability"/>检查设备是否支持时从服务器更新设备列表的超时时间（秒）。设置数值小于等于 0 可以跳过从服务器获取数据。</para>
        /// </summary>
        public float CalibrationDownloaderTimeout = 2;

        private static string obsoleteMessage = $"{typeof(VIOCameraDeviceUnion)} is obsolete and will be removed in the future. " +
            $"Please re-create your AR Session from 'GameObject' menu or context menu in 'Hierarchy' window. " +
            $"You can find replacement for {typeof(VIOCameraDeviceUnion)} from 'EasyAR Sense -> Motion Tracking -> AR Session (Motion Tracking Preset)'.";
        private Action deviceStart;
        private Action deviceStop;
        private Action deviceClose;
        private Action<int> deviceSetBufferCapacity;
        private Func<int> deviceGetBufferCapacity;
        private Action<InputFrameSink> deviceConnect;
        private bool willOpen;
        private bool disableAutoOpen;
        private Optional<bool> calibrationUpdated;
        private float calibrationDownloadTime;
        private bool assembled;
        [SerializeField, HideInInspector]
        private WorldRootController worldRoot;
        private WorldRootController worldRootCache;
        private GameObject worldRootObject;
        private Optional<bool> isAvailable;
        private bool loadLibrary;

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

        /// <summary>
        /// <para xml:lang="en">Strategy of choosing VIO device.</para>
        /// <para xml:lang="zh">选择VIO设备的策略。</para>
        /// </summary>
        public enum DeviceChooseStrategy
        {
            /// <summary>
            /// <para xml:lang="en">Choose VIO device based on system support，in the order of System VIO device (ARKit/ARCore) > EasyAR Motion Tracker.</para>
            /// <para xml:lang="zh">根据系统对VIO设备支持情况进行选择，优先顺序为 系统VIO设备 (ARKit/ARCore) > EasyAR Motion Tracker。</para>
            /// </summary>
            SystemVIOFirst,
            /// <summary>
            /// <para xml:lang="en">Choose VIO device based on system support，in the order of EasyAR Motion Tracker > System VIO device (ARKit/ARCore)。</para>
            /// <para xml:lang="zh">根据系统对VIO设备支持情况进行选择，优先顺序为 EasyAR Motion Tracker > 系统VIO设备 (ARKit/ARCore)。</para>
            /// </summary>
            EasyARMotionTrackerFirst,
            /// <summary>
            /// <para xml:lang="en">Choose only System VIO device (ARKit/ARCore), do not use EasyAR Motion Tracker.</para>
            /// <para xml:lang="zh">只选择系统VIO设备 (ARKit/ARCore)，不使用EasyAR Motion Tracker。</para>
            /// </summary>
            SystemVIOOnly,
            /// <summary>
            /// <para xml:lang="en">Choose only EasyAR Motion Tracker, do not use System VIO device (ARKit/ARCore).</para>
            /// <para xml:lang="zh">只选择EasyAR Motion Tracker，不使用系统VIO设备 (ARKit/ARCore)。</para>
            /// </summary>
            EasyARMotionTrackerOnly,
        }

        public override Optional<bool> IsAvailable { get { return isAvailable; } }

        public override int BufferCapacity
        {
            get
            {
                if (deviceGetBufferCapacity != null)
                {
                    return deviceGetBufferCapacity();
                }
                return bufferCapacity;
            }
            set
            {
                bufferCapacity = value;
                if (deviceSetBufferCapacity != null)
                {
                    deviceSetBufferCapacity(value);
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
            Debug.LogWarning(obsoleteMessage);
            base.Awake();
            if (worldRoot) { return; }
            worldRootCache = FindObjectOfType<WorldRootController>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (deviceStart != null)
            {
                deviceStart();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (deviceStop != null)
            {
                deviceStop();
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
            if (isAvailable.OnNone && CalibrationDownloaderTimeout > 0 && Application.platform == RuntimePlatform.Android && DeviceStrategy != DeviceChooseStrategy.SystemVIOOnly)
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

            var isMTAvailable = MotionTrackerCameraDevice.isAvailable() && MotionTrackerCameraDevice.getQualityLevel() >= DesiredMotionTrackerParameters.MinQualityLevel;

#if UNITY_ANDROID && !UNITY_EDITOR
            if (DeviceStrategy == DeviceChooseStrategy.SystemVIOFirst || DeviceStrategy == DeviceChooseStrategy.SystemVIOOnly || (DeviceStrategy == DeviceChooseStrategy.EasyARMotionTrackerFirst && !isMTAvailable))
            {
                if (Application.platform == RuntimePlatform.Android && !loadLibrary)
                {
                    loadLibrary = true;
                    try
                    {
                        using (var systemClass = new AndroidJavaClass("java.lang.System"))
                        {
                            systemClass.CallStatic("loadLibrary", "arcore_sdk_c");
                        }
                    }
                    catch (AndroidJavaException)
                    {
                        GUIPopup.EnqueueMessage("Fail to load ARCore library: arcore_sdk_c.so not found", 3);
                    }
                }
            }
#endif

            switch (DeviceStrategy)
            {
                case DeviceChooseStrategy.SystemVIOFirst:
                case DeviceChooseStrategy.EasyARMotionTrackerFirst:
                    isAvailable = isMTAvailable || ARKitCameraDevice.isAvailable() || ARCoreCameraDevice.isAvailable();
                    break;
                case DeviceChooseStrategy.SystemVIOOnly:
                    isAvailable = ARKitCameraDevice.isAvailable() || ARCoreCameraDevice.isAvailable();
                    break;
                case DeviceChooseStrategy.EasyARMotionTrackerOnly:
                    isAvailable = isMTAvailable;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// <para xml:lang="en">Performs ray cast from the user's device in the direction of given screen point. Intersections with horizontal plane is detected in real time in the current field of view,and return the 3D point nearest to ray on horizontal plane. <paramref name="pointInView"/> should be normalized to [0, 1]^2.</para>
        /// <para xml:lang="zh">在当前视野内实时检测到的水平面上进行Hit Test,点击到某个水平面后返回该平面上距离Hit Test射线最近的3D点的位置坐标。<paramref name="pointInView"/> 需要被归一化到[0, 1]^2。</para>
        /// </summary>
        public List<Vector3> HitTestAgainstHorizontalPlane(Vector2 pointInView)
        {
            var points = new List<Vector3>();
            if (Device == null || Device.Type() != typeof(MotionTrackerCameraDevice) || !arSession)
            {
                return points;
            }

            var coord = arSession.ImageCoordinatesFromScreenCoordinates(pointInView);
            if (coord.OnNone)
            {
                return points;
            }
            var hitPoints = Device.MotionTrackerCameraDevice.hitTestAgainstHorizontalPlane(coord.Value.ToEasyARVector());

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
            if (Device == null || Device.Type() != typeof(MotionTrackerCameraDevice) || !arSession)
            {
                return points;
            }

            var coord = arSession.ImageCoordinatesFromScreenCoordinates(pointInView);
            if (coord.OnNone)
            {
                return points;
            }
            var hitPoints = Device.MotionTrackerCameraDevice.hitTestAgainstPointCloud(coord.Value.ToEasyARVector());

            foreach (var p in hitPoints)
            {
                points.Add(new Vector3(p.data_0, p.data_1, -p.data_2));
            }

            return points;
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
                    Debug.LogError("Camera permission not granted");
                    return;
                }

                Close();

                switch (DeviceStrategy)
                {
                    case DeviceChooseStrategy.SystemVIOFirst:
                        if (ARKitCameraDevice.isAvailable())
                        {
                            CreateARKitCameraDevice();
                        }
                        else if (ARCoreCameraDevice.isAvailable())
                        {
                            CreateARCoreCameraDevice();
                        }
                        else if (MotionTrackerCameraDevice.isAvailable())
                        {
                            CreateMotionTrackerCameraDevice();
                        }
                        break;
                    case DeviceChooseStrategy.EasyARMotionTrackerFirst:
                        if (MotionTrackerCameraDevice.isAvailable())
                        {
                            CreateMotionTrackerCameraDevice();
                        }
                        else if (ARKitCameraDevice.isAvailable())
                        {
                            CreateARKitCameraDevice();
                        }
                        else if (ARCoreCameraDevice.isAvailable())
                        {
                            CreateARCoreCameraDevice();
                        }
                        break;
                    case DeviceChooseStrategy.SystemVIOOnly:
                        if (ARKitCameraDevice.isAvailable())
                        {
                            CreateARKitCameraDevice();
                        }
                        else if (ARCoreCameraDevice.isAvailable())
                        {
                            CreateARCoreCameraDevice();
                        }
                        break;
                    case DeviceChooseStrategy.EasyARMotionTrackerOnly:
                        if (MotionTrackerCameraDevice.isAvailable())
                        {
                            CreateMotionTrackerCameraDevice();
                        }
                        break;
                    default:
                        break;
                }
                if (DeviceCreated != null)
                {
                    DeviceCreated();
                }

                if (bufferCapacity != 0)
                {
                    deviceSetBufferCapacity(bufferCapacity);
                }

                if (sink != null)
                    deviceConnect(sink);

                if (enabled)
                {
                    OnEnable();
                }

                if (DeviceOpened != null)
                {
                    DeviceOpened();
                }
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
            if (deviceClose != null)
            {
                OnDisable();
                deviceClose();
                if (DeviceClosed != null)
                {
                    DeviceClosed();
                }

                Device = null;
                deviceStart = null;
                deviceStop = null;
                deviceClose = null;
                deviceSetBufferCapacity = null;
                deviceGetBufferCapacity = null;
                deviceConnect = null;
            }
        }

        public override void Connect(InputFrameSink val)
        {
            base.Connect(val);
            if (deviceConnect != null)
            {
                deviceConnect(val);
            }
        }

        private void CreateMotionTrackerCameraDevice()
        {
            var device = new MotionTrackerCameraDevice();
            deviceStart = () =>
            {
                if (DesiredMotionTrackerParameters != null)
                {
                    device.setFrameRateType(DesiredMotionTrackerParameters.FPS);
                    device.setFocusMode(DesiredMotionTrackerParameters.FocusMode);
                    device.setFrameResolutionType(DesiredMotionTrackerParameters.Resolution);
                    device.setTrackingMode(DesiredMotionTrackerParameters.TrackingMode);
                }
                device.start();
            };
            deviceStop = () =>
            {
                device.stop();
            };
            deviceClose = () =>
            {
                device.close();
                device.Dispose();
            };
            deviceSetBufferCapacity = (int capacity) =>
            {
                device.setBufferCapacity(capacity);
            };
            deviceGetBufferCapacity = () =>
            {
                return device.bufferCapacity();
            };
            deviceConnect = (InputFrameSink sink) =>
            {
                device.inputFrameSource().connect(sink);
            };
            Device = device;
        }

        private void CreateARKitCameraDevice()
        {
            var device = new ARKitCameraDevice();
            deviceStart = () =>
            {
                device.start();
            };
            deviceStop = () =>
            {
                device.stop();
            };
            deviceClose = () =>
            {
                device.close();
                device.Dispose();
            };
            deviceSetBufferCapacity = (int capacity) =>
            {
                device.setBufferCapacity(capacity);
            };
            deviceGetBufferCapacity = () =>
            {
                return device.bufferCapacity();
            };
            deviceConnect = (InputFrameSink sink) =>
            {
                device.inputFrameSource().connect(sink);
            };
            Device = device;
        }

        private void CreateARCoreCameraDevice()
        {
            var device = new ARCoreCameraDevice();
            deviceStart = () =>
            {
                device.start();
            };
            deviceStop = () =>
            {
                device.stop();
            };
            deviceClose = () =>
            {
                device.close();
                device.Dispose();
            };
            deviceSetBufferCapacity = (int capacity) =>
            {
                device.setBufferCapacity(capacity);
            };
            deviceGetBufferCapacity = () =>
            {
                return device.bufferCapacity();
            };
            deviceConnect = (InputFrameSink sink) =>
            {
                device.inputFrameSource().connect(sink);
            };
            Device = device;
        }

        private IEnumerator AutoOpen()
        {
            while (!enabled)
            {
                if (disableAutoOpen) { yield break; }
                yield return null;
            }
            if (disableAutoOpen) { yield break; }

            switch (DeviceStrategy)
            {
                case DeviceChooseStrategy.SystemVIOFirst:
                    if (!MotionTrackerCameraDevice.isAvailable() && !ARKitCameraDevice.isAvailable() && !ARCoreCameraDevice.isAvailable())
                    {
                        throw new UIPopupException("VIOCameraDevice not available");
                    }
                    if (!ARKitCameraDevice.isAvailable() && !ARCoreCameraDevice.isAvailable() && MotionTrackerCameraDevice.isAvailable() && MotionTrackerCameraDevice.getQualityLevel() < DesiredMotionTrackerParameters.MinQualityLevel)
                    {
                        throw new UIPopupException("VIOCameraDevice available but disabled with quality level (" + MotionTrackerCameraDevice.getQualityLevel() + "), min level is set to " + DesiredMotionTrackerParameters.MinQualityLevel);
                    }
                    break;
                case DeviceChooseStrategy.EasyARMotionTrackerFirst:
                    if (!MotionTrackerCameraDevice.isAvailable() && !ARKitCameraDevice.isAvailable() && !ARCoreCameraDevice.isAvailable())
                    {
                        throw new UIPopupException("VIOCameraDevice not available");
                    }
                    if (MotionTrackerCameraDevice.isAvailable() && MotionTrackerCameraDevice.getQualityLevel() < DesiredMotionTrackerParameters.MinQualityLevel)
                    {
                        throw new UIPopupException("VIOCameraDevice available but disabled with quality level (" + MotionTrackerCameraDevice.getQualityLevel() + "), min level is set to " + DesiredMotionTrackerParameters.MinQualityLevel);
                    }
                    break;
                case DeviceChooseStrategy.SystemVIOOnly:
                    if (!ARKitCameraDevice.isAvailable() && Application.platform == RuntimePlatform.IPhonePlayer)
                    {
                        throw new UIPopupException(typeof(ARKitCameraDevice) + " not available");
                    }
                    else if (!ARCoreCameraDevice.isAvailable() && Application.platform == RuntimePlatform.Android)
                    {
                        throw new UIPopupException(typeof(ARCoreCameraDevice) + " not available");
                    }
                    else if (!ARKitCameraDevice.isAvailable() && !ARCoreCameraDevice.isAvailable())
                    {
                        throw new UIPopupException("System VIO not available");
                    }
                    break;
                case DeviceChooseStrategy.EasyARMotionTrackerOnly:
                    if (!MotionTrackerCameraDevice.isAvailable())
                    {
                        throw new UIPopupException(typeof(MotionTrackerCameraDevice) + " not available");
                    }
                    if (MotionTrackerCameraDevice.getQualityLevel() < DesiredMotionTrackerParameters.MinQualityLevel)
                    {
                        throw new UIPopupException(typeof(MotionTrackerCameraDevice) + " available but disabled with quality level (" + MotionTrackerCameraDevice.getQualityLevel() + "), min level is set to " + DesiredMotionTrackerParameters.MinQualityLevel);
                    }
                    break;
                default:
                    break;
            }

            Open();
        }

        private void SetupOriginUsingWorldRoot()
        {
            if (worldRoot) { return; }
            worldRoot = worldRootCache;
            if (worldRoot) { return; }
            worldRoot = FindObjectOfType<WorldRootController>();
            if (worldRoot) { return; }
            Debug.Log($"WorldRoot not found, create from {typeof(VIOCameraDeviceUnion)}");
            worldRootObject = new GameObject("WorldRoot");
            worldRoot = worldRootObject.AddComponent<WorldRootController>();
        }

        /// <summary>
        /// <para xml:lang="en">VIO device Union.</para>
        /// <para xml:lang="zh">VIO设备的集合。</para>
        /// </summary>
        public class DeviceUnion
        {
            private MotionTrackerCameraDevice motionTrackerCameraDevice;
            private ARKitCameraDevice arKitCameraDevice;
            private ARCoreCameraDevice arCoreCameraDevice;

            public DeviceUnion(MotionTrackerCameraDevice value) { motionTrackerCameraDevice = value; DeviceType = VIODeviceType.EasyARMotionTracker; }
            public DeviceUnion(ARKitCameraDevice value) { arKitCameraDevice = value; DeviceType = VIODeviceType.ARKit; }
            public DeviceUnion(ARCoreCameraDevice value) { arCoreCameraDevice = value; DeviceType = VIODeviceType.ARCore; }

            /// <summary>
            /// <para xml:lang="en">VIO device type.</para>
            /// <para xml:lang="zh">VIO设备类型。</para>
            /// </summary>
            public enum VIODeviceType
            {
                EasyARMotionTracker,
                ARKit,
                ARCore,
            }

            /// <summary>
            /// <para xml:lang="en">VIO device type.</para>
            /// <para xml:lang="zh">VIO设备类型。</para>
            /// </summary>
            public VIODeviceType DeviceType { get; private set; }

            /// <summary>
            /// <para xml:lang="en">Get <see cref="easyar.MotionTrackerCameraDevice"/>. <see cref="InvalidCastException"/> will be thronw if type error.</para>
            /// <para xml:lang="zh">获取<see cref="easyar.MotionTrackerCameraDevice"/>，如Union非此类型，会抛出<see cref="InvalidCastException"/>。</para>
            /// </summary>
            public MotionTrackerCameraDevice MotionTrackerCameraDevice
            {
                get { if (DeviceType != VIODeviceType.EasyARMotionTracker) throw new InvalidCastException(); ; return motionTrackerCameraDevice; }
                set { motionTrackerCameraDevice = value; DeviceType = VIODeviceType.EasyARMotionTracker; }
            }

            /// <summary>
            /// <para xml:lang="en">Get <see cref="easyar.ARKitCameraDevice"/>. <see cref="InvalidCastException"/> will be thronw if type error.</para>
            /// <para xml:lang="zh">获取<see cref="easyar.ARKitCameraDevice"/>，如Union非此类型，会抛出<see cref="InvalidCastException"/>。</para>
            /// </summary>
            public ARKitCameraDevice ARKitCameraDevice
            {
                get { if (DeviceType != VIODeviceType.ARKit) throw new InvalidCastException(); ; return arKitCameraDevice; }
                set { arKitCameraDevice = value; DeviceType = VIODeviceType.ARKit; }
            }

            /// <summary>
            /// <para xml:lang="en">Get <see cref="easyar.ARCoreCameraDevice"/>. <see cref="InvalidCastException"/> will be thronw if type error.</para>
            /// <para xml:lang="zh">获取<see cref="easyar.ARCoreCameraDevice"/>，如Union非此类型，会抛出<see cref="InvalidCastException"/>。</para>
            /// </summary>
            public ARCoreCameraDevice ARCoreCameraDevice
            {
                get { if (DeviceType != VIODeviceType.ARCore) throw new InvalidCastException(); return arCoreCameraDevice; }
                set { arCoreCameraDevice = value; DeviceType = VIODeviceType.ARCore; }
            }

            public static explicit operator MotionTrackerCameraDevice(DeviceUnion value) { return value.MotionTrackerCameraDevice; }
            public static explicit operator ARKitCameraDevice(DeviceUnion value) { return value.ARKitCameraDevice; }
            public static explicit operator ARCoreCameraDevice(DeviceUnion value) { return value.ARCoreCameraDevice; }

            public static implicit operator DeviceUnion(MotionTrackerCameraDevice value) { return new DeviceUnion(value); }
            public static implicit operator DeviceUnion(ARKitCameraDevice value) { return new DeviceUnion(value); }
            public static implicit operator DeviceUnion(ARCoreCameraDevice value) { return new DeviceUnion(value); }

            /// <summary>
            /// <para xml:lang="en">Underlying data type.</para>
            /// <para xml:lang="zh">Union内部数据类型。</para>
            /// </summary>
            public Type Type()
            {
                switch (DeviceType)
                {
                    case VIODeviceType.EasyARMotionTracker:
                        return typeof(MotionTrackerCameraDevice);
                    case VIODeviceType.ARKit:
                        return typeof(ARKitCameraDevice);
                    case VIODeviceType.ARCore:
                        return typeof(ARCoreCameraDevice);
                    default: return typeof(void);
                }
            }

            public override string ToString()
            {
                switch (DeviceType)
                {
                    case VIODeviceType.EasyARMotionTracker:
                        return motionTrackerCameraDevice.ToString();
                    case VIODeviceType.ARKit:
                        return arKitCameraDevice.ToString();
                    case VIODeviceType.ARCore:
                        return arCoreCameraDevice.ToString();
                    default:
                        return "void";
                }
            }
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
