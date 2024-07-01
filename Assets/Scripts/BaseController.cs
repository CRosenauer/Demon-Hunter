using UnityEngine;

[RequireComponent(typeof(MovementComponent))]
[RequireComponent(typeof(Animator))]
public abstract class BaseController : MonoBehaviour
{
    protected void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_movementComponent = GetComponent<MovementComponent>();
    }

    protected Animator m_animator;
    protected MovementComponent m_movementComponent;
}
