//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace easyar
{
    internal class APIDocGenerator : Editor
    {
        // Unity does not generate XML documentation file for DLLs.
        // Even worse, it removes the file after compilation if -doc is used in compiler options.
        // So, let's hack!
        [DidReloadScripts(1)]
        public static void GenerateAPIDoc()
        {
            var gen = true;
            if (EasyARController.Settings)
            {
                gen = EasyARController.Settings.GenerateXMLDoc;
            }
            if (!gen) { return; }
            var src = Path.GetFullPath("Packages/com.easyar.sense/Documentation~/EasyAR.Sense.xml");
            if (!File.Exists(src)) { return; }
            var dstFolder = Path.GetDirectoryName(Application.dataPath) + "/Library/ScriptAssemblies";
            if (!File.Exists(dstFolder + "/EasyAR.Sense.dll")) { return; }
            File.Copy(src, dstFolder + "/EasyAR.Sense.xml", true);
        }
    }
}
