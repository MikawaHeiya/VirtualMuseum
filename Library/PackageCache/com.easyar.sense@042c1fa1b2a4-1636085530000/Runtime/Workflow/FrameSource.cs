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
using System.Linq;
using UnityEngine;

namespace easyar
{
    /// <summary>
    /// <para xml:lang="en">Abstracts frame source, used when assemble, to provide input frame data to the algorithms.</para>
    /// <para xml:lang="zh">抽象frame源，在组装时使用，提供算法所需的frame输入数据。</para>
    /// </summary>
    [RequireComponent(typeof(RenderCameraController), typeof(CameraImageRenderer))]
    public abstract class FrameSource : MonoBehaviour
    {
        /// <summary>
        /// <para xml:lang="en">Input port connected.</para>
        /// <para xml:lang="en" access="internal">WARNING: use this member inside frame source only when defining a new custom camera.</para>
        /// <para xml:lang="zh">连接着的输入端口。</para>
        /// <para xml:lang="zh" access="internal">警告：仅在定义新的自定义相机时在frame source内部使用这个成员。</para>
        /// </summary>
        protected InputFrameSink sink;
        /// <summary>
        /// <para xml:lang="en">Current connected ARSession.</para>
        /// <para xml:lang="en" access="internal">WARNING: use this member inside frame source only when defining a new custom camera.</para>
        /// <para xml:lang="zh">当前连接的ARSession。</para>
        /// <para xml:lang="zh" access="internal">警告：仅在定义新的自定义相机时在frame source内部使用这个成员。</para>
        /// </summary>
        protected ARSession arSession;
        /// <summary>
        /// <para xml:lang="en" access="internal">WARNING: use this member inside frame source only when defining a new custom camera.</para>
        /// <para xml:lang="zh" access="internal">警告：仅在定义新的自定义相机时在frame source内部使用这个成员。</para>
        /// </summary>
        protected int bufferCapacity;
        /// <summary>
        /// <para xml:lang="en" access="internal">WARNING: use this member inside frame source only when defining a new custom camera.</para>
        /// <para xml:lang="zh" access="internal">警告：仅在定义新的自定义相机时在frame source内部使用这个成员。</para>
        /// </summary>
        protected static IReadOnlyList<ARSession.ARCenterMode> allCenterMode = Enum.GetValues(typeof(ARSession.ARCenterMode)).Cast<ARSession.ARCenterMode>().ToList();
        private CameraImageRenderer cameraRenderer;
        private RenderCameraController renderCamera;
        [SerializeField, HideInInspector]
        private Camera arCamera;

        /// <summary>
        /// <para xml:lang="en">If the frame source is available.</para>
        /// <para xml:lang="en">If the value equals null, <see cref="CheckAvailability"/> must be called and the value can be accessed after <see cref="Coroutine"/> finish. This property is used by <see cref="ARComponentPicker"/> when picking frame source.</para>
        /// <para xml:lang="zh">当前frame source是否可用。</para>
        /// <para xml:lang="zh">如果数值等于null，需要调用<see cref="CheckAvailability"/>，数值将在<see cref="Coroutine"/>结束后可以访问。这个属性会在<see cref="ARComponentPicker"/>选择frame source的时候使用。</para>
        /// </summary>
        public abstract Optional<bool> IsAvailable { get; }

        /// <summary>
        /// <para xml:lang="en">Available center mode of the frame source.</para>
        /// <para xml:lang="zh">该frame source可以使用的中心模式。</para>
        /// </summary>
        public virtual IReadOnlyList<ARSession.ARCenterMode> AvailableCenterMode { get => allCenterMode; }

        /// <summary>
        /// <para xml:lang="en">If the frame source represent AR Eyewears.</para>
        /// <para xml:lang="en">Some frame filter may work different on eyewears.</para>
        /// <para xml:lang="en" access="internal">WARNING: use this member inside frame source only when defining a new custom camera.</para>
        /// <para xml:lang="zh">当前frame source是否是AR眼镜。</para>
        /// <para xml:lang="zh">部分frame filter在眼镜上运行会有不同。</para>
        /// <para xml:lang="zh" access="internal">警告：仅在定义新的自定义相机时在frame source内部使用这个成员。</para>
        /// </summary>
        public virtual bool IsHMD
        {
            get { return false; }
        }

        /// <summary>
        /// <para xml:lang="en">Device buffer capacity.</para>
        /// <para xml:lang="en" access="internal">WARNING: use this member inside frame source only when defining a new custom camera.</para>
        /// <para xml:lang="zh">设备缓冲容量。</para>
        /// <para xml:lang="zh" access="internal">警告：仅在定义新的自定义相机时在frame source内部使用这个成员。</para>
        /// </summary>
        public virtual int BufferCapacity
        {
            get => bufferCapacity;
            set => bufferCapacity = value;
        }

        /// <summary>
        /// <para xml:lang="en"><see cref="UnityEngine.Camera"/> used by the frame source in an <see cref="ARSession"/>.</para>
        /// <para xml:lang="en">Each type of frame source has its own method to check if the camera is usable, and will reject value set if the camera is not usable by the frame source. This property is used by <see cref="ARComponentPicker"/> when picking frame source to determine if the frame source is available, and some frame source will automatic pick a usable <see cref="UnityEngine.Camera"/> from active objects in the scene in the process if the value is not set. You can set this value to a usable <see cref="UnityEngine.Camera"/> before session start. The value cannot be changed after <see cref="ARSession"/> is ready if the frame source is selected in the session.</para>
        /// <para xml:lang="zh">在一个<see cref="ARSession"/>中被该frame source使用的<see cref="UnityEngine.Camera"/>。</para>
        /// <para xml:lang="zh">每种frame source都有自己的方法来检查camera是否可用，设置不可用的camera将会被拒绝。这个属性会在<see cref="ARComponentPicker"/> 选择frame source时用来判断frame source是否可用。在这个过程中，如果数值没有设置，一些frame source会从场景中active的物体中自动选择可以使用的<see cref="UnityEngine.Camera"/>。你可以在session start前设置可用的<see cref="UnityEngine.Camera"/>。如果这个frame source被<see cref="ARSession"/>选用，这个数值将在session ready后无法修改。</para>
        /// </summary>
        public virtual Camera Camera
        {
            get => arCamera;
            set
            {
                if (arSession) { return; }
                if (value && !IsValidCamera(value)) { return; }
                arCamera = value;
            }
        }

        /// <summary>
        /// <para xml:lang="en">If <see cref="Camera"/> transform and projection should be controlled by <see cref="ARSession"/> .</para>
        /// <para xml:lang="en" access="internal">WARNING: use this member inside frame source only when defining a new custom camera.</para>
        /// <para xml:lang="zh"><see cref="Camera"/> transform 和投影矩阵是否需要被<see cref="ARSession"/>控制。</para>
        /// <para xml:lang="zh" access="internal">警告：仅在定义新的自定义相机时在frame source内部使用这个成员。</para>
        /// </summary>
        public virtual bool IsCameraUnderControl { get { return true; } }

        /// <summary>
        /// <para xml:lang="en">Origin of <see cref="ARSession"/> if the frame source can output motion tracking data.</para>
        /// <para xml:lang="en">Each type of motion tracking frame source has its own method to set the origin object containing some specific component. Some frame source will automatic pick a usable object from active objects in the scene or generate an object if the value is not set.</para>
        /// <para xml:lang="zh"><see cref="ARSession"/>的原点，如果frame source可以输出运动跟踪数据。</para>
        /// <para xml:lang="zh">每种运动跟踪frame source都有自己的方法来设置包含特定组件的原点物体。如果原点未设置，一些frame source会从场景中active的物体中自动选择可以使用的物体或创建一个新的物体。</para>
        /// </summary>
        public virtual GameObject Origin { get => null; }

        protected virtual void Awake()
        {
            // for backward compatibility
            renderCamera = GetComponent<RenderCameraController>();
            if (!renderCamera) { renderCamera = gameObject.AddComponent<RenderCameraController>(); }
            cameraRenderer = GetComponent<CameraImageRenderer>();
            if (!cameraRenderer) { cameraRenderer = gameObject.AddComponent<CameraImageRenderer>(); }
        }

        protected virtual void OnEnable()
        {
            if (arSession)
            {
                arSession.Assembly.Resume();
            }
        }

        protected virtual void OnDisable()
        {
            if (arSession)
            {
                arSession.Assembly.Pause();
            }
        }

        /// <summary>
        /// <para xml:lang="en">Usually only for internal assemble use. Connect input port.</para>
        /// <para xml:lang="en" access="internal">WARNING: use this member inside frame source only when defining a new custom camera.</para>
        /// <para xml:lang="zh">通常只在内部组装时使用。连接输入端口。</para>
        /// <para xml:lang="zh" access="internal">警告：仅在定义新的自定义相机时在frame source内部使用这个成员。</para>
        /// </summary>
        public virtual void Connect(InputFrameSink val)
        {
            sink = val;
        }

        /// <summary>
        /// <para xml:lang="en">Usually only for internal assemble use. Assemble response.</para>
        /// <para xml:lang="en" access="internal">WARNING: use this member inside frame source only when defining a new custom camera.</para>
        /// <para xml:lang="zh">通常只在内部组装时使用。组装响应方法。</para>
        /// <para xml:lang="zh" access="internal">警告：仅在定义新的自定义相机时在frame source内部使用这个成员。</para>
        /// </summary>
        public virtual void OnAssemble(ARSession session)
        {
            arSession = session;
            if (IsCameraUnderControl && renderCamera) { renderCamera.OnAssemble(session); }
        }

        /// <summary>
        /// <para xml:lang="en"><see cref="Coroutine"/> to check frame source availability when <see cref="IsAvailable"/> equals null.</para>
        /// <para xml:lang="zh"><see cref="IsAvailable"/>等于null时用于检查frame source是否可用的<see cref="Coroutine"/>。</para>
        /// </summary>
        public virtual IEnumerator CheckAvailability()
        {
            return null;
        }

        /// <summary>
        /// <para xml:lang="en">Pick a usable <see cref="UnityEngine.Camera"/>. Used by <see cref="ARComponentPicker"/> when picking frame source </para>
        /// <para xml:lang="en" access="internal">WARNING: use this member inside frame source only when defining a new custom camera.</para>
        /// <para xml:lang="zh">选择可以使用的<see cref="UnityEngine.Camera"/>。会在<see cref="ARComponentPicker"/> 选择frame source时使用。</para>
        /// <para xml:lang="zh" access="internal">警告：仅在定义新的自定义相机时在frame source内部使用这个成员。</para>
        /// </summary>
        public virtual Camera PickCamera()
        {
            return Camera.main;
        }

        internal void SetHFlip(ARSession.ARHorizontalFlipMode hFlip)
        {
            if (!renderCamera) { return; }
            renderCamera.SetProjectHFlip(hFlip == ARSession.ARHorizontalFlipMode.World);
            renderCamera.SetRenderImageHFilp(hFlip != ARSession.ARHorizontalFlipMode.None);
        }

        protected virtual bool IsValidCamera(Camera cam)
        {
            return true;
        }
    }
}
