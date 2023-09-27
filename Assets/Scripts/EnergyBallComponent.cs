using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnergyBallComponent : MonoBehaviour
{
    [SerializeField] Rigidbody2D m_rbody;
    [SerializeField] float m_speed;

    void OnSetDirection(Vector2 targetDirection)
    {
        Vector2 m_direction = new(targetDirection.x, 0);
        m_rbody.velocity = m_direction.normalized * m_speed;
    }

    void OnHitOther(int layerMask)
    {
        Destroy(gameObject);
    }
}
