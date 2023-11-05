using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFadeOut : MonoBehaviour
{
    [SerializeField] CanvasGroup m_canvasGroup;

    [SerializeField] float m_fadeRate;

    void FadeOut()
    {
        StartCoroutine(FadeCoroutine());
    }

    IEnumerator FadeCoroutine()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;

        while(m_canvasGroup.alpha < 1)
        {
            m_canvasGroup.alpha += m_fadeRate * Time.fixedDeltaTime;
            m_canvasGroup.alpha = Mathf.Clamp01(m_canvasGroup.alpha);

            yield return new WaitForFixedUpdate();
        }
    }
}
