//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace easyar
{
    public class MenuItems
    {
        const int pSpatialMap = 20;
        const int pMotionTracking = 21;
        const int pSurfaceTracking = 22;
        const int pImageTracking = 23;
        const int pObjectTracking = 24;
        const int pEyewear = 25;
        const int pFrame = 26;
        const int pVideo = 27;
        const int pSession = 30;

        enum SessionPreset
        {
            SparseSpatialMap,
            DenseSpatialMap,
            SparseAndDenseSpatialMap,
            CloudSpatialMap,
            MotionTracking_PreferMotionTracker,
            MotionTracking_PreferSystem,
            MotionTracking_PreferARFoundation,
            ImageTracking,
            ImageTracking_MotionFusion,
            ImageTrackingMultiTracker,
            CloudRecognition,
            ObjectTracking,
            ObjectTracking_MotionFusion,
            SurfaceTracking,
            FramePlayer,
            FrameSource_CameraDeviceOnly,
            FrameSource_GroupAll,
        }

        enum MotionTrackerPreset
        {
            PreciseAnchor,
            SpatialMap,
            CloudSpatialMap,
            ObjectSensing,
            MinimumResourceUsage,
        }

        #region ARSession

        [MenuItem("GameObject/EasyAR Sense/AR Session (Preset)/AR Session (Empty)", priority = pSession)]
        static void ARSession() => CreateObject<ARSession>("AR Session (EasyAR)");

        [MenuItem("GameObject/EasyAR Sense/Eyewear/AR Session (Sparse SpatialMap Preset)", priority = pEyewear)]
        [MenuItem("GameObject/EasyAR Sense/AR Session (Preset)/AR Session (Sparse SpatialMap Preset)", priority = pSession)]
        [MenuItem("GameObject/EasyAR Sense/SpatialMap/AR Session (Sparse SpatialMap Preset)", priority = pSpatialMap)]
        static void ARSessionPresetSparseSpatialMap() => CreateARSessionPreset<ARSession>("AR Session (EasyAR)", SessionPreset.SparseSpatialMap);

        [MenuItem("GameObject/EasyAR Sense/Eyewear/AR Session (Dense SpatialMap Preset)", priority = pEyewear)]
        [MenuItem("GameObject/EasyAR Sense/AR Session (Preset)/AR Session (Dense SpatialMap Preset)", priority = pSession)]
        [MenuItem("GameObject/EasyAR Sense/SpatialMap/AR Session (Dense SpatialMap Preset)", priority = pSpatialMap)]
        static void ARSessionPresetDenseSpatialMap() => CreateARSessionPreset<ARSession>("AR Session (EasyAR)", SessionPreset.DenseSpatialMap);

        [MenuItem("GameObject/EasyAR Sense/Eyewear/AR Session (Sparse and Dense SpatialMap Preset)", priority = pEyewear)]
        [MenuItem("GameObject/EasyAR Sense/AR Session (Preset)/AR Session (Sparse and Dense SpatialMap Preset)", priority = pSession)]
        [MenuItem("GameObject/EasyAR Sense/SpatialMap/AR Session (Sparse and Dense SpatialMap Preset)", priority = pSpatialMap)]
        static void ARSessionPresetSparseAndDenseSpatialMap() => CreateARSessionPreset<ARSession>("AR Session (EasyAR)", SessionPreset.SparseAndDenseSpatialMap);

        [MenuItem("GameObject/EasyAR Sense/Eyewear/AR Session (Cloud SpatialMap Preset)", priority = pEyewear)]
        [MenuItem("GameObject/EasyAR Sense/AR Session (Preset)/AR Session (Cloud SpatialMap Preset)", priority = pSession)]
        [MenuItem("GameObject/EasyAR Sense/Cloud SpatialMap/AR Session (Cloud SpatialMap Preset)", priority = pSpatialMap)]
        static void ARSessionPresetCloudSpatialMap() => CreateARSessionPreset<ARSession>("AR Session (EasyAR)", SessionPreset.CloudSpatialMap);

        [MenuItem("GameObject/EasyAR Sense/AR Session (Preset)/AR Session (Motion Tracking Preset) : AR Foundation First", priority = pSession)]
        [MenuItem("GameObject/EasyAR Sense/Motion Tracking/AR Session (Motion Tracking Preset) : AR Foundation First", priority = pMotionTracking)]
        static void ARSessionPresetMotionTrackingPreferARFoundation() => CreateARSessionPreset<ARSession>("AR Session (EasyAR)", SessionPreset.MotionTracking_PreferARFoundation);

        [MenuItem("GameObject/EasyAR Sense/AR Session (Preset)/AR Session (Motion Tracking Preset) : Motion Tracker First", priority = pSession)]
        [MenuItem("GameObject/EasyAR Sense/Motion Tracking/AR Session (Motion Tracking Preset) : Motion Tracker First", priority = pMotionTracking)]
        static void ARSessionPresetMotionTrackingPreferMotionTracker() => CreateARSessionPreset<ARSession>("AR Session (EasyAR)", SessionPreset.MotionTracking_PreferMotionTracker);

        [MenuItem("GameObject/EasyAR Sense/AR Session (Preset)/AR Session (Motion Tracking Preset) : System First", priority = pSession)]
        [MenuItem("GameObject/EasyAR Sense/Motion Tracking/AR Session (Motion Tracking Preset) : System First", priority = pMotionTracking)]
        static void ARSessionPresetMotionTrackingPreferSystem() => CreateARSessionPreset<ARSession>("AR Session (EasyAR)", SessionPreset.MotionTracking_PreferSystem);

        [MenuItem("GameObject/EasyAR Sense/AR Session (Preset)/AR Session (Surface Tracking Preset)", priority = pSession)]
        [MenuItem("GameObject/EasyAR Sense/Surface Tracking/AR Session (Surface Tracking Preset)", priority = pSurfaceTracking)]
        static void ARSessionPresetSurfaceTracking() => CreateARSessionPreset<ARSession>("AR Session (EasyAR)", SessionPreset.SurfaceTracking);

        [MenuItem("GameObject/EasyAR Sense/AR Session (Preset)/AR Session (Image Tracking Preset)", priority = pSession)]
        [MenuItem("GameObject/EasyAR Sense/Image Tracking/AR Session (Image Tracking Preset)", priority = pImageTracking)]
        static void ARSessionPresetImageTracking() => CreateARSessionPreset<ARSession>("AR Session (EasyAR)", SessionPreset.ImageTracking);

        [MenuItem("GameObject/EasyAR Sense/Eyewear/AR Session (Image Tracking with Motion Fusion Preset)", priority = pEyewear)]
        [MenuItem("GameObject/EasyAR Sense/AR Session (Preset)/AR Session (Image Tracking with Motion Fusion Preset)", priority = pSession)]
        [MenuItem("GameObject/EasyAR Sense/Image Tracking/AR Session (Image Tracking with Motion Fusion Preset)", priority = pImageTracking)]
        static void ARSessionPresetImageTrackingMotionFusion() => CreateARSessionPreset<ARSession>("AR Session (EasyAR)", SessionPreset.ImageTracking_MotionFusion);

        [MenuItem("GameObject/EasyAR Sense/AR Session (Preset)/AR Session (Multi Image Tracker Preset)", priority = pSession)]
        [MenuItem("GameObject/EasyAR Sense/Image Tracking/AR Session (Multi Image Tracker Preset)", priority = pImageTracking)]
        static void ARSessionPresetImageTrackingMultiTracker() => CreateARSessionPreset<ARSession>("AR Session (EasyAR)", SessionPreset.ImageTrackingMultiTracker);

        [MenuItem("GameObject/EasyAR Sense/AR Session (Preset)/AR Session (CRS Preset)", priority = pSession)]
        [MenuItem("GameObject/EasyAR Sense/Image Tracking/AR Session (CRS Preset)", priority = pImageTracking)]
        static void ARSessionPresetCloudRecognizer() => CreateARSessionPreset<ARSession>("AR Session (EasyAR)", SessionPreset.CloudRecognition);

        [MenuItem("GameObject/EasyAR Sense/AR Session (Preset)/AR Session (Object Tracking Preset)", priority = pSession)]
        [MenuItem("GameObject/EasyAR Sense/Object Tracking/AR Session (Object Tracking Preset)", priority = pObjectTracking)]
        static void ARSessionPresetObjectTracking() => CreateARSessionPreset<ARSession>("AR Session (EasyAR)", SessionPreset.ObjectTracking);

        [MenuItem("GameObject/EasyAR Sense/Eyewear/AR Session (Object Tracking with Motion Fusion Preset)", priority = pEyewear)]
        [MenuItem("GameObject/EasyAR Sense/AR Session (Preset)/AR Session (Object Tracking with Motion Fusion Preset)", priority = pSession)]
        [MenuItem("GameObject/EasyAR Sense/Object Tracking/AR Session (Object Tracking with Motion Fusion Preset)", priority = pObjectTracking)]
        static void ARSessionPresetObjectTrackingMotionFusion() => CreateARSessionPreset<ARSession>("AR Session (EasyAR)", SessionPreset.ObjectTracking_MotionFusion);

        [MenuItem("GameObject/EasyAR Sense/AR Session (Preset)/AR Session (Frame Player Only Preset)", priority = pSession)]
        [MenuItem("GameObject/EasyAR Sense/Frame Recording and Playback/AR Session (Frame Player Only Preset)", priority = pFrame)]
        static void ARSessionPresetFramePlayer() => CreateARSessionPreset<ARSession>("AR Session (EasyAR)", SessionPreset.FramePlayer);


        [MenuItem("GameObject/EasyAR Sense/AR Session (Preset)/AR Session (FrameSource Preset) : Camera Device Only", priority = pSession)]
        static void ARSessionPresetFrameSourceCameraDeviceOnly() => CreateARSessionPreset<ARSession>("AR Session (EasyAR)", SessionPreset.FrameSource_CameraDeviceOnly);

        [MenuItem("GameObject/EasyAR Sense/AR Session (Preset)/AR Session (FrameSource Preset) : All", priority = pSession)]
        static void ARSessionPresetFrameSourceAll() => CreateARSessionPreset<ARSession>("AR Session (EasyAR)", SessionPreset.FrameSource_GroupAll);

        #endregion

        #region FrameSource
        [MenuItem("GameObject/EasyAR Sense/Surface Tracking/Frame Source : Camera Device (Surface Tracking)", priority = pSurfaceTracking)]
        static void CameraDevicePreferSurfaceTracking() => CreateCameraDevice<CameraDeviceFrameSource>("Camera Device", CameraDevicePreference.PreferSurfaceTracking, true);

        [MenuItem("GameObject/EasyAR Sense/Image Tracking/Frame Source : Camera Device (Object Sensing)", priority = pImageTracking)]
        static void CameraDevicePreferObjectSensingImageTracking() => CreateCameraDevice<CameraDeviceFrameSource>("Camera Device", CameraDevicePreference.PreferObjectSensing, true);

        [MenuItem("GameObject/EasyAR Sense/Object Tracking/Frame Source : Camera Device (Object Sensing)", priority = pObjectTracking)]
        static void CameraDevicePreferObjectSensingObjectTracking() => CreateCameraDevice<CameraDeviceFrameSource>("Camera Device", CameraDevicePreference.PreferObjectSensing, true);


        [MenuItem("GameObject/EasyAR Sense/Motion Tracking/Frame Source : Motion Tracker (Precise Anchor)", priority = pMotionTracking)]
        static void MotionTrackerPreciseAnchor() => CreateMotionTracker<MotionTrackerFrameSource>("Motion Tracker", MotionTrackerPreset.PreciseAnchor, true);

        [MenuItem("GameObject/EasyAR Sense/Motion Tracking/Frame Source : Motion Tracker (SpatialMap)", priority = pMotionTracking)]
        static void MotionTrackerPreciseAnchorSpatialMap() => CreateMotionTracker<MotionTrackerFrameSource>("Motion Tracker", MotionTrackerPreset.SpatialMap, true);

        [MenuItem("GameObject/EasyAR Sense/Motion Tracking/Frame Source : Motion Tracker (Cloud SpatialMap)", priority = pMotionTracking)]
        static void MotionTrackerCloudSpatialMap() => CreateMotionTracker<MotionTrackerFrameSource>("Motion Tracker", MotionTrackerPreset.CloudSpatialMap, true);

        [MenuItem("GameObject/EasyAR Sense/Motion Tracking/Frame Source : Motion Tracker (Object Sensing)", priority = pMotionTracking)]
        static void MotionTrackerObjectSensing() => CreateMotionTracker<MotionTrackerFrameSource>("Motion Tracker", MotionTrackerPreset.ObjectSensing, true);

        [MenuItem("GameObject/EasyAR Sense/Motion Tracking/Frame Source : Motion Tracker (Minimum Resource Usage)", priority = pMotionTracking)]
        static void MotionTrackerMinimumResourceUsage() => CreateMotionTracker<MotionTrackerFrameSource>("Motion Tracker", MotionTrackerPreset.MinimumResourceUsage, true);


        [MenuItem("GameObject/EasyAR Sense/Motion Tracking/Frame Source : ARCore", priority = pMotionTracking)]
        static void ARCore() => CreateObject<ARCoreFrameSource>("ARCore", true);

        [MenuItem("GameObject/EasyAR Sense/Motion Tracking/Frame Source : ARKit", priority = pMotionTracking)]
        static void ARKit() => CreateObject<ARKitFrameSource>("ARKit", true);

        [MenuItem("GameObject/EasyAR Sense/Motion Tracking/Frame Source : AR Foundation", priority = pMotionTracking)]
        static void ARFoundation() => CreateObject<ARFoundationFrameSource>("AR Foundation", true);

        [MenuItem("GameObject/EasyAR Sense/Motion Tracking/Frame Source : Huawei AR Engine", priority = pMotionTracking)]
        static void HuaweiAREngine() => CreateObject<HuaweiAREngineFrameSource>("Huawei AR Engine", true);

        [MenuItem("GameObject/EasyAR Sense/Eyewear/Frame Source : Nreal", priority = pEyewear)]
        [MenuItem("GameObject/EasyAR Sense/Motion Tracking/Frame Source : Nreal", priority = pMotionTracking)]
        static void Nreal() => CreateObject<NrealFrameSource>("Nreal", true);

        [MenuItem("GameObject/EasyAR Sense/Motion Tracking/Frame Source Group : AR Foundation First", priority = pMotionTracking)]
        static void FrameSourceGroup_PreferARFoundation() => CreateFrameSourceGroup(SessionPreset.MotionTracking_PreferARFoundation, true);

        [MenuItem("GameObject/EasyAR Sense/Motion Tracking/Frame Source Group : Motion Tracker First", priority = pMotionTracking)]
        static void FrameSourceGroup_PreferMotionTracker() => CreateFrameSourceGroup(SessionPreset.MotionTracking_PreferMotionTracker, true);

        [MenuItem("GameObject/EasyAR Sense/Motion Tracking/Frame Source Group : System First", priority = pMotionTracking)]
        static void FrameSourceGroup_PreferSystem() => CreateFrameSourceGroup(SessionPreset.MotionTracking_PreferSystem, true);

        [MenuItem("GameObject/EasyAR Sense/Motion Tracking/Frame Source Group : Camera Device Fallback", priority = pMotionTracking)]
        static void FrameSourceGroup_All() => CreateFrameSourceGroup(SessionPreset.FrameSource_GroupAll, true);

        [MenuItem("GameObject/EasyAR Sense/Motion Tracking/Origin : World Root", priority = pMotionTracking)]
        static void WorldRoot() => CreateObject<WorldRootController>("World Root");
        #endregion

        #region FrameFilter

        [MenuItem("GameObject/EasyAR Sense/Surface Tracking/Frame Filter : Surface Tracker", priority = pSurfaceTracking)]
        static void SurfaceTracker() => CreateObject<SurfaceTrackerFrameFilter>("Surface Tracker", true);

        [MenuItem("GameObject/EasyAR Sense/Surface Tracking/Target : Surface Target", priority = pSurfaceTracking)]
        static void SurfaceTarget() => CreateObject<SurfaceTargetController>("Surface Target");

        [MenuItem("GameObject/EasyAR Sense/Image Tracking/Frame Filter : Image Tracker", priority = pImageTracking)]
        static void ImageTracker() => CreateObject<ImageTrackerFrameFilter>("Image Tracker", true);

        [MenuItem("GameObject/EasyAR Sense/Image Tracking/Target : Image Target", priority = pImageTracking)]
        static void ImageTarget() => CreateObject<ImageTargetController>("Image Target");

        [MenuItem("GameObject/EasyAR Sense/Image Tracking/Frame Filter : Cloud Recognizer", priority = pImageTracking)]
        static void CloudRecognizer() => CreateObject<CloudRecognizerFrameFilter>("Cloud Recognizer", true);

        [MenuItem("GameObject/EasyAR Sense/Object Tracking/Frame Filter : Object Tracker", priority = pObjectTracking)]
        static void ObjectTracker() => CreateObject<ObjectTrackerFrameFilter>("Object Tracker", true);

        [MenuItem("GameObject/EasyAR Sense/Object Tracking/Target : Object Target", priority = pObjectTracking)]
        static void ObjectTarget() => CreateObject<ObjectTargetController>("Object Target");


        [MenuItem("GameObject/EasyAR Sense/SpatialMap/Frame Filter : Sparse SpatialMap Worker", priority = pSpatialMap)]
        static void SparseSpatialMapWorker() => CreateObject<SparseSpatialMapWorkerFrameFilter>("Sparse SpatialMap Worker", true);

        [MenuItem("GameObject/EasyAR Sense/SpatialMap/Frame Filter : Dense SpatialMap Builder", priority = pSpatialMap)]
        static void DenseSpatialMapBuilder() => CreateDenseSpatialMapBuilder<DenseSpatialMapBuilderFrameFilter>("Dense SpatialMap Builder", true);

        [MenuItem("GameObject/EasyAR Sense/SpatialMap/Map : Sparse SpatialMap", priority = pSpatialMap)]
        static void SparseSpatialMap() => CreateSparseSpatialMap<SparseSpatialMapController>("Sparse SpatialMap", true);

        [MenuItem("GameObject/EasyAR Sense/SpatialMap/Map Root : Sparse SpatialMap Root", priority = pSpatialMap)]
        static void SparseSpatialMapRoot() => CreateObject<SparseSpatialMapRootController>("Sparse SpatialMap Root");

        [MenuItem("GameObject/EasyAR Sense/Cloud SpatialMap/Frame Filter : Cloud SpatialMap Localizer", priority = pSpatialMap)]
        static void CloudSpatialMapLocalizer() => CreateObject<CloudSpatialMapLocalizerFrameFilter>("Cloud SpatialMap Localizer", true);
        #endregion

        #region Extra
        [MenuItem("GameObject/EasyAR Sense/Frame Recording and Playback/Frame Source : Frame Player", priority = pFrame)]
        static void FramePlayer() => CreateObject<FramePlayer>("Frame Player", true);

        [MenuItem("GameObject/EasyAR Sense/Frame Recording and Playback/Frame Recorder", priority = pFrame)]
        static void FrameRecorder() => CreateObject<FrameRecorder>("Frame Recorder", true);


        [MenuItem("GameObject/EasyAR Sense/Video/Video Recorder", priority = pVideo)]
        static void VideoRecorder() => CreateObject<VideoRecorder>("Video Recorder");
        #endregion


        static GameObject CreateObject<Component>(string name, bool parentSelection = false)
        {
            var go = ObjectFactory.CreateGameObject(name, typeof(Component));
            if (parentSelection)
            {
                Undo.SetTransformParent(go.transform, Selection.activeTransform, $"Parent {name} to selection");
            }
            return go;
        }

        static GameObject CreateCameraDevice<Component>(string name, CameraDevicePreference preference, bool parentSelection = false)
        {
            var go = CreateObject<Component>(name, parentSelection);
            var source = go.GetComponent<CameraDeviceFrameSource>();
            source.CameraPreference = preference;
            return go;
        }

        static GameObject CreateMotionTracker<Component>(string name, MotionTrackerPreset preset, bool parentSelection = false)
        {
            var go = CreateObject<Component>(name, parentSelection);
            var source = go.GetComponent<MotionTrackerFrameSource>();
            SetupMotionTracker(source, preset);
            return go;
        }

        static void SetupMotionTracker(MotionTrackerFrameSource source, MotionTrackerPreset preset)
        {
            switch (preset)
            {
                case MotionTrackerPreset.PreciseAnchor:
                    source.DesiredMotionTrackerParameters.TrackingMode = MotionTrackerCameraDeviceTrackingMode.Anchor;
                    source.DesiredMotionTrackerParameters.MinQualityLevel = MotionTrackerCameraDeviceQualityLevel.NotSupported;
                    source.DesiredMotionTrackerParameters.FocusMode = MotionTrackerCameraDeviceFocusMode.Continousauto;
                    break;
                case MotionTrackerPreset.SpatialMap:
                    source.DesiredMotionTrackerParameters.TrackingMode = MotionTrackerCameraDeviceTrackingMode.SLAM;
                    source.DesiredMotionTrackerParameters.MinQualityLevel = MotionTrackerCameraDeviceQualityLevel.Bad;
                    source.DesiredMotionTrackerParameters.FocusMode = MotionTrackerCameraDeviceFocusMode.Continousauto;
                    break;
                case MotionTrackerPreset.CloudSpatialMap:
                    source.DesiredMotionTrackerParameters.TrackingMode = MotionTrackerCameraDeviceTrackingMode.SLAM;
                    source.DesiredMotionTrackerParameters.MinQualityLevel = MotionTrackerCameraDeviceQualityLevel.Good;
                    source.DesiredMotionTrackerParameters.FocusMode = MotionTrackerCameraDeviceFocusMode.Medium;
                    break;
                case MotionTrackerPreset.ObjectSensing:
                case MotionTrackerPreset.MinimumResourceUsage:
                    source.DesiredMotionTrackerParameters.TrackingMode = MotionTrackerCameraDeviceTrackingMode.VIO;
                    source.DesiredMotionTrackerParameters.MinQualityLevel = MotionTrackerCameraDeviceQualityLevel.NotSupported;
                    source.DesiredMotionTrackerParameters.FocusMode = MotionTrackerCameraDeviceFocusMode.Continousauto;
                    break;
                default:
                    break;
            }
        }

        static GameObject CreateSparseSpatialMap<Component>(string name, bool parentSelection = false)
        {
            var go = CreateObject<Component>(name, parentSelection);
            var controller = go.GetComponent<SparseSpatialMapController>();
            var pgo = ObjectFactory.CreateGameObject("Point Cloud Particle System", typeof(ParticleSystem));
            Undo.SetTransformParent(pgo.transform, go.transform, $"Parent {pgo.name} to {go.name}");

            var particle = pgo.GetComponent<ParticleSystem>();
            var main = particle.main;
            main.loop = false;
            main.startSize = 0.015f;
            main.startColor = new Color(11f / 255f, 205f / 255f, 255f / 255f, 1);
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.playOnAwake = false;
            var emission = particle.emission;
            emission.enabled = false;
            var shape = particle.shape;
            shape.enabled = false;
            var renderer = particle.GetComponent<Renderer>();
            renderer.material = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.easyar.sense/Assets/Materials/PointCloudParticle.mat");
            controller.PointCloudParticleSystem = particle;
            return go;
        }

        static GameObject CreateDenseSpatialMapBuilder<Component>(string name, bool parentSelection = false)
        {
            var go = CreateObject<Component>(name, parentSelection);
            var filter = go.GetComponent<DenseSpatialMapBuilderFrameFilter>();
            var renderer = go.GetComponent<DenseSpatialMapDepthRenderer>();
            filter.MapMeshMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.easyar.sense/Assets/Materials/DenseSpatialMapMesh.mat");
            renderer.Shader = AssetDatabase.LoadAssetAtPath<Shader>("Packages/com.easyar.sense/Assets/Shaders/DenseSpatialMapDepth.shader");
            return go;
        }

        static GameObject CreateFrameSourceGroup(SessionPreset preset, bool parentSelection = false)
        {
            var group = ObjectFactory.CreateGameObject("Frame Source Group");
            if (parentSelection)
            {
                Undo.SetTransformParent(group.transform, Selection.activeTransform, $"Parent {group.name} to selection");
            }
            var sources = new List<GameObject>();
            var sysSources = new List<GameObject>{
                CreateObject<HuaweiAREngineFrameSource>("Huawei AR Engine"),
                CreateObject<ARCoreFrameSource>("ARCore"),
                CreateObject<ARKitFrameSource>("ARKit"),
            };
            switch (preset)
            {
                case SessionPreset.FrameSource_GroupAll:
                    sources.Add(CreateObject<NrealFrameSource>("Nreal"));
                    sources.Add(CreateObject<ARFoundationFrameSource>("AR Foundation"));
                    sources.AddRange(sysSources);
                    sources.Add(CreateObject<MotionTrackerFrameSource>("Motion Tracker"));
                    sources.Add(CreateObject<CameraDeviceFrameSource>("Camera Device"));
                    break;
                case SessionPreset.MotionTracking_PreferMotionTracker:
                    sources.Add(CreateObject<NrealFrameSource>("Nreal"));
                    sources.Add(CreateObject<MotionTrackerFrameSource>("Motion Tracker"));
                    sources.AddRange(sysSources);
                    sources.Add(CreateObject<ARFoundationFrameSource>("AR Foundation"));
                    break;
                case SessionPreset.MotionTracking_PreferSystem:
                    sources.Add(CreateObject<NrealFrameSource>("Nreal"));
                    sources.AddRange(sysSources);
                    sources.Add(CreateObject<ARFoundationFrameSource>("AR Foundation"));
                    sources.Add(CreateObject<MotionTrackerFrameSource>("Motion Tracker"));
                    break;
                case SessionPreset.MotionTracking_PreferARFoundation:
                    sources.Add(CreateObject<NrealFrameSource>("Nreal"));
                    sources.Add(CreateObject<ARFoundationFrameSource>("AR Foundation"));
                    sources.AddRange(sysSources);
                    sources.Add(CreateObject<MotionTrackerFrameSource>("Motion Tracker"));
                    break;
            }
            foreach (var source in sources)
            {
                Undo.SetTransformParent(source.transform, group.transform, $"Parent {source.name} to {group.name}");
            }
            return group;
        }

        static GameObject CreateARSessionPreset<Component>(string name, SessionPreset preset, bool parentSelection = false)
        {
            var session = CreateObject<Component>(name, parentSelection);

            void parentSession(GameObject go) => Undo.SetTransformParent(go.transform, session.transform, $"Parent {go.name} to {session.name}");
            void addFrameSource(SessionPreset p)
            {
                var source = CreateFrameSourceGroup(p);
                parentSession(source);
            }
            void addCameraDevice(CameraDevicePreference preference)
            {
                var source = CreateCameraDevice<CameraDeviceFrameSource>("Camera Device", preference);
                parentSession(source);
            }
            void addFrameRecorder()
            {
                var recorder = CreateObject<FrameRecorder>("Frame Recorder");
                parentSession(recorder);
            }
            void addFramePlayer()
            {
                var player = CreateObject<FramePlayer>("Frame Player");
                parentSession(player);
            }
            void addFrameFilter<Filter>(string n)
            {
                if (typeof(Filter) == typeof(DenseSpatialMapBuilderFrameFilter))
                {
                    var filter = CreateDenseSpatialMapBuilder<Filter>(n);
                    parentSession(filter);
                }
                else
                {
                    var filter = CreateObject<Filter>(n);
                    parentSession(filter);
                }
            }

            switch (preset)
            {
                case SessionPreset.FrameSource_CameraDeviceOnly:
                    addCameraDevice(CameraDevicePreference.PreferObjectSensing);
                    addFramePlayer();
                    addFrameRecorder();
                    break;
                case SessionPreset.FrameSource_GroupAll:
                case SessionPreset.MotionTracking_PreferMotionTracker:
                case SessionPreset.MotionTracking_PreferSystem:
                case SessionPreset.MotionTracking_PreferARFoundation:
                    addFrameSource(preset);
                    addFramePlayer();
                    addFrameRecorder();
                    SetupMotionTracker(session.GetComponentInChildren<MotionTrackerFrameSource>(), MotionTrackerPreset.PreciseAnchor);
                    break;
                case SessionPreset.SparseSpatialMap:
                    addFrameSource(SessionPreset.MotionTracking_PreferARFoundation);
                    addFrameFilter<SparseSpatialMapWorkerFrameFilter>("Sparse SpatialMap Worker");
                    addFramePlayer();
                    addFrameRecorder();
                    SetupMotionTracker(session.GetComponentInChildren<MotionTrackerFrameSource>(), MotionTrackerPreset.SpatialMap);
                    break;
                case SessionPreset.DenseSpatialMap:
                    addFrameSource(SessionPreset.MotionTracking_PreferARFoundation);
                    addFrameFilter<DenseSpatialMapBuilderFrameFilter>("Dense SpatialMap Builder");
                    addFramePlayer();
                    addFrameRecorder();
                    SetupMotionTracker(session.GetComponentInChildren<MotionTrackerFrameSource>(), MotionTrackerPreset.SpatialMap);
                    break;
                case SessionPreset.SparseAndDenseSpatialMap:
                    addFrameSource(SessionPreset.MotionTracking_PreferARFoundation);
                    addFrameFilter<SparseSpatialMapWorkerFrameFilter>("Sparse SpatialMap Worker");
                    addFrameFilter<DenseSpatialMapBuilderFrameFilter>("Dense SpatialMap Builder");
                    addFramePlayer();
                    addFrameRecorder();
                    SetupMotionTracker(session.GetComponentInChildren<MotionTrackerFrameSource>(), MotionTrackerPreset.SpatialMap);
                    break;
                case SessionPreset.ImageTracking:
                    addCameraDevice(CameraDevicePreference.PreferObjectSensing);
                    addFrameFilter<ImageTrackerFrameFilter>("Image Tracker");
                    addFramePlayer();
                    addFrameRecorder();
                    break;
                case SessionPreset.ImageTrackingMultiTracker:
                    addCameraDevice(CameraDevicePreference.PreferObjectSensing);
                    for (int i = 0; i < 3; ++i)
                    {
                        addFrameFilter<ImageTrackerFrameFilter>($"Image Tracker ({i + 1})");
                    }
                    addFramePlayer();
                    addFrameRecorder();
                    break;
                case SessionPreset.CloudRecognition:
                    addCameraDevice(CameraDevicePreference.PreferObjectSensing);
                    addFrameFilter<ImageTrackerFrameFilter>("Image Tracker");
                    addFrameFilter<CloudRecognizerFrameFilter>("Cloud Recognizer");
                    addFramePlayer();
                    addFrameRecorder();
                    break;
                case SessionPreset.ObjectTracking:
                    addCameraDevice(CameraDevicePreference.PreferObjectSensing);
                    addFrameFilter<ObjectTrackerFrameFilter>("Object Tracker");
                    addFramePlayer();
                    addFrameRecorder();
                    break;
                case SessionPreset.SurfaceTracking:
                    addCameraDevice(CameraDevicePreference.PreferSurfaceTracking);
                    addFrameFilter<SurfaceTrackerFrameFilter>("Surface Tracker");
                    addFramePlayer();
                    addFrameRecorder();
                    break;
                case SessionPreset.CloudSpatialMap:
                    addFrameSource(SessionPreset.MotionTracking_PreferARFoundation);
                    addFrameFilter<CloudSpatialMapLocalizerFrameFilter>("Cloud SpatialMap Localizer");
                    addFramePlayer();
                    addFrameRecorder();
                    SetupMotionTracker(session.GetComponentInChildren<MotionTrackerFrameSource>(), MotionTrackerPreset.CloudSpatialMap);
                    session.GetComponentInChildren<ARCoreFrameSource>().AutoFocus = false;
                    session.GetComponentInChildren<ARKitFrameSource>().AutoFocus = false;
                    session.GetComponentInChildren<HuaweiAREngineFrameSource>().UseHighResolutionImage = true;
                    break;
                case SessionPreset.ImageTracking_MotionFusion:
                    addFrameSource(SessionPreset.FrameSource_GroupAll);
                    addFrameFilter<ImageTrackerFrameFilter>("Image Tracker");
                    session.GetComponentInChildren<ImageTrackerFrameFilter>().EnableMotionFusion = true;
                    addFramePlayer();
                    addFrameRecorder();
                    SetupMotionTracker(session.GetComponentInChildren<MotionTrackerFrameSource>(), MotionTrackerPreset.ObjectSensing);
                    break;
                case SessionPreset.ObjectTracking_MotionFusion:
                    addFrameSource(SessionPreset.FrameSource_GroupAll);
                    addFrameFilter<ObjectTrackerFrameFilter>("Object Tracker");
                    session.GetComponentInChildren<ObjectTrackerFrameFilter>().EnableMotionFusion = true;
                    addFramePlayer();
                    addFrameRecorder();
                    SetupMotionTracker(session.GetComponentInChildren<MotionTrackerFrameSource>(), MotionTrackerPreset.ObjectSensing);
                    break;
                case SessionPreset.FramePlayer:
                    addFramePlayer();
                    addFrameRecorder();
                    break;
                default:
                    break;
            }
            return session;
        }
    }
}
