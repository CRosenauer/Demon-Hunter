using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/ScoreChanged")]
public class ScoreChangedEvent : ScriptableObject
{
    public List<ScoreChangedEventListener> m_eventListeners = new List<ScoreChangedEventListener>();

    void Awake()
    {
        Reset();
    }

    public void RegisterListener(ScoreChangedEventListener listener)
    {
        if(m_eventListeners.Contains(listener))
        {
            return;
        }

        m_eventListeners.Add(listener);
    }

    public void UnregisterListener(ScoreChangedEventListener listener)
    {
        if (!m_eventListeners.Contains(listener))
        {
            return;
        }

        m_eventListeners.Remove(listener);
    }

    public void Raise(int deltaScore)
    {
        m_score = m_score + deltaScore;

        NotifyChange();
    }

    public void Reset()
    {
        m_score = 0;

        NotifyChange();
    }

    void NotifyChange()
    {
        foreach (ScoreChangedEventListener listener in m_eventListeners)
        {
            listener.OnEventRaised(m_score);
        }
    }

    int m_score;
}
