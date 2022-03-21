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
    /// <para xml:lang="en"><see cref="MonoBehaviour"/> which holds and manages sparse spatial maps generated from <see cref="SparseSpatialMap"/> in the scene.</para>
    /// <para xml:lang="zh">在场景中持有并管理<see cref="SparseSpatialMap"/>生成的稀疏空间地图的<see cref="MonoBehaviour"/>。</para>
    /// </summary>
    public class SparseSpatialMapHolder : MonoBehaviour
    {
        /// <summary>
        /// <para xml:lang="en">The parent object of all sparse spatial maps, will be generated automatically if not set.</para>
        /// <para xml:lang="zh">所有稀疏空间地图的父节点，如果没设置，将会自动生成。</para>
        /// </summary>
        public SparseSpatialMapRootController SparseSpatialMapRoot;
        private GameObject root;
        private Dictionary<string, SparseSpatialMapController> maps = new Dictionary<string, SparseSpatialMapController>();
        private Optional<ResolvedMapInfo> curMapInfo;
        private Optional<ResolvedMapInfo> preMapInfo;
        private MotionInfo? curMotionInfo;
        private bool hasRTCT;

        /// <summary>
        /// <para xml:lang="en">The parent object of all sparse spatial maps.</para>
        /// <para xml:lang="zh">所有稀疏空间地图的父节点。</para>
        /// </summary>
        public SparseSpatialMapRootController MapRoot { get; private set; }
        /// <summary>
        /// <para xml:lang="en">The sparse spatial maps loaded.</para>
        /// <para xml:lang="zh">所有已经加载的稀疏空间地图。</para>
        /// </summary>
        public List<SparseSpatialMapController> Maps { get { return maps.Values.ToList(); } }
        internal bool EnablePoseStabilizer { get; set; } = true;

        protected virtual void Awake()
        {
            if (SparseSpatialMapRoot)
            {
                MapRoot = SparseSpatialMapRoot;
            }
            else
            {
                root = new GameObject("SparseSpatialMap");
                MapRoot = root.AddComponent<SparseSpatialMapRootController>();
                SparseSpatialMapRoot = MapRoot;
            }
        }

        protected virtual void OnDestroy()
        {
            if (root) { Destroy(root); }
        }

        internal SparseSpatialMapController Hold(SparseSpatialMapController map)
        {
            map.transform.SetParent(MapRoot.transform, false);
            maps[map.MapInfo.ID] = map;
            return map;
        }

        internal bool Unhold(SparseSpatialMapController map)
        {
            return maps.Remove(map.MapInfo.ID);
        }

        internal void OnLocalize(ResolvedMapInfo mapInfo, bool status)
        {
            if (mapInfo.Map)
            {
                mapInfo.Map.OnLocalization(status);
            }
            if (status)
            {
                if (curMapInfo.OnSome && curMapInfo.Value.Map.MapInfo.ID != mapInfo.Map.MapInfo.ID)
                {
                    preMapInfo = curMapInfo;
                }
                curMapInfo = mapInfo;
            }
        }

        internal bool Localized()
        {
            return curMapInfo.OnSome;
        }

        internal void UpdateMotion(Pose cameraToVIOOrigin, Pose? mapToCameraRTCT)
        {
            hasRTCT = mapToCameraRTCT.HasValue;
            curMotionInfo = new MotionInfo
            {
                CameraToVIOOrigin = cameraToVIOOrigin
            };
            if (EnablePoseStabilizer && hasRTCT)
            {
                curMapInfo.Value.Pose = new PoseSet
                {
                    MapToCamera = mapToCameraRTCT.Value,
                    CameraToVIOOrigin = cameraToVIOOrigin
                };
            }
        }

        internal Tuple<GameObject, Pose> TryGetCenter()
        {
            if (curMapInfo.OnNone)
            {
                return null;
            }

            if (preMapInfo.OnSome)
            {
                OnMapChange();
                preMapInfo = null;
            }

            var mapRootToCamera = new Pose(curMapInfo.Value.Map.transform.localPosition, curMapInfo.Value.Map.transform.localRotation).Inverse()
                .GetTransformedBy(curMapInfo.Value.Pose.MapToCamera);
            if (!(EnablePoseStabilizer && hasRTCT) && curMotionInfo.HasValue && curMapInfo.Value.Pose.CameraToVIOOrigin.HasValue)
            {
                mapRootToCamera = mapRootToCamera
                    .GetTransformedBy(curMapInfo.Value.Pose.CameraToVIOOrigin.Value)
                    .GetTransformedBy(curMotionInfo.Value.CameraToVIOOrigin.Inverse());
            }
            OnTrack();

            return Tuple.Create(MapRoot.gameObject, mapRootToCamera);
        }

        internal void UpdateTransform(GameObject center, Pose centerPose)
        {
            if (curMapInfo.OnNone)
            {
                return;
            }

            if (preMapInfo.OnSome)
            {
                OnMapChange();
                preMapInfo = null;
            }

            var mapRootToCamera = new Pose(curMapInfo.Value.Map.transform.localPosition, curMapInfo.Value.Map.transform.localRotation).Inverse()
                .GetTransformedBy(curMapInfo.Value.Pose.MapToCamera);
            if (!(EnablePoseStabilizer && hasRTCT) && curMotionInfo.HasValue && curMapInfo.Value.Pose.CameraToVIOOrigin.HasValue)
            {
                mapRootToCamera = mapRootToCamera
                    .GetTransformedBy(curMapInfo.Value.Pose.CameraToVIOOrigin.Value)
                    .GetTransformedBy(curMotionInfo.Value.CameraToVIOOrigin.Inverse());
            }
            var cameraToWorld = centerPose.Inverse()
                .GetTransformedBy(new Pose(center.transform.localPosition, center.transform.localRotation));

            var mapRootToWorld = mapRootToCamera.GetTransformedBy(cameraToWorld);

            MapRoot.transform.localPosition = mapRootToWorld.position;
            MapRoot.transform.localRotation = mapRootToWorld.rotation;

            OnTrack();
        }

        private void OnMapChange()
        {
            var curMapRootToCameraWhenLocalized = new Pose(preMapInfo.Value.Map.transform.localPosition, preMapInfo.Value.Map.transform.localRotation).Inverse().GetTransformedBy(preMapInfo.Value.Pose.MapToCamera);

            var pose = curMapInfo.Value.Pose.MapToCamera;
            if (!(EnablePoseStabilizer && hasRTCT) && curMotionInfo.HasValue && curMapInfo.Value.Pose.CameraToVIOOrigin.HasValue)
            {
                pose = pose
                    .GetTransformedBy(curMapInfo.Value.Pose.CameraToVIOOrigin.Value)
                    .GetTransformedBy(curMotionInfo.Value.CameraToVIOOrigin.Inverse());
            }
            pose = pose.GetTransformedBy(curMapRootToCameraWhenLocalized.Inverse());

            curMapInfo.Value.Map.transform.localPosition = pose.position;
            curMapInfo.Value.Map.transform.localRotation = pose.rotation;
        }

        private void OnTrack()
        {
            if (!curMapInfo.Value.IsFound)
            {
                MapRoot.OnFound();
                curMapInfo.Value.IsFound = true;
            }
        }

        internal SparseSpatialMapController TryGetMapController(string id)
        {
            SparseSpatialMapController controller;
            if (maps.TryGetValue(id, out controller))
                return controller;
            return null;
        }

        internal struct PoseSet
        {
            public Pose MapToCamera;
            public Pose? CameraToVIOOrigin;
        }

        internal class ResolvedMapInfo
        {
            public SparseSpatialMapController Map;
            public PoseSet Pose;
            public bool IsFound;
        }

        private struct MotionInfo
        {
            public Pose CameraToVIOOrigin;
        }
    }
}
