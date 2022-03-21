using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft;

public class ConfigSceneApplication : MonoBehaviour
{
    private ConfigController configController;
    private bool configChanged;

    private void Start()
    {
        configController = FindObjectOfType<ConfigController>();
    }
}
