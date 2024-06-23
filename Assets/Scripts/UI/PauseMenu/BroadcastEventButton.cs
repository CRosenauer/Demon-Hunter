using UnityEngine;

public class BroadcastEventButton : Button
{
    [SerializeField] GameObject m_targetObject;
    [SerializeField] string m_eventName;

    void Start()
    {
        Debug.Assert(m_targetObject);
        Debug.Assert(!string.IsNullOrEmpty(m_eventName));
    }

    public override void OnSelect() 
    {
        m_targetObject.BroadcastMessage(m_eventName);
    }
}
