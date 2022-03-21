#if !VIVEHANDTRACKING_UNITYXR && UNITY_ANDROID

using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;

namespace ViveHandTracking {

[InitializeOnLoad]
class AndroidPlatformCheck {
  private const string GoogleVRDefine = "VIVEHANDTRACKING_WITH_GOOGLEVR";
  private const string ARCoreDefine = "VIVEHANDTRACKING_WITH_ARCORE";
  private const string WaveVRDefine = "VIVEHANDTRACKING_WITH_WAVEVR";
  private const string WaveVR3Define = "VIVEHANDTRACKING_WITH_WAVEVR3";
  private const string WaveVRHandDefine = "VIVEHANDTRACKING_WAVEVR_HAND";
  private static string IgnoreFilePath;

  static AndroidPlatformCheck() { EditorApplication.update += Check; }

  static void Check() {
    IgnoreFilePath = Application.dataPath + "/../ViveHandTrackingSkipPlatformCheck.txt";
    EditorApplication.update -= Check;
    if (File.Exists(IgnoreFilePath)) return;

    // check if GoogleVR and WaveVR plugin exist
    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
    var types = assemblies.SelectMany(a => a.GetTypes()).ToList();
    bool hasARCorePlugin = types.Any(t => t.FullName == "GoogleARCore.ARCoreSession");
    bool hasGooglevrPlugin = types.Any(t => t.FullName == "GvrSettings");
    bool hasWavevrPlugin = types.Any(t => t.FullName == "WaveVR_Render");
    bool isWaveVR3OrNewer = hasWavevrPlugin && types.Any(t => t.FullName == "WaveVR_ButtonList");
    bool hasWavevrHand =
        hasWavevrPlugin && types.Any(t => t.FullName == "wvr.WVR_HandSkeletonData_t");

    if (hasGooglevrPlugin && hasWavevrPlugin) {
      WarnMultiplePlugin("GoogleVR", GoogleVRDefine, "WaveVR", WaveVRDefine);
      return;
    }
    if (hasGooglevrPlugin && hasARCorePlugin) {
      WarnMultiplePlugin("GoogleVR", GoogleVRDefine, "ARCore", ARCoreDefine);
      return;
    }
    if (hasARCorePlugin && hasWavevrPlugin) {
      WarnMultiplePlugin("ARCore", ARCoreDefine, "WaveVR", WaveVRDefine);
      return;
    }

    // update symbols
    string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
    string newSymbols = "";
    foreach (var define in symbols.Split(';')) {
      if (define == GoogleVRDefine) {
        if (!hasGooglevrPlugin) continue;
        hasGooglevrPlugin = false;
      } else if (define == ARCoreDefine) {
        if (!hasARCorePlugin) continue;
        hasARCorePlugin = false;
      } else if (define == WaveVRDefine) {
        if (!hasWavevrPlugin) continue;
        hasWavevrPlugin = false;
      } else if (define == WaveVR3Define) {
        if (!isWaveVR3OrNewer) continue;
        isWaveVR3OrNewer = false;
      } else if (define == WaveVRHandDefine) {
        if (!hasWavevrHand) continue;
        hasWavevrHand = false;
      }
      AppendDefine(ref newSymbols, define, false);
    }
    if (hasGooglevrPlugin) AppendDefine(ref newSymbols, GoogleVRDefine);
    if (hasARCorePlugin) AppendDefine(ref newSymbols, ARCoreDefine);
    if (hasWavevrPlugin) AppendDefine(ref newSymbols, WaveVRDefine);
    if (isWaveVR3OrNewer) AppendDefine(ref newSymbols, WaveVR3Define);
    if (hasWavevrHand) AppendDefine(ref newSymbols, WaveVRHandDefine);

    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, newSymbols);
  }

  static void AppendDefine(ref string defines, string element, bool print = true) {
    if (defines != "") defines += ";";
    defines += element;
    if (print) Debug.LogFormat("Add scripting define symbol {0} for Android platform", element);
  }

  static void WarnMultiplePlugin(string Plugin1Name, string Plugin1Define, string Plugin2Name,
                                 string Plugin2Define) {
    bool showDialog = EditorPrefs.GetBool("ViveHandTracking.AndroidPlatformCheck.ShowDialog", true);
    if (showDialog) {
      bool result = EditorUtility.DisplayDialog(
          "Your Project continas both " + Plugin1Name + " and " + Plugin2Name + " plugin",
          "Both plugins cannot work together and Vive Hand Tracking plugin cannot determine which API to use," +
              "Please add " + Plugin1Define + " or " + Plugin2Define +
              " to android scripting define symbols manually.",
          "Got it", "Skip Checks");
      if (!result) File.WriteAllText(IgnoreFilePath, "");
    } else
      Debug.LogWarningFormat(
          "Vive Hand Tracking detected both {0} and {1} plugin, please add {0} or {1} to android scripting define symbols manually.",
          Plugin1Name, Plugin2Name, Plugin1Define, Plugin2Define);
  }
}

}

#elif VIVEHANDTRACKING_WAVEXR4 && UNITY_ANDROID

using UnityEditor;
using UnityEngine;

namespace ViveHandTracking {

[InitializeOnLoad]
class AndroidPlatformCheck {
  static string key = "ViveHandTracking.AndroidPlatformCheck.ShowDialogWave4";
  static string message =
      "WaveXR 4.0 is detected, this only works for Vive Focus 3. " +
      "If you are targeting Vive Focus/Focus Plus, you must use WaveXR 1.x versions instead.";

  static AndroidPlatformCheck() { EditorApplication.update += Check; }

  static void Check() {
    EditorApplication.update -= Check;

    bool showDialog = EditorPrefs.GetBool(key, true);
    if (showDialog) {
      bool result =
          EditorUtility.DisplayDialog("WaveXR 4.0 not compatible with Vive Focus/Focus Plus!",
                                      message, "Got it", "Don't show this dialog again");
      if (!result) EditorPrefs.SetBool(key, false);
    } else
      Debug.LogWarning(message);
  }
}
}

#endif
