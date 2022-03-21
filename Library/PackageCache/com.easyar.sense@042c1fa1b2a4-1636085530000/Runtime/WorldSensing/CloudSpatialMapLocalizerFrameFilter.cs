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
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace easyar
{
    /// <summary>
    /// <para xml:lang="en"><see cref="MonoBehaviour"/> which controls <see cref="CloudLocalizer"/> in the scene, providing a few extensions in the Unity environment. There is no need to use <see cref="CloudLocalizer"/> directly.</para>
    /// <para xml:lang="zh">在场景中控制<see cref="CloudLocalizer"/>的<see cref="MonoBehaviour"/>，在Unity环境下提供功能扩展。不需要直接使用<see cref="CloudLocalizer"/>。</para>
    /// </summary>
    public class CloudSpatialMapLocalizerFrameFilter : FrameFilter
    {
        /// <summary>
        /// <para xml:lang="en" access="internal">WARNING: Designed for internal tools only. Do not use this interface in your application. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh" access="internal">警告：仅用于内部工具。不要在应用开发中使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        /// <senseapi/>
        public Accelerometer Accelerometer { get; private set; }

        /// <summary>
        /// <para xml:lang="en">Use global service config or not. The global service config can be changed on the inspector after click Unity menu EasyAR -> Sense -> Configuration.</para>
        /// <para xml:lang="zh">是否使用全局服务器配置。全局配置可以点击Unity菜单EasyAR -> Sense -> Configuration后在属性面板里面进行填写。</para>
        /// </summary>
        public bool UseGlobalServiceConfig = true;

        /// <summary>
        /// <para xml:lang="en">Service config when <see cref="UseGlobalServiceConfig"/> == false, only valid for this object.</para>
        /// <para xml:lang="zh"><see cref="UseGlobalServiceConfig"/> == false时使用的服务器配置，只对该物体有效。</para>
        /// </summary>
        [HideInInspector, SerializeField]
        public CloudLocalizerServiceConfig ServiceConfig = new CloudLocalizerServiceConfig();

        /// <senseapi/>
        private CloudLocalizer cloudLocalizer;
        private readonly Queue<Request> pendingRequets = new Queue<Request>();
        private string message = string.Empty;
        private ARSession arSession;
        private RealTimeCoordinateTransform rtct;
        private int index;

        public override int BufferRequirement
        {
            get { return 0; }
        }

#if EASYAR_ENABLE_CLOUDSPATIALMAP
        /// <summary>
        /// <para xml:lang="en">The map holder which holds and manages maps in the scene.</para>
        /// <para xml:lang="zh">持有地图的组件，在场景中持有并管理地图。</para>
        /// </summary>
        public SpatialMapHolder MapHolder { get; private set; }
        /// <summary>
        /// <para xml:lang="en" access="internal">WARNING: Designed for internal tools only. Do not use this interface in your application. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh" access="internal">警告：仅用于内部工具。不要在应用开发中使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public LocationManager LocationManager { get; private set; }
#endif

        /// <summary>
        /// <para xml:lang="en" access="internal">WARNING: Designed for internal tools only. Do not use this interface in your application. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh" access="internal">警告：仅用于内部工具。不要在应用开发中使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public RealTimeCoordinateTransform RealTimeCoordinateTransform { get => rtct; }

        protected virtual void Awake()
        {
#if EASYAR_ENABLE_CLOUDSPATIALMAP
            MapHolder = gameObject.AddComponent<SpatialMapHolder>();
            LocationManager = gameObject.AddComponent<LocationManager>();
#else
            Debug.LogWarning($"Package com.easyar.spatialmap is required to use {nameof(CloudSpatialMapLocalizerFrameFilter)}");
#endif
            if (!EasyARController.Initialized)
            {
                return;
            }
            rtct = new RealTimeCoordinateTransform();
        }

        protected virtual void OnEnable()
        {
            Accelerometer?.openWithSamplingPeriod(100);
        }

        protected virtual void OnDisable()
        {
            Accelerometer?.close();
        }

        protected virtual void OnDestroy()
        {
            cloudLocalizer?.Dispose();
            Accelerometer?.Dispose();
            rtct?.Dispose();
        }

        public override void OnAssemble(ARSession session)
        {
            base.OnAssemble(session);
            arSession = session;
            var isHMD = session.Assembly != null && session.Assembly.FrameSource ? session.Assembly.FrameSource.IsHMD : false;
            session.FrameUpdate += (outputFrame) =>
            {
                if (cloudLocalizer == null)
                {
                    return;
                }
                while (pendingRequets.Count > 0)
                {
                    using (var iFrame = outputFrame.inputFrame())
                    {
                        var request = pendingRequets.Dequeue();
                        if (iFrame.hasSpatialInformation() && iFrame.trackingStatus() == MotionTrackingStatus.NotTracking)
                        {
                            request.FinishCallback?.Invoke(null, "Skip underlying resolve: NotTracking", 0, Optional<Matrix44F>.Empty);
                            continue;
                        }

                        var acceleration = Optional<Vec3F>.Empty;
                        if (request.Input.OnSome && request.Input.Value.AccelerationProvider.OnSome)
                        {
                            var acc = request.Input.Value.AccelerationProvider.Value(iFrame.timestamp());
                            if (acc.OnSome)
                            {
                                acceleration = acc.Value.ToEasyARVector();
                            }
                        }
                        else if (Accelerometer != null && !isHMD)
                        {
                            var accResult = Accelerometer.getCurrentResult();
                            if (accResult.OnSome)
                            {
                                acceleration = new Vec3F(accResult.Value.x, accResult.Value.y, accResult.Value.z);
                            }
                        }
                        var location = Optional<Vec3D>.Empty;
#if EASYAR_ENABLE_CLOUDSPATIALMAP
                        if (request.Input.OnSome && request.Input.Value.LocationProvider.OnSome)
                        {
                            var loc = request.Input.Value.LocationProvider.Value(iFrame.timestamp());
                            if (loc.OnSome)
                            {
                                location = new Vec3D(loc.Value.latitude, loc.Value.longitude, loc.Value.altitude);
                            }
                        }
                        else
                        {
                            var loc = LocationManager.CurrentResult;
                            if (loc.HasValue)
                            {
                                location = new Vec3D(loc.Value.latitude, loc.Value.longitude, loc.Value.altitude);
                            }
                        }
#endif

                        if (request.StartCallback != null) { request.StartCallback(iFrame); }
                        var cameraToVIOOrigin = iFrame.hasSpatialInformation() ? iFrame.cameraTransform() : Optional<Matrix44F>.Empty;
                        var timestamp = iFrame.timestamp();
                        cloudLocalizer.resolve(iFrame, request.Message, acceleration, location, EasyARController.Scheduler, (result) => { request.FinishCallback(result, string.Empty, timestamp, cameraToVIOOrigin); });
                    }
                }
            };
            StartCoroutine(AutoCreate());
#if EASYAR_ENABLE_CLOUDSPATIALMAP
            RequestLocationPermission();
#endif
        }

        /// <summary>
        /// <para xml:lang="en" access="internal">WARNING: Designed for internal tools only. Do not use this interface in your application. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh" access="internal">警告：仅用于内部工具。不要在应用开发中使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public void ResetRealTimeCoordinateTransform()
        {
            rtct?.Dispose();
            rtct = new RealTimeCoordinateTransform();
        }

        /// <summary>
        /// <para xml:lang="en">Send localization request.</para>
        /// <para xml:lang="zh">发送定位请求。</para>
        /// </summary>
        public void Resolve(Optional<Input> input, Action<InputFrame> start, Action<CloudLocalizeStatus, string> finish)
        {
            ResolveRaw(input, start, (result, error) =>
            {
                if (result.OnNone)
                {
                    finish?.Invoke(CloudLocalizeStatus.ExceptionCaught, error);
                }
                else
                {
                    finish?.Invoke(result.Value.getLocalizeStatus(), result.Value.getExceptionInfo());
                }
            });
        }

        /// <summary>
        /// <para xml:lang="en" access="internal">WARNING: Designed for internal tools only. Do not use this interface in your application. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh" access="internal">警告：仅用于内部工具。不要在应用开发中使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public void ResolveRaw(Optional<Input> input, Action<InputFrame> start, Action<Optional<CloudLocalizeResult>, string> finish)
        {
#if !EASYAR_ENABLE_CLOUDSPATIALMAP
            index++;
            throw new InvalidOperationException($"Package com.easyar.spatialmap is required to use {nameof(CloudSpatialMapLocalizerFrameFilter)}");
#else
            if (cloudLocalizer == null)
            {
                finish?.Invoke(null, "Skip underlying resolve: localizer unavailable");
                return;
            }
            if (!enabled)
            {
                finish?.Invoke(null, "Skip underlying resolve: localizer disabled");
                return;
            }
            var requestIndex = index;
            index++;
            var request = new Request
            {
                Input = input,
                StartCallback = start,
                FinishCallback = (resultO, error, timestamp, cameraToVIOOrigin) =>
                {
                    if (resultO.OnNone)
                    {
                        finish?.Invoke(resultO, error);
                        return;
                    }
                    if (index <= requestIndex)
                    {
                        finish?.Invoke(null, "Skip underlying resolve callback: localizer reset");
                        return;
                    }
                    var result = resultO.Value;
                    if (result.getLocalizeStatus() == CloudLocalizeStatus.FoundMaps)
                    {
                        if (cameraToVIOOrigin.OnSome)
                        {
                            rtct.insertData(timestamp, cameraToVIOOrigin.Value, result.getPose());
                        }

                        var dl = result.getDeviceLocation();

                        MapHolder.OnLocalize(new SpatialMapHolder.MapInfo
                        {
                            ID = result.getLocalizedMapID(),
                            Name = result.getLocalizedMapName(),
                            Pose = new SpatialMapHolder.PoseSet
                            {
                                MapToCamera = result.getPose().ToUnityPose(),
                                CameraToVIOOrigin = cameraToVIOOrigin.OnSome ? cameraToVIOOrigin.Value.ToUnityPose() : default(Pose?),
                            },
                            DeviceLocation = dl.OnNone ? default(Location?) : new Location
                            {
                                latitude = dl.Value.data_1,
                                longitude = dl.Value.data_0,
                                altitude = dl.Value.data_2
                            },
#pragma warning disable 612, 618
                            Block = string.IsNullOrEmpty(result.getLocalizedBlockId()) ? default(SpatialMapHolder.MapInfo.BlockInfo?) : new SpatialMapHolder.MapInfo.BlockInfo
                            {
                                ID = result.getLocalizedBlockId(),
                                Timestamp = result.getLocalizedBlockTimestamp(),
                            }
#pragma warning restore 612, 618
                        });
                    }
                    finish?.Invoke(result, string.Empty);
                },
                Message = message,
            };
            pendingRequets.Enqueue(request);
            StartCoroutine(CheckRequest(request));
#endif
        }

        /// <summary>
        /// <para xml:lang="en" access="internal">WARNING: Designed for internal tools only. Do not use this interface in your application. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh" access="internal">警告：仅用于内部工具。不要在应用开发中使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public void SetMessage(string msg)
        {
            message = msg;
        }

        /// <summary>
        /// <para xml:lang="en" access="internal">WARNING: Designed for internal tools only. Do not use this interface in your application. Accessibility Level may change in future.</para>
        /// <para xml:lang="zh" access="internal">警告：仅用于内部工具。不要在应用开发中使用这个接口。可访问级别可能会在未来产生变化。</para>
        /// </summary>
        public void ResetResolve()
        {
            index = 0;
            rtct?.Dispose();
            rtct = new RealTimeCoordinateTransform();
        }

#if EASYAR_ENABLE_CLOUDSPATIALMAP
        public override void UpdateMotion(double timestamp, MotionTrackingStatus trackingStatus, Matrix44F cameraTransform)
        {
            if (!arSession || !MapHolder.Localized()) { return; }
            MapHolder.UpdateMotion(cameraTransform.ToUnityPose(), rtct.getPoseInMap(timestamp, trackingStatus, cameraTransform).ToUnityPose());
        }

        public override Optional<Tuple<GameObject, Pose>> TryGetCenter(GameObject center)
        {
            if (!arSession || !MapHolder.Localized()) { return null; }
            if (center && center != MapHolder.MapRoot.gameObject) { return null; }
            return MapHolder.TryGetCenter();
        }

        public override void UpdateTransform(GameObject center, Pose centerPose)
        {
            if (!arSession || !MapHolder.Localized()) { return; }
            if (center == MapHolder.MapRoot.gameObject) { return; }
            MapHolder.UpdateTransform(center, centerPose);
        }

        private void RequestLocationPermission()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                LocationManager.SetLocationPermission(null);
                StartCoroutine(CheckLocationPermission());
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
#if UNITY_ANDROID
                if (Permission.HasUserAuthorizedPermission(Permission.FineLocation))
                {
                    LocationManager.SetLocationPermission(true);
                }
                else
                {
#if UNITY_2020_2_OR_NEWER
                    var callbacks = new PermissionCallbacks();
                    callbacks.PermissionGranted += (_) => { if (LocationManager) { LocationManager.SetLocationPermission(true); } };
                    callbacks.PermissionDenied += (_) => { if (LocationManager) { LocationManager.SetLocationPermission(false); } };
                    callbacks.PermissionDeniedAndDontAskAgain += (_) => { if (LocationManager) { LocationManager.SetLocationPermission(false); } };
                    Permission.RequestUserPermission(Permission.FineLocation, callbacks);
#else
                    Permission.RequestUserPermission(Permission.FineLocation);
                    LocationManager.SetLocationPermission(null);
#endif
                }
                StartCoroutine(CheckLocationPermission());
#endif
            }
        }

        private IEnumerator CheckLocationPermission()
        {
            while (!LocationManager.IsPermissionGranted.HasValue) { yield return null; }
            if (!LocationManager.IsPermissionGranted.Value)
            {
                throw new UIPopupException("Location permission not granted");
            }
        }
#endif

        private IEnumerator AutoCreate()
        {
            while (!enabled) { yield return null; }
            if (!CloudLocalizer.isAvailable()) { throw new UIPopupException(typeof(CloudLocalizer) + " not available"); }


            var config = new CloudLocalizerServiceConfig();
            if (UseGlobalServiceConfig)
            {
                if (EasyARController.Settings)
                {
                    config = EasyARController.Settings.GlobalCloudLocalizerServeiceConfig;
                }
            }
            else
            {
                config = ServiceConfig;
            }
            NotifyEmptyConfig(config);
            cloudLocalizer = CloudLocalizer.create(config.ServerAddress, config.APIKey, config.APISecret, config.CloudLocalizerAppID);

            Accelerometer = new Accelerometer();
            if (!Accelerometer.isAvailable())
            {
                Accelerometer.Dispose();
                Accelerometer = null;
            }
            OnEnable();
        }

        private IEnumerator CheckRequest(Request req)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            while (pendingRequets.Count > 0 && pendingRequets.Contains(req))
            {
                var request = pendingRequets.Dequeue();
                request.FinishCallback?.Invoke(null, "Skip underlying resolve: NoFrame", 0, Optional<Matrix44F>.Empty);
            }
        }

        private static void NotifyEmptyConfig(CloudLocalizerServiceConfig config)
        {
            if (string.IsNullOrEmpty(config.ServerAddress) ||
                string.IsNullOrEmpty(config.APIKey) ||
                string.IsNullOrEmpty(config.APISecret) ||
                string.IsNullOrEmpty(config.CloudLocalizerAppID))
            {
                throw new UIPopupException(
                    "Service config (for authentication) NOT set, please set" + Environment.NewLine +
                    "globally on <EasyAR Settings> Asset or" + Environment.NewLine +
                    "locally on <CloudSpatialMapLocalizerFrameFilter> Component." + Environment.NewLine +
                    "Get from EasyAR Develop Center (www.easyar.com) -> CLS -> Database Details.");
            }
        }

        /// <summary>
        /// <para xml:lang="en">Service config for <see cref="easyar.CloudLocalizer"/>.</para>
        /// <para xml:lang="zh"><see cref="easyar.CloudLocalizer"/>服务器配置。</para>
        /// </summary>
        [Serializable]
        public class CloudLocalizerServiceConfig
        {
            /// <summary>
            /// <para xml:lang="en">Server Address, go to EasyAR Develop Center (https://www.easyar.com) for details.</para>
            /// <para xml:lang="zh">服务器地址，详见EasyAR开发中心（https://www.easyar.cn）。</para>
            /// </summary>
            public string ServerAddress = string.Empty;
            /// <summary>
            /// <para xml:lang="en">API Key, go to EasyAR Develop Center (https://www.easyar.com) for details.</para>
            /// <para xml:lang="zh">API Key，详见EasyAR开发中心（https://www.easyar.cn）。</para>
            /// </summary>
            public string APIKey = string.Empty;
            /// <summary>
            /// <para xml:lang="en">API Secret, go to EasyAR Develop Center (https://www.easyar.com) for details.</para>
            /// <para xml:lang="zh">API Secret，详见EasyAR开发中心（https://www.easyar.cn）。</para>
            /// </summary>
            public string APISecret = string.Empty;
            /// <summary>
            /// <para xml:lang="en">Cloud Localizer AppID, go to EasyAR Develop Center (https://www.easyar.com) for details.</para>
            /// <para xml:lang="zh">云定位AppID，详见EasyAR开发中心（https://www.easyar.cn）。</para>
            /// </summary>
            public string CloudLocalizerAppID = string.Empty;
        }

        /// <summary>
        /// <para xml:lang="en">Input data for <see cref="Resolve"/>.</para>
        /// <para xml:lang="zh"><see cref="Resolve"/>的输入数据。</para>
        /// </summary>
        public class Input
        {
#if EASYAR_ENABLE_CLOUDSPATIALMAP
            /// <summary>
            /// <para xml:lang="en">GPS data provider. Optional, <see cref="LocationManager"/> will be used if null. Not required when localize on mobile application. Use this when localize on PC when using eif files.</para>
            /// <para xml:lang="zh">GPS数据提供函数。可选，如果是null，会使用<see cref="LocationManager"/>。在移动应用上非必须输入。在PC上使用eif数据定位的时候使用。</para>
            /// </summary>
            public Optional<Func<double, Optional<Location>>> LocationProvider;
#endif
            /// <summary>
            /// <para xml:lang="en">Acceleration data provider. Optional, <see cref="Accelerometer"/> will be used if null. Not required when localize on mobile phones or officially supported eyewears. Use this when localize on PC when using eif files, or on some special devices like eyewears where system default accelerometer does not is not accelerometer on device.</para>
            /// <para xml:lang="zh">加速度计数据提供函数。可选，如果是null，会使用<see cref="Accelerometer"/>。在手机或官方支持的眼镜上非必须输入。在PC上使用eif数据定位的时候使用，或在一些系统默认加速度计不是设备加速度计时，比如眼镜等特殊设备上使用。</para>
            /// </summary>
            public Optional<Func<double, Optional<Vector3>>> AccelerationProvider;
        }

        private class Request
        {
            public Optional<Input> Input;
            public Action<InputFrame> StartCallback = null;
            public Action<Optional<CloudLocalizeResult>, string, double, Optional<Matrix44F>> FinishCallback = null;
            public string Message = string.Empty;
        }
    }
}
