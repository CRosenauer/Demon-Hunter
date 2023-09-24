using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnergyBallComponent : MonoBehaviour
{
    [SerializeField] float m_speed;

    void OnSetDirection(Vector3 targetPosition)
    {
        Vector3 thisToTarget = targetPosition - transform.position;

        Vector2 m_direction = new(thisToTarget.x, thisToTarget.y);
        m_rbody.velocity = m_direction.normalized * m_speed;
    }

    void OnHitOther(int layerMask)
    {
        Destroy(gameObject);
    }

    Rigidbody2D m_rbody;
}
