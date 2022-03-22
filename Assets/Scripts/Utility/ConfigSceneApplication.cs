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

    public GameObject loading;

    private ConfigController configController;
    private ConfigInfo tempConfig;

    private void Start()
    {
        configController = FindObjectOfType<ConfigController>();
        tempConfig = configController.Config;
        //showDebugConsoleSwitchButton.Status = tempConfig.ShowDebugConsole;
        qrCodeCameraFlushFrequencyDropdown.value = tempConfig.QrCodeCameraFlushFrequency - 1;
        positiveGestureType.value = tempConfig.PositiveGestureType;

        //showDebugConsoleSwitchButton.StatusChanged += ConfigHandler_ShowDebugConsole;
        qrCodeCameraFlushFrequencyDropdown.onValueChanged.AddListener(
            (val) => { ConfigHandler_QrCodeCameraFlushFrequency(val); });
        positiveGestureType.onValueChanged.AddListener((val) => { ConfigHandler_PositiveGestureType(val); });
    }

    public async void OnReturnButtonClicked()
    {
        if (!ConfigController.ConfigInfoEquals(configController.Config, tempConfig))
        {
            loading.SetActive(true);
            configController.Config = tempConfig;
            await configController.WriteConfig();
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
}
