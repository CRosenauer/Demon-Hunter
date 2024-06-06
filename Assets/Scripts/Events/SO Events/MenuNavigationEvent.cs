using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/MenuNavigationEvent")]
public class MenuNavigationEvent : ScriptableObject
{
    public List<MenuNavigationEventListener> m_eventListeners = new List<MenuNavigationEventListener>();

    public void RegisterListener(MenuNavigationEventListener listener)
    {
        if (m_eventListeners.Contains(listener))
        {
            return;
        }

        m_eventListeners.Add(listener);
    }

    public void UnregisterListener(MenuNavigationEventListener listener)
    {
        if (!m_eventListeners.Contains(listener))
        {
            return;
        }

        m_eventListeners.Remove(listener);
    }

    public void Raise(Vector2 input)
    {
        foreach (MenuNavigationEventListener listener in m_eventListeners)
        {
            listener.OnEventRaised(input);
        }
    }
}
