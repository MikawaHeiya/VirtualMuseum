using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.IO;
using UnityEngine;
using UnityEngine.Android;

public class InitializeSceneApplication : MonoBehaviour
{
    public GameObject debugConsolePrefab;

    private SceneLoader sceneLoader;

    private void Start()
    {
        sceneLoader = FindObjectOfType<SceneLoader>();

        Initialize();
    }

    private async void Initialize()
    {/*
#if !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
#endif*/
#if UNITY_EDITOR
        string configFilePath = Application.dataPath + "/StreamingAssets/config.json";
#else
        string configFilePath = Application.persistentDataPath + "/config.json";
#endif
        int nextSceneID = 1;
        if (!File.Exists(configFilePath))
        {
            nextSceneID = 5;
        }

        await ConfigController.ReadConfig();

        if (ConfigController.Config.ShowDebugConsole)
        {
            Instantiate(debugConsolePrefab);
        }

        if (!string.IsNullOrEmpty(ConfigController.Config.Email) &&
            !string.IsNullOrEmpty(ConfigController.Config.Passport))
        {
            var http = new HttpClient();
            var response = await http.GetAsync(
                $"http://101.42.253.148:8080/VirtualMuseum/LoginWithPassport?mail={ConfigController.Config.Email}&passport={ConfigController.Config.Passport}");
            var result = await response.Content.ReadAsStringAsync();

            if (result == "false")
            {
                var config = ConfigController.Config;
                config.Email = string.Empty;
                config.Passport = string.Empty;
                ConfigController.Config = config;
                await ConfigController.WriteConfig();
            }
        }

        sceneLoader.LoadScene(nextSceneID);
    }
}
