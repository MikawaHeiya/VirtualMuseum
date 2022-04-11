using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using Newtonsoft.Json;

public struct AssetBundleInfo
{
    public string url;
    public string name;
}

public class QrCodeSceneApplication : MonoBehaviour
{
    public Camera mainCamera;
    public UnityEngine.UI.RawImage rawImage;
    public UnityEngine.UI.Text text;
    public GameObject requestDialogPrefab;
    public GameObject debugConsolePrefab;
    public Canvas canvas;
    public GameObject instantiateButton;
    public GameObject loading;
    public ExhibitionSceneParameterPasser parameterPasser;

    private WebCamTexture webCamTexture;
    private BarcodeReader reader = new BarcodeReader();

    private SceneLoader sceneLoader;
    private GameObject ar;

    private AssetBundleInfo assetBundleInfo;

    public void OnInstantieteButtonClicked()
    {
        mainCamera.gameObject.SetActive(false);
        ar.SetActive(true);
        parameterPasser.assetBundleInfo = assetBundleInfo;
        sceneLoader.LoadScene(3);
    }

    private void Start()
    {
        ar = FindObjectOfType<DontDestroyOnLoad>().gameObject;
        ar.SetActive(false);
        mainCamera.gameObject.SetActive(true);

        loading.SetActive(false);

        StartCoroutine(InitializeCamera());

        sceneLoader = FindObjectOfType<SceneLoader>();
        sceneLoader.loading = loading;
    }

    private IEnumerator InitializeCamera()
    {
        yield return new WaitForEndOfFrame();

        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        }

        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            var devices = WebCamTexture.devices;
            foreach (var device in devices)
            {
                if (!device.isFrontFacing)
                {
                    webCamTexture = new WebCamTexture(device.name, 800, 800, 60);
                    Debug.Log(device.name);
                    break;
                }
            }
            webCamTexture.Play();
            rawImage.texture = webCamTexture;
            StartCoroutine(Scan());
        }
        else
        {
            var dialog = Instantiate(requestDialogPrefab, canvas.transform);
            dialog.GetComponent<RequestDialog>().Content = "未能获取到摄像机权限。";
        }
    }

    private IEnumerator Scan()
    {
        yield return new WaitForSeconds(1f / ConfigController.Config.QrCodeCameraFlushFrequency);
        yield return new WaitForEndOfFrame();
        
        var result = reader.Decode(webCamTexture.GetPixels32(), webCamTexture.width, webCamTexture.height);

        if (result == null)
        {
            instantiateButton.SetActive(false);
            StartCoroutine(Scan());
            text.text = "请将二维码置于上方方框内";
        }
        else if (string.IsNullOrEmpty(result.Text))
        {
            instantiateButton.SetActive(false);
            StartCoroutine(Scan());
            text.text = "请将二维码置于上方方框内";
        }
        else
        {
            instantiateButton.SetActive(true);
            assetBundleInfo = JsonConvert.DeserializeObject<AssetBundleInfo>(result.Text);
            if (!string.IsNullOrEmpty(assetBundleInfo.url) && !string.IsNullOrEmpty(assetBundleInfo.name))
            {
                text.text = $"检测到了{assetBundleInfo.name}";
            }
        }
    }

    public void OnBackButtonClicked()
    {
        mainCamera.gameObject.SetActive(false);
        ar.SetActive(true);
        sceneLoader.LoadScene(1);
    }
}
