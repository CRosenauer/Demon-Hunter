using System.Collections.Generic;
using UnityEngine;

public class TriggerMessageCallback : TriggerCallback
{
    [SerializeField] List<GameObject> m_callbackObjects;
    [SerializeField] List<string> m_callbackNames;

    new void Start()
    {
        Debug.Assert(m_callbackObjects.Count == m_callbackNames.Count);
        base.Start();
    }

    protected override void Callback()
    {
        for (int i = 0; i < m_callbackObjects.Count; ++i)
        {
            GameObject obj = m_callbackObjects[i];
            string str = m_callbackNames[i];
            obj.SendMessage(str);
        }
    }
}
