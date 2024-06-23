using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnergyBallComponent : MonoBehaviour
{
    [SerializeField] Rigidbody2D m_rbody;
    [SerializeField] float m_speed;
    [SerializeField] bool m_lockedToXAxis;

    void OnSetDirection(Vector2 targetDirection)
    {
        Vector2 m_direction = m_lockedToXAxis ? new(targetDirection.x, 0) : targetDirection;
        m_rbody.velocity = m_direction.normalized * m_speed;
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
