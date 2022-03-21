//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace easyar
{
    /// <summary>
    /// <para xml:lang="en"><see cref="MonoBehaviour"/> which controls <see cref="InputFramePlayer"/> in the scene, providing a few extensions in the Unity environment. There is no need to use <see cref="InputFramePlayer"/> directly.</para>
    /// <para xml:lang="zh">在场景中控制<see cref="InputFramePlayer"/>的<see cref="MonoBehaviour"/>，在Unity环境下提供功能扩展。不需要直接使用<see cref="InputFramePlayer"/>。</para>
    /// </summary>
    public class FramePlayer : FrameSource
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

        private static IReadOnlyList<ARSession.ARCenterMode> availableCenterMode = new List<ARSession.ARCenterMode> { ARSession.ARCenterMode.FirstTarget, ARSession.ARCenterMode.Camera, ARSession.ARCenterMode.SpecificTarget };
        /// <senseapi/>
        private InputFramePlayer player;
        private bool isStarted;
        private bool isPaused;
        private DisplayEmulator display;
        private bool assembled;
        private bool disableAutoPlay;
        [SerializeField, HideInInspector]
        private WorldRootController worldRoot;
        private WorldRootController worldRootCache;
        private GameObject worldRootObject;
        private bool hasSpatialInfo;

        public override Optional<bool> IsAvailable { get => true; }

        public override IReadOnlyList<ARSession.ARCenterMode> AvailableCenterMode { get => availableCenterMode; }

        /// <summary>
        /// <para xml:lang="en"> Whether the playback is completed.</para>
        /// <para xml:lang="zh"> 是否已完成播放。</para>
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                if (isStarted)
                {
                    return player.isCompleted();
                }
                return false;
            }
        }

        /// <summary>
        /// <para xml:lang="en"> Total expected playback time. The unit is second.</para>
        /// <para xml:lang="zh"> 预期的总播放时间。单位为秒。</para>
        /// </summary>
        public float Length
        {
            get
            {
                if (isStarted)
                {
                    return (float)player.totalTime();
                }
                return 0;
            }
        }

        /// <summary>
        /// <para xml:lang="en"> Current time played.</para>
        /// <para xml:lang="zh"> 已经播放的时间。</para>
        /// </summary>
        public float Time
        {
            get
            {
                if (isStarted)
                {
                    return (float)player.currentTime();
                }
                return 0;
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

        public override GameObject Origin { get => hasSpatialInfo && worldRoot ? worldRoot.gameObject : null; }

        internal IDisplay Display
        {
            get { return display; }
        }

        protected override void Awake()
        {
            base.Awake();
            if (!EasyARController.Initialized)
            {
                return;
            }
            if (!worldRoot) { worldRootCache = FindObjectOfType<WorldRootController>(); }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (player != null && isStarted && !isPaused)
            {
                player.resume();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (player != null && isStarted && !isPaused)
            {
                player.pause();
            }
        }

        protected virtual void OnDestroy()
        {
            if (player != null)
            {
                player.Dispose();
            }
            if (worldRootObject) Destroy(worldRootObject);
        }

        public override void OnAssemble(ARSession session)
        {
            base.OnAssemble(session);
            StartCoroutine(AutoPlay());
            assembled = true;
        }

        /// <summary>
        /// <para xml:lang="en">Start eif file playback.</para>
        /// <para xml:lang="zh">播放eif文件。</para>
        /// </summary>
        public bool Play()
        {
            disableAutoPlay = true;
            isPaused = false;

            if (!isStarted)
            {
                var path = FilePath;
                if (FilePathType == WritablePathType.PersistentDataPath)
                {
                    path = Application.persistentDataPath + "/" + path;
                }
                if (player == null)
                {
                    player = InputFramePlayer.create();
                    if (sink != null)
                    {
                        player.output().connect(sink);
                    }
                }
                isStarted = player.start(path);
                if (isStarted)
                {
                    display = new DisplayEmulator();
                    display.EmulateRotation(player.initalScreenRotation());
                }
                else
                {
                    GUIPopup.EnqueueMessage(typeof(FramePlayer) + " fail to start with file: " + path, 5);
                }
            }
            if (enabled)
            {
                OnEnable();
            }
            return isStarted;
        }

        /// <summary>
        /// <para xml:lang="en">Stop eif file playback.</para>
        /// <para xml:lang="zh">停止播放eif文件。</para>
        /// </summary>
        public void Stop()
        {
            disableAutoPlay = true;
            isStarted = false;
            isPaused = false;
            display = null;
            OnDisable();
            if (player != null)
            {
                player.stop();
            }
        }

        /// <summary>
        /// <para xml:lang="en">Pause eif file playback.</para>
        /// <para xml:lang="zh">暂停播放eif文件。</para>
        /// </summary>
        public void Pause()
        {
            if (isStarted)
            {
                isPaused = true;
                player.pause();
            }
        }

        public override void Connect(InputFrameSink val)
        {
            base.Connect(val);
            if (player != null)
            {
                player.output().connect(val);
            }
        }

        internal void RequireSpatial()
        {
            SetupOriginUsingWorldRoot();
        }

        private IEnumerator AutoPlay()
        {
            while (!enabled)
            {
                if (disableAutoPlay) { yield break; }
                yield return null;
            }
            if (disableAutoPlay) { yield break; }
            Play();
        }

        private void SetupOriginUsingWorldRoot()
        {
            hasSpatialInfo = true;
            availableCenterMode = allCenterMode;
            if (worldRoot) { return; }
            worldRoot = worldRootCache;
            if (worldRoot) { return; }
            worldRoot = FindObjectOfType<WorldRootController>();
            if (worldRoot) { return; }
            Debug.Log($"WorldRoot not found, create from {typeof(ARCoreFrameSource)}");
            worldRootObject = new GameObject("WorldRoot");
            worldRoot = worldRootObject.AddComponent<WorldRootController>();
        }
    }
}
