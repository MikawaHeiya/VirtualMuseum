using System.IO;
using UnityEditor;

public class BuildAssetBundle
{
    [MenuItem("Assets/Build AssetsBundles")]
    static void BuildAssetBundles()
    {
        var path = "Assets/StreamingAssets";

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
    }
}
