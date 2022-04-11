using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using ManoMotion;

public struct ConfigInfo
{
    public bool ShowDebugConsole;
    public int QrCodeCameraFlushFrequency;
    public int PositiveGestureType;
    public int RotateScaleGestureType;
    public string email;
    public string password;
}

public class ConfigController : MonoBehaviour
{
    public static ConfigInfo Config { get; set; } = new ConfigInfo();

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private static ConfigInfo DefaultConfig()
    {
        var config = new ConfigInfo();
        config.ShowDebugConsole = true;
        config.QrCodeCameraFlushFrequency = 5;
        config.PositiveGestureType = 3;
        config.RotateScaleGestureType = 2;
        config.email = "";
        config.password = "";

        return config;
    }

    public static bool ConfigInfoEquals(ConfigInfo l, ConfigInfo r)
    {
        return 
            l.ShowDebugConsole == r.ShowDebugConsole &&
            l.QrCodeCameraFlushFrequency == r.QrCodeCameraFlushFrequency &&
            l.PositiveGestureType == r.PositiveGestureType;
    }

    public static int GestureTriggerToIndex(ManoGestureTrigger trigger)
    {
        switch (trigger)
        {
            case ManoGestureTrigger.CLICK: return 0;
            case ManoGestureTrigger.PICK: return 1;
            case ManoGestureTrigger.DROP: return 2;
            case ManoGestureTrigger.GRAB_GESTURE: return 3;
            case ManoGestureTrigger.RELEASE_GESTURE: return 4;
            default: return -1;
        }
    }

    public static ManoGestureTrigger IndexToGestureTrigger(int index)
    {
        switch (index)
        {
            case 0: return ManoGestureTrigger.CLICK;
            case 1: return ManoGestureTrigger.PICK;
            case 2: return ManoGestureTrigger.DROP;
            case 3: return ManoGestureTrigger.GRAB_GESTURE;
            case 4: return ManoGestureTrigger.RELEASE_GESTURE;
            default: return ManoGestureTrigger.NO_GESTURE;
        }
    }


    public static int GestureContinuousToIndex(ManoGestureContinuous continuous)
    {
        switch (continuous)
        {
            case ManoGestureContinuous.OPEN_PINCH_GESTURE: return 0;
            case ManoGestureContinuous.OPEN_HAND_GESTURE: return 1;
            case ManoGestureContinuous.CLOSED_HAND_GESTURE: return 2;
            case ManoGestureContinuous.HOLD_GESTURE: return 3;
            case ManoGestureContinuous.POINTER_GESTURE: return 4;
            default : return -1;
        }
    }

    public static ManoGestureContinuous IndexToGestureContinuous(int index)
    {
        switch (index)
        {
            case 0: return ManoGestureContinuous.OPEN_PINCH_GESTURE;
            case 1: return ManoGestureContinuous.OPEN_HAND_GESTURE;
            case 2: return ManoGestureContinuous.CLOSED_HAND_GESTURE;
            case 3: return ManoGestureContinuous.HOLD_GESTURE;
            case 4: return ManoGestureContinuous.POINTER_GESTURE;
            default : return ManoGestureContinuous.NO_GESTURE;
        }
    }

    public static async Task ReadConfig()
    {
#if UNITY_EDITOR
        string configFilePath = Application.dataPath + "/StreamingAssets/config.json";
#else
        string configFilePath = "/storage/emulated/0/org.MikawaLab.VirtualMuseum/config.json";
#endif
        if (File.Exists(configFilePath))
        {
            var reader = new StreamReader(configFilePath);
            var json = await reader.ReadToEndAsync();
            Config = Newtonsoft.Json.JsonConvert.DeserializeObject<ConfigInfo>(json);
            reader.Close();
        }
        else
        {
            Config = DefaultConfig();
            await WriteConfig();
        }
    }

    public static async Task WriteConfig()
    {
#if UNITY_EDITOR
        string configFilePath = Application.dataPath + "/StreamingAssets/config.json";
#else
        string configFilePath = "/storage/emulated/0/org.MikawaLab.VirtualMuseum/config.json";
#endif
        var writer = new StreamWriter(configFilePath);
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(Config);
        await writer.WriteAsync(json);
        writer.Close();
    }
}
