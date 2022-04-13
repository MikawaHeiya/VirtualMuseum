using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
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

        sceneLoader.LoadScene(1);
    }
}
