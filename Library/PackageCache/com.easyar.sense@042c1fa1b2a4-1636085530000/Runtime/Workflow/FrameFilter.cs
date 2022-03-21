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
    /// <para xml:lang="en">Abstracts frame filter, used when assemble, to run algorithms using input frame data.</para>
    /// <para xml:lang="zh">抽象frame filter，在组装时使用，使用frame输入数据运行算法。</para>
    /// </summary>
    public abstract class FrameFilter : MonoBehaviour
    {
        protected bool horizontalFlip;
        protected Optional<List<Tuple<TargetController, Pose>>> targetResults;
        protected Optional<List<Tuple<TargetController, Pose>>> targetResultsRTCT;

        /// <summary>
        /// <para xml:lang="en">Camera buffers occupied in this component.</para>
        /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh">当前组件占用camera buffer的数量。</para>
        /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public abstract int BufferRequirement { get; }

        /// <summary>
        /// <para xml:lang="en">Usually only for internal assemble use. Assemble response.</para>
        /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh">通常只在内部组装时使用。组装响应方法。</para>
        /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public virtual void OnAssemble(ARSession session)
        {
        }

        /// <summary>
        /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public virtual void UpdateMotion(double timestamp, MotionTrackingStatus trackingStatus, Matrix44F cameraTransform)
        {
        }

        /// <summary>
        /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public virtual Optional<Tuple<GameObject, Pose>> TryGetCenter(GameObject center)
        {
            return null;
        }

        /// <summary>
        /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public virtual void UpdateTransform(GameObject center, Pose centerPose)
        {
        }

        /// <summary>
        /// <para xml:lang="en">Set horizontal flip when using <see cref="ARSession.ARHorizontalFlipMode.Target"/> mode.</para>
        /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh">在<see cref="ARSession.ARHorizontalFlipMode.Target"/>模式下设置镜像翻转。</para>
        /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public void SetHFlip(ARSession.ARHorizontalFlipMode hFlip)
        {
            var flip = hFlip == ARSession.ARHorizontalFlipMode.Target;
            if (horizontalFlip != flip)
            {
                horizontalFlip = flip;
                OnHFlipChange(horizontalFlip);
            }
        }

        /// <summary>
        /// <para xml:lang="en">Horizontal flip response.</para>
        /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh">水平翻转响应方法。</para>
        /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        protected virtual void OnHFlipChange(bool hFlip)
        {
        }

        /// <summary>
        /// <para xml:lang="en">Interface for feedback frame input port.</para>
        /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh">反馈帧输入端口接口。</para>
        /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public interface IFeedbackFrameSink
        {
            /// <summary>
            /// <para xml:lang="en">Usually only for internal assemble use. Feedback frame input port.</para>
            /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
            /// <para xml:lang="zh">通常只在内部组装时使用。反馈帧输入端口。</para>
            /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
            /// </summary>
            FeedbackFrameSink FeedbackFrameSink();
        }

        /// <summary>
        /// <para xml:lang="en">Interface for input frame input port.</para>
        /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh">输入帧输入端口接口。</para>
        /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public interface IInputFrameSink
        {
            /// <summary>
            /// <para xml:lang="en">Usually only for internal assemble use. Input frame input port.</para>
            /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
            /// <para xml:lang="zh">通常只在内部组装时使用。输入帧输入端口。</para>
            /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
            /// </summary>
            InputFrameSink InputFrameSink();
        }

        /// <summary>
        /// <para xml:lang="en">Interface for output frame output port.</para>
        /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh">输出帧输出端口接口。</para>
        /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public interface IOutputFrameSource
        {
            /// <summary>
            /// <para xml:lang="en">Usually only for internal assemble use. Output frame output port.</para>
            /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
            /// <para xml:lang="zh">通常只在内部组装时使用。输出帧输出端口。</para>
            /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
            /// </summary>
            OutputFrameSource OutputFrameSource();
            /// <summary>
            /// <para xml:lang="en">Usually only for internal assemble use. Process tracking results.</para>
            /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
            /// <para xml:lang="zh">通常只在内部组装时使用。处理跟踪结果。</para>
            /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
            /// </summary>
            void OnResult(Optional<FrameFilterResult> frameFilterResult);
        }

        /// <summary>
        /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        protected Optional<Tuple<GameObject, Pose>> TryGetCenterTarget(GameObject center)
        {
            var results = targetResultsRTCT.OnSome ? targetResultsRTCT : targetResults;
            if (results.OnNone) { return null; }

            if (center)
            {
                foreach (var result in results.Value.Where(r => r.Item1.gameObject == center))
                {
                    return Tuple.Create(center, result.Item2);
                }
            }
            else
            {
                foreach (var result in results.Value)
                {
                    return Tuple.Create(result.Item1.gameObject, result.Item2);
                }
            }
            return null;
        }

        /// <summary>
        /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        protected void UpdateTargetTransform(GameObject center, Pose centerPose)
        {
            var results = targetResultsRTCT.OnSome ? targetResultsRTCT : targetResults;
            if (results.OnNone) { return; }
            foreach (var result in results.Value.Where(r => r.Item1.gameObject != center))
            {
                var targetToCamera = result.Item2;
                var cameraToWorld = centerPose.Inverse()
                    .FlipX(horizontalFlip)
                    .GetTransformedBy(new Pose(center.transform.localPosition, center.transform.localRotation));
                var targetToWorld = targetToCamera
                    .FlipX(horizontalFlip)
                    .GetTransformedBy(cameraToWorld);

                result.Item1.transform.localPosition = targetToWorld.position;
                result.Item1.transform.localRotation = targetToWorld.rotation;
            }
        }
    }
}
