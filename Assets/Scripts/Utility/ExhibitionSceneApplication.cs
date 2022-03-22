using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExhibitionSceneApplication : MonoBehaviour
{
    public GameObject canvas;
    public GameObject cameraAnchor;
    public GameObject mainCamera;
    public Vector3 defaultCameraVector;
    public GameObject rotateModeUI;
    public GameObject rotateModeIcon;
    public GameObject scaleModeIcon;
    public GameObject loading;

    private GameObject exhibitor = null;
    private GameObject exhibitorInfo = null;
    private InputUtil inputUtil;
    private SceneLoader sceneLoader;
    private bool rotateMode = true;

    private void Start()
    {
        var assetBundleInfo = FindObjectOfType<ExhibitionSceneParameterPasser>().assetBundleInfo;
        StartCoroutine(LoadFromURL(assetBundleInfo.url, assetBundleInfo.name));

        inputUtil = FindObjectOfType<InputUtil>();
        sceneLoader = FindObjectOfType<SceneLoader>();
        sceneLoader.loading = loading;
    }

    private void Update()
    {   
        if (inputUtil.NextInputEntered)
        {
            rotateMode = !rotateMode;
            rotateModeIcon.SetActive(rotateMode);
            scaleModeIcon.SetActive(!rotateMode);
            rotateModeUI.GetComponent<Animator>().SetTrigger("Enter");
        }

        if (exhibitor != null && exhibitorInfo != null)
        {
            //cameraAnchor.transform.position = exhibitor.transform.position;

            if (inputUtil.RotateInputStayed)
            {
                if (rotateMode)
                {
                    exhibitor.transform.rotation = inputUtil.RotateInput;
                }
                else
                {
                    if (inputUtil.RotateInput.eulerAngles.y > 60)
                    {
                        mainCamera.transform.position *= (1f - 0.1f * Time.deltaTime);
                    }
                    else
                    {
                        mainCamera.transform.position /= (1f - 0.1f * Time.deltaTime);
                    }
                }
            }
        }
    }

    private IEnumerator LoadFromURL(string url, string name)
    {
        var request = UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle(url);
        yield return request.SendWebRequest();

        var bundle = UnityEngine.Networking.DownloadHandlerAssetBundle.GetContent(request);
        var obj = bundle.LoadAsset<GameObject>(name);
        var objInfo = bundle.LoadAsset<GameObject>(name + "_info");
        exhibitor =  Instantiate(obj, new Vector3(0, 0, 0), obj.transform.rotation);
        exhibitorInfo = Instantiate(objInfo, canvas.transform);
    }

    public void OnReturnButtonClicked()
    {
        sceneLoader.LoadScene(1);
    }
}
