using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleComponent : MonoBehaviour
{
    [SerializeField] GameObject m_dropObject;
    [SerializeField] bool m_destroyAfterHit;

    // Start is called before the first frame update
    void Start()
    {
        m_animator = GetComponent<Animator>();

        m_isHit = false;
    }

    void OnHit(float damage)
    {
        if(m_isHit)
        {
            return;
        }

        Collider2D collider = GetComponent<BoxCollider2D>();
        collider.enabled = false;

        if(m_animator)
        {
            m_animator.SetTrigger("OnHit");
        }

        if(m_dropObject)
        {
            Instantiate(m_dropObject, transform.position, Quaternion.identity);
        }

        m_isHit = true;

        if(m_destroyAfterHit)
        {
            Destroy(gameObject);
        }
    }

    Animator m_animator;
    bool m_isHit;
}
