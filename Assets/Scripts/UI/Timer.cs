using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] Text m_text;
    [SerializeField] int m_startTimer;

    void Start()
    {
        Debug.Assert(m_text);
        m_timerCoroutine = StartCoroutine(TimerCoroutine());
    }

    void SetTimerText(int time)
    {
        m_text.text = $"{time}";
    }

    IEnumerator TimerCoroutine()
    {
        int timeRemaining = m_startTimer;
        SetTimerText(timeRemaining);

        while (timeRemaining > 0)
        {
            yield return new WaitForSeconds(1f);
            timeRemaining -= 1;
            SetTimerText(timeRemaining);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.BroadcastMessage("OnDeath");
    }

    public void StopTimer()
    {
        StopCoroutine(m_timerCoroutine);
    }

    Coroutine m_timerCoroutine;
}
