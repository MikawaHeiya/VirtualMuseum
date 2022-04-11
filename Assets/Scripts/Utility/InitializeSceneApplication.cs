using System.Collections;
using System.Collections.Generic;
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
    {
#if UNITY_EDITOR
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
#endif
        await ConfigController.ReadConfig();
        if (ConfigController.Config.ShowDebugConsole)
        {
            Instantiate(debugConsolePrefab);
        }
        sceneLoader.LoadScene(1);
    }
}
