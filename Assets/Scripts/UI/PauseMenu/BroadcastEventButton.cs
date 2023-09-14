using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BroadcastEventButton : Button
{
    [SerializeField] GameObject m_targetObject;
    [SerializeField] string m_eventName;

    new void Start()
    {
        Debug.Assert(m_targetObject);
        Debug.Assert(!string.IsNullOrEmpty(m_eventName));

        base.Start();
    }

    public override void OnSelect() 
    {
        m_targetObject.BroadcastMessage(m_eventName);
    }
}