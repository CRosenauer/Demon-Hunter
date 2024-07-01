using UnityEngine;

[RequireComponent(typeof(MovementComponent))]
public class EnergyBallComponent : BaseController
{
    [SerializeField] float m_speed;
    [SerializeField] bool m_lockedToXAxis;

    private void Awake()
    {
        base.Awake();
        m_movementComponent.ApplyGravity = false;
    }

    void OnSetDirection(Vector2 targetDirection)
    {
        Vector2 m_direction = m_lockedToXAxis ? new(targetDirection.x, 0) : targetDirection;
        m_movementComponent.Move(m_direction.normalized * m_speed);
    }

    void OnHitOther(int layerMask)
    {
        Destroy(gameObject);
    }

    void OnDeath()
    {
        Destroy(gameObject);
    }

    void OnClear()
    {
        Destroy(gameObject);
    }
}
