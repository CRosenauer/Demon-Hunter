using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanOfKnivesObjectComponent : MovementComponent
{
    [SerializeField] float m_speed;

    // Start is called before the first frame update
    void Start()
    {
        ComponentInit();

        m_movementDirection = transform.rotation * new Vector2(1f, 0f);
    }

    void FixedUpdate()
    {
        Move(m_movementDirection * m_speed);

        if(!IsWithinCameraFrustum())
        {
            Destroy(gameObject);
        }
    }

    void OnHitOther()
    {
        Destroy(gameObject);
    }

    Vector2 m_movementDirection;
}
