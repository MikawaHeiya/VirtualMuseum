//================================================================================================================================
//
//  Copyright (c) 2020-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using System;
using UnityEngine;

namespace easyar
{
    /// <summary>
    /// <para xml:lang="en">Parent object of all sparse spatial maps generated from <see cref="SparseSpatialMap"/> in the scene.</para>
    /// <para xml:lang="zh">在场景中<see cref="SparseSpatialMap"/>生成的所有稀疏空间地图的父节点。</para>
    /// </summary>
    [RequireComponent(typeof(SparseSpatialMapActiveController))]
    public class SparseSpatialMapRootController : MonoBehaviour
    {
        private SparseSpatialMapActiveController activeController;

        /// <summary>
        /// <para xml:lang="en">New map localized event.</para>
        /// <para xml:lang="zh">一个新地图定位到的事件。</para>
        /// </summary>
        public event Action MapFound;

        internal void OnFound()
        {
            if (!activeController)
            {
                activeController = GetComponent<SparseSpatialMapActiveController>();
            }
            activeController.OnFound();
            if (MapFound != null)
            {
                MapFound();
            }
        }
    }
}
