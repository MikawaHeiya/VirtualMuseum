using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExhibitionSceneParameterPasser : MonoBehaviour
{
    public AssetBundleInfo assetBundleInfo;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
