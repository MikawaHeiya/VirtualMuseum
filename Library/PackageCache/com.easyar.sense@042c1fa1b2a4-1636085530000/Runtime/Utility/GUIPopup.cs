//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace easyar
{
    /// <summary>
    /// <para xml:lang="en">Popup for message notification. The popup action can be globally controlled using <see cref="EasyARController.ShowPopupMessage"/>.</para>
    /// <para xml:lang="zh">消息提示弹窗。是否需要显示弹窗可以通过<see cref="EasyARController.ShowPopupMessage"/>来进行全局控制。</para>
    /// </summary>
    public class GUIPopup : MonoBehaviour
    {
        private static GUIPopup popup;
        private readonly Queue<MessageData> messageQueue = new Queue<MessageData>();
        private bool isShowing;
        private bool isDisappearing;
        private GUIStyle boxStyle;
        private Texture2D texture;
#if EASYAR_ENABLE_NREAL
        private GameObject textObject;
        private GameObject cubeObject;
        private NRKernal.NRHMDPoseTracker cameraRig;
#endif

        private void Start()
        {
#if EASYAR_ENABLE_NREAL
            cameraRig = FindObjectOfType<NRKernal.NRHMDPoseTracker>();
#endif
            texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, new Color(0, 0, 0, 0.6f));
            texture.Apply();
            boxStyle = new GUIStyle
            {
                wordWrap = true,
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter
            };
            boxStyle.normal.textColor = Color.white;
            boxStyle.normal.background = texture;
            StartCoroutine(ShowMessage());
        }

        private void OnDestroy()
        {
            if (texture)
            {
                Destroy(texture);
            }
#if EASYAR_ENABLE_NREAL
            if (textObject)
            {
                Destroy(textObject);
            }
            if (cubeObject)
            {
                Destroy(cubeObject);
            }
#endif
        }

        /// <summary>
        /// <para xml:lang="en">Add one message and its duration for display.</para>
        /// <para xml:lang="zh">添加一条要显示的消息及时长。</para>
        /// </summary>
        public static void EnqueueMessage(string message, float seconds, bool isFatal = false)
        {
            if (EasyARController.Instance && !EasyARController.Instance.ShowPopupMessage)
            {
                if (isFatal)
                {
                    Debug.LogError(message);
                }
                else
                {
                    Debug.Log(message);
                }
                return;
            }
            if (isFatal)
            {
                Debug.LogError(message);
            }

            if (popup == null)
            {
                var go = new GameObject("MessagePopup");
                popup = go.AddComponent<GUIPopup>();
            }
            popup.messageQueue.Enqueue(new MessageData
            {
                Message = message,
                Time = seconds,
                IsFatal = isFatal
            });
        }

        private IEnumerator ShowMessage()
        {
            while (true)
            {
                if (EasyARController.Instance && !EasyARController.Instance.ShowPopupMessage)
                {
                    while (messageQueue.Count > 0)
                    {
                        var message = messageQueue.Dequeue();
                        Debug.Log(message);
                    }
                }

                if (messageQueue.Count > 0)
                {
                    var color = boxStyle.normal.textColor;
                    color.a = 0;
                    boxStyle.normal.textColor = color;
                    isShowing = true;
                    isDisappearing = false;

                    ShowMessage(messageQueue.Peek());
                    var time = messageQueue.Peek().Time;
                    yield return new WaitForSeconds(time > 1 ? time - 0.5f : time / 2);
                    isDisappearing = true;
                    yield return new WaitForSeconds(time > 1 ? 0.5f : time / 2);

                    ShowMessage(Optional<MessageData>.Empty);
                    messageQueue.Dequeue();
                    isShowing = false;
                }
                else
                {
                    yield return 0;
                }
            }
        }

        private void OnGUI()
        {
            if (!isShowing)
            {
                return;
            }

            var color = boxStyle.normal.textColor;
            color.a += isDisappearing ? -Time.deltaTime * 2 : Time.deltaTime * 2;
            color.a = color.a > 1 ? 1 : (color.a < 0 ? 0 : color.a);
            boxStyle.normal.textColor = color;
            GUI.Box(new Rect(0, Screen.height / 2, Screen.width, Math.Min(Screen.height / 4, 160)), messageQueue.Peek().Message, boxStyle);
        }

        private void ShowMessage(Optional<MessageData> message)
        {
#if EASYAR_ENABLE_NREAL
            if (!cameraRig) { return; }

            if (message.OnNone)
            {
                if (textObject)
                {
                    textObject.SetActive(false);
                }
                if (cubeObject)
                {
                    cubeObject.SetActive(false);
                }
                return;
            }

            if (!textObject)
            {
                textObject = new GameObject("MessagePopupWorld");
                textObject.transform.localPosition = new Vector3(0, -0.8f, 5);
                textObject.transform.localScale = new Vector3(0.075f, 0.075f, 1);
                var mesh = textObject.AddComponent<MeshRenderer>();
                var defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                mesh.material = defaultFont.material;
                var textMesh = textObject.AddComponent<TextMesh>();
                mesh.material.color = textMesh.color;
                textMesh.anchor = TextAnchor.MiddleCenter;
            }
            if (!cubeObject)
            {
                cubeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
#if EASYAR_URP_ENABLE
                if (GraphicsSettings.currentRenderPipeline is UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset)
                {
                    cubeObject.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                }
#endif
                cubeObject.GetComponent<MeshRenderer>().material.color = new Color(0.5f, 0, 0, 0.5f);
                cubeObject.transform.localPosition = new Vector3(0, -0.8f, 5.1f);
                cubeObject.transform.localScale = new Vector3(5, 5, 0.1f);
            }
            textObject.transform.SetParent(cameraRig.transform, false);
            textObject.transform.SetParent(null, true);
            var text = textObject.GetComponent<TextMesh>();
            text.text = message.Value.Message;
            textObject.SetActive(true);
            if (message.Value.IsFatal)
            {
                cubeObject.transform.SetParent(cameraRig.transform, false);
                cubeObject.transform.SetParent(null, true);
                cubeObject.SetActive(true);
            }
#endif
        }

        private struct MessageData
        {
            public string Message;
            public float Time;
            public bool IsFatal;
        }
    }

    /// <summary>
    /// <para xml:lang="en">Exception that need popup for notification.</para>
    /// <para xml:lang="zh">需要通过弹窗提示的异常。</para>
    /// </summary>
    public class UIPopupException : Exception
    {
        public UIPopupException(string message, float seconds) : base(message)
        {
            GUIPopup.EnqueueMessage(message, seconds, true);
        }

        public UIPopupException(string message) : this(message, 10)
        {
        }
    }
}
