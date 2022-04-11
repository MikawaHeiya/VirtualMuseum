using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ManoMotion;

public class InputUtil : MonoBehaviour
{
    private ManoGestureTrigger positiveGestureType;
    private ManoGestureContinuous rotateScaleGestureType;

    public static bool NextInputEntered { get; private set; }
    public static bool ClickInputEntened { get; private set; }
    public static bool RotateInputStayed { get; private set; }
    public static Vector3 RotateInput { get; private set; }

    private void Update()
    {
#if UNITY_EDITOR
        NextInputEntered = Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
        ClickInputEntened = Input.GetKeyDown(KeyCode.Space);
        RotateInputStayed = false;
        RotateInput = Vector3.zero;
#else
        var hand = ManomotionManager.Instance.Hand_infos[0].hand_info;
        NextInputEntered = hand.gesture_info.mano_gesture_trigger == ManoGestureTrigger.CLICK;
        ClickInputEntened = hand.gesture_info.mano_gesture_trigger == positiveGestureType;
        RotateInputStayed = hand.gesture_info.mano_gesture_continuous == rotateScaleGestureType;
        RotateInput = hand.tracking_info.palm_center;
#endif
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        positiveGestureType = ConfigController.IndexToGestureTrigger(ConfigController.Config.PositiveGestureType);
        rotateScaleGestureType = ConfigController.IndexToGestureContinuous(ConfigController.Config.RotateScaleGestureType);
    }
}
