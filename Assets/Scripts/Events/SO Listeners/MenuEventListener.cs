using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MenuEventListener : MonoBehaviour
{
    public MenuEvent m_menuEvent;

    public UnityEvent m_response;

    public void OnEventRaised()
    {
        m_response.Invoke();
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
