//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using System;
using UnityEngine;

namespace easyar
{
    [Obsolete("This Prefab is obsolete and will be removed in the future. Please re-create your AR Session from 'GameObject -> EasyAR Sense' menu.")]
    [ExecuteInEditMode]
    public class ObsoletePrefabNotifier : MonoBehaviour
    {
        public MessageType Type;
        public string Message;

        const string obsolete = "This Prefab is obsolete and will be removed in the future.";

        public enum MessageType
        {
            MenuPath,
            Full,
        }

        private void OnEnable()
        {
            switch (Type)
            {
                case MessageType.Full:
                    Debug.LogWarning($"{obsolete} {Message}");
                    break;
                case MessageType.MenuPath:
                    Debug.LogWarning($"{obsolete} Please use menu 'GameObject -> EasyAR Sense -> {Message}' for replacement.");
                    break;
                default:
                    break;
            }
        }
    }
}
