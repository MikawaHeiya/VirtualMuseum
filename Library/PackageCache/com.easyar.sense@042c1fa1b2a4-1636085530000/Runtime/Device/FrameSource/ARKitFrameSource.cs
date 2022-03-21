//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using System;
using System.Collections;
using UnityEngine;

namespace easyar
{
    /// <summary>
    /// <para xml:lang="en"><see cref="MonoBehaviour"/> which controls ARKit camera device (<see cref="ARKitCameraDevice"/>) in the scene, providing a few extensions in the Unity environment. Use <see cref="Device"/> directly when necessary.</para>
    /// <para xml:lang="en">This frame source is one type of motion tracking device, and will output motion data in a <see cref="ARSession"/>.</para>
    /// <para xml:lang="en">To choose frame source in runtime, you can deactive Camera GameObject and set all required values of all frame sources for availability check, and active Camera GameObject when this frame source is chosen.</para>
    /// <para xml:lang="zh">在场景中控制ARKit相机设备（<see cref="ARKitCameraDevice"/>）的<see cref="MonoBehaviour"/>，在Unity环境下提供功能扩展。如有需要可以直接使用<see cref="Device"/>。</para>
    /// <para xml:lang="zh">这个frame source是一种运动跟踪设备，在<see cref="ARSession"/>中会输出运动数据。</para>
    /// <para xml:lang="zh">如果要在运行时选择 frame source，可以deactive Camera GameObject，并设置所有frame source可用性检查所需要的数值，然后在这个frame source被选择后active Camera GameObject。</para>
    /// </summary>
    public class ARKitFrameSource : FrameSource
    {
        /// <summary>
        /// <para xml:lang="en">EasyAR Sense API. Accessible between <see cref="DeviceCreated"/> and <see cref="DeviceClosed"/> event if available.</para>
        /// <para xml:lang="zh">EasyAR Sense API，如果功能可以使用，可以在<see cref="DeviceCreated"/>和<see cref="DeviceClosed"/>事件之间访问。</para>
        /// </summary>
        /// <senseapi/>
        public ARKitCameraDevice Device { get; private set; }

        public bool AutoFocus = true;

        private bool willOpen;
        private bool disableAutoOpen;
        private bool assembled;
        [SerializeField, HideInInspector]
        private WorldRootController worldRoot;
        private WorldRootController worldRootCache;
        private GameObject worldRootObject;

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

        public override Optional<bool> IsAvailable { get => ARKitCameraDevice.isAvailable(); }

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
                Device.setFocusMode(AutoFocus ? ARKitCameraDeviceFocusMode.Auto : ARKitCameraDeviceFocusMode.Fixed);
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

                Device = new ARKitCameraDevice();
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

        private IEnumerator AutoOpen()
        {
            while (!enabled)
            {
                if (disableAutoOpen) { yield break; }
                yield return null;
            }
            if (disableAutoOpen) { yield break; }
            if (IsAvailable.OnNone || !IsAvailable.Value) { throw new UIPopupException(typeof(ARKitCameraDevice) + " not available"); }
            Open();
        }

        private void SetupOriginUsingWorldRoot()
        {
            if (worldRoot) { return; }
            worldRoot = worldRootCache;
            if (worldRoot) { return; }
            worldRoot = FindObjectOfType<WorldRootController>();
            if (worldRoot) { return; }
            Debug.Log($"WorldRoot not found, create from {typeof(ARKitFrameSource)}");
            worldRootObject = new GameObject("WorldRoot");
            worldRoot = worldRootObject.AddComponent<WorldRootController>();
        }
    }
}
