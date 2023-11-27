using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ScoreEvent : UnityEvent<int> { }

public class ScoreChangedEventListener : MonoBehaviour
{
    public ScoreChangedEvent m_menuEvent;
    public ScoreEvent m_response;

    public void OnEventRaised(int score)
    {
        m_response.Invoke(score);
    }

    void OnEnable()
    {
        m_menuEvent.RegisterListener(this);
    }

    private void OnDisable()
    {
        m_menuEvent.UnregisterListener(this);
    }
}
