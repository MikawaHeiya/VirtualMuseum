using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveHandTracking;

public class InputUtil : MonoBehaviour
{
    private GestureType positiveGestureType;

#if !UNITY_EDITOR
    private bool lastPinch = false;
    private bool currentPinch = false;
    private GestureType lastGestureType = GestureType.Unknown;
    private GestureType currentGestureType = GestureType.Unknown;
#endif

    public bool NextInputEntered { get; private set; }
    public bool ClickInputEntened { get; private set; }
    public bool RotateInputStayed { get; private set; }
    public Quaternion RotateInput { get; private set; }

    private ConfigController configController;

    private void Update()
    {
#if UNITY_EDITOR
        NextInputEntered = Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
        ClickInputEntened = Input.GetKeyDown(KeyCode.Space);
        RotateInputStayed = false;
        RotateInput = Quaternion.Euler(0, 0, 0);
#else
        var hand = GestureProvider.LeftHand;
        if (hand == null)
        {
            currentPinch = false;
            currentGestureType = GestureType.Unknown;
            RotateInput = Quaternion.identity;
        }
        else
        {
            currentPinch = hand.pinch.isPinching;
            currentGestureType = hand.gesture;
            RotateInput = hand.rotation;
        }

        NextInputEntered = !lastPinch && currentPinch;
        ClickInputEntened = lastGestureType != positiveGestureType && currentGestureType == positiveGestureType;
        RotateInputStayed = lastGestureType == positiveGestureType && currentGestureType == positiveGestureType;

        lastPinch = currentPinch;
        lastGestureType = currentGestureType;
#endif
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        configController = FindObjectOfType<ConfigController>();
        positiveGestureType = ConfigController.IndexToGestureType(configController.Config.PositiveGestureType);
    }
}
