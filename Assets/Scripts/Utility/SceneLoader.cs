using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public GameObject loading;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(LoadSceneImp(sceneIndex));
    }

    private IEnumerator LoadSceneImp(int sceneIndex)
    {
        var loadScene = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneIndex);
        loading.SetActive(true);
        yield return loadScene;
        //loading.SetActive(false);
    }
}
