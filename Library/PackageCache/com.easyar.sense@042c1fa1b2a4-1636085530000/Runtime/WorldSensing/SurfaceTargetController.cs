//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

namespace easyar
{
    /// <summary>
    /// <para xml:lang="en"><see cref="UnityEngine.MonoBehaviour"/> which controls surface target in the scene. The surface target is a virtual node, representing the relative node when the camera moves in surface tracking.</para>
    /// <para xml:lang="zh">在场景中控制surface target的<see cref="UnityEngine.MonoBehaviour"/>。surface target是一个虚拟的节点，它表示在表面跟踪中，camera移动的相对节点。</para>
    /// </summary>
    public class SurfaceTargetController : TargetController
    {
        protected override void OnTracking() { }

        internal void Load() => IsLoaded = true;
    }
}
