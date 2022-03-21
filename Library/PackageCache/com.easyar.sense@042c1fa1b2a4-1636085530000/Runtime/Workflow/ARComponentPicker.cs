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
    /// <para xml:lang="en">AR component picker. <see cref="ARSession"/> use this picker to pick components and compose <see cref="ARAssembly"/> when start.</para>
    /// <para xml:lang="zh">AR组件选择器。<see cref="ARSession"/>会在start的时候使用这个选择器来挑选组件并组成<see cref="ARAssembly"/>。</para>
    /// </summary>
    public class ARComponentPicker : MonoBehaviour
    {
        /// <summary>
        /// <para xml:lang="en"><see cref="easyar.FrameSource"/> selection strategy.</para>
        /// <para xml:lang="zh"><see cref="easyar.FrameSource"/>的选择策略。</para>
        /// </summary>
        public SingleSelection FrameSource;
        /// <summary>
        /// <para xml:lang="en"><see cref="easyar.FrameFilter"/> selection strategy.</para>
        /// <para xml:lang="zh"><see cref="easyar.FrameFilter"/>的选择策略。</para>
        /// </summary>
        public MultipleSelection FrameFilter;
        /// <summary>
        /// <para xml:lang="en"><see cref="easyar.FramePlayer"/> selection strategy.</para>
        /// <para xml:lang="zh"><see cref="easyar.FramePlayer"/>的选择策略。</para>
        /// </summary>
        public SingleSelection FramePlayer = SingleSelection.Disable;
        /// <summary>
        /// <para xml:lang="en"><see cref="easyar.FrameRecorder"/> selection strategy.</para>
        /// <para xml:lang="zh"><see cref="easyar.FrameRecorder"/>的选择策略。</para>
        /// </summary>
        public SingleSelection FrameRecorder = SingleSelection.Disable;
        /// <summary>
        /// <para xml:lang="en">Record specified components.</para>
        /// <para xml:lang="zh">记录手动指定的组件。</para>
        /// </summary>
        public ARAssembly.Components SpecifiedComponents = new ARAssembly.Components();

        /// <summary>
        /// <para xml:lang="en">Selection strategy to pick only one component.</para>
        /// <para xml:lang="zh">仅挑选一个组件的选择策略。</para>
        /// </summary>
        public enum SingleSelection
        {
            /// <summary>
            /// <para xml:lang="en">Select first available and active child.</para>
            /// <para xml:lang="zh">选择第一个可用且active的子节点。</para>
            /// </summary>
            FirstAvailableActiveChild,
            /// <summary>
            /// <para xml:lang="en">Manually specified.</para>
            /// <para xml:lang="zh">手动指定。</para>
            /// </summary>
            Specify,
            /// <summary>
            /// <para xml:lang="en">Do not select.</para>
            /// <para xml:lang="zh">不进行选择。</para>
            /// </summary>
            Disable,
        }

        /// <summary>
        /// <para xml:lang="en">Selection strategy to pick multiple components.</para>
        /// <para xml:lang="zh">挑选多个组件的选择策略。</para>
        /// </summary>
        public enum MultipleSelection
        {
            /// <summary>
            /// <para xml:lang="en">Select all active children.</para>
            /// <para xml:lang="zh">选择所有active的子节点。</para>
            /// </summary>
            AllActiveChildren,
            /// <summary>
            /// <para xml:lang="en">Manually specified.</para>
            /// <para xml:lang="zh">手动指定。</para>
            /// </summary>
            Specify,
            /// <summary>
            /// <para xml:lang="en">Do not select.</para>
            /// <para xml:lang="zh">不进行选择。</para>
            /// </summary>
            Disable,
        }

        /// <summary>
        /// <para xml:lang="en">Pick components. Pick may take a few frames to finish due to availability check of some components may take some time.</para>
        /// <para xml:lang="zh">挑选组件。由于部分组件的可用性检查会花一些实际，挑选操作可能会经过若干帧后才结束。</para>
        /// </summary>
        public IEnumerator Pick(Action<ARAssembly.Components> callback)
        {
            var components = new ARAssembly.Components();
            PickFrameFilter(components);
            PickFrameRecorder(components);
            if (PickFramePlayer(components))
            {
                components.FrameSource = components.FramePlayer;
            }
            else
            {
                yield return PickFrameSource(components);
            }
            if (components.FrameSource && !components.FrameSource.Camera)
            {
                components.FrameSource.Camera = components.FrameSource.PickCamera();
            }
            callback?.Invoke(components);
        }

        /// <summary>
        /// <para xml:lang="en">Preview pick results. Preview results may differ from runtime selection. It is only used for editor display.</para>
        /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh">预览组件挑选结果。预览结果可能与实际运行不同，仅用于编辑器中的显示。</para>
        /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public ARAssembly.Components Preview()
        {
            var components = new ARAssembly.Components();
            if (PickFramePlayer(components))
            {
                components.FrameSource = components.FramePlayer;
            }
            else
            {
                PickFrameSourceForPreview(components);
            }
            PickFrameFilter(components);
            PickFrameRecorder(components);
            return components;
        }

        /// <summary>
        /// <para xml:lang="en" access="internal">WARNING: Designed for deep customize only. Do not use this interface unless you are writing a customized AR component. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh" access="internal">警告：仅为深度定制设计。除非在写自定义AR组件，否则不要使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public List<CType> GetComponentsInChildrenTransformOrder<CType>()
        {
            var list = new List<CType>();
            GetComponentsInChildrenTransformOrder(list, transform);
            return list;
        }

        private void PickFrameSourceForPreview(ARAssembly.Components components)
        {
            if (FrameSource == SingleSelection.Specify)
            {
                components.FrameSource = SpecifiedComponents.FrameSource;
            }
        }

        private void PickFrameFilter(ARAssembly.Components components)
        {
            if (FrameFilter == MultipleSelection.AllActiveChildren)
            {
                components.FrameFilters = new List<FrameFilter>(GetComponentsInChildren<FrameFilter>());
            }
            else if (FrameFilter == MultipleSelection.Specify)
            {
                if (SpecifiedComponents.FrameFilters != null) { components.FrameFilters = SpecifiedComponents.FrameFilters; }
            }
        }

        private void PickFrameRecorder(ARAssembly.Components components)
        {
            if (FrameRecorder == SingleSelection.FirstAvailableActiveChild)
            {
                components.FrameRecorder = GetComponentInChildren<FrameRecorder>();
            }
            else if (FrameRecorder == SingleSelection.Specify)
            {
                components.FrameRecorder = SpecifiedComponents.FrameRecorder;
            }
        }

        private bool PickFramePlayer(ARAssembly.Components components)
        {
            if (FramePlayer == SingleSelection.FirstAvailableActiveChild)
            {
                components.FramePlayer = GetComponentInChildren<FramePlayer>();
                return true;
            }
            else if (FramePlayer == SingleSelection.Specify)
            {
                components.FramePlayer = SpecifiedComponents.FramePlayer;
                return true;
            }
            return false;
        }

        private IEnumerator PickFrameSource(ARAssembly.Components components)
        {
            if (FrameSource == SingleSelection.Specify)
            {
                components.FrameSource = SpecifiedComponents.FrameSource;
                yield break;
            }
            else if (FrameSource == SingleSelection.FirstAvailableActiveChild)
            {
                yield return SelectFirstAvailableActiveFrameSource((frameSource)=>
                {
                    components.FrameSource = frameSource;
                });
            }
        }

        private IEnumerator SelectFirstAvailableActiveFrameSource(Action<FrameSource> callback)
        {
            FrameSource frameSource = null;
            foreach (var fs in GetComponentsInChildrenTransformOrder<FrameSource>())
            {
                if (fs is FramePlayer) { continue; }

                var check = fs.CheckAvailability();
                if (check != null)
                {
                    yield return check;
                }
                if (fs.IsAvailable.OnSome && fs.IsAvailable.Value)
                {
                    frameSource = fs;
                    break;
                }
            }
            if (!frameSource)
            {
                var message = string.Empty;
                foreach (var fs in GetComponentsInChildrenTransformOrder<FrameSource>())
                {
                    if (fs is FramePlayer) { continue; }
                    message += $"{fs.GetType().ToString().Replace("easyar.", "").Replace("FrameSource", "")} ";
                }
                GUIPopup.EnqueueMessage($"Available frame source not found from candidates:\n{message}", 10, true);
            }
            callback?.Invoke(frameSource);
        }

        private void GetComponentsInChildrenTransformOrder<CType>(List<CType> transforms, Transform transform)
        {
            if (!transform || (!transform.gameObject.activeSelf && transform != this.transform)) { return; }
            transforms.AddRange(transform.GetComponents<CType>());
            foreach (Transform t in transform)
            {
                GetComponentsInChildrenTransformOrder(transforms, t);
            }
        }
    }
}
