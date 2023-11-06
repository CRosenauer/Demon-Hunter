using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerObjectActivator : TriggerCallback
{
    [SerializeField] GameObject m_activatingObject;

    // Start is called before the first frame update
    new void Start()
    {
        Debug.Assert(m_activatingObject);
        base.Start();
    }

    protected override void Callback()
    {
        m_activatingObject.SetActive(true);
    }
}
