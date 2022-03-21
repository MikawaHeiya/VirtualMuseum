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
    /// <para xml:lang="en"><see cref="MonoBehaviour"/> which controls <see cref="CameraDevice"/> in the scene, providing a few extensions in the Unity environment. Use <see cref="Device"/> directly when necessary.</para>
    /// <para xml:lang="en">This frame source is not a motion tracking device, and will not output motion data in a <see cref="ARSession"/>.</para>
    /// <para xml:lang="en">To choose frame source in runtime, you can deactive Camera GameObject and set all required values of all frame sources for availability check, and active Camera GameObject when this frame source is chosen.</para>
    /// <para xml:lang="zh">在场景中控制<see cref="CameraDevice"/>的<see cref="MonoBehaviour"/>，在Unity环境下提供功能扩展。如有需要可以直接使用<see cref="Device"/>。</para>
    /// <para xml:lang="zh">这个frame source不是运动跟踪设备，在<see cref="ARSession"/>中不会输出运动数据。</para>
    /// <para xml:lang="zh">如果要在运行时选择 frame source，可以deactive Camera GameObject，并设置所有frame source可用性检查所需要的数值，然后在这个frame source被选择后active Camera GameObject。</para>
    /// </summary>
    public class CameraDeviceFrameSource : FrameSource
    {
        /// <summary>
        /// <para xml:lang="en">EasyAR Sense API. Accessible between <see cref="DeviceCreated"/> and <see cref="DeviceClosed"/> event if available.</para>
        /// <para xml:lang="zh">EasyAR Sense API，如果功能可以使用，可以在<see cref="DeviceCreated"/>和<see cref="DeviceClosed"/>事件之间访问。</para>
        /// </summary>
        /// <senseapi/>
        public CameraDevice Device { get; private set; }

        /// <summary>
        /// <para xml:lang="en">Focus mode used only when create <see cref="Device"/>.</para>
        /// <para xml:lang="zh">创建<see cref="Device"/>时使用的聚焦模式，只在创建时使用。</para>
        /// </summary>
        public CameraDeviceFocusMode FocusMode = CameraDeviceFocusMode.Continousauto;

        /// <summary>
        /// <para xml:lang="en">Camera preview size used only when create <see cref="Device"/>.</para>
        /// <para xml:lang="zh">创建<see cref="Device"/>时使用的图像大小，只在创建时使用。</para>
        /// </summary>
        public Vector2 CameraSize = new Vector2(1280, 960);

        /// <summary>
        /// <para xml:lang="en">Camera open method used only when create <see cref="Device"/>.</para>
        /// <para xml:lang="zh">创建<see cref="Device"/>时使用的方法，只在创建时使用。</para>
        /// </summary>
        public CameraDeviceOpenMethod CameraOpenMethod = CameraDeviceOpenMethod.DeviceType;

        /// <summary>
        /// <para xml:lang="en">Camera type used only when create <see cref="Device"/>, used when <see cref="CameraOpenMethod"/> == <see cref="CameraDeviceOpenMethod.DeviceType"/>.</para>
        /// <para xml:lang="zh">创建<see cref="Device"/>时使用的Camera类型，只在创建时<see cref="CameraOpenMethod"/> == <see cref="CameraDeviceOpenMethod.DeviceType"/>的时候使用。</para>
        /// </summary>
        [HideInInspector, SerializeField]
        public CameraDeviceType CameraType = CameraDeviceType.Back;

        /// <summary>
        /// <para xml:lang="en">Camera index used only when create <see cref="Device"/>, used when <see cref="CameraOpenMethod"/> == <see cref="CameraDeviceOpenMethod.DeviceIndex"/>.</para>
        /// <para xml:lang="zh">创建<see cref="Device"/>时使用的设备索引，只在创建时<see cref="CameraOpenMethod"/> == <see cref="CameraDeviceOpenMethod.DeviceIndex"/>的时候使用。</para>
        /// </summary>
        [HideInInspector, SerializeField]
        public int CameraIndex = 0;

        private static IReadOnlyList<ARSession.ARCenterMode> availableCenterMode = new List<ARSession.ARCenterMode> { ARSession.ARCenterMode.FirstTarget, ARSession.ARCenterMode.Camera, ARSession.ARCenterMode.SpecificTarget };
        [HideInInspector, SerializeField]
        private CameraDevicePreference cameraPreference = CameraDevicePreference.PreferObjectSensing;
        private CameraParameters parameters = null;
        private bool willOpen;
        private bool disableAutoOpen;

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
        /// <para xml:lang="en">Open method of <see cref="CameraDevice"/>.</para>
        /// <para xml:lang="zh"><see cref="CameraDevice"/>开启方式。</para>
        /// </summary>
        public enum CameraDeviceOpenMethod
        {
            /// <summary>
            /// <para xml:lang="en">Open <see cref="CameraDevice"/> type.</para>
            /// <para xml:lang="zh">根据<see cref="CameraDevice"/>的类型打开<see cref="CameraDevice"/>。</para>
            /// </summary>
            DeviceType,
            /// <summary>
            /// <para xml:lang="en">Open <see cref="CameraDevice"/> index.</para>
            /// <para xml:lang="zh">根据<see cref="CameraDevice"/>的索引打开<see cref="CameraDevice"/>。</para>
            /// </summary>
            DeviceIndex,
        }

        public override Optional<bool> IsAvailable { get => CameraDevice.isAvailable(); }

        public override IReadOnlyList<ARSession.ARCenterMode> AvailableCenterMode { get => availableCenterMode; }

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
        /// <para xml:lang="en">Camera preference used only when create <see cref="Device"/>. It will switch focus mode to the preferred value, change the focus after this value changed if it not the desired case.</para>
        /// <para xml:lang="zh">创建<see cref="Device"/>时使用的Camera偏好设置，只在创建时使用。它会同时控制对焦模式到推荐使用值，如果需要使用特定对焦模式，需要在修改这个值之后重新设置对焦模式。</para>
        /// </summary>
        public CameraDevicePreference CameraPreference
        {
            get { return cameraPreference; }

            // Switch to preferred FocusMode when switch CameraPreference.
            // You can set other FocusMode after this, but the tracking results may differ.
            set
            {
                cameraPreference = value;
                FocusMode = CameraDeviceSelector.getFocusMode(cameraPreference);
            }
        }

        /// <summary>
        /// <para xml:lang="en">Camera parameters used only when create <see cref="Device"/>. It is for advanced usage and will overwrite other values like <see cref="CameraSize"/>.</para>
        /// <para xml:lang="zh">创建<see cref="Device"/>时使用的相机参数，只在创建时使用。这个参数是高级设置，会覆盖<see cref="CameraSize"/>等其它值。</para>
        /// </summary>
        public CameraParameters Parameters
        {
            get
            {
                if (Device != null)
                {
                    return Device.cameraParameters();
                }
                return parameters;
            }
            set
            {
                parameters = value;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Device != null)
            {
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
        }

        public override void OnAssemble(ARSession session)
        {
            base.OnAssemble(session);
            StartCoroutine(AutoOpen());
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
                Device = CameraDeviceSelector.createCameraDevice(CameraPreference);
                if (DeviceCreated != null)
                {
                    DeviceCreated();
                }

                bool openResult = false;
                switch (CameraOpenMethod)
                {
                    case CameraDeviceOpenMethod.DeviceType:
                        openResult = Device.openWithPreferredType(CameraType);
                        break;
                    case CameraDeviceOpenMethod.DeviceIndex:
                        openResult = Device.openWithIndex(CameraIndex);
                        break;
                    default:
                        break;
                }
                if (!openResult)
                {
                    Debug.LogError("Camera open failed");
                    Device.Dispose();
                    Device = null;
                    return;
                }

                Device.setFocusMode(FocusMode);
                Device.setSize(new Vec2I((int)CameraSize.x, (int)CameraSize.y));
                if (parameters != null)
                {
                    Device.setCameraParameters(parameters);
                }
                if (bufferCapacity != 0)
                {
                    Device.setBufferCapacity(bufferCapacity);
                }

                if (sink != null)
                {
                    Device.inputFrameSource().connect(sink);
                }

                if (DeviceOpened != null)
                {
                    DeviceOpened();
                }

                if (enabled)
                {
                    OnEnable();
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
            if (Device != null)
            {
                OnDisable();
                Device.close();
                Device.Dispose();
                if (DeviceClosed != null)
                {
                    DeviceClosed();
                }
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

        private IEnumerator AutoOpen()
        {
            while (!enabled)
            {
                if (disableAutoOpen) { yield break; }
                yield return null;
            }
            if (disableAutoOpen) { yield break; }
            if (IsAvailable.OnNone || !IsAvailable.Value) { throw new UIPopupException(typeof(CameraDevice) + " not available"); }
            Open();
        }
    }
}
