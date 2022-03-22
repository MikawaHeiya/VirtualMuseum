using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoDialog : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OnPositiveButtonClicked()
    {
        animator.SetTrigger("Exit");
    }
}
