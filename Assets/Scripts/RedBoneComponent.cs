using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedBoneComponent : EnemyComponent
{
    [SerializeField] float m_activeDistance;
    [Space]

    [SerializeField] float m_xSpeed;
    [SerializeField] float m_ySpeed;

    // Start is called before the first frame update
    void Start()
    {
        m_player = GameObject.FindGameObjectWithTag("Player");
        Debug.Assert(m_player);

        QueryDirectionToPlayer();

        Vector2 velocity = new(m_xSpeed * transform.localScale.x, m_ySpeed);
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
}
