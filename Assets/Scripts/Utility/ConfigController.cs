using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public struct ConfigInfo
{
    public bool ShowDebugConsole;
    public int QrCodeCameraFlushFrequency;
    public int PositiveGestureType;
}

public class ConfigController : MonoBehaviour
{
    public ConfigInfo Config { get; set; } = new ConfigInfo();

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private ConfigInfo DefaultConfig()
    {
        var config = new ConfigInfo();
        config.ShowDebugConsole = false;
        config.QrCodeCameraFlushFrequency = 5;
        config.PositiveGestureType = 0;

        return config;
    }

    public static bool ConfigInfoEquals(ConfigInfo l, ConfigInfo r)
    {
        return 
            l.ShowDebugConsole == r.ShowDebugConsole &&
            l.QrCodeCameraFlushFrequency == r.QrCodeCameraFlushFrequency &&
            l.PositiveGestureType == r.PositiveGestureType;
    }

    public static int GestureTypeToIndex(ViveHandTracking.GestureType gestureType)
    {
        switch (gestureType)
        {
            case ViveHandTracking.GestureType.Fist:
                return 0;
            case ViveHandTracking.GestureType.Five:
                return 1;
            case ViveHandTracking.GestureType.Like:
                return 2;
            case ViveHandTracking.GestureType.OK:
                return 3;
            case ViveHandTracking.GestureType.Point:
                return 4;
            case ViveHandTracking.GestureType.Unknown:
                return 5;
            case ViveHandTracking.GestureType.Victory:
                return 6;
            default:
                return -1;
        }
    }

    public static ViveHandTracking.GestureType IndexToGestureType(int index)
    {
        switch (index)
        {
            case 0: return ViveHandTracking.GestureType.Fist;
            case 1: return ViveHandTracking.GestureType.Five;
            case 2: return ViveHandTracking.GestureType.Like;
            case 3: return ViveHandTracking.GestureType.OK;
            case 4: return ViveHandTracking.GestureType.Point;
            case 5: return ViveHandTracking.GestureType.Unknown;
            case 6: return ViveHandTracking.GestureType.Victory;
            default: return ViveHandTracking.GestureType.Unknown;
        }
    }

    public async Task ReadConfig()
    {
#if UNITY_EDITOR
        string configFilePath = Application.dataPath + "/StreamingAssets/config.json";
#else
        string configFilePath = Application.persistentDataPath + "/config.json";
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

    public async Task WriteConfig()
    {
#if UNITY_EDITOR
        string configFilePath = Application.dataPath + "/StreamingAssets/config.json";
#else
        string configFilePath = Application.persistentDataPath + "/config.json";
#endif
        var writer = new StreamWriter(configFilePath);
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(Config);
        await writer.WriteAsync(json);
        writer.Close();
    }
}
