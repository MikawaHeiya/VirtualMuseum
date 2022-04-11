using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExhibitionSceneApplication : MonoBehaviour
{
    public GameObject canvas;
    public Vector3 defaultCameraVector;
    public GameObject rotateModeUI;
    public GameObject rotateModeIcon;
    public GameObject scaleModeIcon;
    public GameObject loading;

    private GameObject exhibitor = null;
    private GameObject exhibitorInfo = null;
    private SceneLoader sceneLoader;
    private bool rotateMode = true;
    private float scaleFix = 1f;

    private void Start()
    {
        var assetBundleInfo = FindObjectOfType<ExhibitionSceneParameterPasser>().assetBundleInfo;
        StartCoroutine(LoadFromURL(assetBundleInfo.url, assetBundleInfo.name));

        sceneLoader = FindObjectOfType<SceneLoader>();
        sceneLoader.loading = loading;
    }

    private void Update()
    {   
        if (InputUtil.NextInputEntered)
        {
            rotateMode = !rotateMode;
            rotateModeIcon.SetActive(rotateMode);
            scaleModeIcon.SetActive(!rotateMode);
            rotateModeUI.GetComponent<Animator>().SetTrigger("Enter");
        }

        if (exhibitor != null && exhibitorInfo != null)
        {
            //cameraAnchor.transform.position = exhibitor.transform.position;

            if (InputUtil.RotateInputStayed)
            {
                if (rotateMode)
                {
                    exhibitor.transform.rotation = Quaternion.Euler(new Vector3(InputUtil.RotateInput.y * 360f, InputUtil.RotateInput.x * 360f, 0f));
                }
                else
                {
                    if (InputUtil.RotateInput.x > 0.5f)
                    {
                        exhibitor.transform.position = new Vector3(0f, 0f, 
                            exhibitor.transform.position.z + scaleFix * Time.deltaTime);
                    }
                    else
                    {
                        exhibitor.transform.position = new Vector3(0f, 0f,
                            exhibitor.transform.position.z - scaleFix * Time.deltaTime);
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
        exhibitor =  Instantiate(obj, new Vector3(0, 0, 5), obj.transform.rotation);
        exhibitorInfo = Instantiate(objInfo, canvas.transform);
    }

    public void OnReturnButtonClicked()
    {
        sceneLoader.LoadScene(1);
    }
}
