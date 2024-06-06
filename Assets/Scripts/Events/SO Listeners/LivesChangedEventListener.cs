using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class LivesEvent : UnityEvent<int> { }

public class LivesChangedEventListener : MonoBehaviour
{
    public LivesChangedEvent m_livesEvent;
    public ScoreEvent m_response;

    public void OnEventRaised(int lives)
    {
        m_response.Invoke(lives);
    }

    void OnEnable()
    {
        m_livesEvent.RegisterListener(this);
    }

    private void OnDisable()
    {
        m_livesEvent.UnregisterListener(this);
    }
}
