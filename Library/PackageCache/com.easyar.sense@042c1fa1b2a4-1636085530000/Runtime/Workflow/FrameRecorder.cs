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
    /// <para xml:lang="en"><see cref="MonoBehaviour"/> which controls <see cref="InputFrameRecorder"/> in the scene, providing a few extensions in the Unity environment. There is no need to use <see cref="InputFrameRecorder"/> directly.</para>
    /// <para xml:lang="en"><see cref="Behaviour.enabled"/> can be used to control record start/stop.</para>
    /// <para xml:lang="zh">在场景中控制<see cref="InputFrameRecorder"/>的<see cref="MonoBehaviour"/>，在Unity环境下提供功能扩展。不需要直接使用<see cref="InputFrameRecorder"/>。</para>
    /// <para xml:lang="zh"><see cref="Behaviour.enabled"/>可以控制录制开始和结束。</para>
    /// </summary>
    public class FrameRecorder : MonoBehaviour
    {
        /// <summary>
        /// <para xml:lang="en">File path type. Set before OnEnable or <see cref="ARSession.Start"/>.</para>
        /// <para xml:lang="zh">路径类型。可以在OnEnable或<see cref="ARSession.Start"/>之前设置。</para>
        /// </summary>
        public WritablePathType FilePathType;

        /// <summary>
        /// <para xml:lang="en">File path. Set before OnEnable or <see cref="ARSession.Start"/>.</para>
        /// <para xml:lang="zh">文件路径。可以在OnEnable或<see cref="ARSession.Start"/>之前设置。</para>
        /// </summary>
        public string FilePath = string.Empty;

        /// <senseapi/>
        private InputFrameRecorder recorder;
        private bool isStarted;
        private ARSession arSession;

        /// <summary>
        /// <para xml:lang="en">Camera buffers occupied in this component.</para>
        /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh">当前组件占用camera buffer的数量。</para>
        /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public int BufferRequirement
        {
            get
            {
                return recorder.bufferRequirement();
            }
        }

        protected virtual void Awake()
        {
            if (!EasyARController.Initialized)
            {
                return;
            }

            recorder = InputFrameRecorder.create();
        }

        protected virtual void OnEnable()
        {
            if (recorder != null && isStarted)
            {
                var path = FilePath;
                if (FilePathType == WritablePathType.PersistentDataPath)
                {
                    path = Application.persistentDataPath + "/" + path;
                }
                var status = recorder.start(path, arSession.Assembly.Display.Rotation);
                if (!status)
                {
                    GUIPopup.EnqueueMessage(typeof(FrameRecorder) + " fail to start with file: " + path, 5);
                }
            }
        }

        protected virtual void OnDisable()
        {
            if (recorder == null)
            {
                return;
            }
            recorder.stop();
        }

        protected virtual void OnDestroy()
        {
            if (recorder != null)
            {
                recorder.Dispose();
            }
        }

        /// <summary>
        /// <para xml:lang="en">Usually only for internal assemble use. Assemble response.</para>
        /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh">通常只在内部组装时使用。组装响应方法。</para>
        /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public void OnAssemble(ARSession session)
        {
            arSession = session;
            isStarted = true;
            if (enabled)
            {
                OnEnable();
            }
        }

        /// <summary>
        /// <para xml:lang="en">Usually only for internal assemble use. Assemble response.</para>
        /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh">通常只在内部组装时使用。组装响应方法。</para>
        /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public InputFrameSource Output()
        {
            return recorder.output();
        }

        /// <summary>
        /// <para xml:lang="en">Usually only for internal assemble use. Assemble response.</para>
        /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh">通常只在内部组装时使用。组装响应方法。</para>
        /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>

        public InputFrameSink Input()
        {
            return recorder.input();
        }
    }
}
