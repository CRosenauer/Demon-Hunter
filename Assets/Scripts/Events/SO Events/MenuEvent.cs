using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/MenuEvent")]
public class MenuEvent : ScriptableObject
{
    public List<MenuEventListener> m_eventListeners = new List<MenuEventListener>();

    public void RegisterListener(MenuEventListener listener)
    {
        if(m_eventListeners.Contains(listener))
        {
            return;
        }

        m_eventListeners.Add(listener);
    }

    public void UnregisterListener(MenuEventListener listener)
    {
        if (!m_eventListeners.Contains(listener))
        {
            return;
        }

        m_eventListeners.Remove(listener);
    }

    public void Raise()
    {
        foreach (MenuEventListener listener in m_eventListeners)
        {
            listener.OnEventRaised();
        }
    }
}
