//================================================================================================================================
//
//  Copyright (c) 2020-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using UnityEngine;

namespace easyar
{
    /// <summary>
    /// <para xml:lang="en"><see cref="MonoBehaviour"/> which controls the sparse spatial map root <see cref="GameObject.active"/> strategy in the scene.</para>
    /// <para xml:lang="zh">在场景中控制稀疏空间地图根节点<see cref="GameObject.active"/>策略的<see cref="MonoBehaviour"/>。</para>
    /// </summary>
    public class SparseSpatialMapActiveController : MonoBehaviour
    {
        /// <summary>
        /// <para xml:lang="en">Strategy to control the <see cref="GameObject.active"/>. If you are willing to control <see cref="GameObject.active"/> or there are other components controlling <see cref="GameObject.active"/>, make sure to set it to <see cref="Strategy.None"/>.</para>
        /// <para xml:lang="zh"><see cref="GameObject.active"/>的控制策略。如果你打算自己控制<see cref="GameObject.active"/>或是有其它组件在控制<see cref="GameObject.active"/>，需要设为<see cref="Strategy.None"/>。</para>
        /// </summary>
        public Strategy ActiveControl;

        private bool found;

        /// <summary>
        /// <para xml:lang="en">Strategy to control the <see cref="GameObject.active"/>.</para>
        /// <para xml:lang="zh"><see cref="GameObject.active"/>的控制策略。</para>
        /// </summary>
        public enum Strategy
        {
            /// <summary>
            /// <para xml:lang="en">False before the fist time a map is localized, then true.</para>
            /// <para xml:lang="zh">在第一次有地图定位到之前Active为false，之后为true。</para>
            /// </summary>
            HideBeforeFound,
            /// <summary>
            /// <para xml:lang="en">Do not control <see cref="GameObject.active"/>.</para>
            /// <para xml:lang="zh">不控制<see cref="GameObject.active"/>。</para>
            /// </summary>
            None,
        }

        protected virtual void Start()
        {
            if (!found && (ActiveControl == Strategy.HideBeforeFound))
            {
                gameObject.SetActive(false);
            }
        }

        internal void OnFound()
        {
            if (!found)
            {
                if (ActiveControl == Strategy.HideBeforeFound)
                {
                    gameObject.SetActive(true);
                }
                found = true;
            }
        }
    }
}
