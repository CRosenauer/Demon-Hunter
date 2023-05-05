using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleComponent : MonoBehaviour
{
    [SerializeField] GameObject m_dropObject;

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

        if(m_animator)
        {
            m_animator.SetTrigger("OnHit");
        }

        if(m_dropObject)
        {
            Instantiate(m_dropObject, transform.position, Quaternion.identity);
        }

        m_isHit = true;
    }

    Animator m_animator;
    bool m_isHit;
}
