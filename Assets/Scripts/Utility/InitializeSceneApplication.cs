using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitializeSceneApplication : MonoBehaviour
{
    private SceneLoader sceneLoader;
    private ConfigController configController;

    private void Start()
    {
        sceneLoader = FindObjectOfType<SceneLoader>();
        configController = FindObjectOfType<ConfigController>();

        Initialize();
    }

    private async void Initialize()
    {
        await configController.ReadConfig();
        sceneLoader.LoadScene(0);
    }
}
