using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListUIContentAnimate : MonoBehaviour
{
    public int index = 0;
    public float delayTime = 0.15f;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        yield return new WaitForSeconds(delayTime * index);
        animator.SetTrigger("Begin");
    }
}
