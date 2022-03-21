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
    /// <summary>
    /// <para xml:lang="en"><see cref="MonoBehaviour"/> which controls <see cref="SurfaceTracker"/> in the scene, providing a few extensions in the Unity environment. Use <see cref="Tracker"/> directly when necessary.</para>
    /// <para xml:lang="zh">在场景中控制<see cref="SurfaceTracker"/>的<see cref="MonoBehaviour"/>，在Unity环境下提供功能扩展。如有需要可以直接使用<see cref="Tracker"/>。</para>
    /// </summary>
    public class SurfaceTrackerFrameFilter : FrameFilter, FrameFilter.IInputFrameSink, FrameFilter.IOutputFrameSource
    {
        /// <summary>
        /// <para xml:lang="en">EasyAR Sense API. Accessible after Awake if available.</para>
        /// <para xml:lang="zh">EasyAR Sense API，如果功能可以使用，可以在Awake之后访问。</para>
        /// </summary>
        /// <senseapi/>
        public SurfaceTracker Tracker { get; private set; }

        [HideInInspector, SerializeField]
        private SurfaceTargetController target;
        private bool isStarted;

        public override int BufferRequirement
        {
            get { return Tracker.bufferRequirement(); }
        }

        /// <summary>
        /// <para xml:lang="en">The object Camera move against, will be automatically get from the scene or generate if not set.</para>
        /// <para xml:lang="zh">相机运动的相对物体，如果没设置，将会自动从场景中获取或生成。</para>
        /// </summary>
        public SurfaceTargetController Target
        {
            get => target;
            set
            {
                if (isStarted) { return; }
                target = value;
            }
        }

        protected virtual void Awake()
        {
            if (!EasyARController.Initialized)
            {
                return;
            }
            if (!SurfaceTracker.isAvailable())
            {
                throw new UIPopupException(typeof(SurfaceTracker) + " not available");
            }

            Tracker = SurfaceTracker.create();
        }

        protected virtual void OnEnable()
        {
            if (Tracker != null && isStarted)
            {
                Tracker.start();
            }
        }

        protected virtual void OnDisable()
        {
            if (Tracker != null)
            {
                Tracker.stop();
            }
        }

        protected virtual void OnDestroy()
        {
            if (Tracker != null)
            {
                Tracker.Dispose();
            }
        }

        public InputFrameSink InputFrameSink()
        {
            if (Tracker != null)
            {
                return Tracker.inputFrameSink();
            }
            return null;
        }

        public OutputFrameSource OutputFrameSource()
        {
            if (Tracker != null)
            {
                return Tracker.outputFrameSource();
            }
            return null;
        }

        public override void OnAssemble(ARSession session)
        {
            base.OnAssemble(session);

            isStarted = true;
            if (!target)
            {
                target = FindObjectOfType<SurfaceTargetController>();
                if (!target)
                {
                    var gameObject = new GameObject("SurfaceTarget");
                    target = gameObject.AddComponent<SurfaceTargetController>();
                }
            }
            target.Load();
            if (enabled)
            {
                OnEnable();
            }
        }

        public void OnResult(Optional<FrameFilterResult> frameFilterResult)
        {
            var list = new List<Tuple<TargetController, Pose>>();
            if (frameFilterResult.OnSome)
            {
                var result = frameFilterResult.Value as SurfaceTrackerResult;
                list.Add(Tuple.Create((TargetController)target, result.transform().ToUnityPose().Inverse()));
                target.OnTracking(true);
            }
            else
            {
                target.OnTracking(false);
            }
            targetResults = list;
        }

        public override Optional<Tuple<GameObject, Pose>> TryGetCenter(GameObject center) => TryGetCenterTarget(center);

        public override void UpdateTransform(GameObject center, Pose centerPose) => UpdateTargetTransform(center, centerPose);
    }
}

