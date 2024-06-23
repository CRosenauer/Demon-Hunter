using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/LivesChanged")]
public class LivesChangedEvent : ScriptableObject
{
    public List<LivesChangedEventListener> m_eventListeners = new List<LivesChangedEventListener>();

    public void RegisterListener(LivesChangedEventListener listener)
    {
        if (m_eventListeners.Contains(listener))
        {
            return;
        }

        m_eventListeners.Add(listener);
    }

    public void UnregisterListener(LivesChangedEventListener listener)
    {
        if (!m_eventListeners.Contains(listener))
        {
            return;
        }

        m_eventListeners.Remove(listener);
    }

    public void Raise()
    {
        m_lives = m_lives - 1;

        NotifyChange();
    }

    public void Reset()
    {
        m_lives = 3;

        NotifyChange();
    }

    void NotifyChange()
    {
        foreach (LivesChangedEventListener listener in m_eventListeners)
        {
            listener.OnEventRaised(m_lives);
        }
    }

    int m_lives;
}
