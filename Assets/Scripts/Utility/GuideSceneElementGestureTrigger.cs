using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ManoMotion;

public enum GuideSceneElementTriggerType
{
    Click = ManoGestureTrigger.CLICK, Grab = ManoGestureTrigger.GRAB_GESTURE
}

public class GuideSceneElementGestureTrigger : MonoBehaviour
{
    public GuideSceneElementTriggerType triggerType = GuideSceneElementTriggerType.Click;

    private ManoGestureTrigger lastTrigger = ManoGestureTrigger.NO_GESTURE;
    private ManoGestureTrigger currentTrigger = ManoGestureTrigger.NO_GESTURE;
    private GuideSceneElement sceneElement;

    private bool TriggerEntered
    {
        get { return lastTrigger != (ManoGestureTrigger)triggerType && 
                currentTrigger == (ManoGestureTrigger)triggerType; }
    }

    private void Start()
    {
        sceneElement = GetComponent<GuideSceneElement>();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space))
        {
            sceneElement.NextWidget();
        }
#else
        var hand = ManomotionManager.Instance.Hand_infos[0].hand_info;
        currentTrigger = hand.gesture_info.mano_gesture_trigger;

        if (TriggerEntered)
        {
            sceneElement.NextWidget();
        }

        lastTrigger = currentTrigger;
        currentTrigger = ManoGestureTrigger.NO_GESTURE;
#endif
    }
}
