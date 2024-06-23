using System.Collections;
using UnityEngine;

public class FreezeFrame : FreezeComponent
{
    [SerializeField] float m_freezeDuration;

    void OnHit()
    {
        if(m_unfreezeCoroutine != null)
        {
            StopCoroutine(m_unfreezeCoroutine);
            m_unfreezeCoroutine = null;
        }

        m_unfreezeCoroutine = StartCoroutine(FreezeCoroutine());
    }

    IEnumerator FreezeCoroutine()
    {
        Freeze();

        yield return new WaitForSeconds(m_freezeDuration);

        Unfreeze();
    }

    Coroutine m_unfreezeCoroutine;
}
