using System.Collections;
using UnityEngine;
#if !UNITY_EDITOR && UNITY_ANDROID
#if VIVEHANDTRACKING_WAVEXR_HAND
using UnityEngine.XR;
using Wave.XR;
using Wave.Native;
#elif VIVEHANDTRACKING_WAVEVR_HAND
using wvr;
using RigidTransform = WaveVR_Utils.RigidTransform;
#endif
#if VIVEHANDTRACKING_WAVEXR_HAND4
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#endif
#endif

namespace ViveHandTracking {

public class WaveVRHandEngine : HandTrackingEngine {
#if VIVEHANDTRACKING_WAVEXR_HAND4 && !UNITY_EDITOR && UNITY_ANDROID
  private WVR_HandGestureData_t gestureData = new WVR_HandGestureData_t();
  private WVR_HandTrackerType trackerType = WVR_HandTrackerType.WVR_HandTrackerType_Natural;
  // key: SDK joint index; value: WaveVR joint index; initialized in TryHandTrackerType
  private int[] trackerIndex = new int[21];
  private WVR_HandTrackingData_t trackerData = new WVR_HandTrackingData_t();
  private WVR_HandPoseData_t poseData = new WVR_HandPoseData_t();
  private GestureResultRaw leftHand = null, rightHand = null;
  private bool hasGesture = true;

  public override bool IsSupported() { return true; }

  public override IEnumerator Setup() {
    leftHand = GestureResultExtension.CreateHand(true);
    rightHand = GestureResultExtension.CreateHand(false);
    yield break;
  }

  public override IEnumerator StartDetection(GestureOption option) {
    if (State.Status == GestureStatus.Starting || State.Status == GestureStatus.Running)
      yield break;

    ulong feature = Interop.WVR_Base.Instance.GetSupportedFeatures();
    hasGesture = (feature & (ulong)WVR_SupportedFeature.WVR_SupportedFeature_HandGesture) > 0;
    if (!hasGesture) Debug.LogWarning("WaveVR gesture not supported");
    if ((feature & (ulong)WVR_SupportedFeature.WVR_SupportedFeature_HandTracking) == 0) {
      Debug.LogError("WaveVR tracking not supported");
      State.Status = GestureStatus.Error;
      yield break;
    }

    if (TryHandTrackerType(WVR_HandTrackerType.WVR_HandTrackerType_Natural))
      trackerType = WVR_HandTrackerType.WVR_HandTrackerType_Natural;
    else if (TryHandTrackerType(WVR_HandTrackerType.WVR_HandTrackerType_Electronic))
      trackerType = WVR_HandTrackerType.WVR_HandTrackerType_Electronic;
    else {
      State.Status = GestureStatus.Error;
      yield break;
    }

    if (hasGesture) yield return StartWaveGesture();
    var operation = new ThreadYieldInstruction<WVR_Result>(
        () => Interop.WVR_Base.Instance.StartHandTracking(trackerType));
    yield return operation;
    if (operation.result == null || operation.result.Value != WVR_Result.WVR_Success) {
      Debug.LogError("WaveVR tracking start failed: " + operation.result);
      State.Status = GestureStatus.Error;
      Interop.WVR_Base.Instance.StopHandGesture();
      yield break;
    }

    InitTrackerData();
    State.Status = GestureStatus.Starting;
    State.Mode = GestureMode.Skeleton;
    State.Status = GestureStatus.Running;
  }

  public override void UpdateResult() {
    if (State.Status != GestureStatus.Running) return;

    var result = Interop.WVR_Base.Instance.GetHandTrackingData(
        trackerType, WVR_HandModelType.WVR_HandModelType_WithoutController, GetWaveOrigin(),
        ref trackerData, ref poseData);
    if (result != WVR_Result.WVR_Success) {
      Debug.LogError("Get tracking data failed: " + result);
      State.Status = GestureStatus.Error;
      State.Error = GestureFailure.Internal;
      return;
    }

    if (hasGesture) {
      result = Interop.WVR_Base.Instance.GetHandGestureData(ref gestureData);
      if (result != WVR_Result.WVR_Success) {
        Debug.LogError("Get gesture data failed: " + result);
        State.Status = GestureStatus.Error;
        State.Error = GestureFailure.Internal;
        return;
      }
    }

    if (trackerData.left.isValidPose) {
      leftHand.gesture = hasGesture ? MapGesture(gestureData.left) : GestureType.Unknown;
      SetHandPoints(leftHand, trackerData.left, poseData.left);
      State.SetRaw(leftHand);
    } else
      State.LeftHand = null;
    if (trackerData.right.isValidPose) {
      rightHand.gesture = hasGesture ? MapGesture(gestureData.right) : GestureType.Unknown;
      SetHandPoints(rightHand, trackerData.right, poseData.right);
      State.SetRaw(rightHand);
    } else
      State.RightHand = null;
  }

  public override void StopDetection() {
    Interop.WVR_Base.Instance.StopHandTracking(trackerType);
    if (hasGesture) Interop.WVR_Base.Instance.StopHandGesture();
    if (trackerData.left.joints != IntPtr.Zero) Marshal.FreeHGlobal(trackerData.left.joints);
    if (trackerData.right.joints != IntPtr.Zero) Marshal.FreeHGlobal(trackerData.right.joints);
    trackerData.left.joints = trackerData.right.joints = IntPtr.Zero;
  }

  bool TryHandTrackerType(WVR_HandTrackerType type) {
    uint jointCount = 0;
    var result = Interop.WVR_Base.Instance.GetHandJointCount(type, ref jointCount);
    if (result != WVR_Result.WVR_Success) {
      Debug.LogErrorFormat("WaveVR get hand joint count for {0} failed: {1}", type, result);
      return false;
    }
    if (jointCount < 21) {
      Debug.LogErrorFormat("WaveVR too few hand joints ({1}) for {0}", type, jointCount);
      return false;
    }

    WVR_HandTrackerInfo_t info = new WVR_HandTrackerInfo_t();
    info.jointCount = jointCount;
    info.handModelTypeBitMask = 0;
    info.jointMappingArray = Marshal.AllocHGlobal(sizeof(int) * (int)jointCount);
    info.jointValidFlagArray = Marshal.AllocHGlobal(sizeof(ulong) * (int)jointCount);

    result = Interop.WVR_Base.Instance.GetHandTrackerInfo(type, ref info);
    if (result != WVR_Result.WVR_Success) {
      Debug.LogErrorFormat("WaveVR get hand tracker info for {0} failed: {1}", type, result);
      Marshal.FreeHGlobal(info.jointMappingArray);
      Marshal.FreeHGlobal(info.jointValidFlagArray);
      return false;
    }

    if ((info.handModelTypeBitMask &
         (ulong)WVR_HandModelType.WVR_HandModelType_WithoutController) == 0) {
      Debug.LogErrorFormat("WaveVR bare hand model ({1}) not supported for {0}", type,
                           info.handModelTypeBitMask);
      Marshal.FreeHGlobal(info.jointMappingArray);
      Marshal.FreeHGlobal(info.jointValidFlagArray);
      return false;
    }

    int[] joints = new int[jointCount];  // WVR_HandJoint
    Marshal.Copy(info.jointMappingArray, joints, 0, joints.Length);
    byte[] validFlagBytes = new byte[sizeof(ulong) * jointCount];  // ulong
    Marshal.Copy(info.jointValidFlagArray, validFlagBytes, 0, validFlagBytes.Length);
    Marshal.FreeHGlobal(info.jointMappingArray);
    Marshal.FreeHGlobal(info.jointValidFlagArray);
    info.jointMappingArray = info.jointValidFlagArray = IntPtr.Zero;

    var jointMap = new Dictionary<WVR_HandJoint, int>() {
      { WVR_HandJoint.WVR_HandJoint_Wrist, 0 },
      { WVR_HandJoint.WVR_HandJoint_Thumb_Joint0, 1 },
      { WVR_HandJoint.WVR_HandJoint_Thumb_Joint1, 2 },
      { WVR_HandJoint.WVR_HandJoint_Thumb_Joint2, 3 },
      { WVR_HandJoint.WVR_HandJoint_Thumb_Tip, 4 },
      { WVR_HandJoint.WVR_HandJoint_Index_Joint1, 5 },
      { WVR_HandJoint.WVR_HandJoint_Index_Joint2, 6 },
      { WVR_HandJoint.WVR_HandJoint_Index_Joint3, 7 },
      { WVR_HandJoint.WVR_HandJoint_Index_Tip, 8 },
      { WVR_HandJoint.WVR_HandJoint_Middle_Joint1, 9 },
      { WVR_HandJoint.WVR_HandJoint_Middle_Joint2, 10 },
      { WVR_HandJoint.WVR_HandJoint_Middle_Joint3, 11 },
      { WVR_HandJoint.WVR_HandJoint_Middle_Tip, 12 },
      { WVR_HandJoint.WVR_HandJoint_Ring_Joint1, 13 },
      { WVR_HandJoint.WVR_HandJoint_Ring_Joint2, 14 },
      { WVR_HandJoint.WVR_HandJoint_Ring_Joint3, 15 },
      { WVR_HandJoint.WVR_HandJoint_Ring_Tip, 16 },
      { WVR_HandJoint.WVR_HandJoint_Pinky_Joint1, 17 },
      { WVR_HandJoint.WVR_HandJoint_Pinky_Joint2, 18 },
      { WVR_HandJoint.WVR_HandJoint_Pinky_Joint3, 19 },
      { WVR_HandJoint.WVR_HandJoint_Pinky_Tip, 20 },
    };
    for (int i = 0; i < 21; i++) trackerIndex[i] = -1;
    for (int i = 0; i < jointCount; i++) {
      ulong flag = BitConverter.ToUInt64(validFlagBytes, i * sizeof(ulong));
      if ((flag & (ulong)WVR_HandJointValidFlag.WVR_HandJointValidFlag_PositionValid) == 0)
        continue;
      int index;
      if (jointMap.TryGetValue((WVR_HandJoint)joints[i], out index)) trackerIndex[index] = i;
    }
    for (int i = 0; i < 21; i++) {
      if (trackerIndex[i] == -1) {
        Debug.LogErrorFormat("WaveVR missing joint {1} support for {0}", type, i);
        return false;
      }
    }

    trackerData.left.jointCount = trackerData.right.jointCount = jointCount;
    return true;
  }

  void InitTrackerData() {
    int size = Marshal.SizeOf(typeof(WVR_Pose_t));
    trackerData.left.joints = Marshal.AllocHGlobal(size * (int)trackerData.left.jointCount);
    trackerData.right.joints = Marshal.AllocHGlobal(size * (int)trackerData.right.jointCount);
  }

  IEnumerator StartWaveGesture() {
    WVR_HandGestureInfo_t gestureInfo = new WVR_HandGestureInfo_t();
    var result = Interop.WVR_Base.Instance.GetHandGestureInfo(ref gestureInfo);
    if (result != WVR_Result.WVR_Success) {
      Debug.LogWarning("WaveVR get gesture info failed: " + result);
      hasGesture = false;
      yield break;
    }
    ulong gestureDemands = (1 << (int)WVR_HandGestureType.WVR_HandGestureType_Invalid) |
                           (1 << (int)WVR_HandGestureType.WVR_HandGestureType_Unknown) |
                           (1 << (int)WVR_HandGestureType.WVR_HandGestureType_Fist) |
                           (1 << (int)WVR_HandGestureType.WVR_HandGestureType_Five) |
                           (1 << (int)WVR_HandGestureType.WVR_HandGestureType_OK) |
                           (1 << (int)WVR_HandGestureType.WVR_HandGestureType_ThumbUp) |
                           (1 << (int)WVR_HandGestureType.WVR_HandGestureType_IndexUp);
    var operation =
        new ThreadYieldInstruction<WVR_Result>(() => Interop.WVR_Base.Instance.StartHandGesture(
                                                   gestureDemands & gestureInfo.supportedMask));
    yield return operation;
    if (operation.result == null || operation.result.Value != WVR_Result.WVR_Success) {
      Debug.LogWarning("WaveVR gesture start failed: " + operation.result);
      hasGesture = false;
    }
  }

  void SetHandPoints(GestureResultRaw hand, WVR_HandJointData_t jointData,
                     WVR_HandPoseState_t poseState) {
    if (hand == null || !jointData.isValidPose) return;

    var structSize = Marshal.SizeOf(typeof(WVR_Pose_t));
    for (int i = 0; i < 21; i++) {
      var ptr = new IntPtr(jointData.joints.ToInt64() + structSize * trackerIndex[i]);
      var pose = (WVR_Pose_t)Marshal.PtrToStructure(ptr, typeof(WVR_Pose_t));
      hand.points[i] = Coordinate.GetVectorFromGL(pose.position);
    }

    // get pinch level & confidence
    if (poseState.state.type == WVR_HandPoseType.WVR_HandPoseType_Pinch)
      hand.pinch.pinchLevel = poseState.pinch.strength;
    else
      hand.pinch.pinchLevel = 0;
    hand.confidence = jointData.confidence;

    // apply camera offset to hand points
    var transform = GestureProvider.Current.transform;
    if (transform.parent != null) {
      for (int i = 0; i < 21; i++) hand.points[i] = transform.parent.TransformPoint(hand.points[i]);
    }
    hand.GenerateRotation();
  }

#elif (VIVEHANDTRACKING_WAVEVR_HAND || VIVEHANDTRACKING_WAVEXR_HAND) && !UNITY_EDITOR && UNITY_ANDROID

  private WVR_HandGestureData_t gestureData = new WVR_HandGestureData_t();
  private WVR_HandSkeletonData_t skeletonData = new WVR_HandSkeletonData_t();
  private WVR_HandPoseData_t poseData = new WVR_HandPoseData_t();
  private RigidTransform rigidTransform = RigidTransform.identity;
  private GestureResultRaw leftHand = null, rightHand = null;
  private bool hasGesture = true;

  public override bool IsSupported() { return true; }

  public override IEnumerator Setup() {
    leftHand = GestureResultExtension.CreateHand(true);
    rightHand = GestureResultExtension.CreateHand(false);
    yield break;
  }

  public override IEnumerator StartDetection(GestureOption option) {
    if (State.Status == GestureStatus.Starting || State.Status == GestureStatus.Running)
      yield break;

    ulong feature = Interop.WVR_Base.Instance.GetSupportedFeatures();
    hasGesture = (feature & (ulong)WVR_SupportedFeature.WVR_SupportedFeature_HandGesture) > 0;
    if (!hasGesture) Debug.LogWarning("WaveVR gesture not supported");
    if ((feature & (ulong)WVR_SupportedFeature.WVR_SupportedFeature_HandTracking) == 0) {
      Debug.LogError("WaveVR tracking not supported");
      State.Status = GestureStatus.Error;
      yield break;
    }

    State.Status = GestureStatus.Starting;

    var operation =
        new ThreadYieldInstruction<WVR_Result>(Interop.WVR_Base.Instance.StartHandTracking);
    yield return operation;
    if (operation.result == null || operation.result.Value != WVR_Result.WVR_Success) {
      Debug.LogError("WaveVR tracking start failed: " + operation.result);
      State.Status = GestureStatus.Error;
      Interop.WVR_Base.Instance.StopHandGesture();
      yield break;
    }
    if (hasGesture) {
      operation =
          new ThreadYieldInstruction<WVR_Result>(Interop.WVR_Base.Instance.StartHandGesture);
      yield return operation;
      if (operation.result == null || operation.result.Value != WVR_Result.WVR_Success) {
        Debug.LogWarning("WaveVR gesture start failed: " + operation.result);
        hasGesture = false;
      }
    }

    State.Mode = GestureMode.Skeleton;
    State.Status = GestureStatus.Running;
  }

  public override void UpdateResult() {
    if (State.Status != GestureStatus.Running) return;

    var result = Interop.WVR_Base.Instance.GetHandTrackingData(ref skeletonData, ref poseData,
                                                               GetWaveOrigin());
    if (result != WVR_Result.WVR_Success) {
      Debug.LogError("Get tracking data failed: " + result);
      State.Status = GestureStatus.Error;
      State.Error = GestureFailure.Internal;
      return;
    }

    if (hasGesture) {
      result = Interop.WVR_Base.Instance.GetHandGestureData(ref gestureData);
      if (result != WVR_Result.WVR_Success) {
        Debug.LogError("Get gesture data failed: " + result);
        State.Status = GestureStatus.Error;
        State.Error = GestureFailure.Internal;
        return;
      }
    }

    if (skeletonData.left.wrist.IsValidPose) {
      leftHand.gesture = hasGesture ? MapGesture(gestureData.left) : GestureType.Unknown;
      SetHandPoints(leftHand, skeletonData.left, poseData.left);
      State.SetRaw(leftHand);
    } else
      State.LeftHand = null;
    if (skeletonData.right.wrist.IsValidPose) {
      rightHand.gesture = hasGesture ? MapGesture(gestureData.right) : GestureType.Unknown;
      SetHandPoints(rightHand, skeletonData.right, poseData.right);
      State.SetRaw(rightHand);
    } else
      State.RightHand = null;
  }

  public override void StopDetection() {
    Interop.WVR_Base.Instance.StopHandTracking();
    if (hasGesture) Interop.WVR_Base.Instance.StopHandGesture();
  }

  void SetHandPoints(GestureResultRaw hand, WVR_HandSkeletonState_t skeleton,
                     WVR_HandPoseState_t pose) {
    if (hand == null || !skeleton.wrist.IsValidPose) return;
    rigidTransform.update(skeleton.wrist.PoseMatrix);
    hand.points[0] = rigidTransform.pos;
    SetFingerPoints(hand, skeleton.thumb, 1);
    SetFingerPoints(hand, skeleton.index, 5);
    SetFingerPoints(hand, skeleton.middle, 9);
    SetFingerPoints(hand, skeleton.ring, 13);
    SetFingerPoints(hand, skeleton.pinky, 17);

    // get pinch level & confidence
    if (pose.state.type == WVR_HandPoseType.WVR_HandPoseType_Pinch)
      hand.pinch.pinchLevel = pose.pinch.strength;
    else
      hand.pinch.pinchLevel = 0;
    hand.confidence = skeleton.confidence;

    // apply camera offset to hand points
    var transform = GestureProvider.Current.transform;
    if (transform.parent != null) {
      for (int i = 0; i < 21; i++) hand.points[i] = transform.parent.TransformPoint(hand.points[i]);
    }
    hand.GenerateRotation();
  }

  void SetFingerPoints(GestureResultRaw hand, WVR_FingerState_t finger, int startIndex) {
#if VIVEHANDTRACKING_WAVEXR_HAND
    hand.points[startIndex] = Coordinate.GetVectorFromGL(finger.joint1);
    hand.points[startIndex + 1] = Coordinate.GetVectorFromGL(finger.joint2);
    hand.points[startIndex + 2] = Coordinate.GetVectorFromGL(finger.joint3);
    hand.points[startIndex + 3] = Coordinate.GetVectorFromGL(finger.tip);
#else
    hand.points[startIndex] = WaveVR_Utils.GetPosition(finger.joint1);
    hand.points[startIndex + 1] = WaveVR_Utils.GetPosition(finger.joint2);
    hand.points[startIndex + 2] = WaveVR_Utils.GetPosition(finger.joint3);
    hand.points[startIndex + 3] = WaveVR_Utils.GetPosition(finger.tip);
#endif
  }

#else

  public override bool IsSupported() { return false; }

  public override IEnumerator Setup() { yield break; }

  public override IEnumerator StartDetection(GestureOption option) { yield break; }

  public override void UpdateResult() {}

  public override void StopDetection() {}

  public override string Description() {
#if !UNITY_ANDROID
    return "[Experimental] Only supported on Android WaveVR device";
#elif VIVEHANDTRACKING_WAVEXR_HAND4
    return "[Experimental] Supports Vive Focus 3 (Use WaveXR 1.x for Vive Focus/Focus Plus)";
#elif VIVEHANDTRACKING_WAVEXR_HAND
    return "[Experimental] Supports Vive Focus/Focus Plus (Use WaveXR 4.x for Vive Focus 3)";
#elif VIVEHANDTRACKING_WAVEVR_HAND
    return "[Experimental] Supports Vive Focus/Focus Plus (Vive Focus 3 not supported)";
#elif VIVEHANDTRACKING_UNITYXR
    return "[Experimental] Requires WaveXR Native package";
#else
    return "[Experimental] Requires WaveVR 3.2.0";
#endif
  }

#endif

#if (VIVEHANDTRACKING_WAVEVR_HAND || VIVEHANDTRACKING_WAVEXR_HAND) && !UNITY_EDITOR && UNITY_ANDROID
  private static GestureType MapGesture(WVR_HandGestureType gesture) {
    switch (gesture) {
      case WVR_HandGestureType.WVR_HandGestureType_Fist:
        return GestureType.Fist;
      case WVR_HandGestureType.WVR_HandGestureType_Five:
        return GestureType.Five;
      case WVR_HandGestureType.WVR_HandGestureType_OK:
        return GestureType.OK;
      case WVR_HandGestureType.WVR_HandGestureType_ThumbUp:
        return GestureType.Like;
      case WVR_HandGestureType.WVR_HandGestureType_IndexUp:
        return GestureType.Point;
      default:
        return GestureType.Unknown;
    }
  }

  private static WVR_PoseOriginModel GetWaveOrigin() {
#if VIVEHANDTRACKING_WAVEXR_HAND
    WVR_PoseOriginModel origin = WVR_PoseOriginModel.WVR_PoseOriginModel_OriginOnHead;
    var unityOrigin = Utils.InputSubsystem.GetTrackingOriginMode();
    if (unityOrigin == TrackingOriginModeFlags.Floor)
      origin = WVR_PoseOriginModel.WVR_PoseOriginModel_OriginOnGround;
    else if (unityOrigin == TrackingOriginModeFlags.TrackingReference)
      origin = WVR_PoseOriginModel.WVR_PoseOriginModel_OriginOnTrackingObserver;
    return origin;
#else
    return WaveVR_Render.Instance.origin;
#endif
  }
#endif
}
}
