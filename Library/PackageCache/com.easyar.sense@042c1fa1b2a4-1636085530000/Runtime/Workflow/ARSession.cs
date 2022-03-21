//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace easyar
{
    /// <summary>
    /// <para xml:lang="en"><see cref="MonoBehaviour"/> which controls AR session in the scene. One session contains a set of components assembled as <see cref="ARAssembly"/> and controls data flow in the whole life cycle. This component is the entrance of AR, it is possible to create a new session class and replace this one in the scene to implement fully different AR workflow.</para>
    /// <para xml:lang="en">Relative transform between <see cref="Camera"/> and a few AR components are controlled by the session, one of those objects is called <see cref="CenterObject"/>, it stays still in the scene, while other objects move relatively to <see cref="CenterObject"/>. This object is selected according to the value of <see cref="CenterMode"/>. See description of <see cref="ARCenterMode"/> for more details.</para>
    /// <para xml:lang="zh">在场景中控制AR会话的<see cref="MonoBehaviour"/>。一个会话包含一组组装成<see cref="ARAssembly"/>的组件，并控制整个生命周期的数据流。这个组件是AR的入口，如果要实现完全不同的AR工作流可以创建一个新的会话类并在场景中替换这个类。</para>
    /// <para xml:lang="zh"><see cref="Camera"/>和一部分AR组件之间的相对transform是受session控制的，其中的一个物体被称为<see cref="CenterObject"/>，它在场景中不动，其它物体相对这个<see cref="CenterObject"/>运动。这个物体是根据<see cref="CenterMode"/>的数值进行选择的。更详细的说明可以查看<see cref="ARCenterMode"/>的描述。</para>
    /// </summary>
    [RequireComponent(typeof(EasyARController), typeof(ARComponentPicker))]
    public class ARSession : MonoBehaviour
    {
        /// <summary>
        /// <para xml:lang="en">AR center mode. Modify at any time and takes effect immediately. If the specified mode is not available in a session, it will be change to one of the available mode automatically.</para>
        /// <para xml:lang="zh">AR中心模式。可随时修改，立即生效。如果指定的模式不可用，它将会被自动修改为可用的模式。</para>
        /// </summary>
        [SerializeField, HideInInspector]
        public ARCenterMode CenterMode;

        /// <summary>
        /// <para xml:lang="en">Horizontal flip rendering mode for normal camera. Modify at any time and takes effect immediately. Only available when using image or object tracking.</para>
        /// <para xml:lang="zh">正常相机的水平镜像渲染模式。可随时修改，立即生效。仅在使用图像或物体跟踪时可用。</para>
        /// </summary>
        public ARHorizontalFlipMode HorizontalFlipNormal;

        /// <summary>
        /// <para xml:lang="en">Horizontal flip rendering mode for front camera. Modify at any time and takes effect immediately. Only available when using image or object tracking.</para>
        /// <para xml:lang="zh">前置相机的水平镜像渲染模式。可随时修改，立即生效。仅在使用图像或物体跟踪时可用。</para>
        /// </summary>
        public ARHorizontalFlipMode HorizontalFlipFront = ARHorizontalFlipMode.World;

        [SerializeField, HideInInspector]
        private GameObject specificTargetCenter;
        private int frameIndex = -1;
        private Tuple<bool, Optional<Tuple<MotionTrackingStatus, Matrix44F>>> frameStatus = Tuple.Create(false, Optional<Tuple<MotionTrackingStatus, Matrix44F>>.CreateNone());
        private SessionState state;

        /// <summary>
        /// <para xml:lang="en">Output frame change event delegate.</para>
        /// <para xml:lang="zh">输出帧发生改变的委托。</para>
        /// </summary>
        public delegate void FrameChangeAction(OutputFrame outputFrame, Quaternion displayCompensation);

        /// <summary>
        /// <para xml:lang="en">Output frame change event. It is triggered when the data itself changes, the frequency is affected by <see cref="FrameSource"/> data change (like <see cref="CameraDevice"/> FPS).</para>
        /// <para xml:lang="zh">输出帧发生改变的事件。该事件会在数据本身产生变化的时候发生，频率受<see cref="FrameSource"/>数据变化（比如<see cref="CameraDevice"/>帧率）影响。</para>
        /// </summary>
        public event FrameChangeAction FrameChange;

        /// <summary>
        /// <para xml:lang="en">Output frame update event. It has the same frequency as MonoBehaviour Update.</para>
        /// <para xml:lang="zh">输出帧更新事件，该更新频率和MonoBehaviour Update频率相同。</para>
        /// </summary>
        public event Action<OutputFrame> FrameUpdate;

        /// <summary>
        /// <para xml:lang="en"><see cref="State"/> change event.</para>
        /// <para xml:lang="zh">session状态改变的事件。</para>
        /// </summary>
        public event Action<SessionState> StateChanged;

        /// <summary>
        /// <para xml:lang="en">AR center mode.</para>
        /// <para xml:lang="en">*NOTE: In EasyAR Sense Unity Plugin, there are four different types of center modes. Similar concept may not exist in some other AR frameworks like AR Foundation, and the behavior of object relationships is usually equal to* <see cref="SessionOrigin"/> *mode here.*</para>
        /// <para xml:lang="en">Relative transform between <see cref="ARAssembly.Camera"/> and a few AR components are controlled by the session, one of those objects is called <see cref="CenterObject"/>, it stays still in the scene, while other objects move relatively to <see cref="CenterObject"/>. This object is selected according to the value of <see cref="CenterMode"/>.</para>
        /// <para xml:lang="en"><see cref="CenterObject"/> represents an object or parent of object that do not move in Unity space. It can be <see cref="Origin"/>, <see cref="ARAssembly.Camera"/> or some `target`. A `target` could be object containing one of the following component: <see cref="TargetController"/>, <see cref="SparseSpatialMapRootController"/> or <see cref="SpatialMapRootController"/>. While in the context of sparse spatial map or cloud spatial map, the exact center <see cref="GameObject"/> is the localized map object under the root, and <see cref="CenterObject"/> is parent of this object.</para>
        /// <para xml:lang="en"><see cref="CenterObject"/> may change to other `target` in <see cref="FirstTarget"/> or <see cref="SpecificTarget"/> mode when `target` is not found or lost in a frame. If no `target` is found in this frame, the <see cref="CenterObject"/> will fallback to the center of first available mode in the order of <see cref="SessionOrigin"/> and <see cref="Camera"/>.</para>
        /// <para xml:lang="en">The relative transform between `target` and <see cref="ARAssembly.Camera"/> is controlled by this session according to <see cref="ARAssembly.OutputFrame"/> data every frame. The relative transform between <see cref="Origin"/> and <see cref="ARAssembly.Camera"/> is also controlled by this session according to <see cref="ARAssembly.OutputFrame"/> data every frame when <see cref="FrameSource.IsCameraUnderControl"/> is true. When <see cref="FrameSource.IsCameraUnderControl"/> is false, the relative transform between <see cref="Origin"/> and <see cref="ARAssembly.Camera"/> is not controlled by this session and is usually controlled by other AR Frameworks like AR Foundation.</para>
        /// <para xml:lang="zh">AR中心模式。</para>
        /// <para xml:lang="zh">*注意：在EasyAR Sense Unity Plugin中总共有四种中心模式。在其它AR框架比如AR Foundation中可能并不存在类似的概念，通常它们里面的物体间相对关系的行为与这里的*<see cref="SessionOrigin"/>*模式一致。*</para>
        /// <para xml:lang="zh"><see cref="ARAssembly.Camera"/>和一部分AR组件之间的相对transform是受session控制的，其中的一个物体被称为<see cref="CenterObject"/>，它在场景中不动，其它物体相对这个<see cref="CenterObject"/>运动。这个物体是根据<see cref="CenterMode"/>的数值进行选择的。</para>
        /// <para xml:lang="zh"><see cref="CenterObject"/> 表示在Unity空间中不运动的物体或这个物体的父节点。它可能是 <see cref="Origin"/>，<see cref="ARAssembly.Camera"/> 或某个 `target` 。 `Target` 可以是包含以下任一组件的物体：<see cref="TargetController"/>, <see cref="SparseSpatialMapRootController"/>或<see cref="SpatialMapRootController"/>。在使用稀疏空间地图和云空间地图的时候，实际的中心<see cref="GameObject"/>是root节点下具体定位到的map物体，<see cref="CenterObject"/> 是这个物体的父节点。</para>
        /// <para xml:lang="zh">在<see cref="FirstTarget"/> 或 <see cref="SpecificTarget"/>模式下，当 `target` 在某一帧中未被识别到或丢失的时候，<see cref="CenterObject"/> 可能会变成其它 `target` ，而如果在帧内找不到 `target` ，<see cref="CenterObject"/>会按先后顺序退变为<see cref="SessionOrigin"/>和<see cref="Camera"/>里面第一个可用的模式的中心。</para>
        /// <para xml:lang="zh">`Target` 和<see cref="ARAssembly.Camera"/>的相对位置关系由当前session根据每帧<see cref="ARAssembly.OutputFrame"/>数据控制。<see cref="Origin"/> 和<see cref="ARAssembly.Camera"/>的相对位置关系，在<see cref="FrameSource.IsCameraUnderControl"/> 为true的时候，也由当前session根据每帧<see cref="ARAssembly.OutputFrame"/>数据控制，而当<see cref="FrameSource.IsCameraUnderControl"/> 为false的时候，它是不受当前session控制的，通常由其它AR框架比如AR Foundation控制。</para>
        /// </summary>
        public enum ARCenterMode
        {
            /// <summary>
            /// <para xml:lang="en">The session will use the first tracked `target` as center.</para>
            /// <para xml:lang="en">You can move or rotate the `target` and <see cref="ARAssembly.Camera"/> will follow. You cannot manually change the transform of <see cref="ARAssembly.Camera"/> in this mode. <see cref="Origin"/> will also follow if any type of motion tracking is running, and its transform cannot be manually changed.</para>
            /// <para xml:lang="en">When the `target` is lost, the center object will be recalculated. While in the context of sparse spatial map or cloud spatial map, the exact center <see cref="GameObject"/> is the localized map object under the root. Start localizing another map is treated as lost from localizing the previous one, and the center object will be recalculated.</para>
            /// <para xml:lang="zh">当前session是以第一个跟踪到的 `target` 为中心的。</para>
            /// <para xml:lang="zh">你可以移动或旋转 `target` ，<see cref="ARAssembly.Camera"/>会跟着动。在这个模式下你将无法手动控制<see cref="ARAssembly.Camera"/>的transform。如果任意一种运动跟踪在运行，<see cref="Origin"/>也会跟着动，它的transform也是不能手动控制的。</para>
            /// <para xml:lang="zh">当 `target` 丢失之后，中心物体会重新计算。在使用稀疏空间地图和云空间地图的时候，实际的中心<see cref="GameObject"/>是root节点下具体定位到的map物体。并且，定位一张新的地图将会被认作从前一张地图定位过程中的丢失，中心物体会重新计算。</para>
            /// </summary>
            FirstTarget,

            /// <summary>
            /// <para xml:lang="en">The session is <see cref="ARAssembly.Camera"/> centered.</para>
            /// <para xml:lang="en">You can move or rotate the <see cref="ARAssembly.Camera"/> and `target` will follow. You cannot manually change the transform of `target`. <see cref="Origin"/> will also follow if any type of motion tracking is running, and its transform cannot be manually changed.</para>
            /// <para xml:lang="zh">当前session是以<see cref="ARAssembly.Camera"/>为中心的。</para>
            /// <para xml:lang="zh">你可以移动或旋转<see cref="ARAssembly.Camera"/>，`target` 会跟着动。在这个模式下你将无法手动控制 `target` 的transform。如果任意一种运动跟踪在运行，<see cref="Origin"/>也会跟着动，它的transform也是不能手动控制的。</para>
            /// </summary>
            Camera,

            /// <summary>
            /// <para xml:lang="en">The session will use <see cref="SpecificTargetCenter"/> as center.</para>
            /// <para xml:lang="en">You can move or rotate the `target` and <see cref="ARAssembly.Camera"/> will follow. You cannot manually change the transform of <see cref="ARAssembly.Camera"/> in this mode. <see cref="Origin"/> will also follow if any type of motion tracking is running, and its transform cannot be manually changed.</para>
            /// <para xml:lang="zh">当前session是以<see cref="SpecificTargetCenter"/>为中心的。</para>
            /// <para xml:lang="zh">你可以移动或旋转 `target` ，<see cref="ARAssembly.Camera"/>会跟着动。在这个模式下你将无法手动控制<see cref="ARAssembly.Camera"/>的transform。如果任意一种运动跟踪在运行，<see cref="Origin"/>也会跟着动，它的transform也是不能手动控制的。</para>
            /// </summary>
            SpecificTarget,

            /// <summary>
            /// <para xml:lang="en">The session will use <see cref="Origin"/> as center.</para>
            /// <para xml:lang="en">You can move or rotate the <see cref="Origin"/> and the <see cref="ARAssembly.Camera"/> will follow. You cannot manually change the <see cref="ARAssembly.Camera"/>'s transform in this mode. If there are any `target` being tracked, it will also follow, and its transform cannot be manually changed.</para>
            /// <para xml:lang="zh">当前session是以<see cref="Origin"/>为中心的。</para>
            /// <para xml:lang="zh">你可以移动或旋转<see cref="Origin"/>，<see cref="ARAssembly.Camera"/>会跟着动。在这个模式下你将无法手动控制<see cref="ARAssembly.Camera"/>的transform。如果有任何 `target` 正在被跟踪，它也会跟着动，并且它的transform也是不能手动控制的。</para>
            /// </summary>
            SessionOrigin,
        }

        /// <summary>
        /// <para xml:lang="en">Horizontal flip rendering mode.</para>
        /// <para xml:lang="en">In a flip rendering mode, the camera image will be mirrored. And to display to tracked objects in the right way, it will affect the 3D object rendering as well, so there are two different ways of doing horizontal flip. Horizontal flip can only work in object sensing like image or object tracking algorithms.</para>
        /// <para xml:lang="zh">水平镜像渲染模式。</para>
        /// <para xml:lang="zh">在水平翻转状态下，相机图像将镜像显示，为确保物体跟踪正常，它同时会影响3D物体的渲染，因此提供两种不同的方式。水平翻转只能在物体感知（比如图像跟踪或物体跟踪）算法下工作。</para>
        /// </summary>
        public enum ARHorizontalFlipMode
        {
            /// <summary>
            /// <para xml:lang="en">No flip.</para>
            /// <para xml:lang="zh">不翻转。</para>
            /// </summary>
            None,
            /// <summary>
            /// <para xml:lang="en">Render with horizontal flip, the camera image will be flipped in rendering, the camera projection matrix will be changed to do flip rendering. Target scale will not change.</para>
            /// <para xml:lang="zh">水平镜像渲染，camera图像会镜像显示，camera投影矩阵会变化进行镜像渲染，target scale不会改变。</para>
            /// </summary>
            World,
            /// <summary>
            /// <para xml:lang="en">Render with horizontal flip, the camera image will be flipped in rendering, the target scale will be changed to do flip rendering. Camera projection matrix will not change.</para>
            /// <para xml:lang="zh">水平镜像渲染，camera图像会镜像显示，target scale会改变进行镜像渲染，camera投影矩阵不会改变。</para>
            /// </summary>
            Target,
        }

        /// <summary>
        /// <para xml:lang="en">The state of session.</para>
        /// <para xml:lang="zh">Session的状态。</para>
        /// </summary>
        public enum SessionState
        {
            /// <summary>
            /// <para xml:lang="en">Initialize is not called or initialize failed.</para>
            /// <para xml:lang="zh">未初始化或初始化失败。</para>
            /// </summary>
            UnInitialized,
            /// <summary>
            /// <para xml:lang="en">In the process of assembling.</para>
            /// <para xml:lang="zh">在组装过程中。</para>
            /// </summary>
            Assembling,
            /// <summary>
            /// <para xml:lang="en"><see cref="ARAssembly"/> fail to assemble or broken.</para>
            /// <para xml:lang="zh"><see cref="ARAssembly"/>组装失败或被破坏。</para>
            /// </summary>
            Broken,
            /// <summary>
            /// <para xml:lang="en">Session is ready.</para>
            /// <para xml:lang="zh">Session已经准备好。</para>
            /// </summary>
            Ready,
            /// <summary>
            /// <para xml:lang="en">Session is running.</para>
            /// <para xml:lang="zh">Session在运行中。</para>
            /// </summary>
            Running,
            /// <summary>
            /// <para xml:lang="en">Session is paused.</para>
            /// <para xml:lang="en">Session will be paused when <see cref="FrameSource"/> generate empty frames, usually when device stop or application pause.</para>
            /// <para xml:lang="zh">Session暂停运行。</para>
            /// <para xml:lang="zh">Session会在<see cref="FrameSource"/>生成空帧数据的时候暂停，通常会发生在设备停止或应用暂停的时候。</para>
            /// </summary>
            Paused,
        }

        /// <summary>
        /// <para xml:lang="en">Specified AR center object. <see cref="CenterObject"/> will be set to this object when <see cref="CenterMode"/> == <see cref="ARCenterMode.SpecificTarget"/>. Modify at any time and takes effect immediately.</para>
        /// <para xml:lang="en">The object must contain one of the following component: <see cref="TargetController"/>, <see cref="SparseSpatialMapRootController"/> or <see cref="SpatialMapRootController"/>.</para>
        /// <para xml:lang="zh">手动指定的中心物体。<see cref="CenterMode"/> == <see cref="ARCenterMode.SpecificTarget"/>时<see cref="CenterObject"/>将被设成这个物体。可随时修改，立即生效。</para>
        /// <para xml:lang="zh">该物体必须包含以下任一组件：<see cref="TargetController"/>, <see cref="SparseSpatialMapRootController"/>或<see cref="SpatialMapRootController"/>。</para>
        /// </summary>
        public GameObject SpecificTargetCenter
        {
            get => specificTargetCenter;
            set
            {
                if (value
                    && !value.GetComponent<TargetController>()
                    && !value.GetComponent<SparseSpatialMapRootController>()
#if EASYAR_ENABLE_CLOUDSPATIALMAP
                    && !value.GetComponent<SpatialMapRootController>()
#endif
                    )
                {
                    Debug.LogWarning($"Ignore set SpecificTargetCenter: Cannot find target component from {value}");
                    return;
                }
                specificTargetCenter = value;
            }
        }

        /// <summary>
        /// <para xml:lang="en">Center object this session is using in current frame.</para>
        /// <para xml:lang="en">This object represents an object or parent of object that do not move in Unity space. It can be <see cref="Origin"/>, <see cref="ARAssembly.Camera"/> or some `target`. A `target` could be object containing one of the following component: <see cref="TargetController"/>, <see cref="SparseSpatialMapRootController"/> or <see cref="SpatialMapRootController"/>. While in the context of sparse spatial map or cloud spatial map, the exact center <see cref="GameObject"/> is the localized map object under the root, and <see cref="CenterObject"/> is parent of this object. See description of <see cref="ARCenterMode"/> for more details.</para>
        /// <para xml:lang="zh">这个session在当前帧使用的中心物体。</para>
        /// <para xml:lang="zh">这个物体表示在Unity空间中不运动的物体或这个物体的父节点。它可能是 <see cref="Origin"/>，<see cref="ARAssembly.Camera"/> 或某个 `target` 。 `Target` 可以是包含以下任一组件的物体：<see cref="TargetController"/>, <see cref="SparseSpatialMapRootController"/>或<see cref="SpatialMapRootController"/>。在使用稀疏空间地图和云空间地图的时候，实际的中心<see cref="GameObject"/>是root节点下具体定位到的map物体，<see cref="CenterObject"/> 是这个物体的父节点。更详细的说明可以查看<see cref="ARCenterMode"/>的描述。</para>
        /// </summary>
        public GameObject CenterObject { get; private set; }

        /// <summary>
        /// <para xml:lang="en">Assembly of AR components.</para>
        /// <para xml:lang="zh">AR组件的组装体。</para>
        /// </summary>
        public ARAssembly Assembly { get; private set; }

        /// <summary>
        /// <para xml:lang="en"><see cref="CameraParameters"/> from current frame.</para>
        /// <para xml:lang="zh">当前帧的<see cref="CameraParameters"/>。</para>
        /// </summary>
        public Optional<CameraParameters> FrameCameraParameters { get; private set; }

        /// <summary>
        /// <para xml:lang="en">Available center mode in the session.</para>
        /// <para xml:lang="zh">当前session可用的中心模式。</para>
        /// </summary>
        public IReadOnlyList<ARCenterMode> AvailableCenterMode
        {
            get => (Assembly != null && Assembly.FrameSource) ? Assembly.FrameSource.AvailableCenterMode : new List<ARCenterMode>();
        }

        /// <summary>
        /// <para xml:lang="en">Origin of session when one type of motion tracking is running.</para>
        /// <para xml:lang="zh">在任一运动跟踪功能运行时的session原点。</para>
        /// </summary>
        public GameObject Origin
        {
            get => (Assembly != null && Assembly.FrameSource) ? Assembly.FrameSource.Origin : null;
        }

        /// <summary>
        /// <para xml:lang="en">Tracking status when one type of motion tracking is running.</para>
        /// <para xml:lang="zh">在任一运动跟踪功能运行时的运动跟踪状态。</para>
        /// </summary>
        public Optional<MotionTrackingStatus> TrackingStatus
        {
            get => frameStatus.Item1 && frameStatus.Item2.OnSome ? frameStatus.Item2.Value.Item1 : Optional<MotionTrackingStatus>.CreateNone();
        }

        /// <summary>
        /// <para xml:lang="en">The state of current session.</para>
        /// <para xml:lang="zh">当前session的状态。</para>
        /// </summary>
        public SessionState State
        {
            get => state;
            private set
            {
                if (state == value) { return; }

                state = value;
                StateChanged?.Invoke(state);
            }
        }

        private void Start()
        {
            EasyARController.Instance.PostUpdate += UpdateSession;
            if (!EasyARController.Initialized)
            {
                return;
            }
            var picker = GetComponent<ARComponentPicker>();
            if (!picker)
            {
                // for backward compatibility
                picker = gameObject.AddComponent<ARComponentPicker>();
            }
            State = SessionState.Assembling;
            StartCoroutine(picker.Pick((components) =>
            {
                Assembly = new ARAssembly();
                try
                {
                    Assembly.Assemble(components, this);
                    State = SessionState.Ready;
                }
                catch (Exception e)
                {
                    Debug.LogError("Fail to Assemble: " + e.Message);
                    State = SessionState.Broken;
                }
            }));
        }

        private void OnDestroy()
        {
            EasyARController.Instance.PostUpdate -= UpdateSession;
            if (Assembly != null)
            {
                Assembly.Dispose();
            }
            if (FrameCameraParameters.OnSome)
            {
                FrameCameraParameters.Value.Dispose();
            }
        }

        private void UpdateSession()
        {
            if (Assembly == null || !Assembly.Ready)
            {
                OnEmptyFrame();
                return;
            }
            var oFrame = Assembly.OutputFrame;
            if (oFrame.OnNone)
            {
                OnEmptyFrame();
                return;
            }

            State = SessionState.Running;
            using (var outputFrame = oFrame.Value)
            using (var iFrame = outputFrame.inputFrame())
            {
                if (FrameCameraParameters.OnSome)
                {
                    FrameCameraParameters.Value.Dispose();
                }
                FrameCameraParameters = iFrame.cameraParameters();
                var displayCompensation = Quaternion.Euler(0, 0, -FrameCameraParameters.Value.imageOrientation(Assembly.Display.Rotation));
                var index = iFrame.index();
                if (frameIndex != index && FrameChange != null)
                {
                    FrameChange(outputFrame, displayCompensation);
                }
                frameIndex = index;
                // update self first, some flags will pass down to other components
                OnFrameUpdate(outputFrame, iFrame, displayCompensation);
                if (FrameUpdate != null)
                {
                    FrameUpdate(outputFrame);
                }
            }
        }

        /// <summary>
        /// <para xml:lang="en">Transforms points from screen coordinate system ([0, 1]^2) to image coordinate system ([0, 1]^2). <paramref name="pointInView"/> should be normalized to [0, 1]^2.</para>
        /// <para xml:lang="zh">从屏幕坐标系（[0, 1]^2）变换到图像坐标系（[0, 1]^2）。<paramref name="pointInView"/> 需要被归一化到[0, 1]^2。</para>
        /// </summary>
        public Optional<Vector2> ImageCoordinatesFromScreenCoordinates(Vector2 pointInView)
        {
            if (FrameCameraParameters.OnNone || Assembly == null || !Assembly.Camera)
            {
                return Optional<Vector2>.CreateNone();
            }
            return FrameCameraParameters.Value.imageCoordinatesFromScreenCoordinates(
                Assembly.Camera.aspect, Assembly.Display.Rotation, true, false, new Vec2F(pointInView.x, 1 - pointInView.y)).ToUnityVector();
        }

        private void OnFrameUpdate(OutputFrame outputFrame, InputFrame inputFrame, Quaternion displayCompensation)
        {
            // check filters
            foreach (var filter in Assembly.FrameFilters)
            {
                if (!filter)
                {
                    Assembly.Break();
                    State = SessionState.Broken;
                    return;
                }
            }
            // check session origin
            if (inputFrame.hasSpatialInformation() && !Assembly.FrameSource.Origin)
            {
                if (Assembly.FrameSource is FramePlayer)
                {
                    (Assembly.FrameSource as FramePlayer).RequireSpatial();
                }
                if (!Assembly.FrameSource.Origin)
                {
                    GUIPopup.EnqueueMessage("missing session origin", 10, true);
                    return;
                }
            }
            // check center mode
            if (!AvailableCenterMode.Contains(CenterMode))
            {
                Debug.LogWarning($"Center mode {CenterMode} is unavailable in this session, reset to {AvailableCenterMode[0]}.");
                CenterMode = AvailableCenterMode[0];
            }

            // horizontal flip
            var hflip = HorizontalFlipNormal;
            using (var cameraParameters = inputFrame.cameraParameters())
            {
                if (cameraParameters.cameraDeviceType() == CameraDeviceType.Front)
                {
                    hflip = HorizontalFlipFront;
                }
            }
            var targetHFlip = hflip == ARHorizontalFlipMode.Target;
            Assembly.FrameSource.SetHFlip(hflip);
            foreach (var filter in Assembly.FrameFilters)
            {
                filter.SetHFlip(hflip);
            }

            // dispatch results
            var results = outputFrame.results();
            void disposeResults() { foreach (var result in results.Where(r => r.OnSome)) { result.Value.Dispose(); } }

            var joinIndex = 0;
            foreach (var filter in Assembly.FrameFilters.Where(f => f is FrameFilter.IOutputFrameSource))
            {
                (filter as FrameFilter.IOutputFrameSource).OnResult(results[joinIndex]);
                joinIndex++;
            }

            // update frame status
            var timestamp = inputFrame.timestamp();
            if (inputFrame.hasSpatialInformation())
            {
                frameStatus = Tuple.Create(true, Optional<Tuple<MotionTrackingStatus, Matrix44F>>.CreateSome(
                    Tuple.Create(inputFrame.trackingStatus(),
                    inputFrame.trackingStatus() != MotionTrackingStatus.NotTracking ? inputFrame.cameraTransform() : (frameStatus.Item2.OnSome ? frameStatus.Item2.Value.Item2 : Pose.identity.ToEasyARPose())
                )));
            }
            else
            {
                frameStatus = Tuple.Create(true, Optional<Tuple<MotionTrackingStatus, Matrix44F>>.CreateNone());
            }

            // update motion
            if (frameStatus.Item2.OnSome)
            {
                foreach (var filter in Assembly.FrameFilters)
                {
                    filter.UpdateMotion(timestamp, frameStatus.Item2.Value.Item1, frameStatus.Item2.Value.Item2);
                }
            }

            // get session origin
            var sessionOrigin = Optional<Tuple<GameObject, Pose>>.CreateNone();
            if (inputFrame.hasSpatialInformation())
            {
                sessionOrigin = Tuple.Create(Origin, frameStatus.Item2.Value.Item2.ToUnityPose().Inverse());
                var worldRoot = Origin.GetComponent<WorldRootController>();
                if (worldRoot)
                {
                    worldRoot.OnTracking(inputFrame.trackingStatus());
                }
            }

            // get center
            var center = Optional<Tuple<GameObject, Pose>>.CreateNone();
            if (CenterMode == ARCenterMode.FirstTarget)
            {
                for (var i = 0; i < 2; ++i)
                {
                    foreach (var filter in Assembly.FrameFilters)
                    {
                        center = filter.TryGetCenter(CenterObject);
                        if (center.OnSome) { break; }
                    }
                    if (center.OnSome || !CenterObject)
                    {
                        break;
                    }
                    CenterObject = null;
                }
            }
            else if (CenterMode == ARCenterMode.SpecificTarget && SpecificTargetCenter)
            {
                foreach (var filter in Assembly.FrameFilters)
                {
                    center = filter.TryGetCenter(SpecificTargetCenter);
                    if (center.OnSome) { break; }
                }
            }
            else if (CenterMode == ARCenterMode.SessionOrigin && sessionOrigin.OnSome)
            {
                center = sessionOrigin;
            }
            else if (CenterMode == ARCenterMode.Camera)
            {
                center = Tuple.Create(Assembly.Camera.gameObject, new Pose(Vector3.zero, displayCompensation).Inverse());
            }

            if (center.OnNone && AvailableCenterMode.Contains(ARCenterMode.SessionOrigin) && sessionOrigin.OnSome)
            {
                center = sessionOrigin;
            }
            if (center.OnNone && AvailableCenterMode.Contains(ARCenterMode.Camera))
            {
                center = Tuple.Create(Assembly.Camera.gameObject, new Pose(Vector3.zero, displayCompensation).Inverse());
            }

            if (center.OnNone)
            {
                disposeResults();
                return;
            }
            CenterObject = center.Value.Item1;

            // set camera transform
            if (center.Value.Item1 != Assembly.Camera.gameObject && Assembly.FrameSource.IsCameraUnderControl)
            {
                var cameraToCenter = center.Value.Item2.Inverse();
                var p = new Pose(Vector3.zero, displayCompensation).Inverse()
                    .GetTransformedBy(cameraToCenter)
                    .FlipX(targetHFlip)
                    .GetTransformedBy(new Pose(center.Value.Item1.transform.localPosition, center.Value.Item1.transform.localRotation));

                Assembly.Camera.transform.localPosition = p.position;
                Assembly.Camera.transform.localRotation = p.rotation;
            }

            // set world root transform
            if (sessionOrigin.OnSome && center.Value.Item1 != sessionOrigin.Value.Item1)
            {
                var worldRootToCamera = sessionOrigin.Value.Item2;
                var cameraToWorld = center.Value.Item2.Inverse()
                    .FlipX(targetHFlip)
                    .GetTransformedBy(new Pose(center.Value.Item1.transform.localPosition, center.Value.Item1.transform.localRotation));
                var p = worldRootToCamera
                    .FlipX(targetHFlip)
                    .GetTransformedBy(cameraToWorld);

                sessionOrigin.Value.Item1.transform.localPosition = p.position;
                sessionOrigin.Value.Item1.transform.localRotation = p.rotation;
            }

            // set tracked object transform
            foreach (var filter in Assembly.FrameFilters)
            {
                filter.UpdateTransform(center.Value.Item1, center.Value.Item2);
            }

            disposeResults();
        }

        private void OnEmptyFrame()
        {
            if (State >= SessionState.Ready)
            {
                State = SessionState.Paused;
            }
            if (frameStatus.Item1)
            {
                if (FrameChange != null)
                {
                    FrameChange(null, Quaternion.identity);
                }

                foreach (var filter in Assembly.FrameFilters.Where(f => f is FrameFilter.IOutputFrameSource))
                {
                    (filter as FrameFilter.IOutputFrameSource).OnResult(null);
                }

                if (Origin)
                {
                    var worldRoot = Origin.GetComponent<WorldRootController>();
                    if (worldRoot)
                    {
                        worldRoot.OnTracking(MotionTrackingStatus.NotTracking);
                    }
                }
                frameStatus = Tuple.Create(false, Optional<Tuple<MotionTrackingStatus, Matrix44F>>.CreateNone());
            }
            if (FrameCameraParameters.OnSome)
            {
                FrameCameraParameters.Value.Dispose();
                FrameCameraParameters = Optional<CameraParameters>.CreateNone();
            }
        }
    }
}
