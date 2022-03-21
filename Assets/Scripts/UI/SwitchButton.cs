using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchButton : MonoBehaviour
{
    public Image background;
    public GameObject ball;

    private Animator animator;

    private bool currentStatus = false;
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

    public event System.Action StatusChanged;

    public void OnStatusChanged()
    {
        StatusChanged?.Invoke();
        animator.SetTrigger(Status ? "Enable" : "Disable");
    }

    public void OnClick()
    {
        Status = !Status;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
}
