using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideSceneApplication : MonoBehaviour
{
    public List<GameObject> widgetPrefabs;
    public GameObject canvas;
    public GameObject loading;

    private int widgetIndex = -1;
    public int WidgetIndex 
    { 
        get { return widgetIndex; }
        set
        {
            if (widgetIndex != value)
            {
                if (value >= widgetPrefabs.Count)
                {
                    CancelButtonClicked();
                    return;
                }

                widgetIndex = value;

                if (currentWidget != null)
                {
                    DestroyImmediate(currentWidget);
                    currentWidget = null;
                }

                currentWidget = Instantiate(widgetPrefabs[widgetIndex], canvas.transform);
            }
        }
    }

    private GameObject currentWidget = null;

    private void Start()
    {
        WidgetIndex = 0;
    }

    public void CancelButtonClicked()
    {
        var loader = FindObjectOfType<SceneLoader>();
        loader.loading = loading;
        loader.LoadScene(1);
    }
}
