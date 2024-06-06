using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class MenuNavigationInputEvent : UnityEvent<Vector2> { }

public class MenuNavigationEventListener : MonoBehaviour
{
    public MenuNavigationEvent m_menuEvent;

    public MenuNavigationInputEvent m_response;

    public void OnEventRaised(Vector2 input)
    {
        m_response.Invoke(input);
    }

    void OnEnable()
    {
        m_menuEvent.RegisterListener(this);
    }

    void OnDisable()
    {
        m_menuEvent.UnregisterListener(this);
    }
}
