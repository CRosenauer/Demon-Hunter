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
    new void Start()
    {
        base.Start();

        Vector2 velocity = new(m_xSpeed * transform.localScale.x, m_ySpeed);
        m_rbody.velocity = velocity;
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
