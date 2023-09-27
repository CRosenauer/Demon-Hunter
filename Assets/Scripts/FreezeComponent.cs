using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeComponent : MonoBehaviour
{
    [SerializeField] public List<Behaviour> m_componentsToFreeze;

    void Start()
    {
        InitializeComponents();
    }

    protected void InitializeComponents()
    {
        if (m_componentsToFreeze.Count == 0)
        {
            foreach (MonoBehaviour component in gameObject.GetComponents<MonoBehaviour>())
            {
                m_componentsToFreeze.Add(component);
            }
        }
    }

    protected void Freeze()
    {
        MovementComponent movementComponent = GetComponent<MovementComponent>();
        if (movementComponent)
        {
            movementComponent.SendMessage("FreezeMovement");
        }

        foreach (MonoBehaviour component in m_componentsToFreeze)
        {
            component.enabled = false;
        }
    }

    protected void Unfreeze()
    {
        MovementComponent movementComponent = GetComponent<MovementComponent>();
        if (movementComponent)
        {
            movementComponent.SendMessage("UnfreezeMovement");
        }

        foreach (MonoBehaviour component in m_componentsToFreeze)
        {
            component.enabled = true;
        }
    }
}
