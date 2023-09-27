using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeComponent : MonoBehaviour
{
    [SerializeField] public List<Behaviour> m_componentsToExclude;

    void Start()
    {
        m_componentsToExclude.Add(this);
    }

    protected void Freeze()
    {
        MovementComponent movementComponent = GetComponent<MovementComponent>();
        if (movementComponent)
        {
            movementComponent.SendMessage("FreezeMovement");
        }

        Behaviour[] components = gameObject.GetComponents<Behaviour>();
        foreach (Behaviour component in components)
        {
            component.enabled = false;
        }

        foreach (Behaviour component in m_componentsToExclude)
        {
            component.enabled = true;
        }
    }

    protected void Unfreeze()
    {
        MovementComponent movementComponent = GetComponent<MovementComponent>();
        if (movementComponent)
        {
            movementComponent.SendMessage("UnfreezeMovement");
        }

        Behaviour[] components = gameObject.GetComponents<Behaviour>();
        foreach (Behaviour component in components)
        {
            component.enabled = true;
        }
    }
}
