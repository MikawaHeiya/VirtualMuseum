using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAnimation : MonoBehaviour
{
    public float originTransparency = 0f;
    public float targetTransparency = 1f;
    public float transparencyStep = 0.1f;

    private List<UnityEngine.UI.Graphic> graphics = new List<UnityEngine.UI.Graphic>();
    private float transparency;

    private IEnumerator Enter()
    {
        int index = (int)((targetTransparency - originTransparency) / transparencyStep);
        while (index-- >= 0)
        {
            transparency += transparencyStep;
            foreach (var g in graphics)
            {
                g.color = new Color(g.color.r, g.color.g, g.color.b, transparency);
            }

            yield return new WaitForFixedUpdate();
        }

        yield break;
    }

    private IEnumerator Exit(System.Action callback)
    {
        int index = (int)((targetTransparency - originTransparency) / transparencyStep);
        while (index-- >= 0)
        {
            transparency -= transparencyStep;
            foreach (var g in graphics)
            {
                g.color = new Color(g.color.r, g.color.g, g.color.b, transparency);
            }

            yield return new WaitForFixedUpdate();
        }

        callback?.Invoke();
        yield break;
    }

    private void Start()
    {
        graphics.AddRange(GetComponents<UnityEngine.UI.Graphic>());
        graphics.AddRange(GetComponentsInChildren<UnityEngine.UI.Graphic>());

        transparency = originTransparency;

        StartCoroutine(Enter());
    }

    public void PlayExitAnimation(System.Action callback)
    {
        StartCoroutine(Exit(callback));
    }
}
