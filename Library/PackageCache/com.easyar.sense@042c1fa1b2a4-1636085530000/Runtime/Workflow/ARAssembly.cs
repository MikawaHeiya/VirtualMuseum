//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace easyar
{
    ///<remarks>
    ///                                            +-- .-- .-- .-- .-- .-- .-- .-- .-- .-- .-- .-- .-- .-- .-- .-- .-- .-- .-- .-- .-- .-- .-- .-- .-- .-- .-- .-- .--+
    ///                                            |                                                                                                                  .
    ///                                            .                                 +---------------------------------------------------------------+                |
    ///                                            |                                 |                                                               |                .
    ///                                            .                                 |                       + -> ObjectTracker - - - - +            |                |
    ///                                            |                                 v                       '                          '            |                .
    ///                                            .                        +--> i2FAdapter --> fbFrameFork - - > ImageTracker - - - +  '            |                |
    ///                                            |                        |                                                        '  '            |                .
    ///                                            v                        |                                                        v  v            |                |
    ///  FrameSource --> (FrameRecorder) --> iFrameThrottler --> iFrameFork --> i2OAdapter ------------------------------------> oFrameJoin --> oFrameFork --> oFrameBuffer ~~> o
    ///                                                                     '                                                        ^  ^
    ///                                                                     '                                                        '  '
    ///                                                                     + - - - - - - - - - - - - - - - - - > SparseSpatialMap - +  '
    ///                                                                     '                                                           '
    ///                                                                     + - - - - - - - - - - - - - - - - - > SurfaceTracker - - - -+
    ///                                                                     '
    ///                                                                     + - - - - - - - - - - - - - - - - - > DenseSpatialMap ~ ~ > o
    ///                                                                     '
    ///                                                                     + - - - - - - - - - - - - - - - - - > CloudRecognizer ~ ~ > o
    ///</remarks>

    /// <summary>
    /// <para xml:lang="en">Assembly of AR components. It implements one typical assemble strategy for all EasyAR Sense components. Inherit this class and override some methods can make a more customized assembly.</para>
    /// <para xml:lang="zh">AR组件的组装体。它实现了一种对所有EasyAR Sense组件的典型组装。继承这个类并重载部分可以实现更定制化的组装。</para>
    /// </summary>
    public class ARAssembly : IDisposable
    {
        /// <senseapi/>
        protected InputFrameThrottler iFrameThrottler;
        /// <senseapi/>
        protected InputFrameFork iFrameFork;
        /// <senseapi/>
        protected InputFrameToOutputFrameAdapter i2OAdapter;
        /// <senseapi/>
        protected InputFrameToFeedbackFrameAdapter i2FAdapter;
        /// <senseapi/>
        protected FeedbackFrameFork fbFrameFork;
        /// <senseapi/>
        protected OutputFrameJoin oFrameJoin;
        /// <senseapi/>
        protected OutputFrameFork oFrameFork;
        /// <senseapi/>
        protected OutputFrameBuffer oFrameBuffer;
        private FramePlayer framePlayer;
        private int extraBufferCapacity;

        ~ARAssembly()
        {
            DisposeAll();
        }

        /// <summary>
        /// <para xml:lang="en">Frame source.</para>
        /// <para xml:lang="zh">Frame数据源。</para>
        /// </summary>
        public FrameSource FrameSource { get; private set; }

        /// <summary>
        /// <para xml:lang="en">Frame recorder.</para>
        /// <para xml:lang="zh">输入帧录制器。</para>
        /// </summary>
        public FrameRecorder FrameRecorder { get; private set; }

        /// <summary>
        /// <para xml:lang="en"><see cref="FrameFilter"/> list.</para>
        /// <para xml:lang="zh"><see cref="FrameFilter"/>的列表。</para>
        /// </summary>
        public List<FrameFilter> FrameFilters { get; private set; } = new List<FrameFilter>();

        /// <summary>
        /// <para xml:lang="en"><see cref="UnityEngine.Camera"/> in the virtual world in reflection of real world camera device, its projection matrix and transform will be set to reflect the real world camera.</para>
        /// <para xml:lang="zh">现实环境中相机设备在虚拟世界中对应的<see cref="UnityEngine.Camera"/>，其投影矩阵和位置都将与真实相机对应。</para>
        /// </summary>
        public Camera Camera { get; private set; }

        /// <summary>
        /// <para xml:lang="en">The assembly can be used.</para>
        /// <para xml:lang="zh">组装体可以使用。</para>
        /// </summary>
        public bool Ready { get; private set; }

        /// <summary>
        /// <para xml:lang="en">Output frame.</para>
        /// <para xml:lang="zh">输出帧。</para>
        /// </summary>
        public Optional<OutputFrame> OutputFrame
        {
            get
            {
                if (!Ready)
                {
                    return null;
                }
                return oFrameBuffer.peek();
            }
        }

        /// <summary>
        /// <para xml:lang="en">Extra device buffer capacity. When you hold a OutputFrame/InputFrame or image from InputFrame for more than one render frame, you should increase this value by one.</para>
        /// <para xml:lang="zh">额外需要的设备缓冲容量。如果需要保留OutputFrame/InputFrame或InputFrame中的image超过渲染的一帧，需要增加1。</para>
        /// </summary>
        public int ExtraBufferCapacity
        {
            get
            {
                return extraBufferCapacity;
            }
            set
            {
                extraBufferCapacity = value;
                ResetBufferCapacity();
            }
        }

        /// <summary>
        /// <para xml:lang="en">Display information used by the assembly.</para>
        /// <para xml:lang="zh">Assembly使用的显示设备信息。</para>
        /// </summary>
        public IDisplay Display
        {
            get
            {
                if (framePlayer && framePlayer.Display != null)
                {
                    return framePlayer.Display;
                }
                return EasyARController.Instance.Display;
            }
        }

        /// <summary>
        /// <para xml:lang="en">Dispose resources.</para>
        /// <para xml:lang="zh">销毁资源。</para>
        /// </summary>
        public virtual void Dispose()
        {
            DisposeAll();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// <para xml:lang="en">Assemble AR components.</para>
        /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh">组装AR组件。</para>
        /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public virtual void Assemble(Components components, ARSession session)
        {
            FrameSource = components.FrameSource;
            FrameRecorder = components.FrameRecorder;
            FrameFilters = components.FrameFilters;

            if (!FrameSource) { throw new InvalidOperationException($"missing {typeof(FrameSource)}"); }
            if (FrameSource.AvailableCenterMode.Count <= 0) { throw new InvalidOperationException("No center mode available."); }

            Camera = FrameSource.Camera;
            if (!Camera) { throw new InvalidOperationException($"cannot determine {typeof(Camera)} from {FrameSource}"); }

            FrameSource.OnAssemble(session);
            if (FrameSource is FramePlayer) { framePlayer = FrameSource as FramePlayer; }
            if (FrameRecorder) { FrameRecorder.OnAssemble(session); }
            foreach (var filter in FrameFilters) { filter.OnAssemble(session); }

            Assemble();
        }

        /// <summary>
        /// <para xml:lang="en">Break the assembly. The assembly cannot be used once broken.</para>
        /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh">破坏AR组件体。一旦破坏将无法再使用。</para>
        /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public void Break()
        {
            Ready = false;
        }

        /// <summary>
        /// <para xml:lang="en">Pause output.</para>
        /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh">暂停输出。</para>
        /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public void Pause()
        {
            if (!Ready)
            {
                return;
            }
            oFrameBuffer.pause();
        }

        /// <summary>
        /// <para xml:lang="en">Resume output.</para>
        /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh">继续输出。</para>
        /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public void Resume()
        {
            if (!Ready)
            {
                return;
            }
            oFrameBuffer.resume();
        }

        /// <summary>
        /// <para xml:lang="en">Reset buffer capacity.</para>
        /// <para xml:lang="zh">重置缓冲的容量。</para>
        /// </summary>
        public void ResetBufferCapacity()
        {
            if (FrameSource != null)
            {
                FrameSource.BufferCapacity = GetBufferRequirement();
            }
        }

        /// <summary>
        /// <para xml:lang="en">Get buffer requirement.</para>
        /// <para xml:lang="zh">获取当前需要的缓冲容量。</para>
        /// </summary>
        protected int GetBufferRequirement()
        {
            int count = 1; // for OutputFrameBuffer.peek
            if (FrameSource != null) { count += 1; }
            if (FrameRecorder != null) { count += FrameRecorder.BufferRequirement; }
            if (iFrameThrottler != null) { count += iFrameThrottler.bufferRequirement(); }
            if (i2FAdapter != null) { count += i2FAdapter.bufferRequirement(); }
            if (oFrameBuffer != null) { count += oFrameBuffer.bufferRequirement(); }
            foreach (var filter in FrameFilters)
            {
                if (filter != null) { count += filter.BufferRequirement; }
            }
            count += extraBufferCapacity;
            return count;
        }

        /// <summary>
        /// <para xml:lang="en">Get <see cref="FrameFilter"/> number of certain type.</para>
        /// <para xml:lang="zh">获取指定<see cref="FrameFilter"/>的数量。</para>
        /// </summary>
        protected int GetFrameFilterCount<T>()
        {
            if (FrameFilters == null)
            {
                return 0;
            }
            int count = 0;
            foreach (var filter in FrameFilters)
            {
                if (filter is T)
                {
                    count++;
                }
            }
            return count;
        }

        private void Assemble()
        {
            // throttler
            iFrameThrottler = InputFrameThrottler.create();

            // fork input
            iFrameFork = InputFrameFork.create(2 + GetFrameFilterCount<FrameFilter.IInputFrameSink>());
            iFrameThrottler.output().connect(iFrameFork.input());
            var iFrameForkIndex = 0;
            i2OAdapter = InputFrameToOutputFrameAdapter.create();
            iFrameFork.output(iFrameForkIndex).connect(i2OAdapter.input());
            iFrameForkIndex++;
            i2FAdapter = InputFrameToFeedbackFrameAdapter.create();
            iFrameFork.output(iFrameForkIndex).connect(i2FAdapter.input());
            iFrameForkIndex++;
            foreach (var filter in FrameFilters)
            {
                if (filter is FrameFilter.IInputFrameSink)
                {
                    FrameFilter.IInputFrameSink unit = filter as FrameFilter.IInputFrameSink;
                    var sink = unit.InputFrameSink();
                    if (sink != null)
                    {
                        iFrameFork.output(iFrameForkIndex).connect(unit.InputFrameSink());
                    }
                    iFrameForkIndex++;
                }
            }

            // feedback
            fbFrameFork = FeedbackFrameFork.create(GetFrameFilterCount<FrameFilter.IFeedbackFrameSink>());
            i2FAdapter.output().connect(fbFrameFork.input());
            var fbFrameForkIndex = 0;
            foreach (var filter in FrameFilters)
            {
                if (filter is FrameFilter.IFeedbackFrameSink)
                {
                    FrameFilter.IFeedbackFrameSink unit = filter as FrameFilter.IFeedbackFrameSink;
                    fbFrameFork.output(fbFrameForkIndex).connect(unit.FeedbackFrameSink());
                    fbFrameForkIndex++;
                }
            }

            // join
            oFrameJoin = OutputFrameJoin.create(1 + GetFrameFilterCount<FrameFilter.IOutputFrameSource>());
            var joinIndex = 0;
            foreach (var filter in FrameFilters)
            {
                if (filter is FrameFilter.IOutputFrameSource)
                {
                    FrameFilter.IOutputFrameSource unit = filter as FrameFilter.IOutputFrameSource;
                    unit.OutputFrameSource().connect(oFrameJoin.input(joinIndex));
                    joinIndex++;
                }
            }
            i2OAdapter.output().connect(oFrameJoin.input(joinIndex));

            // fork output for feedback
            oFrameFork = OutputFrameFork.create(2);
            oFrameJoin.output().connect(oFrameFork.input());
            oFrameBuffer = OutputFrameBuffer.create();
            oFrameFork.output(0).connect(oFrameBuffer.input());
            oFrameFork.output(1).connect(i2FAdapter.sideInput());

            // signal throttler
            oFrameBuffer.signalOutput().connect(iFrameThrottler.signalInput());
            var inputFrameSink = iFrameThrottler.input();

            // connect recorder
            if (FrameRecorder)
            {
                FrameRecorder.Output().connect(inputFrameSink);
                inputFrameSink = FrameRecorder.Input();
            }

            // connect source
            if (FrameSource != null)
            {
                FrameSource.Connect(inputFrameSink);
            }

            // set BufferCapacity
            ResetBufferCapacity();

            Ready = true;
        }

        private void DisposeAll()
        {
            if (iFrameThrottler != null) { iFrameThrottler.Dispose(); }
            if (iFrameFork != null) { iFrameFork.Dispose(); }
            if (i2OAdapter != null) { i2OAdapter.Dispose(); }
            if (i2FAdapter != null) { i2FAdapter.Dispose(); }
            if (fbFrameFork != null) { fbFrameFork.Dispose(); }
            if (oFrameJoin != null) { oFrameJoin.Dispose(); }
            if (oFrameFork != null) { oFrameFork.Dispose(); }
            if (oFrameBuffer != null) { oFrameBuffer.Dispose(); }
            Ready = false;
        }

        [Serializable]
        public class Components
        {
            public FramePlayer FramePlayer;
            public FrameSource FrameSource;
            public FrameRecorder FrameRecorder;
            public List<FrameFilter> FrameFilters = new List<FrameFilter>();
        }
    }
}
