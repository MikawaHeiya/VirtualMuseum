using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchButton : MonoBehaviour
{
    public Image background;
    public GameObject ball;

    private Animator animator;

    public bool currentStatus = false;
    public bool Status
    {
        get { return currentStatus; }
        set 
        { 
            if (currentStatus != value)
            {
                currentStatus = value;
                OnStatusChanged();
            }
        }
    }

    public event System.Action<bool> StatusChanged;

    public void OnStatusChanged()
    {
        StatusChanged?.Invoke(Status);
        animator.SetTrigger(currentStatus ? "Enable" : "Disable");
        Debug.Log(currentStatus);
    }

    public void OnClick()
    {
        Status = !Status;
    }

    private void Start()
    {   
        animator = GetComponent<Animator>();
        if (currentStatus)
        {
            animator.SetTrigger("Enable");
        }
    }
}
