//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using System;
using System.Collections;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Networking;

namespace easyar
{
    /// <summary>
    /// <para xml:lang="en">Path type.</para>
    /// <para xml:lang="zh">路径类型。</para>
    /// </summary>
    public enum PathType
    {
        /// <summary>
        /// <para xml:lang="en">Absolute path.</para>
        /// <para xml:lang="zh">绝对路径。</para>
        /// </summary>
        Absolute,
        /// <summary>
        /// <para xml:lang="en">Unity StreamingAssets path.</para>
        /// <para xml:lang="zh">UnityStreamingAssets路径。</para>
        /// </summary>
        StreamingAssets,
        /// <summary>
        /// <para xml:lang="en">Not file storage.</para>
        /// <para xml:lang="zh">不是文件存储。</para>
        /// </summary>
        None,
    }

    /// <summary>
    /// <para xml:lang="en">Output file path type.</para>
    /// <para xml:lang="zh">文件输出路径类型。</para>
    /// </summary>
    public enum WritablePathType
    {
        /// <summary>
        /// <para xml:lang="en">Absolute path.</para>
        /// <para xml:lang="zh">绝对路径。</para>
        /// </summary>
        Absolute,
        /// <summary>
        /// <para xml:lang="en">Unity persistent data path.</para>
        /// <para xml:lang="zh">Unity沙盒路径。</para>
        /// </summary>
        PersistentDataPath,
    }

    /// <summary>
    /// <para xml:lang="en">File utility.</para>
    /// <para xml:lang="zh">文件工具。</para>
    /// </summary>
    public static class FileUtil
    {
        private static bool streamingAssetsImported;
        /// <summary>
        /// <para xml:lang="en">Async Load file and return <see cref="Buffer"/> object in the callback.</para>
        /// <para xml:lang="zh">异步加载文件，回调返回<see cref="Buffer"/>对象。</para>
        /// </summary>
        public static IEnumerator LoadFile(string filePath, PathType filePathType, Action<Buffer> onLoad)
        {
            return LoadFile(filePath, filePathType, (data) =>
            {
                if (onLoad == null)
                {
                    return;
                }
                using (var buffer = Buffer.wrapByteArray(data))
                {
                    onLoad(buffer);
                }
            });
        }

        /// <summary>
        /// <para xml:lang="en">Async Load file and return byte array in the callback.</para>
        /// <para xml:lang="zh">异步加载文件，回调返回字节数组。</para>
        /// </summary>
        public static IEnumerator LoadFile(string filePath, PathType filePathType, Action<byte[]> onLoad, Action<string> onError = null)
        {
            if (onLoad == null)
            {
                yield break;
            }
            var path = filePath;
            if (filePathType == PathType.StreamingAssets)
            {
                path = Application.streamingAssetsPath + "/" + path;
            }
            byte[] data;
            using (var handle = new DownloadHandlerBuffer())
            {
                var webRequest = new UnityWebRequest(PathToUrl(path), "GET", handle, null);
                webRequest.SendWebRequest();
                while (!handle.isDone)
                {
                    yield return 0;
                }
                if (!string.IsNullOrEmpty(webRequest.error))
                {
                    if (onError != null)
                    {
                        onError(webRequest.error);
                    }
                    else
                    {
                        Debug.LogError(webRequest.error);
                    }
                    yield break;
                }
                data = handle.data;
            }
            onLoad(data);
        }

        /// <summary>
        /// <para xml:lang="en">Convert file path to URL.</para>
        /// <para xml:lang="zh">将路径转换成URL。</para>
        /// </summary>
        public static string PathToUrl(string path)
        {
            if (string.IsNullOrEmpty(path) || path.StartsWith("jar:file://") || path.StartsWith("file://") || path.StartsWith("http://") || path.StartsWith("https://"))
            {
                return path;
            }
            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.OSXPlayer ||
                Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.Android)
            {
                path = "file://" + path;
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                path = "file:///" + path;
            }
            return path;
        }

        /// <summary>
        /// <para xml:lang="en">Import sample streaming assets into Unity project.</para>
        /// <para xml:lang="zh">导入sample streaming assets到工程中。</para>
        /// </summary>
        public static void ImportSampleStreamingAssets()
        {
#if UNITY_EDITOR
            if (streamingAssetsImported)
            {
                return;
            }
            streamingAssetsImported = true;
            var pacakge = "Packages/com.easyar.sense/Samples~/StreamingAssets/assets.unitypackage";
            if (!File.Exists($"{Application.streamingAssetsPath}/com.easyar.sense-{EasyARVersion.FullVersion}") && File.Exists(Path.GetFullPath(pacakge)))
            {
                AssetDatabase.ImportPackage(pacakge, false);
            }
#endif
        }
    }
}
