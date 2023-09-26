using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedBoneComponent : MovementComponent
{
    [SerializeField] float m_activeDistance;
    [Space]

    [SerializeField] float m_xSpeed;
    [SerializeField] float m_ySpeed;

    // Start is called before the first frame update
    void Start()
    {
        ComponentInit();

        m_player = GameObject.FindGameObjectWithTag("Player");
        Debug.Assert(m_player);

        Vector3 thisToPlayer = m_player.transform.position - transform.position;
        UpdateDirection(thisToPlayer.x);

        Vector2 velocity = new( thisToPlayer.x >= 0f ? m_xSpeed : -m_xSpeed, m_ySpeed);
        m_rbody.velocity = velocity;
    }

    void FixedUpdate()
    {
        Vector3 distSquared = transform.position - m_player.transform.position;

        if (distSquared.sqrMagnitude > m_activeDistance * m_activeDistance)
        {
            Destroy(gameObject);
        }
    }

    void OnHitOther(int layerMask)
    {
        Destroy(gameObject);
    }

    GameObject m_player;
}
