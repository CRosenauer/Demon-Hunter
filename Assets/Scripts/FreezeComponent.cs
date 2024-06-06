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

        List<Behaviour> components = new(gameObject.GetComponents<Behaviour>());
        foreach(Behaviour component in m_componentsToExclude)
        {
            components.Remove(component);
        }

        foreach (Behaviour component in components)
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

        List<Behaviour> components = new(gameObject.GetComponents<Behaviour>());
        foreach (Behaviour component in m_componentsToExclude)
        {
            components.Remove(component);
        }

        foreach (Behaviour component in components)
        {
            component.enabled = true;
        }
    }
}
