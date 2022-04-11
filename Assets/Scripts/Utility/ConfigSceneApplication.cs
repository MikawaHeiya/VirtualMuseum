using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft;

public class ConfigSceneApplication : MonoBehaviour
{
    public SwitchButton showDebugConsoleSwitchButton;
    public Dropdown qrCodeCameraFlushFrequencyDropdown;
    public Dropdown positiveGestureType;
    public Dropdown rotateScaleGestureType;

    public GameObject loading;

    private ConfigInfo tempConfig;

    private void Start()
    {
        tempConfig = ConfigController.Config;
        showDebugConsoleSwitchButton.currentStatus = tempConfig.ShowDebugConsole;
        qrCodeCameraFlushFrequencyDropdown.value = tempConfig.QrCodeCameraFlushFrequency - 1;
        positiveGestureType.value = tempConfig.PositiveGestureType;
        rotateScaleGestureType.value = tempConfig.RotateScaleGestureType;

        showDebugConsoleSwitchButton.StatusChanged += ConfigHandler_ShowDebugConsole;
        qrCodeCameraFlushFrequencyDropdown.onValueChanged.AddListener(
            (val) => { ConfigHandler_QrCodeCameraFlushFrequency(val); });
        positiveGestureType.onValueChanged.AddListener((val) => { ConfigHandler_PositiveGestureType(val); });
        rotateScaleGestureType.onValueChanged.AddListener((val) => { ConfigHandler_RotateScaleGestureType(val); });
    }

    public async void OnReturnButtonClicked()
    {
        if (!ConfigController.ConfigInfoEquals(ConfigController.Config, tempConfig))
        {
            loading.SetActive(true);
            ConfigController.Config = tempConfig;
            await ConfigController.WriteConfig();
        }

        var loader = FindObjectOfType<SceneLoader>();
        loader.loading = loading;
        loader.LoadScene(1);
    }

    public void ConfigHandler_ShowDebugConsole(bool status)
    {
        tempConfig.ShowDebugConsole = status;
    }

    public void ConfigHandler_QrCodeCameraFlushFrequency(int value)
    {
        tempConfig.QrCodeCameraFlushFrequency = value + 1;
    }

    public void ConfigHandler_PositiveGestureType(int value)
    {
        tempConfig.PositiveGestureType = value;
    }

    public void ConfigHandler_RotateScaleGestureType(int value)
    {
        tempConfig.RotateScaleGestureType = value;
    }
}
