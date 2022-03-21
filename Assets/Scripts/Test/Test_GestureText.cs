using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_GestureText : MonoBehaviour
{
    public UnityEngine.UI.Text text;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var hand = ViveHandTracking.GestureProvider.LeftHand;
        if (hand != null)
        {
            text.text = "not null";
        }
        else
        {
            text.text = "null";
        }
    }
}
